using System.Text.Json;

namespace FpsAtolyesi.Core;

public static class Reg
{
    public static RegistryKey Base(string hive) => RegistryKey.OpenBaseKey(hive switch
    {
        "HKLM" => RegistryHive.LocalMachine,
        "HKU" => RegistryHive.Users,
        _ => RegistryHive.CurrentUser,
    }, RegistryView.Registry64);

    public static object Get(string hive, string path, string name)
    {
        try
        {
            using var b = Base(hive);
            using var k = b.OpenSubKey(path);
            return k?.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        }
        catch { return null; }
    }

    public static bool KeyExists(string hive, string path)
    {
        try { using var b = Base(hive); using var k = b.OpenSubKey(path); return k != null; } catch { return false; }
    }

    public static string[] ValueNames(string hive, string path)
    {
        try { using var b = Base(hive); using var k = b.OpenSubKey(path); return k?.GetValueNames() ?? Array.Empty<string>(); }
        catch { return Array.Empty<string>(); }
    }

    /// <summary>Değeri yazar; önce orijinal hâlini yedekler.</summary>
    public static void Set(string group, string hive, string path, string name, object value, RegistryValueKind kind)
    {
        Backup.RecordReg(group, hive, path, name);
        RawSet(hive, path, name, value, kind);
    }

    public static void RawSet(string hive, string path, string name, object value, RegistryValueKind kind)
    {
        using var b = Base(hive);
        using var k = b.CreateSubKey(path, true);
        k.SetValue(name, value, kind);
    }
}

public class BackupEntry
{
    public string Type;      // reg | pscheme | psetting
    public string Group;     // hangi modül/ayar
    public string Hive, Path, Name, Extra;
    public bool Existed;
    public string Kind, Data;
    public DateTime Time;
}

/// <summary>Her değişikliğin orijinal değerini saklar; tek tuşla geri almayı sağlar.</summary>
public static class Backup
{
    static readonly object Lock = new();
    static string FilePath => System.IO.Path.Combine(Paths.Data, "backup.json");
    public static List<BackupEntry> Entries { get; private set; } = new();

    public static void Load()
    {
        try { if (File.Exists(FilePath)) Entries = JsonSerializer.Deserialize<List<BackupEntry>>(File.ReadAllText(FilePath), Paths.Json) ?? new(); }
        catch
        {
            // Bozuk yedek dosyasını silmek yerine kenara al.
            try { File.Copy(FilePath, FilePath + ".bozuk-" + DateTime.Now.ToString("yyyyMMddHHmmss"), true); } catch { }
            Entries = new();
        }
    }

    class ExportFile { public string App = "UPO"; public string Machine; public DateTime Exported; public List<BackupEntry> Entries; }

    /// <summary>Yedeği bir dosyaya kaydeder (format sonrası veya başka bilgisayarda geri yüklemek için).</summary>
    public static int Export(string file)
    {
        lock (Lock)
        {
            var x = new ExportFile { Machine = Environment.MachineName, Exported = DateTime.Now, Entries = Entries.ToList() };
            File.WriteAllText(file, JsonSerializer.Serialize(x, Paths.Json));
            return x.Entries.Count;
        }
    }

    /// <summary>Dışa aktarılmış yedeği içeri alır; aynı ayar için mevcut kayıt varsa onu korur.</summary>
    public static (int added, int skipped, string machine) Import(string file)
    {
        var x = JsonSerializer.Deserialize<ExportFile>(File.ReadAllText(file), Paths.Json);
        if (x?.Entries == null || x.App != "UPO") throw new Exception(L.T("Bu dosya bir UPO yedeği değil."));
        lock (Lock)
        {
            int add = 0, skip = 0;
            foreach (var e in x.Entries)
            {
                bool exists = Entries.Any(o => o.Type == e.Type && o.Hive == e.Hive && string.Equals(o.Path, e.Path, StringComparison.OrdinalIgnoreCase)
                                               && string.Equals(o.Name, e.Name, StringComparison.OrdinalIgnoreCase) && o.Extra == e.Extra);
                if (exists) { skip++; continue; }
                Entries.Add(e); add++;
            }
            Save();
            return (add, skip, x.Machine);
        }
    }

