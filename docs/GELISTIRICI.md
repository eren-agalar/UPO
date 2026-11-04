# UPO · Ultimate Performance Optimizer

Windows 10/11 için oyun performansı optimizasyon aracı. Tek bir `.exe` dosyasıdır, kurulum gerektirmez.

Tasarım: **Eren Ağalar**

## İndirme seçenekleri
- **UPO-Setup-1.4.0.exe** (önerilen): Kurulum sihirbazı. `C:\Program Files\UPO` klasörüne kurar; Başlat menüsü ve masaüstü kısayolu oluşturur, `.upobackup` dosyalarını UPO ile eşleştirir ve "Ayarlar → Uygulamalar" listesinden kaldırılabilir. Kaldırırken geri alınmamış değişiklik varsa uyarır ve UPO'yu 08 · Tümünü Geri Al sayfasında açmayı önerir.
- **UPO-Portable-1.4.0.zip**: Kurulumsuz tek dosya. USB bellekte taşınabilir.

## Nasıl çalıştırılır
1. `UPO.exe` dosyasına **sağ tıklayın → Yönetici olarak çalıştır**.
2. Windows "Bilgisayarınız korundu" (SmartScreen) uyarısı gösterirse **Ek bilgi → Yine de çalıştır**'a tıklayın.
   Bu uyarının nedeni dosyanın dijital imzasının olmamasıdır.
3. Uygulama açılınca sistemi otomatik olarak tarar ve bir skor verir. Bu tarama hiçbir şeyi değiştirmez.

## Modüller
| # | Modül | Ne yapar |
|---|---|---|
| 01 | Sistem Taraması | Ayarları okur, 100 üzerinden skor verir, masaüstüne HTML rapor çıkarır. Hiçbir şeyi değiştirmez. |
| 02 | Tek Tık Optimizasyon | **Dengeli**, **Rekabetçi** veya **Özel** profille eksik ayarları uygular. |
| 03 | Başlangıç Uygulamaları | Açılışta başlayan uygulamaları açar/kapatır (Görev Yöneticisi yöntemi; uygulamayı silmez). |
| 04 | Temizlik Merkezi | Temp, shader önbelleği, Windows Update önbelleği, hata raporları, DISM bileşen deposu. |
| 05 | Windows Onarımı | Önce DISM /RestoreHealth, ardından SFC /scannow çalıştırır ve ilerlemeyi gösterir. |
| 06 | Önce / Sonra | İlk skoru şimdiki skorla karşılaştırır, düzelen ayarları listeler. |
| 07 | Bellek Bütünlüğü (VBS) | HVCI'yi açar/kapatır. Kapatmak için "EVET" yazmak gerekir, işlem geri alınabilir. |
| 08 | Tümünü Geri Al | Yapılan her ayar değişikliğini tek tuşla orijinal değerine döndürür. |
| 09 | Donanım Testi | İşlemci (tek/çok çekirdek + ısınma), RAM ve disk hızını ölçer; ekran kartıyla birlikte 100 üzerinden puanlar, darboğaz ve yükseltme önerisi verir. |

| 10 | Ayarlar | Görünüm, dil, güvenlik/yedek ve modül tercihleri (aşağıya bakın). |

### Ayarlar (Modül 10)
- **Genel ve görünüm:** Açılışta otomatik tarama · Vurgu rengi (sarı, kırmızı, mavi, yeşil, mor) · Canlı kayıt panelini göster/gizle · Dil: Türkçe / English
- **Güvenlik ve yedek:** Geri yükleme noktası oluştur (aç/kapa) · Yeniden başlatma: Sor / Hiç sorma · Yedeği dışa/içe aktar (`.upobackup`) · Veri klasörünü aç · Verileri sıfırla (bekleyen yedek varsa engellenir)
- **Modüller:** Tek Tık varsayılan profili · Donanım testi süresi (Kısa / Normal / Uzun) · HTML rapor kayıt yeri (Masaüstü / Belgeler)
- **Asla dokunma listesi:** İşaretlenen ayarlar hiçbir profilde uygulanmaz ve skora katılmaz.
- **Başlangıç beyaz listesi:** İşaretlenen başlangıç uygulamaları "kapatılabilir" önerilmez ve skoru düşürmez.
- Renk ve dil değişikliği, uygulama yeniden açılınca geçerli olur ("Şimdi yeniden aç" düğmesi).

### Donanım Testi nasıl puanlar?
- İşlemci ve ekran kartı, uygulamanın içindeki veritabanındaki oyun performansına göre puanlanır (internet gerekmez). Listede olmayan modeller isimden tahmin edilir ve "TAHMİNİ" olarak işaretlenir.
- RAM: kapasite, tek/çift kanal, XMP/EXPO durumu ve ölçülen kopyalama hızı.
- Disk: önbellek atlanarak ölçülen okuma/yazma hızı (1 GB geçici dosya, test sonrası silinir).
- Genel skor = Ekran kartı %45 + İşlemci %30 + RAM %15 + Disk %10.

