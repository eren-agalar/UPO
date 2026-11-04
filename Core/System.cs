using System.Management;

namespace FpsAtolyesi.Core;

// ───────────────────────────── Donanım bilgisi ─────────────────────────────
public class SysInfo
{
    public string Cpu = "?", Gpu = "?", Os = "Windows";
    public long Ram;

    public static SysInfo Load()
    {
        var s = new SysInfo();
        try { s.Cpu = ((Reg.Get("HKLM", @"HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString") as string) ?? "?").Trim(); } catch { }
        try
        {
            var names = new List<string>();
            using (var q = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                foreach (ManagementObject o in q.Get()) { var n = o["Name"]?.ToString(); if (!string.IsNullOrWhiteSpace(n)) names.Add(n.Trim()); }
            names = names.Where(n => !Regex.IsMatch(n, "basic|virtual|remote|parsec|meta", RegexOptions.IgnoreCase)).ToList();
            s.Gpu = names.FirstOrDefault(n => Regex.IsMatch(n, "nvidia|geforce|radeon|amd|arc", RegexOptions.IgnoreCase)) ?? names.FirstOrDefault() ?? "?";
        }
        catch { }
        // Takılı RAM modüllerinin toplamı (ör. 16 GB). Okunamazsa Windows'un kullanılabilir bellek değerine düşülür.
        try
        {
            using var q = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
            foreach (ManagementObject o in q.Get()) s.Ram += Convert.ToInt64(o["Capacity"]);
        }
        catch { s.Ram = 0; }
        if (s.Ram <= 0)
        {
            try
            {
                using var q = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (ManagementObject o in q.Get()) s.Ram = Convert.ToInt64(o["TotalPhysicalMemory"]);
            }
            catch { s.Ram = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes; }
        }
        try
        {
            const string cv = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            var ed = (Reg.Get("HKLM", cv, "EditionID") as string ?? "") switch
            {
                "Core" => "Home", "CoreSingleLanguage" => "Home Single Language", "Professional" => "Pro",
                "EnterpriseS" => "Enterprise LTSC", "IoTEnterpriseS" => "IoT Enterprise LTSC", "EnterpriseG" => "Enterprise G",
                "ProfessionalWorkstation" => "Pro for Workstations", "Education" => "Education", var x => x,
            };
            var dv = Reg.Get("HKLM", cv, "DisplayVersion") as string ?? "";
            s.Os = $"{(Tweaks.Build >= 22000 ? "Windows 11" : "Windows 10")} {ed} {dv}".Trim();
        }
        catch { }
        return s;
    }

    public string RamText => Ram > 0 ? Math.Round(Ram / (double)(1L << 30)) + " GB RAM" : "";
}

// ───────────────────────────── Bellek Bütünlüğü (VBS / HVCI) ─────────────────────────────
public static class Vbs
{
    public const string DG = @"SYSTEM\CurrentControlSet\Control\DeviceGuard";
    public const string HV = DG + @"\Scenarios\HypervisorEnforcedCodeIntegrity";

    public static bool HvciConfigured()
    {
        var v = Reg.Get("HKLM", HV, "Enabled");
        return v != null && Convert.ToInt32(v) == 1;
    }

    public static bool PolicyManaged() => Reg.Get("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", "HypervisorEnforcedCodeIntegrity") != null;

    /// <summary>vbs: 0 kapalı, 1 etkin ama çalışmıyor, 2 çalışıyor, -1 bilinmiyor.</summary>
    public static (int vbs, bool hvciRunning) Runtime()
    {
        try
        {
            using var q = new ManagementObjectSearcher(@"root\Microsoft\Windows\DeviceGuard", "SELECT * FROM Win32_DeviceGuard");
            foreach (ManagementObject o in q.Get())
            {
                int st = Convert.ToInt32(o["VirtualizationBasedSecurityStatus"]);
                bool h = o["SecurityServicesRunning"] is uint[] run && run.Contains(2u);
                return (st, h);
            }
        }
        catch { }
        return (-1, false);
    }

    public static void Disable() => Reg.Set("vbs", "HKLM", HV, "Enabled", 0, RegistryValueKind.DWord);

