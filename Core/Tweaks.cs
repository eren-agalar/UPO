namespace FpsAtolyesi.Core;

public enum Profile { Balanced, Competitive, Custom }

public class RegOp
{
    public string Hive, Path, Name;
    public object Value;
    public RegistryValueKind Kind;
    public bool MissingOk;

    public RegOp(string hive, string path, string name, object value, RegistryValueKind kind = RegistryValueKind.DWord, bool missingOk = false)
    { Hive = hive; Path = path; Name = name; Value = value; Kind = kind; MissingOk = missingOk; }

    public bool IsSet()
    {
        var cur = Reg.Get(Hive, Path, Name);
        if (cur == null) return MissingOk;
        if (Kind == RegistryValueKind.DWord)
        {
            try { return Convert.ToInt32(cur) == (int)Value; } catch { return false; }
        }
        return string.Equals(cur.ToString(), Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}

public class Tweak
{
    public string Id, Title, Desc, Cat;
    public bool Balanced, Competitive, NeedsRestart;
    public int Weight = 4;
    public List<RegOp> Ops = new();
    public Func<bool> CheckFn, SupportedFn;
    public Action<Profile> ApplyFn;
    public Action AfterApply;

    public bool Supported { get { try { return SupportedFn?.Invoke() ?? true; } catch { return false; } } }
    public bool InProfile(Profile p) => p == Profile.Balanced ? Balanced : p == Profile.Competitive ? Competitive : false;

    public bool IsApplied()
    {
        try { return CheckFn != null ? CheckFn() : Ops.All(o => o.IsSet()); }
        catch { return false; }
    }

    public void Apply(Profile p)
    {
        if (ApplyFn != null) ApplyFn(p);
        else foreach (var o in Ops) Reg.Set(Id, o.Hive, o.Path, o.Name, o.Value, o.Kind);
        AfterApply?.Invoke();
    }
}

/// <summary>
/// Tüm optimizasyon ayarları. Her biri gerçek, belgelenmiş bir Windows ayarıdır ve yedeklenerek uygulanır.
/// </summary>
public static class Tweaks
{
    const string MMCSS = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
    const string CDM = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
    const string DX = @"Software\Microsoft\DirectX\UserGpuPreferences";
    const RegistryValueKind SZ = RegistryValueKind.String;

    public static int Build
    {
        get { int.TryParse(Reg.Get("HKLM", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuild") as string, out var b); return b; }
    }

    public static readonly List<Tweak> All = new()
    {
        new Tweak
        {
            Id = "power", Cat = L.T("Güç"), Title = L.T("Yüksek / Nihai Performans güç planı"), Weight = 10, Balanced = true, Competitive = true,
            Desc = L.T("İşlemcinin frekans düşürmesini azaltır. Dengeli: Yüksek Performans · Rekabetçi: Nihai Performans."),
            SupportedFn = () => Power.Active().Guid != null,
            CheckFn = Power.IsPerformance,
            ApplyFn = p => Power.ApplyPerformance("power", p == Profile.Competitive),
        },
        new Tweak
        {
            Id = "usb", Cat = L.T("Güç"), Title = L.T("USB güç tasarrufu (seçici askıya alma) kapalı"), Weight = 3, Competitive = true,
            Desc = L.T("Fare/klavye gibi USB cihazların uykuya geçip gecikme yaratmasını önler (yalnızca prizdeyken)."),
            SupportedFn = () => Power.QueryAc(Power.UsbSub, Power.UsbSuspend) != null,
            CheckFn = () => Power.QueryAc(Power.UsbSub, Power.UsbSuspend) == 0,
            ApplyFn = _ => Power.SetAc("usb", Power.UsbSub, Power.UsbSuspend, 0),
        },
        new Tweak
        {
            Id = "pcie", Cat = L.T("Güç"), Title = L.T("PCIe güç tasarrufu kapalı"), Weight = 3, Competitive = true,
            Desc = L.T("Ekran kartı bağlantısının enerji tasarrufu moduna geçmesini engeller (yalnızca prizdeyken)."),
            SupportedFn = () => Power.QueryAc(Power.PciSub, Power.PciAspm) != null,
            CheckFn = () => Power.QueryAc(Power.PciSub, Power.PciAspm) == 0,
            ApplyFn = _ => Power.SetAc("pcie", Power.PciSub, Power.PciAspm, 0),
        },
        new Tweak
        {
            Id = "throttle", Cat = L.T("Güç"), Title = L.T("Güç kısıtlaması (Power Throttling) kapalı"), Weight = 4, Competitive = true,
            Desc = L.T("Windows'un arka plandaki işlemleri yavaşlatırken oyunu da etkilemesini önler."),
            Ops = { new RegOp("HKLM", @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1) },
        },
        new Tweak
        {
            Id = "gamemode", Cat = L.T("Oyun"), Title = L.T("Oyun Modu açık"), Weight = 6, Balanced = true, Competitive = true,
            Desc = L.T("Windows oyun sırasında kaynakları oyuna öncelikli verir, güncelleme kurulumlarını erteler."),
            Ops =
            {
                new RegOp("HKCU", @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 1, missingOk: true),
                new RegOp("HKCU", @"Software\Microsoft\GameBar", "AllowAutoGameMode", 1, missingOk: true),
            },
        },
        new Tweak
        {
            Id = "gamedvr", Cat = L.T("Oyun"), Title = L.T("Arka plan oyun kaydı (Game DVR) kapalı"), Weight = 8, Balanced = true, Competitive = true,
            Desc = L.T("Xbox Game Bar'ın arka planda sürekli video kaydı tutmasını kapatır (FPS ve kare süresi iyileşir)."),
            Ops =
            {
                new RegOp("HKCU", @"System\GameConfigStore", "GameDVR_Enabled", 0),
                new RegOp("HKCU", @"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0),
            },
        },
        new Tweak
        {
            Id = "hags", Cat = L.T("Oyun"), Title = L.T("Donanım hızlandırmalı GPU zamanlaması (HAGS) açık"), Weight = 5, Competitive = true, NeedsRestart = true,
            Desc = L.T("GPU kendi belleğini yönetir, gecikme azalır. Destekleyen ekran kartı/sürücü gerekir."),
            Ops = { new RegOp("HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2) },
        },
        new Tweak
        {
            Id = "dxwindowed", Cat = L.T("Oyun"), Title = L.T("Pencereli oyun optimizasyonu + VRR (Windows 11)"), Weight = 5, Balanced = true, Competitive = true,
            Desc = L.T("Pencereli/kenarlıksız oyunlarda gecikmeyi azaltır ve değişken yenileme hızını (G-Sync/FreeSync) iyileştirir."),
            SupportedFn = () => Build >= 22000,
            CheckFn = () =>
            {
                var d = ParseDx(Reg.Get("HKCU", DX, "DirectXUserGlobalSettings") as string);
                return d.TryGetValue("SwapEffectUpgradeEnable", out var a) && a == "1" && d.TryGetValue("VRROptimizeEnable", out var b) && b == "1";
            },
            ApplyFn = _ =>
            {
                var d = ParseDx(Reg.Get("HKCU", DX, "DirectXUserGlobalSettings") as string);
                d["SwapEffectUpgradeEnable"] = "1";
                d["VRROptimizeEnable"] = "1";
                Reg.Set("dxwindowed", "HKCU", DX, "DirectXUserGlobalSettings", string.Concat(d.Select(kv => kv.Key + "=" + kv.Value + ";")), SZ);
            },
        },
        new Tweak
        {
            Id = "mmcss", Cat = L.T("Oyun"), Title = L.T("Oyun görevlerine yüksek öncelik (MMCSS)"), Weight = 5, Balanced = true, Competitive = true,
            Desc = L.T("Windows çoklu ortam zamanlayıcısında oyunlara daha yüksek CPU/GPU önceliği verir."),
            Ops =
            {
                new RegOp("HKLM", MMCSS + @"\Tasks\Games", "GPU Priority", 8),
                new RegOp("HKLM", MMCSS + @"\Tasks\Games", "Priority", 6),
                new RegOp("HKLM", MMCSS + @"\Tasks\Games", "Scheduling Category", "High", SZ),
                new RegOp("HKLM", MMCSS + @"\Tasks\Games", "SFIO Priority", "High", SZ),
            },
        },
        new Tweak
        {
            Id = "foreground", Cat = L.T("Oyun"), Title = L.T("Öndeki pencereye (oyuna) işlemci önceliği"), Weight = 4, Competitive = true,
            Desc = L.T("İşlemci zaman dilimlerini aktif pencere lehine ayarlar (Win32PrioritySeparation)."),
            Ops = { new RegOp("HKLM", @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 0x26) },
        },
        new Tweak
        {
            Id = "network", Cat = L.T("Oyun"), Title = L.T("Ağ kısıtlaması kapalı, sistem tepkiselliği oyuna"), Weight = 3, Competitive = true,
            Desc = L.T("Çoklu ortam oynatılırken uygulanan ağ kısıtlamasını kaldırır, arka plana ayrılan CPU payını düşürür."),
            Ops =
            {
                new RegOp("HKLM", MMCSS, "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF)),
                new RegOp("HKLM", MMCSS, "SystemResponsiveness", 10),
            },
        },
        new Tweak
        {
            Id = "mouse", Cat = L.T("Giriş"), Title = L.T("Fare ivmesi (İşaretçi hassasiyetini artır) kapalı"), Weight = 4, Competitive = true,
            Desc = L.T("Nişan alırken fare hareketi hıza göre değişmez, 1:1 olur. Kas hafızası için önemlidir."),
            Ops =
            {
                new RegOp("HKCU", @"Control Panel\Mouse", "MouseSpeed", "0", SZ),
                new RegOp("HKCU", @"Control Panel\Mouse", "MouseThreshold1", "0", SZ),
                new RegOp("HKCU", @"Control Panel\Mouse", "MouseThreshold2", "0", SZ),
            },
            AfterApply = Native.RefreshMouse,
        },
        new Tweak
        {
            Id = "stickykeys", Cat = L.T("Giriş"), Title = L.T("Yapışkan Tuşlar kısayolu kapalı"), Weight = 2, Balanced = true, Competitive = true,
            Desc = L.T("Oyunda Shift'e 5 kez basınca açılan pencere oyunu simge durumuna küçültmez."),
            Ops =
            {
                new RegOp("HKCU", @"Control Panel\Accessibility\StickyKeys", "Flags", "506", SZ),
                new RegOp("HKCU", @"Control Panel\Accessibility\ToggleKeys", "Flags", "58", SZ),
                new RegOp("HKCU", @"Control Panel\Accessibility\Keyboard Response", "Flags", "122", SZ),
            },
        },
        new Tweak
        {
            Id = "update", Cat = "Windows", Title = L.T("Windows Update oyun saatlerinde yeniden başlatmaz"), Weight = 4, Balanced = true, Competitive = true,
            Desc = L.T("Etkin saatleri 10:00 – 02:00 olarak ayarlar; Windows bu aralıkta kendiliğinden yeniden başlatmaz."),
            Ops =
            {
                new RegOp("HKLM", @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "SmartActiveHoursState", 0),
                new RegOp("HKLM", @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "ActiveHoursStart", 10),
                new RegOp("HKLM", @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "ActiveHoursEnd", 2),
            },
        },
        new Tweak
        {
            Id = "delivery", Cat = "Windows", Title = L.T("Teslim Optimizasyonu (eşler arası güncelleme) kapalı"), Weight = 3, Balanced = true, Competitive = true,
            Desc = L.T("Bilgisayarınızın internetteki diğer bilgisayarlara güncelleme yüklemesini durdurur (ping ve bant genişliği)."),
            Ops = { new RegOp("HKU", @"S-1-5-20\Software\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Settings", "DownloadMode", 0) },
        },
        new Tweak
        {
            Id = "ads", Cat = "Windows", Title = L.T("Windows önerileri ve reklamları kapalı"), Weight = 3, Balanced = true, Competitive = true,
            Desc = L.T("Başlat menüsü önerileri, ipuçları ve sessizce yüklenen uygulamaları kapatır."),
            Ops =
            {
                new RegOp("HKCU", CDM, "SystemPaneSuggestionsEnabled", 0),
                new RegOp("HKCU", CDM, "SilentInstalledAppsEnabled", 0),
                new RegOp("HKCU", CDM, "SoftLandingEnabled", 0),
                new RegOp("HKCU", CDM, "SubscribedContent-338388Enabled", 0),
                new RegOp("HKCU", CDM, "SubscribedContent-338389Enabled", 0),
                new RegOp("HKCU", CDM, "SubscribedContent-353694Enabled", 0),
                new RegOp("HKCU", CDM, "SubscribedContent-353696Enabled", 0),
                new RegOp("HKCU", @"Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", 0),
            },
        },
        new Tweak
        {
            Id = "visual", Cat = "Windows", Title = L.T("Saydamlık ve animasyonlar kapalı"), Weight = 3, Competitive = true,
            Desc = L.T("Masaüstü saydamlık efektlerini ve pencere/görev çubuğu animasyonlarını kapatır."),
            Ops =
            {
                new RegOp("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0),
                new RegOp("HKCU", @"Control Panel\Desktop\WindowMetrics", "MinAnimate", "0", SZ),
                new RegOp("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", 0),
            },
        },
        new Tweak
        {
            Id = "bgapps", Cat = L.T("Arka plan"), Title = L.T("Arka plan uygulamaları kapalı"), Weight = 3, Competitive = true,
            Desc = L.T("Mağaza uygulamalarının siz kullanmazken arka planda çalışmasını engeller."),
            Ops =
            {
                new RegOp("HKCU", @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1),
                new RegOp("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", 0),
            },
        },
        new Tweak
        {
            Id = "startdelay", Cat = L.T("Arka plan"), Title = L.T("Açılış gecikmesi kapalı"), Weight = 2, Balanced = true, Competitive = true,
            Desc = L.T("Windows'un açılışta başlangıç uygulamalarını bilerek geciktirmesini kaldırır."),
            Ops = { new RegOp("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", 0) },
        },
        new Tweak
        {
            Id = "telemetry", Cat = L.T("Arka plan"), Title = L.T("Windows telemetri hizmeti (DiagTrack) kapalı"), Weight = 3, Balanced = true, Competitive = true,
            Desc = L.T("Bağlı Kullanıcı Deneyimleri ve Telemetri hizmetini durdurur (disk/ağ kullanımı azalır)."),
            SupportedFn = () => Reg.KeyExists("HKLM", @"SYSTEM\CurrentControlSet\Services\DiagTrack"),
            Ops = { new RegOp("HKLM", @"SYSTEM\CurrentControlSet\Services\DiagTrack", "Start", 4) },
            AfterApply = () => ProcRunner.Capture("sc.exe", "stop DiagTrack", 15000),
        },
    };

    static Dictionary<string, string> ParseDx(string s)
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in (s ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var i = part.IndexOf('=');
            if (i > 0) d[part[..i].Trim()] = part[(i + 1)..].Trim();
        }
        return d;
    }

    public static string GroupTitle(string group) => group switch
    {
        "startup" => L.T("Başlangıç uygulamaları"),
        "vbs" => L.T("Bellek Bütünlüğü (VBS)"),
        _ => All.FirstOrDefault(t => t.Id == group)?.Title ?? group,
    };
}