## Uygulanan ayarlar (02 · Tek Tık)
| Ayar | Dengeli | Rekabetçi |
|---|:-:|:-:|
| Yüksek Performans / Nihai Performans güç planı | ✓ (Yüksek) | ✓ (Nihai) |
| USB seçici askıya alma kapalı (prizde) | | ✓ |
| PCIe güç tasarrufu kapalı (prizde) | | ✓ |
| Power Throttling kapalı | | ✓ |
| Oyun Modu açık | ✓ | ✓ |
| Game DVR (arka plan kaydı) kapalı | ✓ | ✓ |
| HAGS (GPU zamanlaması) açık, yeniden başlatma gerekir | | ✓ |
| Pencereli oyun optimizasyonu + VRR (Win 11) | ✓ | ✓ |
| MMCSS oyun önceliği | ✓ | ✓ |
| Öndeki pencereye CPU önceliği | | ✓ |
| Ağ kısıtlaması kapalı / SystemResponsiveness | | ✓ |
| Fare ivmesi kapalı | | ✓ |
| Yapışkan Tuşlar kısayolu kapalı | ✓ | ✓ |
| Windows Update etkin saatler 10:00–02:00 | ✓ | ✓ |
| Teslim Optimizasyonu (P2P) kapalı | ✓ | ✓ |
| Windows önerileri/reklamları kapalı | ✓ | ✓ |
| Saydamlık ve animasyonlar kapalı | | ✓ |
| Arka plan uygulamaları kapalı | | ✓ |
| Açılış gecikmesi kapalı | ✓ | ✓ |
| Telemetri hizmeti (DiagTrack) kapalı | ✓ | ✓ |

## Güvenlik
- **Sıfır veri toplama:** Uygulama hiçbir kişisel veya donanım verisi toplamaz ya da göndermez.
  İnternete yalnızca güncelleme kontrolü için (GitHub Releases) bağlanır; bu, Ayarlar'dan kapatılabilir.
  (Windows'un kendi DISM onarımı Windows Update'ten dosya indirebilir.)
- **İmzalı güncellemeler:** Güncellemeler ECDSA P-256 ile imzalanır; UPO imzası ve SHA-256 değeri tutmayan dosyayı kurmaz. Yayınlama adımları: `RELEASE.md`.
- **Yedekleme:** Her ayarın orijinal değeri `C:\ProgramData\UPO\backup.json` dosyasına yazılır.
  Modül 08 bu dosyadan geri yükleme yapar. (Eski "FPS Atölyesi" sürümünün `C:\ProgramData\FPSAtolyesi` klasöründeki yedekler ilk açılışta otomatik olarak buraya taşınır.)
- **Geri yükleme noktası:** İlk değişiklikten önce Windows sistem geri yükleme noktası oluşturulmaya çalışılır.
  Sistem Koruması kapalıysa bu adım atlanır, uygulamanın kendi yedeği yine de alınır.
- **Geri alınamayan işlemler:** Temizlik (yalnızca geçici/önbellek dosyaları) ve SFC/DISM onarımı ayar değil dosya işlemi olduğu için geri alınamaz.
- Anti-cheat, antivirüs ve ses/ekran kartı sürücüsü girişleri başlangıç listesinde **DOKUNMAYIN** olarak işaretlenir.
- Kullanıcı ayarları (HKCU) uygulamayı çalıştıran hesaba yazılır. Standart bir hesapta başka bir yönetici hesabının şifresiyle yükseltme yapılırsa bu ayarlar o yönetici hesabına uygulanır.

> Skor, oyun performansını etkileyen ayarların durumuna göre hesaplanır. Gerçek FPS artışı donanıma, oyuna ve sürücüye göre değişir.

## Kurulum dosyasını derleme
NSIS 3 gerekir (Linux: `apt install nsis`, Windows: nsis.sourceforge.io).
```
dotnet publish -c Release -o publish
makensis installer/UPO.nsi
```
Çıktı: `publish/UPO-Setup-<sürüm>.exe`

## Kaynaktan derleme
.NET 8 SDK gerekir. Windows'ta veya Linux'ta şu komutla derlenir:
```
dotnet publish -c Release -o publish
```
Komut, içinde .NET çalışma zamanı da bulunan tek bir `publish/UPO.exe` dosyası üretir.

## Dil desteği
Arayüz metinleri Türkçe yazılır ve `L.T(...)` / `L.F(...)` ile sarılır. İngilizce karşılıkları `Core/Lang.En.cs` sözlüğündedir; sözlükte olmayan bir metin Türkçe gösterilir.

## Proje yapısı
```
Core/     Kayıt defteri, yedekleme, güç planı, ayar kataloğu, tarama, temizlik, VBS
UI/       Tema, özel çizilmiş HUD bileşenleri, ana pencere
Pages/    8 modülün sayfaları
```
