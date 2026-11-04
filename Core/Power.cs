namespace FpsAtolyesi.Core;

public static class Power
{
    public const string High = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
    public const string Ultimate = "e9a42b02-d5df-448d-aa00-03f14749eb61";

    public const string UsbSub = "2a737441-1930-4402-8d77-b2bebba308a3", UsbSuspend = "48e6b7a6-50f5-4782-a5d4-53bb8f07e226";
    public const string PciSub = "501a4d13-42af-4429-9fd1-a8218c268e20", PciAspm = "ee12f906-d277-404b-b6da-e5fa1a576df5";

    static readonly Regex GuidRx = new(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");

    public static (string Guid, string Name) Active()
    {
        var (code, o) = ProcRunner.Exec("powercfg.exe", "/getactivescheme");
        if (code != 0) return (null, null);
        var m = GuidRx.Match(o);
        if (!m.Success) return (null, null);
        var n = Regex.Match(o, @"\((.*)\)");
        return (m.Value.ToLowerInvariant(), n.Success ? n.Groups[1].Value.Trim() : "");
    }

    public static List<(string Guid, string Name)> List()
    {
        var res = new List<(string, string)>();
        var (code, o) = ProcRunner.Exec("powercfg.exe", "/list");
        if (code != 0) return res;
        foreach (var line in o.Split('\n'))
        {
            var m = GuidRx.Match(line);
            if (!m.Success) continue;
            var n = Regex.Match(line, @"\((.*)\)");
            res.Add((m.Value.ToLowerInvariant(), n.Success ? n.Groups[1].Value.Trim() : ""));
        }
        return res;
    }

    static bool IsUltName(string n) { n = (n ?? "").ToLowerInvariant(); return n.Contains("ultimate") || n.Contains("nihai"); }
    static bool IsPerfName(string n) { n = (n ?? "").ToLowerInvariant(); return IsUltName(n) || n.Contains("high perf") || n.Contains("yüksek perf"); }

    public static bool IsPerformance()
    {
        var a = Active();
        if (a.Guid == null) return false;
        return a.Guid == High || a.Guid == Ultimate || IsPerfName(a.Name) || State.Data.CreatedSchemes.Contains(a.Guid);
    }

    public static string ActiveName() => Active().Name ?? "?";

    public static void ApplyPerformance(string group, bool ultimate)
    {
        var a = Active();
        var list = List();
        string target = null;
        if (ultimate)
        {
            target = list.FirstOrDefault(s => s.Guid == Ultimate || IsUltName(s.Name)).Guid ?? Duplicate(Ultimate);
        }
        target ??= list.FirstOrDefault(s => s.Guid == High || (IsPerfName(s.Name) && !IsUltName(s.Name))).Guid ?? Duplicate(High);
        if (target == null) throw new Exception(L.T("Performans güç planı oluşturulamadı (bu cihaz desteklemiyor olabilir)."));
        if (target == a.Guid) return;
        if (a.Guid != null) Backup.RecordPowerScheme(group, a.Guid);
        var (sc, _) = ProcRunner.Exec("powercfg.exe", "/setactive " + target);
        if (sc != 0 || Active().Guid != target) throw new Exception(L.T("Güç planı etkinleştirilemedi."));
        Log.Dim(L.T("  Güç planı: ") + ActiveName());
    }

    static string Duplicate(string src)
    {
        var (code, o) = ProcRunner.Exec("powercfg.exe", "-duplicatescheme " + src);
        if (code != 0) return null;
        var m = GuidRx.Match(o);
        if (!m.Success) return null;
        var g = m.Value.ToLowerInvariant();
        State.Data.CreatedSchemes.Add(g);
        State.Save();
        return g;
    }

    /// <summary>Etkin plandaki bir ayarın prizde (AC) değerini döndürür; ayar yoksa null.</summary>
    public static int? QueryAc(string sub, string setting)
    {
        var (code, o) = ProcRunner.Exec("powercfg.exe", $"/query SCHEME_CURRENT {sub} {setting}");
        if (code != 0) return null;
        var ms = Regex.Matches(o, @"0x([0-9a-fA-F]{8})");
        if (ms.Count < 2) return null;
        return Convert.ToInt32(ms[ms.Count - 2].Groups[1].Value, 16);
    }

    public static void SetAc(string group, string sub, string setting, int val)
    {
        var a = Active();
        var cur = QueryAc(sub, setting) ?? throw new Exception(L.T("Bu güç ayarı bu cihazda yok."));
        Backup.RecordPowerSetting(group, a.Guid, sub, setting, cur);
        var (code, _) = ProcRunner.Exec("powercfg.exe", $"/setacvalueindex {a.Guid} {sub} {setting} {val}");
        if (code != 0) throw new Exception(L.T("Güç ayarı yazılamadı."));
        ProcRunner.Exec("powercfg.exe", "/setactive " + a.Guid);
    }
}