    public static void Enable()
    {
        if (Backup.Entries.Any(e => e.Group == "vbs")) Backup.Restore(e => e.Group == "vbs");
        if (!HvciConfigured()) Reg.RawSet("HKLM", HV, "Enabled", 1, RegistryValueKind.DWord);
    }
}

// ───────────────────────────── Başlangıç uygulamaları ─────────────────────────────
public enum StartupClass { Neutral, Recommended, Protected, Whitelisted }

public class StartupEntry
{
    public string Name, Command, Source, ApprovedHive, ApprovedPath;
    public bool Enabled;
    public StartupClass Class;
}

public static class Startup
{
    const string Approved = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\";

    // Kapatılması önerilmeyenler: anti-cheat, güvenlik, ses/ekran/dokunmatik yüzey sürücüleri.
    static readonly string[] Protect =
    {
        "vgc", "vgtray", "vanguard", "faceit", "easyanticheat", "battleye", "esea", "securityhealth", "windowsdefender", "defender",
        "avast", "avg", "kaspersky", "eset", "bitdefender", "mcafee", "norton", "malwarebytes", "sophos", "trend micro",
        "realtek", "rtkaud", "rthdvcpl", "nahimic", "waves", "maxxaudio", "igfx", "intel", "nvidia", "nvcpl", "amd", "radeon",
        "synaptics", "elan", "ctfmon", "rundll32", "wacom", "bluetooth",
    };
    // Genelde güvenle kapatılabilenler (uygulama isteyince yine elle açılır).
    static readonly string[] Recommend =
    {
        "discord", "steam", "epicgames", "epic games", "spotify", "opera", "teams", "skype", "microsoftedgeautolaunch", "edgeupdate",
        "eadesktop", "ea desktop", "origin", "ubisoft", "upc.exe", "battle.net", "galaxyclient", "gog galaxy", "adobe", "ccleaner",
        "utorrent", "bittorrent", "qbittorrent", "zoom", "whatsapp", "telegram", "cortana", "lghub", "icue", "razer synapse",
        "wallpaper", "overwolf", "medal", "outplayed", "googledrive", "dropbox", "yandex", "itunes", "cisco webex", "slack",
    };

    public static List<StartupEntry> Load()
    {
        var list = new List<StartupEntry>();
        void FromReg(string hive, string run, string appr, string label)
        {
            try
            {
                using var b = Reg.Base(hive);
                using var k = b.OpenSubKey(run);
                if (k == null) return;
                foreach (var n in k.GetValueNames())
                {
                    if (string.IsNullOrEmpty(n)) continue;
                    list.Add(Make(n, k.GetValue(n)?.ToString() ?? "", label, hive, Approved + appr));
                }
            }
            catch { }
        }
        void FromDir(Environment.SpecialFolder f, string hive, string label)
        {
            try
            {
                var d = Environment.GetFolderPath(f);
                if (!Directory.Exists(d)) return;
                foreach (var file in Directory.GetFiles(d))
                {
                    var n = System.IO.Path.GetFileName(file);
                    if (n.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase)) continue;
                    list.Add(Make(n, file, label, hive, Approved + "StartupFolder"));
                }
            }
            catch { }
        }
        FromReg("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Run", "Run", L.T("Kullanıcı"));
        FromReg("HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Run", L.T("Tüm kullanıcılar"));
        FromReg("HKLM", @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", "Run32", L.T("Tüm kullanıcılar (32-bit)"));
        FromDir(Environment.SpecialFolder.Startup, "HKCU", L.T("Başlangıç klasörü"));
        FromDir(Environment.SpecialFolder.CommonStartup, "HKLM", L.T("Ortak başlangıç klasörü"));
        return list.OrderBy(e => e.Class == StartupClass.Recommended ? 0 : e.Class == StartupClass.Neutral ? 1 : 2).ThenBy(e => e.Name).ToList();
    }

    static StartupEntry Make(string name, string cmd, string src, string hive, string approvedPath)
    {
        var e = new StartupEntry { Name = name, Command = cmd, Source = src, ApprovedHive = hive, ApprovedPath = approvedPath };
        var b = Reg.Get(hive, approvedPath, name) as byte[];
        e.Enabled = b == null || b.Length == 0 || (b[0] & 1) == 0;
        var key = (name + " " + cmd).ToLowerInvariant();
        e.Class = Settings.IsWhitelisted(name) ? StartupClass.Whitelisted
                : Protect.Any(key.Contains) ? StartupClass.Protected
                : Recommend.Any(key.Contains) ? StartupClass.Recommended : StartupClass.Neutral;
        return e;
    }

