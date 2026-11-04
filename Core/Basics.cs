using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace FpsAtolyesi.Core;

public enum LogKind { Info, Ok, Warn, Error, Dim }

/// <summary>Uygulama içi canlı kayıt. Hiçbir yere gönderilmez; yalnızca ekranda gösterilir.</summary>
public static class Log
{
    public static event Action<string, LogKind> Line;
    public static void W(string s, LogKind k = LogKind.Info) => Line?.Invoke(s, k);
    public static void Ok(string s) => W(s, LogKind.Ok);
    public static void Warn(string s) => W(s, LogKind.Warn);
    public static void Err(string s) => W(s, LogKind.Error);
    public static void Dim(string s) => W(s, LogKind.Dim);
}

public static class Native
{
    [DllImport("dwmapi.dll")] public static extern int DwmSetWindowAttribute(IntPtr h, int attr, ref int val, int size);
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)] public static extern int SetWindowTheme(IntPtr h, string app, string id);
    [DllImport("user32.dll", SetLastError = true)] public static extern bool SystemParametersInfo(uint action, uint param, int[] pv, uint winIni);
    [DllImport("kernel32.dll")] public static extern uint GetOEMCP();

    public const uint SPI_SETMOUSE = 0x0004, SPIF_UPDATEINIFILE = 0x01, SPIF_SENDCHANGE = 0x02;

    static Encoding _oem;
    public static Encoding Oem
    {
        get
        {
            if (_oem != null) return _oem;
            try { _oem = Encoding.GetEncoding((int)GetOEMCP()); } catch { _oem = Encoding.UTF8; }
            return _oem;
        }
    }

    public static void DarkTitleBar(IntPtr h)
    {
        try { int on = 1; if (DwmSetWindowAttribute(h, 20, ref on, 4) != 0) DwmSetWindowAttribute(h, 19, ref on, 4); } catch { }
    }

    public static void DarkScroll(IntPtr h)
    {
        try { SetWindowTheme(h, "DarkMode_Explorer", null); } catch { }
    }

    /// <summary>Fare ayarlarını kayıt defterinden okuyup oturumu kapatmadan uygular.</summary>
    public static void RefreshMouse()
    {
        try
        {
            int R(string n, int d) => int.TryParse(Reg.Get("HKCU", @"Control Panel\Mouse", n) as string, out var v) ? v : d;
            var arr = new[] { R("MouseThreshold1", 6), R("MouseThreshold2", 10), R("MouseSpeed", 1) };
            SystemParametersInfo(SPI_SETMOUSE, 0, arr, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }
        catch { }
    }
}

public static class Fmt
{
    public static CultureInfo Tr => L.Culture;
    public static string Size(long b)
    {
        if (b < 0) return "—";
        if (b >= 1L << 30) return (b / (double)(1L << 30)).ToString("0.0", Tr) + " GB";
        if (b >= 1L << 20) return (b / (double)(1L << 20)).ToString("0", Tr) + " MB";
        if (b >= 1L << 10) return (b / 1024.0).ToString("0", Tr) + " KB";
        return b + " B";
    }
    public static string Upper(string s) => s.ToUpper(Tr);
    /// <summary>Yüzde: Türkçede "%50", İngilizcede "50%".</summary>
    public static string Pct(double v) => L.En ? $"{v}%" : $"%{v}";
}

public static class Paths
{
    public static string Data
    {
        get
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var d = Path.Combine(root, "UPO");
            if (!Directory.Exists(d))
            {
                // Eski adla (FPS Atölyesi) alınmış yedekleri kaybetmemek için klasörü taşı.
                var old = Path.Combine(root, "FPSAtolyesi");
                try { if (Directory.Exists(old)) Directory.Move(old, d); } catch { }
                if (Directory.Exists(old) && !Directory.Exists(d))
                {
                    Directory.CreateDirectory(d);
                    foreach (var f in Directory.GetFiles(old)) try { File.Copy(f, Path.Combine(d, Path.GetFileName(f)), false); } catch { }
                }
            }
            Directory.CreateDirectory(d);
            return d;
        }
    }
    public static readonly JsonSerializerOptions Json = new() { IncludeFields = true, WriteIndented = true };
}

