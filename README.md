<div align="center">

# UPO · Ultimate Performance Optimizer

**A safe, transparent and fully reversible game performance optimizer for Windows 10 / 11**
**Windows 10 / 11 için güvenli, şeffaf ve tamamen geri alınabilir oyun performansı optimizasyon aracı**

[![Latest release](https://img.shields.io/github/v/release/eren-agalar/UPO?label=latest%20release&color=f7c523)](https://github.com/eren-agalar/UPO/releases/latest)
![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011%20(64--bit)-28e2d2)
![Language](https://img.shields.io/badge/language-English%20%7C%20T%C3%BCrk%C3%A7e-b078ff)

Designer / Tasarım: **Eren Ağalar**

**[⬇️ Download / İndir](https://github.com/eren-agalar/UPO/releases/latest)** · [English](#-english) · [Türkçe](#-türkçe)

</div>

---

# 🇬🇧 English

## What is UPO?
UPO scans the Windows settings and background load that affect gaming performance, gives your system a **score out of 100**, and applies the missing optimizations with **one click**. It also cleans caches, repairs Windows, tests your hardware and tells you whether an upgrade is worth it.

What makes UPO different:
- **Everything is reversible.** Before UPO changes a setting, it saves the original value. **08 · Undo Everything** returns your PC to its original state with one click.
- **No magic, no hidden tweaks.** Every setting UPO touches is a real, documented Windows setting, listed below and shown inside the app before it's applied.
- **Honest.** The score reflects how well your settings are configured for gaming. UPO never promises "+50% FPS"; the actual gain depends on your hardware, game and drivers.
- **Private.** UPO collects and sends **no** personal or hardware data.

## 📥 Download & install
Go to **[Releases](https://github.com/eren-agalar/UPO/releases/latest)** and download:

| File | Description |
|---|---|
| **UPO-Setup-x.y.z.exe** | ✅ **Recommended.** Setup wizard: Start menu and desktop shortcuts, automatic updates, uninstall from Windows Settings |
| **UPO-Portable-x.y.z.zip** | Single file, no installation. Good for USB sticks. No automatic installation of updates |

1. Run **UPO-Setup-x.y.z.exe** and follow the wizard (English / Türkçe).
2. If Windows shows **"Windows protected your PC"**, click **More info → Run anyway**. This appears because the file isn't signed with a paid code-signing certificate yet; it doesn't mean the file is harmful. You can verify the file with the SHA-256 value on the release page (`Get-FileHash .\UPO-Setup-x.y.z.exe` in PowerShell).
3. UPO asks for **administrator rights**, because changing Windows settings requires them.

**Requirements:** Windows 10 or 11 (64-bit), administrator account, ~65 MB disk space. Nothing else to install; the .NET runtime is built in.

## 🧩 Modules

### 01 · System Scan
Reads (never changes) your settings and background load and gives a score out of 100. Each finding is listed as **OK** or **MISSING** with an explanation. Can export an **HTML report** to your Desktop or Documents.

### 02 · One-Click Optimization
Applies the missing settings with a profile:
- **Balanced:** everyday use + gaming. The safest settings, no noticeable effect on security or battery.
- **Competitive:** lowest latency for games like CS2, VALORANT and LoL. Uses more power; recommended for desktops.
- **Custom:** pick settings one by one.

Every change is backed up first. Settings that need a restart are marked.

<details>
<summary><b>All 20 settings UPO can apply (click to expand)</b></summary>

| Setting | Balanced | Competitive |
|---|:-:|:-:|
| High Performance (Balanced) / Ultimate Performance (Competitive) power plan | ✓ | ✓ |
| USB selective suspend off (when plugged in) | | ✓ |
| PCIe link power saving off (when plugged in) | | ✓ |
| Power Throttling off | | ✓ |
| Game Mode on | ✓ | ✓ |
| Background game recording (Game DVR) off | ✓ | ✓ |
| Hardware-accelerated GPU scheduling (HAGS) on *(restart required)* | | ✓ |
| Windowed game optimizations + VRR (Windows 11) | ✓ | ✓ |
| High priority for game tasks (MMCSS) | ✓ | ✓ |
| CPU priority for the foreground window | | ✓ |
| Network throttling off, system responsiveness to games | | ✓ |
| Mouse acceleration ("Enhance pointer precision") off | | ✓ |
| Sticky Keys shortcut off | ✓ | ✓ |
| Windows Update active hours 10:00–02:00 (no restarts while gaming) | ✓ | ✓ |
| Delivery Optimization (peer-to-peer updates) off | ✓ | ✓ |
| Windows suggestions and ads off | ✓ | ✓ |
| Transparency and animations off | | ✓ |
| Background apps off | | ✓ |
| Startup delay off | ✓ | ✓ |
| Windows telemetry service (DiagTrack) off | ✓ | ✓ |

</details>

### 03 · Startup Apps
Lists apps that start with Windows and lets you turn them off **the same way Task Manager does** (nothing is uninstalled or deleted). Apps that are usually safe to disable (Discord, Steam, Spotify, launchers…) are marked **CAN DISABLE**; audio/graphics drivers, antivirus and anti-cheat (Vanguard, FACEIT, EasyAntiCheat, BattlEye…) are marked **DON'T TOUCH** and ask for extra confirmation.

### 04 · Cleanup Center
Shows the size first, then cleans: temporary files, shader caches (DirectX / NVIDIA / AMD / Intel), Windows Update cache, error reports and (optionally) the Windows component store via DISM. Files in use are skipped; personal files are never touched.

### 05 · Windows Repair
Runs **DISM /RestoreHealth** and then **SFC /scannow** with live progress and elapsed time. Useful for crashes, freezes and programs that won't start. Can take 15–40 minutes; DISM waiting around 62% for a while is normal.

### 06 · Before / After
Compares your first scan score with your current score and lists which settings were fixed.

### 07 · Memory Integrity (VBS)
Shows the state of Virtualization-Based Security and Memory Integrity (HVCI), which can cost FPS on some systems. Turning it off requires typing **YES**, is clearly explained (it reduces security; some anti-cheat systems may require it) and can be undone.

### 08 · Undo Everything
Lists every backed-up change grouped by setting. Undo selected groups or **everything** with one click. Before the first change, UPO also tries to create a **Windows System Restore point** as an extra safety net.

### 09 · Hardware Test
Measures your **CPU** (single-core, multi-core and slowdown under sustained load), **RAM** (copy speed, capacity, single/dual channel, XMP/EXPO hint) and **system drive** (uncached read/write and 4K random read). Together with your **graphics card** (model, VRAM, driver age) each part gets a score out of 100 against today's games, and the overall hardware score is weighted **GPU 45% · CPU 30% · RAM 15% · Drive 10%**.

UPO then explains what to do: CPU bottleneck, single-channel RAM, XMP off, old GPU driver, HDD system drive, overheating, and **which upgrade fits your motherboard** (e.g. Ryzen 7 5700X3D on AM4). The CPU/GPU database is built into the app (no internet needed); models not in it are estimated and marked **ESTIMATED**.

### 10 · Settings
- Automatic scan on startup · accent color (yellow, red, blue, green, purple) · show/hide live log · **English / Türkçe**
- Check for updates automatically · Check now
- Create a restore point before changes · restart prompt (ask / never ask)
- **Export / import backups** (`.upobackup`), e.g. to keep them after reinstalling Windows
- Open the data folder · reset app data (blocked while there are changes to undo)
- One-Click default profile · hardware test length · HTML report location
- **Never-touch list:** settings UPO will never apply · **Startup whitelist:** apps never marked as "can disable"

## 🔄 Automatic updates
When UPO starts, it checks this repository's latest release. If there's a new version, it shows the release notes and offers **Update now / Later / Skip this version**. On "Update now", UPO downloads the installer, **verifies its digital signature (ECDSA P-256) and SHA-256**, installs it silently and reopens itself. Your settings, backups and score history are kept.

UPO **refuses** any update whose signature doesn't match the key built into the app, so even a compromised download link can't push a fake update. Automatic checks can be turned off in Settings. The portable version only notifies you and opens the download page.

## 🛡️ Privacy & safety
- **No data collection.** No accounts, no analytics, no telemetry. Nothing about you or your PC is sent anywhere.
- **Internet is used only for update checks** (GitHub). This can be turned off. (Windows' own DISM repair may download files from Windows Update.)
- **Every settings change is backed up** to `C:\ProgramData\UPO\backup.json` and can be undone in Module 08.
- **File operations are the exception:** cleanup and SFC/DISM repair delete/repair files, so they aren't in the undo list. UPO only deletes temporary and cache files.
- **Uninstalling safely:** if there are changes you haven't undone, the uninstaller warns you and offers to open UPO on the **Undo Everything** page first. Backups are never deleted silently.

## ❓ FAQ
**Will UPO increase my FPS?** It removes common Windows settings that cost performance or add latency. How much you gain depends on your system; on an already well-configured PC the difference can be small. UPO never claims a fixed percentage.

**Is it safe for anti-cheat games?** UPO doesn't touch game files or anti-cheat software, and it marks anti-cheat startup entries as "DON'T TOUCH". The only related setting is Memory Integrity (Module 07): some anti-cheat systems may need it on, which the app explains.

**How do I undo everything?** Open **08 · Undo Everything → UNDO EVERYTHING**. Some settings fully revert after a restart.

**Why does Windows show a SmartScreen warning?** The installer isn't signed with a paid code-signing certificate yet. Click **More info → Run anyway**, or verify the SHA-256 value from the release page first.

**Where does UPO store its data?** In `C:\ProgramData\UPO` (backups, settings, score history). Nothing is stored online.

## 🐞 Feedback
Found a bug or have an idea? Open an **[Issue](https://github.com/eren-agalar/UPO/issues)**. Please include your Windows version and, if possible, a screenshot of the live log.

UPO is built with C# / .NET 8. The source code is maintained privately; this repository hosts the official releases.

---

# 🇹🇷 Türkçe

## UPO nedir?
UPO, oyun performansını etkileyen Windows ayarlarını ve arka plan yükünü tarar, sistemine **100 üzerinden puan** verir ve eksik optimizasyonları **tek tıkla** uygular. Bunun yanında önbellekleri temizler, Windows'u onarır, donanımını test eder ve yükseltme gerekip gerekmediğini söyler.

UPO'yu farklı kılanlar:
- **Her şey geri alınabilir.** UPO bir ayarı değiştirmeden önce orijinal değerini kaydeder. **08 · Tümünü Geri Al** bilgisayarını tek tıkla ilk hâline döndürür.
- **Sihir yok, gizli ayar yok.** UPO'nun dokunduğu her ayar gerçek ve belgelenmiş bir Windows ayarıdır. Hepsi aşağıda listelenir ve uygulanmadan önce uygulama içinde gösterilir.
- **Dürüst.** Skor, ayarlarının oyun için ne kadar uygun olduğunu gösterir. UPO asla "%50 FPS artışı" vaat etmez; gerçek kazanç donanımına, oyununa ve sürücülerine bağlıdır.
- **Gizlilik.** UPO hiçbir kişisel veya donanım verisi **toplamaz ve göndermez**.

## 📥 İndirme ve kurulum
**[Releases](https://github.com/eren-agalar/UPO/releases/latest)** sayfasına git ve indir:

| Dosya | Açıklama |
|---|---|
| **UPO-Setup-x.y.z.exe** | ✅ **Önerilen.** Kurulum sihirbazı: Başlat menüsü ve masaüstü kısayolu, otomatik güncelleme, Windows Ayarları'ndan kaldırma |
| **UPO-Portable-x.y.z.zip** | Kurulumsuz tek dosya. USB bellekte taşımak için. Güncellemeler otomatik kurulmaz |

1. **UPO-Setup-x.y.z.exe**'yi çalıştır ve sihirbazı takip et (Türkçe / English).
2. Windows **"Bilgisayarınız korundu"** derse **Ek bilgi → Yine de çalıştır**'a tıkla. Bu uyarı, dosya henüz ücretli bir kod imzalama sertifikasıyla imzalanmadığı için çıkar; dosyanın zararlı olduğu anlamına gelmez. Dosyayı sürüm sayfasındaki SHA-256 değeriyle doğrulayabilirsin (PowerShell'de `Get-FileHash .\UPO-Setup-x.y.z.exe`).
3. UPO **yönetici yetkisi** ister, çünkü Windows ayarlarını değiştirmek bunu gerektirir.

**Gereksinimler:** Windows 10 veya 11 (64-bit), yönetici hesabı, ~65 MB disk alanı. Başka bir şey kurmana gerek yok; .NET çalışma zamanı içinde gelir.

## 🧩 Modüller

### 01 · Sistem Taraması
Ayarlarını ve arka plan yükünü okur (asla değiştirmez) ve 100 üzerinden puan verir. Her bulgu açıklamasıyla birlikte **UYGUN** veya **EKSİK** olarak listelenir. Masaüstüne veya Belgeler'e **HTML rapor** çıkarabilir.

### 02 · Tek Tık Optimizasyon
Eksik ayarları bir profille uygular:
- **Dengeli:** günlük kullanım + oyun. Güvenliği veya pil ömrünü belirgin şekilde etkilemeyen en güvenli ayarlar.
- **Rekabetçi:** CS2, VALORANT, LoL gibi oyunlar için en düşük gecikme. Daha fazla güç tüketir; masaüstü bilgisayarlar için önerilir.
- **Özel:** ayarları tek tek kendin seç.

Her değişiklik önce yedeklenir. Yeniden başlatma gerektiren ayarlar işaretlidir.

<details>
<summary><b>UPO'nun uygulayabildiği 20 ayarın tamamı (açmak için tıkla)</b></summary>

| Ayar | Dengeli | Rekabetçi |
|---|:-:|:-:|
| Yüksek Performans (Dengeli) / Nihai Performans (Rekabetçi) güç planı | ✓ | ✓ |
| USB seçici askıya alma kapalı (prizdeyken) | | ✓ |
| PCIe güç tasarrufu kapalı (prizdeyken) | | ✓ |
| Güç kısıtlaması (Power Throttling) kapalı | | ✓ |
| Oyun Modu açık | ✓ | ✓ |
| Arka plan oyun kaydı (Game DVR) kapalı | ✓ | ✓ |
| Donanım hızlandırmalı GPU zamanlaması (HAGS) açık *(yeniden başlatma gerekir)* | | ✓ |
| Pencereli oyun optimizasyonu + VRR (Windows 11) | ✓ | ✓ |
| Oyun görevlerine yüksek öncelik (MMCSS) | ✓ | ✓ |
| Öndeki pencereye işlemci önceliği | | ✓ |
| Ağ kısıtlaması kapalı, sistem tepkiselliği oyuna | | ✓ |
| Fare ivmesi ("İşaretçi hassasiyetini artır") kapalı | | ✓ |
| Yapışkan Tuşlar kısayolu kapalı | ✓ | ✓ |
| Windows Update etkin saatleri 10:00–02:00 (oyun sırasında yeniden başlatmaz) | ✓ | ✓ |
| Teslim Optimizasyonu (eşler arası güncelleme) kapalı | ✓ | ✓ |
| Windows önerileri ve reklamları kapalı | ✓ | ✓ |
| Saydamlık ve animasyonlar kapalı | | ✓ |
| Arka plan uygulamaları kapalı | | ✓ |
| Açılış gecikmesi kapalı | ✓ | ✓ |
| Windows telemetri hizmeti (DiagTrack) kapalı | ✓ | ✓ |

</details>

### 03 · Başlangıç Uygulamaları
Windows ile birlikte başlayan uygulamaları listeler ve onları **Görev Yöneticisi'nin yaptığı şekilde** kapatmanı sağlar (hiçbir şey kaldırılmaz veya silinmez). Genelde güvenle kapatılabilenler (Discord, Steam, Spotify, başlatıcılar…) **KAPATILABİLİR**; ses/ekran kartı sürücüleri, antivirüs ve anti-cheat (Vanguard, FACEIT, EasyAntiCheat, BattlEye…) **DOKUNMAYIN** olarak işaretlenir ve ek onay ister.

### 04 · Temizlik Merkezi
Önce boyutu gösterir, sonra temizler: geçici dosyalar, shader önbellekleri (DirectX / NVIDIA / AMD / Intel), Windows Update önbelleği, hata raporları ve (isteğe bağlı) DISM ile Windows bileşen deposu. Kullanımdaki dosyalar atlanır; kişisel dosyalara asla dokunulmaz.

### 05 · Windows Onarımı
Önce **DISM /RestoreHealth**, ardından **SFC /scannow** çalıştırır; ilerlemeyi ve geçen süreyi canlı gösterir. Oyun çökmeleri, donmalar ve açılmayan programlar için faydalıdır. 15–40 dakika sürebilir; DISM'in %62 civarında bir süre beklemesi normaldir.

### 06 · Önce / Sonra
İlk taramadaki skorunu şimdiki skorla karşılaştırır ve hangi ayarların düzeldiğini listeler.

### 07 · Bellek Bütünlüğü (VBS)
Bazı sistemlerde FPS kaybına yol açabilen Sanallaştırma Tabanlı Güvenlik ve Bellek Bütünlüğü (HVCI) durumunu gösterir. Kapatmak için **EVET** yazmak gerekir; ne anlama geldiği açıkça anlatılır (güvenliği azaltır, bazı anti-cheat sistemleri açık olmasını isteyebilir) ve geri alınabilir.

### 08 · Tümünü Geri Al
Yedeklenen her değişikliği ayar grubuna göre listeler. Seçili grupları veya **hepsini** tek tıkla geri al. UPO, ilk değişiklikten önce ekstra güvenlik için ayrıca bir **Windows Sistem Geri Yükleme noktası** oluşturmayı dener.

### 09 · Donanım Testi
**İşlemciyi** (tek çekirdek, çok çekirdek ve uzun yük altında yavaşlama), **RAM'i** (kopyalama hızı, kapasite, tek/çift kanal, XMP/EXPO ipucu) ve **sistem diskini** (önbelleksiz okuma/yazma ve 4K rastgele okuma) ölçer. **Ekran kartıyla** (model, VRAM, sürücü yaşı) birlikte her parça bugünün oyunlarına göre 100 üzerinden puanlanır; genel donanım skoru **Ekran kartı %45 · İşlemci %30 · RAM %15 · Disk %10** ağırlıklıdır.

Ardından ne yapman gerektiğini söyler: işlemci darboğazı, tek kanal RAM, kapalı XMP, eski ekran kartı sürücüsü, HDD sistem diski, ısınma ve **anakartına uyan yükseltme** (ör. AM4 için Ryzen 7 5700X3D). İşlemci/ekran kartı veritabanı uygulamanın içindedir (internet gerekmez); listede olmayan modeller tahmin edilir ve **TAHMİNİ** olarak işaretlenir.

### 10 · Ayarlar
- Açılışta otomatik tarama · vurgu rengi (sarı, kırmızı, mavi, yeşil, mor) · canlı kaydı göster/gizle · **Türkçe / English**
- Güncellemeleri otomatik kontrol et · Şimdi kontrol et
- Değişiklikten önce geri yükleme noktası oluştur · yeniden başlatma (sor / hiç sorma)
- **Yedeği dışa / içe aktar** (`.upobackup`), örneğin Windows'u yeniden kurduktan sonra kullanmak için
- Veri klasörünü aç · uygulama verilerini sıfırla (geri alınmamış değişiklik varken engellenir)
- Tek Tık varsayılan profili · donanım testi süresi · HTML rapor kayıt yeri
- **Asla dokunma listesi:** UPO'nun hiç uygulamayacağı ayarlar · **Başlangıç beyaz listesi:** asla "kapatılabilir" diye işaretlenmeyecek uygulamalar

## 🔄 Otomatik güncelleme
UPO açılınca bu deponun en son sürümünü kontrol eder. Yeni bir sürüm varsa sürüm notlarını gösterir ve **Şimdi güncelle / Daha sonra / Bu sürümü atla** seçeneklerini sunar. "Şimdi güncelle" dersen kurulum dosyasını indirir, **dijital imzasını (ECDSA P-256) ve SHA-256 değerini doğrular**, sessizce kurar ve kendini yeniden açar. Ayarların, yedeklerin ve skor geçmişin korunur.

UPO, imzası uygulamanın içindeki anahtarla eşleşmeyen hiçbir güncellemeyi **kurmaz**. Bu yüzden indirme bağlantısı ele geçirilse bile sahte bir güncelleme dağıtılamaz. Otomatik kontrol Ayarlar'dan kapatılabilir. Portable sürüm yalnızca haber verir ve indirme sayfasını açar.

## 🛡️ Gizlilik ve güvenlik
- **Veri toplanmaz.** Hesap yok, analiz yok, telemetri yok. Senin veya bilgisayarın hakkında hiçbir şey bir yere gönderilmez.
- **İnternet yalnızca güncelleme kontrolü için kullanılır** (GitHub). Kapatılabilir. (Windows'un kendi DISM onarımı Windows Update'ten dosya indirebilir.)
- **Her ayar değişikliği yedeklenir:** `C:\ProgramData\UPO\backup.json` dosyasına kaydedilir ve Modül 08'den geri alınabilir.
- **Dosya işlemleri istisnadır:** Temizlik ve SFC/DISM onarımı dosya siler/onarır, bu yüzden geri alma listesinde yer almaz. UPO yalnızca geçici ve önbellek dosyalarını siler.
- **Güvenli kaldırma:** Geri alınmamış değişikliklerin varsa kaldırıcı uyarır ve önce UPO'yu **Tümünü Geri Al** sayfasında açmayı önerir. Yedekler asla sessizce silinmez.

## ❓ Sık sorulan sorular
**UPO FPS'imi artırır mı?** Performans kaybettiren veya gecikme ekleyen yaygın Windows ayarlarını düzeltir. Ne kadar kazanacağın sistemine bağlıdır; zaten iyi ayarlanmış bir bilgisayarda fark küçük olabilir. UPO asla sabit bir yüzde iddia etmez.

**Anti-cheat kullanan oyunlar için güvenli mi?** UPO oyun dosyalarına veya anti-cheat yazılımlarına dokunmaz ve anti-cheat başlangıç girişlerini "DOKUNMAYIN" olarak işaretler. İlgili tek ayar Bellek Bütünlüğü'dür (Modül 07): bazı anti-cheat sistemleri açık olmasını isteyebilir, uygulama bunu açıklar.

**Her şeyi nasıl geri alırım?** **08 · Tümünü Geri Al → TÜMÜNÜ GERİ AL**. Bazı ayarlar yeniden başlattıktan sonra tamamen eski hâline döner.

**Windows neden SmartScreen uyarısı veriyor?** Kurulum dosyası henüz ücretli bir kod imzalama sertifikasıyla imzalanmadı. **Ek bilgi → Yine de çalıştır**'a tıkla ya da önce sürüm sayfasındaki SHA-256 değerini doğrula.

**UPO verilerini nerede saklıyor?** `C:\ProgramData\UPO` klasöründe (yedekler, ayarlar, skor geçmişi). İnternette hiçbir şey saklanmaz.

## 🐞 Geri bildirim
Bir hata mı buldun ya da bir fikrin mi var? Bir **[Issue](https://github.com/eren-agalar/UPO/issues)** aç. Lütfen Windows sürümünü ve mümkünse canlı kaydın ekran görüntüsünü ekle.

UPO, C# / .NET 8 ile geliştirildi. Kaynak kod gizli tutulur; bu depo resmi sürümleri barındırır.

---

<div align="center">

**UPO · Ultimate Performance Optimizer** · Designer / Tasarım: **Eren Ağalar** · © 2026

</div>