    /// <summary>Görev Yöneticisi'nin kullandığı yöntemle aç/kapat (silmez, geri alınabilir).</summary>
    public static void SetEnabled(StartupEntry e, bool on)
    {
        var data = new byte[12];
        if (on) data[0] = 0x02;
        else
        {
            data[0] = 0x03;
            BitConverter.GetBytes(DateTime.Now.ToFileTime()).CopyTo(data, 4);
        }
        Reg.Set("startup", e.ApprovedHive, e.ApprovedPath, e.Name, data, RegistryValueKind.Binary);
        e.Enabled = on;
    }
}

// ───────────────────────────── Temizlik ─────────────────────────────
public class CleanTarget
{
    public string Id, Title, Desc;
    public string[] Paths = Array.Empty<string>();
    public bool StopUpdate, Dism, DefaultOn = true;
    public long Size = -1;
}

public static class Cleaner
{
    static string LA(string rel) => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), rel);
    static string Win(string rel) => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), rel);
    static string LocalLow(string rel) => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", rel);
    static string PD(string rel) => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), rel);

    public static List<CleanTarget> Targets() => new()
    {
        new CleanTarget { Id = "temp", Title = L.T("Geçici dosyalar"), Desc = L.T("Kullanıcı ve Windows Temp klasörleri"),
            Paths = new[] { System.IO.Path.GetTempPath(), Win("Temp") } },
        new CleanTarget { Id = "shader", Title = L.T("Shader önbelleği (DirectX / NVIDIA / AMD / Intel)"), Desc = L.T("Oyunlar ilk açılışta yeniden oluşturur; bozuk önbellek takılmaya yol açabilir"),
            Paths = new[] { LA("D3DSCache"), LA(@"NVIDIA\DXCache"), LA(@"NVIDIA\GLCache"), LocalLow(@"NVIDIA\PerDriverVersion\DXCache"),
                            PD(@"NVIDIA Corporation\NV_Cache"), LA(@"AMD\DxCache"), LA(@"AMD\DxcCache"), LA(@"AMD\GLCache"), LA(@"AMD\VkCache"), LocalLow(@"Intel\ShaderCache") } },
        new CleanTarget { Id = "wu", Title = L.T("Windows Update önbelleği"), Desc = L.T("İndirilip kurulmuş güncelleme dosyaları"), StopUpdate = true,
            Paths = new[] { Win(@"SoftwareDistribution\Download") } },
        new CleanTarget { Id = "wer", Title = L.T("Hata raporları"), Desc = L.T("Windows hata raporlama arşivi"),
            Paths = new[] { PD(@"Microsoft\Windows\WER\ReportArchive"), PD(@"Microsoft\Windows\WER\ReportQueue") } },
        new CleanTarget { Id = "dism", Title = L.T("Windows bileşen deposu (DISM)"), Desc = L.T("Eski güncelleme bileşenlerini kaldırır · uzun sürebilir"), Dism = true, DefaultOn = false },
    };

    static readonly System.IO.EnumerationOptions Opt = new() { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };

    public static long DirSize(string p)
    {
        long s = 0;
        try { if (Directory.Exists(p)) foreach (var f in new DirectoryInfo(p).EnumerateFiles("*", Opt)) { try { s += f.Length; } catch { } } }
        catch { }
        return s;
    }

    public static long Size(CleanTarget t) => t.Dism ? -1 : t.Paths.Distinct(StringComparer.OrdinalIgnoreCase).Sum(DirSize);

    /// <summary>Klasörün içini siler (klasörün kendisini değil). Kullanımdaki dosyalar atlanır.</summary>
    public static (long freed, int files, int skipped) CleanDir(string p)
    {
        long freed = 0; int n = 0, skip = 0;
        if (!Directory.Exists(p)) return (0, 0, 0);
        var root = new DirectoryInfo(p);
        foreach (var f in root.EnumerateFiles("*", Opt))
        {
            try { long len = f.Length; f.Attributes = FileAttributes.Normal; f.Delete(); freed += len; n++; }
            catch { skip++; }
        }
        try
        {
            foreach (var d in root.EnumerateDirectories("*", Opt).OrderByDescending(d => d.FullName.Length))
                try { if (!d.EnumerateFileSystemInfos().Any()) d.Delete(false); } catch { }
        }
        catch { }
        return (freed, n, skip);
    }

    public static long QuickTempSize() => DirSize(System.IO.Path.GetTempPath()) + DirSize(Win("Temp"));
}

