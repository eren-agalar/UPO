using System.Net.Http;
using System.Security.Cryptography;

namespace FpsAtolyesi.Core;

/// <summary>
/// GitHub Releases üzerinden güncelleme. Yalnızca imzalı bildirim (upo-update.json) okunur;
/// hiçbir kişisel veya donanım bilgisi gönderilmez.
/// </summary>
public static class Updater
{
    /// <summary>Güncellemelerin yayınlandığı GitHub deposu ("kullanıcı/depo").</summary>
    public const string Repo = "eren-agalar/UPO";

    /// <summary>Güncellemeleri doğrulayan açık anahtar(lar). Anahtar değişirse eskisiyle birlikte yenisi eklenir.</summary>
    static readonly string[] PublicKeys =
    {
        "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAECCkWEk8imw73HVlfDWFxY48WKpA4j4H+OUuLB1HFrQ2hR8tiTWdcwngN0hSQEssmwwOdQfjP1Stnv/qY4W6FAQ==",
    };

    // Yalnızca geliştirme testi için: UPO_UPDATE_BASE=http://127.0.0.1:8000  (imza yine zorunludur)
    static readonly string TestBase = Environment.GetEnvironmentVariable("UPO_UPDATE_BASE");

    public static bool Configured => TestBase != null || !Repo.StartsWith("OWNER/");
    public static string ReleasesPage => $"https://github.com/{Repo}/releases/latest";
    public static string CurrentVersion => typeof(Updater).Assembly.GetName().Version is Version v ? $"{v.Major}.{v.Minor}.{Math.Max(0, v.Build)}" : "0.0.0";

    static string ManifestUrl => TestBase != null ? TestBase + "/upo-update.json" : $"https://github.com/{Repo}/releases/latest/download/upo-update.json";
    static string SetupUrl(UpdateManifest m) => TestBase != null ? $"{TestBase}/{m.Setup}" : $"https://github.com/{Repo}/releases/download/v{m.Version}/{m.Setup}";

    static readonly HttpClient Http = CreateClient();
    static HttpClient CreateClient()
    {
        var c = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        c.DefaultRequestHeaders.UserAgent.ParseAdd("UPO-Updater/" + CurrentVersion);
        return c;
    }

    /// <summary>Yeni sürüm varsa doğrulanmış bildirimi döndürür; yoksa null.</summary>
    public static async Task<UpdateManifest> Check(CancellationToken ct = default)
    {
        if (!Configured) return null;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(15));
        var json = await Http.GetStringAsync(ManifestUrl, cts.Token);
        if (json.Length > 64 * 1024) throw new Exception(L.T("Güncelleme bilgisi geçersiz."));
        var m = UpdateManifest.Parse(json);
        if (m == null || !m.IsWellFormed()) throw new Exception(L.T("Güncelleme bilgisi geçersiz."));
#if !WINE_TEST // Yalnızca Wine testi için ayrı derlenen sürümde atlanır (Wine ECDSA desteklemiyor). Dağıtılan sürümde imza her zaman zorunludur.
        if (!m.Verify(PublicKeys)) throw new Exception(L.T("Güncelleme imzası doğrulanamadı. Güvenlik için güncelleme yok sayıldı."));
#endif
        return UpdateManifest.IsNewer(m.Version, CurrentVersion) ? m : null;
    }

    /// <summary>Kurulum klasöründen mi çalışıyor (kurulum dosyasıyla kurulmuş)?</summary>
    public static bool IsInstalled()
    {
        try
        {
            var loc = Reg.Get("HKLM", @"Software\Microsoft\Windows\CurrentVersion\Uninstall\UPO", "InstallLocation") as string;
            if (string.IsNullOrEmpty(loc)) return false;
            return string.Equals(Path.GetFullPath(loc).TrimEnd('\\'), Path.GetDirectoryName(Environment.ProcessPath)?.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    static string Dir => Path.Combine(Paths.Data, "updates");

    /// <summary>Kurulum dosyasını indirir ve boyut + SHA-256 doğrular.</summary>
    public static async Task<string> Download(UpdateManifest m, Action<double> progress, CancellationToken ct)
    {
        Directory.CreateDirectory(Dir);
        foreach (var old in Directory.GetFiles(Dir)) try { File.Delete(old); } catch { }
        var file = Path.Combine(Dir, m.Setup);
        using (var resp = await Http.GetAsync(SetupUrl(m), HttpCompletionOption.ResponseHeadersRead, ct))
        {
            resp.EnsureSuccessStatusCode();
            await using var src = await resp.Content.ReadAsStreamAsync(ct);
            await using var dst = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None);
            var buf = new byte[81920];
            long got = 0; int n;
            while ((n = await src.ReadAsync(buf, ct)) > 0)
            {
                got += n;
                if (got > m.Size) throw new Exception(L.T("İndirilen dosya beklenenden büyük."));
                await dst.WriteAsync(buf.AsMemory(0, n), ct);
                progress?.Invoke(got * 100.0 / m.Size);
            }
            if (got != m.Size) throw new Exception(L.T("İndirme eksik kaldı."));
        }
        if (!HashMatches(file, m.Sha256))
        {
            try { File.Delete(file); } catch { }
            throw new Exception(L.T("İndirilen dosya doğrulanamadı (SHA-256 eşleşmedi)."));
        }
        return file;
    }

    static bool HashMatches(string file, string sha)
    {
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA256.HashData(fs)).Equals(sha, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Dosyayı tekrar doğrular, sessiz kurulumu başlatır ve UPO'yu kapatır (kurulum bitince UPO yeniden açılır).</summary>
    public static void InstallAndExit(string file, string sha)
    {
        // Doğrulama ile çalıştırma arasında dosyanın değiştirilmesini önlemek için yazmaya kapalı tut.
        using var hold = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (!Convert.ToHexString(SHA256.HashData(hold)).Equals(sha, StringComparison.OrdinalIgnoreCase))
            throw new Exception(L.T("İndirilen dosya doğrulanamadı (SHA-256 eşleşmedi)."));
        Process.Start(new ProcessStartInfo(file, "/S /RELAUNCH") { UseShellExecute = false });
        Program.ExitNow();
    }
}
