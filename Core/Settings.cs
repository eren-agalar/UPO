using System.Globalization;
using System.Text.Json;

namespace FpsAtolyesi.Core;

/// <summary>Kullanıcı ayarları. C:\ProgramData\UPO\settings.json dosyasında saklanır.</summary>
public class AppSettings
{
    // Genel ve görünüm
    public bool AutoScan = true;
    public string Accent = "yellow";     // yellow | red | blue | green | purple
    public bool ShowLog = true;
    public string Lang = "tr";           // tr | en

    // Güvenlik ve yedek
    public bool RestorePoint = true;
    public string Restart = "ask";       // ask | never

    // Modüller
    public Profile DefaultProfile = Profile.Balanced;
    public List<string> NeverTouch = new();
    public List<string> StartupWhitelist = new();
    public string BenchLength = "normal"; // short | normal | long
    public string ReportDir = "desktop";  // desktop | documents

    // Güncelleme
    public bool AutoUpdate = true;
    public string SkipVersion;
    public DateTime? LastUpdateCheck;
}

public static class Settings
{
    static string FilePath => Path.Combine(Paths.Data, "settings.json");
    public static AppSettings S { get; private set; } = new();
    public static event Action Changed;

    public static void Load()
    {
        try { if (File.Exists(FilePath)) S = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), Paths.Json) ?? new(); }
        catch { S = new(); }
        S.NeverTouch ??= new(); S.StartupWhitelist ??= new();
    }

    public static void Save()
    {
        try { File.WriteAllText(FilePath, JsonSerializer.Serialize(S, Paths.Json)); } catch { }
        Changed?.Invoke();
    }

    public static bool IsWhitelisted(string startupName) => S.StartupWhitelist.Any(n => string.Equals(n, startupName, StringComparison.OrdinalIgnoreCase));

    public static (double single, double multi, double ram) BenchSeconds => S.BenchLength switch
    {
        "short" => (3, 5, 2),
        "long" => (10, 45, 5),
        _ => (6, 10, 3),
    };

    public static string ReportFolder => Environment.GetFolderPath(S.ReportDir == "documents"
        ? Environment.SpecialFolder.MyDocuments : Environment.SpecialFolder.DesktopDirectory);
}

/// <summary>Dil desteği. Metinler Türkçe yazılır; İngilizce seçiliyse sözlükten karşılığı alınır.</summary>
public static partial class L
{
    // Dil, uygulama açılırken bir kez belirlenir (değişiklik yeniden açınca geçerli olur).
    public static bool En { get; private set; }
    public static void Init() => En = Settings.S.Lang == "en";

    public static string T(string tr) => En && Dict.TryGetValue(tr, out var e) ? e : tr;
    public static string F(string tr, params object[] args) => string.Format(Fmt.Tr, T(tr), args);

    public static CultureInfo Culture => En ? CultureInfo.GetCultureInfo("en-US") : CultureInfo.GetCultureInfo("tr-TR");
}