// ───────────────────────────── Tarama ve skor ─────────────────────────────
public class ScanItem
{
    public string Id, Title, Detail, Cat;
    public bool Ok, Info;
    public int Weight;
    public double Partial = -1;
}

public class ScanResult
{
    public List<ScanItem> Items = new();
    public int Score;
    public DateTime Time;
    public SysInfo Sys;
    public int Missing => Items.Count(i => !i.Ok && !i.Info);
}

public static class Scanner
{
    public static ScanResult Run(SysInfo sys)
    {
        var r = new ScanResult { Time = DateTime.Now, Sys = sys };
        foreach (var t in Tweaks.All)
        {
            if (!t.Supported || Settings.S.NeverTouch.Contains(t.Id)) continue;
            r.Items.Add(new ScanItem { Id = t.Id, Title = t.Title, Detail = t.Desc, Cat = t.Cat, Ok = t.IsApplied(), Weight = t.Weight });
        }

        var se = Startup.Load();
        int en = se.Count(e => e.Enabled), rec = se.Count(e => e.Enabled && e.Class == StartupClass.Recommended);
        r.Items.Add(new ScanItem
        {
            Id = "startupapps", Cat = L.T("Arka plan"), Title = L.T("Başlangıç uygulamaları"), Weight = 8, Ok = rec == 0 && en <= 8,
            Partial = Math.Max(0, 1 - rec * 0.2),
            Detail = L.F("{0} uygulama açılışta başlıyor, {1} tanesi güvenle kapatılabilir.", en, rec),
        });

        long tmp = Cleaner.QuickTempSize();
        r.Items.Add(new ScanItem
        {
            Id = "temp", Cat = L.T("Sistem"), Title = L.T("Geçici dosyalar"), Weight = 4, Ok = tmp < 1L << 30,
            Detail = L.F("Temp klasörlerinde {0} dosya var.", Fmt.Size(tmp)) + (tmp >= 1L << 30 ? L.T(" Temizlik Merkezi ile boşaltılabilir.") : ""),
        });

        try
        {
            var d = new DriveInfo(System.IO.Path.GetPathRoot(Environment.SystemDirectory));
            double free = d.AvailableFreeSpace / (double)d.TotalSize;
            r.Items.Add(new ScanItem
            {
                Id = "disk", Cat = L.T("Sistem"), Title = L.T("Sistem diskinde boş alan"), Weight = 4, Ok = free >= 0.15,
                Detail = L.F("{0} sürücüsünde %{1} boş ({2}). %15'in altı yavaşlamaya yol açabilir.", d.Name, Math.Round(free * 100), Fmt.Size(d.AvailableFreeSpace)),
            });
        }
        catch { }

        var (vbs, hvci) = Vbs.Runtime();
        r.Items.Add(new ScanItem
        {
            Id = "vbs", Cat = L.T("Sistem"), Title = L.T("Bellek Bütünlüğü (VBS / HVCI)"), Info = true, Ok = !hvci,
            Detail = hvci ? L.T("Açık. Güvenliği artırır ama bazı sistemlerde FPS'i düşürebilir (Modül 07).")
                   : vbs == -1 ? L.T("Durum okunamadı.") : L.T("Kapalı."),
        });
        if (sys != null)
            r.Items.Add(new ScanItem { Id = "ram", Cat = L.T("Sistem"), Title = L.T("Bellek"), Info = true, Ok = true, Detail = sys.RamText });

        double tot = 0, got = 0;
        foreach (var i in r.Items.Where(i => !i.Info))
        {
            tot += i.Weight;
            got += i.Ok ? i.Weight : i.Partial >= 0 ? i.Weight * i.Partial : 0;
        }
        r.Score = tot > 0 ? (int)Math.Round(100 * got / tot) : 0;
        return r;
    }
}