    static void Save()
    {
        var tmp = FilePath + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(Entries, Paths.Json));
        File.Move(tmp, FilePath, true);
    }

    public static void RecordReg(string group, string hive, string path, string name)
    {
        lock (Lock)
        {
            if (Entries.Any(e => e.Type == "reg" && e.Hive == hive && string.Equals(e.Path, path, StringComparison.OrdinalIgnoreCase)
                                  && string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase))) return;
            var e = new BackupEntry { Type = "reg", Group = group, Hive = hive, Path = path, Name = name, Time = DateTime.Now };
            try
            {
                using var b = Reg.Base(hive);
                using var k = b.OpenSubKey(path);
                var v = k?.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                if (v != null)
                {
                    var kind = k.GetValueKind(name);
                    e.Existed = true;
                    e.Kind = kind.ToString();
                    e.Data = Ser(v, kind);
                }
            }
            catch { }
            Entries.Add(e);
            Save();
        }
    }

    public static void RecordPowerScheme(string group, string oldGuid)
    {
        lock (Lock)
        {
            if (Entries.Any(e => e.Type == "pscheme")) return;
            Entries.Add(new BackupEntry { Type = "pscheme", Group = group, Data = oldGuid, Time = DateTime.Now });
            Save();
        }
    }

    public static void RecordPowerSetting(string group, string scheme, string sub, string setting, int ac)
    {
        lock (Lock)
        {
            if (Entries.Any(e => e.Type == "psetting" && e.Path == scheme && e.Name == sub && e.Extra == setting)) return;
            Entries.Add(new BackupEntry { Type = "psetting", Group = group, Path = scheme, Name = sub, Extra = setting, Data = ac.ToString(), Time = DateTime.Now });
            Save();
        }
    }

    static string Ser(object v, RegistryValueKind k) => k switch
    {
        RegistryValueKind.DWord => Convert.ToInt32(v).ToString(),
        RegistryValueKind.QWord => Convert.ToInt64(v).ToString(),
        RegistryValueKind.Binary => Convert.ToBase64String((byte[])v),
        RegistryValueKind.MultiString => string.Join("\u0001", (string[])v),
        _ => v.ToString(),
    };

    static object Deser(RegistryValueKind k, string d) => k switch
    {
        RegistryValueKind.DWord => int.Parse(d),
        RegistryValueKind.QWord => long.Parse(d),
        RegistryValueKind.Binary => Convert.FromBase64String(d),
        RegistryValueKind.MultiString => d.Length == 0 ? Array.Empty<string>() : d.Split('\u0001'),
        _ => d,
    };

    public static IEnumerable<IGrouping<string, BackupEntry>> Groups() { lock (Lock) return Entries.GroupBy(e => e.Group).ToList(); }

    /// <summary>Seçilen kayıtları ters sırayla geri yükler. Başarılı olanlar yedekten çıkarılır.</summary>
    public static (int ok, int fail) Restore(Func<BackupEntry, bool> filter)
    {
        lock (Lock)
        {
            var sel = Entries.Where(filter).ToList();
            int ok = 0, fail = 0;
            bool mouse = false, scheme = false;
            var services = new List<string>();
            for (int i = sel.Count - 1; i >= 0; i--)
            {
                var e = sel[i];
                try
                {
                    switch (e.Type)
                    {
                        case "reg":
                            RestoreReg(e);
                            if (e.Path.Equals(@"Control Panel\Mouse", StringComparison.OrdinalIgnoreCase)) mouse = true;
                            var m = Regex.Match(e.Path, @"^SYSTEM\\CurrentControlSet\\Services\\([^\\]+)$", RegexOptions.IgnoreCase);
                            if (m.Success && e.Name == "Start") services.Add(m.Groups[1].Value);
                            Log.Dim($"  [<] {e.Hive}\\{e.Path} → {e.Name}");
                            break;
                        case "psetting":
                            if (ProcRunner.Exec("powercfg.exe", $"/setacvalueindex {e.Path} {e.Name} {e.Extra} {e.Data}").Code != 0)
                                throw new Exception(L.T("Güç ayarı geri yüklenemedi."));
                            Log.Dim(L.F("  [<] Güç ayarı {0}… → {1}", e.Extra[..8], e.Data));
                            break;
                        case "pscheme":
                            if (ProcRunner.Exec("powercfg.exe", "/setactive " + e.Data).Code != 0)
                                throw new Exception(L.T("Eski güç planı etkinleştirilemedi."));
                            scheme = true;
                            Log.Dim(L.F("  [<] Güç planı → {0}", e.Data));
                            break;
                    }
                    Entries.Remove(e);
                    ok++;
                }
                catch (Exception ex) { fail++; Log.Err($"  [X] {e.Path}\\{e.Name}: {ex.Message}"); }
            }
            var act = Power.Active().Guid;
            if (act != null) ProcRunner.Capture("powercfg.exe", "/setactive " + act);
            if (scheme)
            {
                foreach (var g in State.Data.CreatedSchemes.ToList())
                    if (g != act) { ProcRunner.Capture("powercfg.exe", "-delete " + g); State.Data.CreatedSchemes.Remove(g); }
                State.Save();
            }
            if (mouse) Native.RefreshMouse();
            foreach (var s in services)
            {
                var st = Reg.Get("HKLM", @"SYSTEM\CurrentControlSet\Services\" + s, "Start");
                if (st != null && Convert.ToInt32(st) == 2) ProcRunner.Capture("sc.exe", "start " + s);
            }
            Save();
            return (ok, fail);
        }
    }

    static void RestoreReg(BackupEntry e)
    {
        using var b = Reg.Base(e.Hive);
        if (!e.Existed)
        {
            using var k = b.OpenSubKey(e.Path, true);
            k?.DeleteValue(e.Name, false);
        }
        else
        {
            var kind = Enum.Parse<RegistryValueKind>(e.Kind);
            using var k = b.CreateSubKey(e.Path, true);
            k.SetValue(e.Name, Deser(kind, e.Data), kind);
        }
    }

    /// <summary>İlk değişiklikten önce Windows sistem geri yükleme noktası oluşturmayı dener (ekstra güvenlik).</summary>
    public static async Task EnsureRestorePoint()
    {
        if (Entries.Count > 0) return;
        if (!Settings.S.RestorePoint) { Log.Dim(L.T("Geri yükleme noktası oluşturma Ayarlar'dan kapatılmış; yalnızca uygulamanın kendi yedeği alınıyor.")); return; }
        if (State.Data.LastRestorePoint is DateTime t && (DateTime.Now - t).TotalHours < 24) return;
        Log.W(L.T("Sistem geri yükleme noktası oluşturuluyor (ekstra güvenlik)…"));
        var o = await Task.Run(() => ProcRunner.Capture("powershell.exe",
            "-NoProfile -ExecutionPolicy Bypass -Command \"try { Checkpoint-Computer -Description 'UPO Ultimate Performance Optimizer' -RestorePointType 'MODIFY_SETTINGS' -ErrorAction Stop; 'OK' } catch { 'FAIL: ' + $_.Exception.Message }\"",
            180000));
        if (o.Trim() == "OK")
        {
            Log.Ok(L.T("[OK] Geri yükleme noktası oluşturuldu."));
            State.Data.LastRestorePoint = DateTime.Now; State.Save();
        }
        else Log.Warn(L.T("[!] Geri yükleme noktası oluşturulamadı (Sistem Koruması kapalı olabilir). Uygulamanın kendi yedeği yine de alınıyor."));
    }
}
