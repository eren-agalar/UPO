# UPO · Yeni sürüm yayınlama rehberi

UPO açılınca GitHub'daki **en son sürümün** `upo-update.json` dosyasını okur. Dosyanın imzası
`keys/upo-private.pem` ile atılmış değilse güncelleme **asla kurulmaz**.

## ⚠️ Gizli anahtar (upo-private.pem)
- Bu dosyayla imzalanan her şey UPO kullanıcılarına "resmi güncelleme" olarak kurulur. **Kimseyle paylaşma, GitHub'a yükleme.**
- **Yedekle** (ör. şifreli bir USB'ye ve bir parola yöneticisine). Kaybedersen yeni güncelleme imzalayamazsın;
  kullanıcıların yeni sürümü elle indirip kurması gerekir.
- `upo-public.txt` gizli değildir; zaten UPO'nun içine gömülüdür.

## Her yeni sürümde
1. **Sürüm numarasını artır** (ikisi aynı olmalı):
   - `FpsAtolyesi.csproj` → `<Version>1.5.1</Version>`
   - `installer/UPO.nsi` → `!define VERSION "1.5.1"`
2. **Derle:**
   ```
   dotnet publish -c Release -o publish
   makensis installer/UPO.nsi
   ```
   Çıktı: `publish/UPO-Setup-1.5.1.exe`
3. **Sürüm notlarını yaz:** `notlar-tr.txt` ve (isteğe bağlı) `notlar-en.txt`. Örnek:
   ```
   • Donanım testine yeni ekran kartları eklendi
   • Küçük düzeltmeler
   ```
4. **İmzala:**
   ```
   UpoSign sign publish\UPO-Setup-1.5.1.exe keys\upo-private.pem notlar-tr.txt notlar-en.txt
   ```
   Aynı klasörde `upo-update.json` oluşur. Kontrol için:
   ```
   UpoSign verify publish\upo-update.json keys\upo-public.txt publish\UPO-Setup-1.5.1.exe
   ```
   Üç satırda da OK / GEÇERLİ / EŞLEŞİYOR görmelisin.
5. **GitHub'da yayınla:** Depo → **Releases** → **Draft a new release**
   - Tag: **`v1.5.1`** (başında `v` olmalı, sürümle aynı)
   - Dosyalar: **`UPO-Setup-1.5.1.exe`** ve **`upo-update.json`** (isteğe bağlı: `UPO-Portable-1.5.1.zip`)
   - "Set as the latest release" işaretli olsun, "pre-release" **işaretli olmasın** → **Publish release**
6. Eski bir UPO'yu açıp **Ayarlar → Güncelleme → Şimdi kontrol et** ile dene.

## Bilinmesi gerekenler
- Kurulumlu UPO: güncellemeyi indirir, imzayı + SHA-256'yı doğrular, sessizce kurar ve kendini yeniden açar.
- Portable UPO: yalnızca "yeni sürüm var" der ve indirme sayfasını açar.
- Kullanıcı "Bu sürümü atla" derse o sürüm bir daha otomatik gösterilmez (elle kontrolde gösterilir).
- Güncelleme kontrolü Ayarlar'dan kapatılabilir. Gönderilen tek şey dosya isteğidir; kişisel veya donanım bilgisi gönderilmez.
- v1.4 ve öncesinde güncelleme sistemi yoktu; o kullanıcılar v1.5'i bir kez elle kurmalı.
