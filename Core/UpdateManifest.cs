using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FpsAtolyesi.Core;

/// <summary>
/// Güncelleme bildirimi (GitHub sürümündeki upo-update.json). Hem UPO hem de UpoSign aracı bu dosyayı kullanır.
/// İmza, sürüm + dosya adı + SHA-256 + boyut + notlar üzerinde ECDSA P-256 (SHA-256) ile atılır.
/// </summary>
public class UpdateManifest
{
    public string Version, Setup, Sha256, Published, NotesTr, NotesEn, Sig;
    public long Size;

    static readonly JsonSerializerOptions Opt = new() { IncludeFields = true, WriteIndented = true };

    public string Canonical() => string.Join("\n", "UPO-UPDATE-V1", Version ?? "", Setup ?? "", (Sha256 ?? "").ToLowerInvariant(),
        Size.ToString(), Published ?? "", NotesTr ?? "", NotesEn ?? "");

    public void SignWith(string privatePem)
    {
        using var ec = ECDsa.Create();
        ec.ImportFromPem(privatePem);
        Sig = Convert.ToBase64String(ec.SignData(Encoding.UTF8.GetBytes(Canonical()), HashAlgorithmName.SHA256));
    }

    public bool Verify(string[] publicKeysB64)
    {
        byte[] sig;
        try { sig = Convert.FromBase64String(Sig ?? ""); } catch { return false; }
        var data = Encoding.UTF8.GetBytes(Canonical());
        foreach (var k in publicKeysB64)
        {
            try
            {
                using var ec = ECDsa.Create();
                ec.ImportSubjectPublicKeyInfo(Convert.FromBase64String(k), out _);
                if (ec.VerifyData(data, sig, HashAlgorithmName.SHA256)) return true;
            }
            catch { }
        }
        return false;
    }

    /// <summary>Dosya adı yalnızca "UPO-Setup-1.2.3.exe" biçiminde olabilir (klasör geçişini engeller).</summary>
    public bool IsWellFormed() =>
        System.Version.TryParse(Version, out _) && Setup != null && Regex.IsMatch(Setup, @"^UPO-Setup-[0-9]+(\.[0-9]+){1,3}\.exe$")
        && Sha256 != null && Regex.IsMatch(Sha256, "^[0-9a-fA-F]{64}$") && Size > 0 && Size < 1L << 30;

    public static bool IsNewer(string candidate, string current) =>
        System.Version.TryParse(candidate, out var a) && System.Version.TryParse(current, out var b) && a > b;

    public string ToJson() => JsonSerializer.Serialize(this, Opt);
    public static UpdateManifest Parse(string json) => JsonSerializer.Deserialize<UpdateManifest>(json, Opt);
}
