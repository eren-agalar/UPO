using System.Security.Cryptography;
using FpsAtolyesi.Core;

// UpoSign — UPO güncellemelerini imzalama aracı.
//   UpoSign keygen <klasör>                                     → upo-private.pem + upo-public.txt
//   UpoSign sign <UPO-Setup-x.y.z.exe> <upo-private.pem> [notlar-tr.txt] [notlar-en.txt]  → upo-update.json
//   UpoSign verify <upo-update.json> <upo-public.txt> [setup.exe]
static int Usage()
{
    Console.WriteLine("Kullanım:\n  UpoSign keygen <klasör>\n  UpoSign sign <UPO-Setup-x.y.z.exe> <upo-private.pem> [notlar-tr.txt] [notlar-en.txt]\n  UpoSign verify <upo-update.json> <upo-public.txt> [setup.exe]");
    return 2;
}

if (args.Length == 0) return Usage();
try
{
    switch (args[0])
    {
        case "keygen":
        {
            if (args.Length < 2) return Usage();
            Directory.CreateDirectory(args[1]);
            var priv = Path.Combine(args[1], "upo-private.pem");
            if (File.Exists(priv)) { Console.Error.WriteLine("HATA: " + priv + " zaten var; üzerine yazılmadı."); return 1; }
            using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            File.WriteAllText(priv, ec.ExportPkcs8PrivateKeyPem());
            File.WriteAllText(Path.Combine(args[1], "upo-public.txt"), Convert.ToBase64String(ec.ExportSubjectPublicKeyInfo()));
            Console.WriteLine("Anahtarlar oluşturuldu: " + args[1]);
            Console.WriteLine("upo-private.pem dosyasını GİZLİ tutun ve yedekleyin. Kaybederseniz yeni güncelleme imzalayamazsınız.");
            return 0;
        }
        case "sign":
        {
            if (args.Length < 3) return Usage();
            var setup = args[1];
            var name = Path.GetFileName(setup);
            var m = System.Text.RegularExpressions.Regex.Match(name, @"^UPO-Setup-([0-9]+(\.[0-9]+){1,3})\.exe$");
            if (!m.Success) { Console.Error.WriteLine("HATA: Dosya adı UPO-Setup-<sürüm>.exe olmalı."); return 1; }
            byte[] bytes = File.ReadAllBytes(setup);
            var man = new UpdateManifest
            {
                Version = m.Groups[1].Value,
                Setup = name,
                Size = bytes.LongLength,
                Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
                Published = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                NotesTr = args.Length > 3 ? File.ReadAllText(args[3]).Trim() : "",
                NotesEn = args.Length > 4 ? File.ReadAllText(args[4]).Trim() : "",
            };
            man.SignWith(File.ReadAllText(args[2]));
            var outFile = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(setup)), "upo-update.json");
            File.WriteAllText(outFile, man.ToJson());
            Console.WriteLine($"İmzalandı: v{man.Version} · {man.Size:N0} bayt · SHA-256 {man.Sha256}");
            Console.WriteLine("Oluşturulan dosya: " + outFile);
            Console.WriteLine($"GitHub'da 'v{man.Version}' etiketli bir sürüm açıp şu iki dosyayı yükleyin: {name} ve upo-update.json");
            return 0;
        }
        case "verify":
        {
            if (args.Length < 3) return Usage();
            var man = UpdateManifest.Parse(File.ReadAllText(args[1]));
            bool sig = man.Verify(new[] { File.ReadAllText(args[2]).Trim() });
            Console.WriteLine("Biçim: " + (man.IsWellFormed() ? "OK" : "HATALI"));
            Console.WriteLine("İmza: " + (sig ? "GEÇERLİ" : "GEÇERSİZ"));
            if (args.Length > 3)
            {
                var h = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[3]))).ToLowerInvariant();
                Console.WriteLine("SHA-256: " + (h == man.Sha256.ToLowerInvariant() ? "EŞLEŞİYOR" : "EŞLEŞMİYOR"));
                if (h != man.Sha256.ToLowerInvariant()) return 1;
            }
            return sig && man.IsWellFormed() ? 0 : 1;
        }
        default: return Usage();
    }
}
catch (Exception ex) { Console.Error.WriteLine("HATA: " + ex.Message); return 1; }