/// <summary>Kalıcı uygulama durumu (önce/sonra skorları vb.). Yalnızca bu bilgisayarda saklanır.</summary>
public class AppData
{
    public int? BeforeScore;
    public DateTime? BeforeTime;
    public Dictionary<string, bool> BeforeItems = new();
    public int? LastScore;
    public DateTime? LastTime;
    public Dictionary<string, bool> LastItems = new();
    public List<string> CreatedSchemes = new();
    public DateTime? LastRestorePoint;
    public BenchResult LastBench;
}

public static class State
{
    static string FilePath => Path.Combine(Paths.Data, "state.json");
    public static AppData Data { get; private set; } = new();

    public static void Load()
    {
        try { if (File.Exists(FilePath)) Data = JsonSerializer.Deserialize<AppData>(File.ReadAllText(FilePath), Paths.Json) ?? new(); }
        catch { Data = new(); }
    }
    public static void Save()
    {
        try { File.WriteAllText(FilePath, JsonSerializer.Serialize(Data, Paths.Json)); } catch { }
    }
}

public static class ProcRunner
{
    /// <summary>Kısa komutu çalıştırır ve çıktısını döndürür (hata olursa boş metin).</summary>
    public static string Capture(string exe, string args, int timeoutMs = 30000, Encoding enc = null) => Exec(exe, args, timeoutMs, enc).Output;

    /// <summary>Komutu çalıştırır; çıkış kodu ve çıktı döner. Başlatılamazsa kod -1, çıktı boş olur.</summary>
    public static (int Code, string Output) Exec(string exe, string args, int timeoutMs = 30000, Encoding enc = null)
    {
        try
        {
            var psi = new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
                StandardOutputEncoding = enc ?? Native.Oem, StandardErrorEncoding = enc ?? Native.Oem,
            };
            using var p = Process.Start(psi);
            var o = p.StandardOutput.ReadToEndAsync();
            var e = p.StandardError.ReadToEndAsync();
            if (!p.WaitForExit(timeoutMs)) { try { p.Kill(true); } catch { } return (-2, ""); }
            Task.WaitAll(new Task[] { o, e }, 2000);
            return (p.ExitCode, (o.IsCompleted ? o.Result : "") + (e.IsCompleted ? e.Result : ""));
        }
        catch { return (-1, ""); }
    }

    /// <summary>Uzun süren komutu satır satır okuyarak çalıştırır (SFC/DISM ilerlemesi için).</summary>
    public static async Task<int> RunAsync(string exe, string args, Action<string> onLine, Encoding enc = null, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo(exe, args)
        {
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
            StandardOutputEncoding = enc ?? Native.Oem, StandardErrorEncoding = enc ?? Native.Oem,
        };
        using var p = Process.Start(psi);
        using var reg = ct.Register(() => { try { p.Kill(true); } catch { } });
        await Task.WhenAll(Pump(p.StandardOutput, onLine), Pump(p.StandardError, onLine));
        await p.WaitForExitAsync();
        ct.ThrowIfCancellationRequested();
        return p.ExitCode;
    }

    static async Task Pump(StreamReader r, Action<string> on)
    {
        var buf = new char[2048];
        var sb = new StringBuilder();
        int n;
        while ((n = await r.ReadAsync(buf, 0, buf.Length)) > 0)
        {
            for (int i = 0; i < n; i++)
            {
                char c = buf[i];
                if (c == '\r' || c == '\n' || c == '\b')
                {
                    if (sb.Length > 0) { var s = sb.ToString().Trim(); sb.Clear(); if (s.Length > 0) on(s); }
                }
                else if (c != '\0') sb.Append(c);
            }
        }
        if (sb.Length > 0) { var s = sb.ToString().Trim(); if (s.Length > 0) on(s); }
    }
}
