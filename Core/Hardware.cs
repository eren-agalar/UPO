using System.Management;

namespace FpsAtolyesi.Core;

/// <summary>
/// Uygulama içi donanım veritabanı. Değerler, oyunlardaki yaklaşık göreli performanstır
/// (en güçlü güncel parça ≈ 100). İnternete bağlanmadan çalışır.
/// Listede olmayan modeller için isimden tahmin yapılır ve "tahmini" olarak işaretlenir.
/// </summary>
public static class HwDb
{
    public static string Version => L.T("Kasım 2026");

    // ── Ekran kartları (oyun performansı, RTX 5090 ≈ 100) ──
    static readonly (string key, double rel)[] Gpus =
    {
        ("rtx 5090", 100), ("rtx 4090", 88), ("rtx 5080", 76), ("rtx 4080 super", 71), ("rtx 4080", 69), ("rtx 5070 ti", 67),
        ("rtx 4070 ti super", 61), ("rtx 4070 ti", 57), ("rtx 5070", 56), ("rtx 4070 super", 55), ("rtx 3090 ti", 53), ("rtx 3090", 49),
        ("rtx 4070", 48), ("rtx 3080 ti", 47), ("rtx 3080", 44), ("rtx 5060 ti", 41), ("rtx 3070 ti", 37), ("rtx 4060 ti", 36),
        ("rtx 3070", 35), ("rtx 5060", 33), ("rtx 2080 ti", 33), ("rtx 3060 ti", 31), ("rtx 4060", 29), ("rtx 2080 super", 28),
        ("rtx 2080", 27), ("rtx 5050", 25), ("rtx 2070 super", 25), ("rtx 3060", 24), ("rtx 2070", 22), ("rtx 2060 super", 21),
        ("rtx 2060", 18), ("rtx 3050", 15), ("gtx 1080 ti", 23), ("gtx 1080", 18), ("gtx 1070 ti", 16), ("gtx 1660 ti", 14),
        ("gtx 1660 super", 14), ("gtx 1070", 14), ("gtx 1660", 12), ("gtx 1650 super", 10), ("gtx 1060", 9.5), ("gtx 1650", 7.5),
        ("gtx 980 ti", 11), ("gtx 980", 8.5), ("gtx 970", 7.5), ("gtx 1050 ti", 5), ("gtx 1050", 4), ("gtx 960", 4.5), ("gt 1030", 2),
        ("rx 9070 xt", 65), ("rx 9070", 58), ("rx 9060 xt", 38), ("rx 7900 xtx", 69), ("rx 7900 xt", 61), ("rx 7900 gre", 51),
        ("rx 7800 xt", 46), ("rx 7700 xt", 39), ("rx 7600 xt", 29), ("rx 7600", 28), ("rx 6950 xt", 47), ("rx 6900 xt", 44),
        ("rx 6800 xt", 42), ("rx 6800", 37), ("rx 6750 xt", 32), ("rx 6700 xt", 30), ("rx 6700", 27), ("rx 6650 xt", 26),
        ("rx 6600 xt", 25), ("rx 6600", 22), ("rx 6500 xt", 8.5), ("rx 6400", 6), ("rx 5700 xt", 20), ("rx 5700", 18),
        ("rx 5600 xt", 16), ("rx 5500 xt", 10), ("rx 590", 10.5), ("rx 580", 10), ("rx 570", 8), ("rx vega 64", 15), ("rx vega 56", 13),
        ("arc b580", 28), ("arc b570", 25), ("arc a770", 24), ("arc a750", 22), ("arc a580", 19), ("arc a380", 6),
        ("radeon 890m", 10), ("radeon 880m", 9), ("radeon 780m", 8), ("radeon 760m", 6.5), ("radeon 680m", 7), ("radeon 660m", 5),
        ("arc 140v", 8), ("arc graphics", 6), ("iris xe", 3), ("uhd graphics", 1.5), ("hd graphics", 1),
        ("radeon vega 8", 2.5), ("radeon vega 11", 3), ("radeon graphics", 3.5),
    };

    // ── İşlemciler (oyun performansı, Ryzen 7 9800X3D ≈ 100) ──
    static readonly Dictionary<string, double> Cpus = new()
    {
        ["9800x3d"] = 100, ["9950x3d"] = 99, ["9900x3d"] = 94, ["7800x3d"] = 90, ["7950x3d"] = 89, ["7900x3d"] = 84,
        ["9950x"] = 84, ["9900x"] = 82, ["9700x"] = 81, ["9600x"] = 77, ["9600"] = 75,
        ["7950x"] = 80, ["7900x"] = 79, ["7900"] = 76, ["7700x"] = 78, ["7700"] = 76, ["7600x"] = 75, ["7600"] = 73, ["8700g"] = 66, ["8600g"] = 62, ["8500g"] = 55,
        ["5800x3d"] = 78, ["5700x3d"] = 75, ["5600x3d"] = 73, ["5950x"] = 68, ["5900x"] = 68, ["5800x"] = 66, ["5700x"] = 64, ["5800"] = 64,
        ["5600x"] = 63, ["5600"] = 62, ["5600xt"] = 63, ["5500"] = 55, ["5700g"] = 57, ["5600g"] = 53, ["5600gt"] = 54, ["5500gt"] = 52, ["4500"] = 50,
        ["3950x"] = 54, ["3900x"] = 53, ["3800x"] = 51, ["3700x"] = 50, ["3600x"] = 48, ["3600"] = 47, ["3400g"] = 36, ["3200g"] = 30, ["3100"] = 40, ["3300x"] = 45,
        ["2700x"] = 38, ["2700"] = 36, ["2600x"] = 35, ["2600"] = 34, ["2400g"] = 28, ["2200g"] = 24, ["1800x"] = 31, ["1700"] = 29, ["1600"] = 28, ["1600af"] = 32,
        ["285k"] = 83, ["265k"] = 80, ["245k"] = 75, ["225"] = 66,
        ["14900ks"] = 88, ["14900k"] = 86, ["14900"] = 82, ["14700k"] = 83, ["14700"] = 79, ["14600k"] = 79, ["14600"] = 72, ["14400"] = 67, ["14100"] = 56,
        ["13900ks"] = 86, ["13900k"] = 84, ["13900"] = 80, ["13700k"] = 81, ["13700"] = 77, ["13600k"] = 77, ["13600"] = 70, ["13500"] = 68, ["13400"] = 65, ["13100"] = 55,
        ["12900ks"] = 77, ["12900k"] = 75, ["12900"] = 72, ["12700k"] = 73, ["12700"] = 70, ["12600k"] = 70, ["12600"] = 66, ["12500"] = 64, ["12400"] = 63, ["12100"] = 54,
        ["11900k"] = 63, ["11700k"] = 61, ["11600k"] = 59, ["11400"] = 55, ["10900k"] = 58, ["10850k"] = 57, ["10700k"] = 55, ["10700"] = 53,
        ["10600k"] = 52, ["10400"] = 46, ["10100"] = 38, ["9900k"] = 52, ["9900ks"] = 54, ["9700k"] = 48, ["9600k"] = 42, ["9400"] = 38,
        ["8700k"] = 45, ["8700"] = 43, ["8600k"] = 40, ["8400"] = 38, ["8100"] = 30, ["7700k"] = 36, ["7600k"] = 30, ["6700k"] = 32, ["6600k"] = 27,
        ["4790k"] = 26, ["4770k"] = 24, ["4690k"] = 22, ["3770k"] = 19,
    };

    /// <summary>GPU göreli performansı; tahmin ise estimated=true.</summary>
    public static (double rel, bool estimated, bool laptop) Gpu(string name)
    {
        var n = (name ?? "").ToLowerInvariant().Replace("(r)", "").Replace("(tm)", "");
        n = Regex.Replace(n, @"\s+", " ");
        bool laptop = Regex.IsMatch(n, @"laptop|mobile|max-q|notebook");
        double k = laptop ? 0.72 : 1;
        foreach (var (key, rel) in Gpus.OrderByDescending(g => g.key.Length))
            if (Regex.IsMatch(n, @"(^|\W)" + Regex.Escape(key) + @"($|\W)")) return (rel * k, false, laptop);

        // Tahmin: isimden seri ve seviye çıkar.
        var m = Regex.Match(n, @"\b(rtx|gtx)\s*(\d{2})(\d{2})\s*(ti|super)?");
        if (m.Success)
        {
            int gen = int.Parse(m.Groups[2].Value), tier = int.Parse(m.Groups[3].Value);
            var tierRel = new Dictionary<int, double> { [50] = 25, [60] = 33, [70] = 56, [80] = 76, [90] = 100 };
            double r = tierRel.TryGetValue(tier, out var t) ? t : 30;
            r *= Math.Pow(1.25, (gen - 50) / 10.0);
            if (m.Groups[4].Success) r *= 1.08;
            return (Math.Clamp(r, 2, 130) * k, true, laptop);
        }
        m = Regex.Match(n, @"\brx\s*(\d)(\d)(\d)0\s*(xtx|xt|gre)?");
        if (m.Success)
        {
            int tier = int.Parse(m.Groups[2].Value);
            double r = tier switch { >= 9 => 62, 8 => 46, 7 => 38, 6 => 27, _ => 12 };
            if (m.Groups[4].Value == "xt" || m.Groups[4].Value == "xtx") r *= 1.1;
            return (r * k, true, laptop);
        }
        return (5 * k, true, laptop);
    }

    public static (double rel, bool estimated, string token) Cpu(string name)
    {
        var n = (name ?? "").ToLowerInvariant().Replace("(r)", "").Replace("(tm)", "");
        n = Regex.Replace(n, @"\s+", " ");
        string tok = null;
        var m = Regex.Match(n, @"\bi([3579])-(\d{4,5})([a-z]*)");
        if (m.Success) tok = m.Groups[2].Value + m.Groups[3].Value;
        var mu = Regex.Match(n, @"ultra\s+[3579]\s+(\d{3})([a-z]*)");
        if (mu.Success) tok = mu.Groups[1].Value + mu.Groups[2].Value;
        var mr = Regex.Match(n, @"ryzen\s+[3579]\s+(?:pro\s+)?(\d{4})([a-z0-9]*)\b");
        if (mr.Success) tok = mr.Groups[1].Value + mr.Groups[2].Value;

        if (tok != null)
        {
            foreach (var t in new[] { tok, Regex.Replace(tok, "kf$", "k"), Regex.Replace(tok, "f$", ""), Regex.Replace(tok, "(x3d|x|xt|g|gt|ks|k|t|f)$", "") })
                if (Cpus.TryGetValue(t, out var v)) return (v, false, tok);
        }
        return (EstimateCpu(n, m, mu, mr), true, tok ?? "");
    }

    static double EstimateCpu(string n, Match intel, Match ultra, Match ryzen)
    {
        if (intel.Success)
        {
            var digits = intel.Groups[2].Value;
            int gen = digits.Length == 5 ? int.Parse(digits[..2]) : int.Parse(digits[..1]);
            var b = new Dictionary<int, double> { [2] = 14, [3] = 16, [4] = 20, [5] = 22, [6] = 25, [7] = 28, [8] = 34, [9] = 38, [10] = 42, [11] = 50, [12] = 58, [13] = 64, [14] = 66 };
            double r = b.TryGetValue(gen, out var v) ? v : gen > 14 ? 68 : 12;
            r += intel.Groups[1].Value switch { "3" => -12, "7" => 6, "9" => 10, _ => 0 };
            var s = intel.Groups[3].Value;
            r += s.StartsWith("k") ? 4 : s.StartsWith("u") ? -14 : s.StartsWith("p") ? -10 : s.StartsWith("t") ? -6 : s == "h" || s == "hk" ? -4 : 0;
            return Math.Clamp(r, 8, 95);
        }
        if (ultra.Success)
        {
            int num = int.Parse(ultra.Groups[1].Value);
            var s = ultra.Groups[2].Value;
            double r = num >= 200 ? (s.StartsWith("k") || s == "" || s == "f" ? 74 : 60) : 55;
            return r + (num % 100 >= 80 ? 4 : num % 100 >= 60 ? 2 : 0);
        }
        if (ryzen.Success)
        {
            var d = ryzen.Groups[1].Value;
            int series = d[0] - '0';
            var b = new Dictionary<int, double> { [1] = 26, [2] = 32, [3] = 45, [4] = 42, [5] = 60, [6] = 55, [7] = 70, [8] = 65, [9] = 75 };
            double r = b.TryGetValue(series, out var v) ? v : 70;
            var tier = Regex.Match(n, @"ryzen\s+([3579])").Groups[1].Value;
            r += tier switch { "3" => -14, "7" => 4, "9" => 6, _ => 0 };
            var s = ryzen.Groups[2].Value;
            r += s.Contains("x3d") ? 14 : s.StartsWith("g") ? -8 : s.StartsWith("u") ? -14 : s.StartsWith("hs") || s == "h" ? -6 : s.StartsWith("hx") ? -2 : 0;
            return Math.Clamp(r, 15, 100);
        }
        if (n.Contains("ryzen ai")) return 62;
        if (n.Contains("xeon") || n.Contains("epyc")) return 40;
        if (n.Contains("celeron") || n.Contains("pentium") || n.Contains("athlon")) return 15;
        return 30;
    }

    /// <summary>Ekran kartının rahat çalıştırdığı tipik çözünürlük/ayar.</summary>
    public static string GpuTier(double rel) => rel switch
    {
        >= 70 => L.T("4K yüksek ayarlarda rahat"),
        >= 40 => L.T("1440p yüksek · 4K orta ayarlarda rahat"),
        >= 28 => L.T("1080p yüksek · 1440p orta ayarlarda rahat"),
        >= 15 => L.T("1080p orta ayarlarda rahat"),
        >= 7 => L.T("1080p düşük ayarlarda oynanabilir"),
        _ => L.T("Güncel oyunlar için yetersiz"),
    };
}

// ───────────────────────────── Algılanan donanım ─────────────────────────────
public class GpuInfo { public string Name; public long Vram; public string DriverVersion; public DateTime? DriverDate; }
public class RamInfo { public long Total; public int Modules; public int Speed; public int Configured; public int Type; public int Slots; }
public class DiskInfo { public string Name, Media, Bus; public long Size; public bool IsSystem; }

public class HwInfo
{
    public string Cpu = "?";
    public int Cores, Threads, MaxMhz;
    public GpuInfo Gpu = new();
    public RamInfo Ram = new();
    public List<DiskInfo> Disks = new();
    public bool Laptop;

    public static HwInfo Detect(SysInfo sys)
    {
        var h = new HwInfo { Cpu = sys?.Cpu ?? "?", Threads = Environment.ProcessorCount };
        Try(() =>
        {
            using var q = new ManagementObjectSearcher("SELECT NumberOfCores, MaxClockSpeed FROM Win32_Processor");
            foreach (ManagementObject o in q.Get()) { h.Cores += Convert.ToInt32(o["NumberOfCores"]); h.MaxMhz = Math.Max(h.MaxMhz, Convert.ToInt32(o["MaxClockSpeed"])); }
        });

        h.Gpu.Name = sys?.Gpu ?? "?";
        Try(() =>
        {
            const string cls = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
            using var b = Reg.Base("HKLM");
            using var root = b.OpenSubKey(cls);
            foreach (var sub in root?.GetSubKeyNames() ?? Array.Empty<string>())
            {
                if (!Regex.IsMatch(sub, @"^\d{4}$")) continue;
                using var k = root.OpenSubKey(sub);
                if (k == null || !string.Equals((k.GetValue("DriverDesc") as string)?.Trim(), h.Gpu.Name, StringComparison.OrdinalIgnoreCase)) continue;
                var q = k.GetValue("HardwareInformation.qwMemorySize");
                if (q is long ql) h.Gpu.Vram = ql;
                else if (q is byte[] qb && qb.Length >= 8) h.Gpu.Vram = BitConverter.ToInt64(qb, 0);
                if (h.Gpu.Vram <= 0)
                {
                    var m = k.GetValue("HardwareInformation.MemorySize");
                    if (m is int mi) h.Gpu.Vram = (uint)mi;
                    else if (m is byte[] mb && mb.Length >= 4) h.Gpu.Vram = BitConverter.ToUInt32(mb, 0);
                }
                h.Gpu.DriverVersion = k.GetValue("DriverVersion") as string;
                if (DateTime.TryParseExact(k.GetValue("DriverDate") as string, new[] { "M-d-yyyy", "MM-dd-yyyy" }, null, System.Globalization.DateTimeStyles.None, out var dd))
                    h.Gpu.DriverDate = dd;
                break;
            }
        });

        Try(() =>
        {
            using var q = new ManagementObjectSearcher("SELECT Capacity, Speed, ConfiguredClockSpeed, SMBIOSMemoryType FROM Win32_PhysicalMemory");
            foreach (ManagementObject o in q.Get())
            {
                h.Ram.Total += Convert.ToInt64(o["Capacity"]);
                h.Ram.Modules++;
                h.Ram.Speed = Math.Max(h.Ram.Speed, Convert.ToInt32(o["Speed"] ?? 0));
                var c = Convert.ToInt32(o["ConfiguredClockSpeed"] ?? 0);
                h.Ram.Configured = h.Ram.Configured == 0 ? c : Math.Min(h.Ram.Configured, c);
                h.Ram.Type = Convert.ToInt32(o["SMBIOSMemoryType"] ?? 0);
            }
        });
        Try(() =>
        {
            using var q = new ManagementObjectSearcher("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray");
            foreach (ManagementObject o in q.Get()) h.Ram.Slots += Convert.ToInt32(o["MemoryDevices"] ?? 0);
        });
        if (h.Ram.Total <= 0 && sys != null) h.Ram.Total = sys.Ram;

        Try(() =>
        {
            int sysDisk = -1;
            var letter = Path.GetPathRoot(Environment.SystemDirectory)?[0].ToString();
            using (var q = new ManagementObjectSearcher(@"root\Microsoft\Windows\Storage", $"SELECT DiskNumber FROM MSFT_Partition WHERE DriveLetter='{letter}'"))
                foreach (ManagementObject o in q.Get()) sysDisk = Convert.ToInt32(o["DiskNumber"]);
            using var d = new ManagementObjectSearcher(@"root\Microsoft\Windows\Storage", "SELECT DeviceId, FriendlyName, MediaType, BusType, Size FROM MSFT_PhysicalDisk");
            foreach (ManagementObject o in d.Get())
            {
                int media = Convert.ToInt32(o["MediaType"] ?? 0), bus = Convert.ToInt32(o["BusType"] ?? 0);
                if (bus == 7 && media != 4) continue; // USB bellekler
                h.Disks.Add(new DiskInfo
                {
                    Name = (o["FriendlyName"]?.ToString() ?? "Disk").Trim(),
                    Media = media switch { 3 => "HDD", 4 => "SSD", 5 => "SCM", _ => "?" },
                    Bus = bus switch { 17 => "NVMe", 11 => "SATA", 7 => "USB", 8 => "RAID", 10 => "SAS", _ => "" },
                    Size = Convert.ToInt64(o["Size"] ?? 0L),
                    IsSystem = o["DeviceId"]?.ToString() == sysDisk.ToString(),
                });
            }
        });

        Try(() =>
        {
            using var q = new ManagementObjectSearcher("SELECT ChassisTypes FROM Win32_SystemEnclosure");
            foreach (ManagementObject o in q.Get())
                if (o["ChassisTypes"] is ushort[] ct && ct.Any(c => c is 8 or 9 or 10 or 14 or 31 or 32)) h.Laptop = true;
        });
        return h;
    }

    static void Try(Action a) { try { a(); } catch { } }

    public string RamTypeText => Ram.Type switch { 26 => "DDR4", 34 => "DDR5", 24 => "DDR3", 35 => "LPDDR5", 30 => "LPDDR4", _ => "RAM" };
    public DiskInfo SystemDisk => Disks.FirstOrDefault(d => d.IsSystem) ?? Disks.FirstOrDefault();

    /// <summary>Anakart platformu (yükseltme önerisi için).</summary>
    public string Platform
    {
        get
        {
            var n = Cpu.ToLowerInvariant();
            var r = Regex.Match(n, @"ryzen\s+[3579]\s+(?:pro\s+)?(\d)(\d{3})([a-z0-9]*)");
            if (r.Success)
            {
                int s = r.Groups[1].Value[0] - '0';
                var suf = r.Groups[3].Value;
                if (Regex.IsMatch(suf, "^(u|h|hs|hx)")) return "laptop";
                if (s is 7 or 9 || (s == 8 && suf.StartsWith("g"))) return "AM5";
                if (s is >= 1 and <= 5) return "AM4";
            }
            var i = Regex.Match(n, @"\bi[3579]-(\d{4,5})([a-z]*)");
            if (i.Success)
            {
                if (Regex.IsMatch(i.Groups[2].Value, "^(u|h|p|hx|hk|g)")) return "laptop";
                var d = i.Groups[1].Value;
                int gen = d.Length == 5 ? int.Parse(d[..2]) : int.Parse(d[..1]);
                if (gen is >= 12 and <= 14) return "LGA1700";
                if (gen is 10 or 11) return "LGA1200";
                return "eski-intel";
            }
            if (Regex.IsMatch(n, @"ultra\s+[579]\s+2\d\d(k|f|kf)?\b")) return "LGA1851";
            return "?";
        }
    }
}

// ───────────────────────────── Puanlama ve öneriler ─────────────────────────────
public class CompScore
{
    public string Title, Model;
    public int Points;          // 0-100
    public bool Estimated;
    public List<string> Details = new();
    public string Verdict;
    public Color Color;
}

public class Advice { public string Title, Detail; public int Level; } // 0 bilgi, 1 öneri, 2 önemli

public class HwReport
{
    public HwInfo Hw;
    public CompScore Cpu, Gpu, Ram, Disk;
    public int Overall;
    public string Summary;
    public List<Advice> Advices = new();
}

public static class HwScorer
{
    static string Label(int p) => p >= 85 ? L.T("ÜST SEVİYE") : p >= 65 ? L.T("GÜÇLÜ") : p >= 45 ? L.T("ORTA") : p >= 25 ? L.T("GİRİŞ SEVİYESİ") : L.T("YETERSİZ");

    public static HwReport Score(HwInfo h, BenchResult b)
    {
        var r = new HwReport { Hw = h };

        // İşlemci
        var (crel, cest, _) = HwDb.Cpu(h.Cpu);
        var cpu = new CompScore { Title = L.T("İŞLEMCİ"), Model = h.Cpu, Estimated = cest, Points = (int)Math.Clamp(Math.Round(crel / 80 * 100), 0, 100) };
        cpu.Details.Add((h.Cores > 0 ? L.F("{0} çekirdek / ", h.Cores) : "") + L.F("{0} iş parçacığı", h.Threads) + (h.MaxMhz > 0 ? $" · {h.MaxMhz / 1000.0:0.0} GHz taban" : ""));
        if (b?.CpuSingle > 0) cpu.Details.Add(L.F("Test: tek çekirdek {0:N0} · çok çekirdek {1:N0} puan", b.CpuSingle, b.CpuMulti));
        if (b?.CpuMulti > 0) cpu.Details.Add(b.Throttle >= 10 ? L.F("Yük altında hız %{0:0} düştü (ısınma olabilir)", b.Throttle) : L.T("Yük altında hız sabit kaldı ✓"));
        r.Cpu = cpu;

        // Ekran kartı
        var (grel, gest, glap) = HwDb.Gpu(h.Gpu.Name);
        var gpu = new CompScore { Title = L.T("EKRAN KARTI"), Model = h.Gpu.Name, Estimated = gest, Points = (int)Math.Clamp(Math.Round(grel / 55 * 100), 0, 100) };
        long vramGb = (long)Math.Round(h.Gpu.Vram / (double)(1L << 30));
        if (vramGb > 0 && vramGb < 8 && grel >= 15) gpu.Points = Math.Max(0, gpu.Points - 8);
        gpu.Details.Add((vramGb > 0 ? $"{vramGb} GB VRAM · " : "") + HwDb.GpuTier(grel));
        if (h.Gpu.DriverDate is DateTime dd) gpu.Details.Add(L.F("Sürücü: {0} · {1:dd.MM.yyyy}", NvVersion(h.Gpu) ?? h.Gpu.DriverVersion, dd));
        r.Gpu = gpu;

        // RAM
        int ramGb = (int)Math.Round(h.Ram.Total / (double)(1L << 30));
        int rp = ramGb >= 32 ? 100 : ramGb >= 16 ? 85 : ramGb >= 12 ? 60 : ramGb >= 8 ? 40 : 20;
        bool single = h.Ram.Modules == 1 && !h.Laptop;
        bool xmpOff = (h.Ram.Type == 26 && h.Ram.Configured is > 0 and <= 2666) || (h.Ram.Type == 34 && h.Ram.Configured is > 0 and <= 4800);
        if (single) rp -= 15;
        if (xmpOff) rp -= 10;
        var ram = new CompScore { Title = L.T("BELLEK (RAM)"), Model = $"{ramGb} GB {h.RamTypeText}" + (h.Ram.Configured > 0 ? $" · {h.Ram.Configured} MT/s" : ""), Points = Math.Clamp(rp, 0, 100) };
        ram.Details.Add(h.Ram.Modules > 0 ? L.F("{0} modül", h.Ram.Modules) + (h.Ram.Slots > 0 ? $" / {h.Ram.Slots} yuva" : "") + (single ? " · tek kanal" : h.Ram.Modules >= 2 ? L.T(" · çift kanal") : "") : L.T("Modül bilgisi okunamadı"));
        if (b?.RamGBs > 0) ram.Details.Add(L.F("Test: {0:0.0} GB/s kopyalama hızı", b.RamGBs));
        r.Ram = ram;

        // Disk
        var sd = h.SystemDisk;
        int dp = sd == null ? 60 : sd.Media == "HDD" ? 25 : sd.Bus == "NVMe" ? 95 : sd.Media == "SSD" ? 75 : 60;
        if (b?.DiskRead > 0) dp = b.DiskRead >= 3000 ? 100 : b.DiskRead >= 1500 ? 92 : b.DiskRead >= 450 ? 75 : b.DiskRead >= 200 ? 45 : 25;
        var disk = new CompScore { Title = L.T("SİSTEM DİSKİ"), Model = sd != null ? $"{sd.Name}" : "Bilinmiyor", Points = dp };
        if (sd != null) disk.Details.Add($"{sd.Media} {sd.Bus} · {Fmt.Size(sd.Size)}".Replace("  ", " "));
        if (b?.DiskRead > 0) disk.Details.Add($"Test: okuma {b.DiskRead:N0} MB/s · yazma {b.DiskWrite:N0} MB/s · 4K {b.Disk4k:0.0} MB/s");
        else if (b != null && b.DiskSkipped != null) disk.Details.Add(b.DiskSkipped);
        r.Disk = disk;

        foreach (var c in new[] { cpu, gpu, ram, disk })
        {
            c.Verdict = Label(c.Points) + (c.Estimated ? L.T(" · TAHMİNİ") : "");
            c.Color = ScoreColor(c.Points);
        }

        r.Overall = (int)Math.Round(gpu.Points * .45 + cpu.Points * .30 + ram.Points * .15 + disk.Points * .10);
        r.Summary = L.F("{0} SİSTEM · Ekran kartı: {1}.", Label(r.Overall), HwDb.GpuTier(grel));
        BuildAdvice(r, h, b, grel, crel, ramGb, single, xmpOff, glap);
        return r;
    }

    static string NvVersion(GpuInfo g)
    {
        // NVIDIA: 32.0.15.6094 → 560.94
        if (g.Name?.ToLowerInvariant().Contains("nvidia") != true || g.DriverVersion == null) return null;
        var digits = g.DriverVersion.Replace(".", "");
        if (digits.Length < 5) return null;
        var last = digits[^5..];
        return $"{last[..3]}.{last[3..]}";
    }

    static void BuildAdvice(HwReport r, HwInfo h, BenchResult b, double grel, double crel, int ramGb, bool single, bool xmpOff, bool glap)
    {
        var a = r.Advices;
        int gp = r.Gpu.Points, cp = r.Cpu.Points;

        if (cp + 25 < gp)
            a.Add(new Advice { Level = 2, Title = L.T("İşlemci darboğazı"), Detail = L.T("Ekran kartın işlemcinden belirgin şekilde güçlü; özellikle yüksek FPS'li rekabetçi oyunlarda işlemci kartı bekletiyor.") });
        else if (gp + 25 < cp && !h.Laptop && !glap)
            a.Add(new Advice { Level = 1, Title = L.T("En zayıf halka: ekran kartı"), Detail = L.T("İşlemcin ekran kartından çok daha güçlü; oyunlarda en büyük artışı ekran kartı yükseltmesi sağlar.") });

        if (gp < 45 && !glap && !h.Laptop)
            a.Add(new Advice { Level = gp < 25 ? 2 : 1, Title = L.T("Ekran kartı yükseltmesi"), Detail = gp < 25
                ? L.T("Güncel oyunlar için yetersiz. RTX 5060 / RX 9060 XT seviyesi bir kart çok büyük fark yaratır.")
                : L.T("RTX 5060 Ti / RX 9060 XT 16 GB veya üstü bir kart, 1440p'de belirgin artış sağlar.") });

        if (cp < 60 && !h.Laptop)
        {
            var p = h.Platform;
            string det = p switch
            {
                "AM4" => L.T("Aynı anakartla Ryzen 7 5700X3D'ye geçebilirsin (önce anakart BIOS'unu güncelle). Oyunlarda en ucuz büyük sıçrama budur."),
                "AM5" => L.T("Aynı anakartla Ryzen 7 7800X3D / 9800X3D'ye geçebilirsin."),
                "LGA1700" => L.T("Aynı anakartla Core i5-14600K veya üstüne geçebilirsin (13./14. nesil için BIOS'u güncelle)."),
                "LGA1200" => L.T("Aynı anakartla en fazla Core i7-11700K'ya çıkılabilir; büyük artış için platform (anakart + RAM) değişikliği gerekir."),
                _ => L.T("Bu platformda anlamlı yükseltme yok; anakart + işlemci + RAM birlikte değişmeli (ör. AM5 + Ryzen 5 7600X / 9600X)."),
            };
            a.Add(new Advice { Level = cp < 40 ? 2 : 1, Title = L.T("İşlemci yükseltmesi"), Detail = det });
        }

        if (ramGb > 0 && ramGb < 16)
            a.Add(new Advice { Level = 2, Title = $"RAM yetersiz ({ramGb} GB)", Detail = L.T("Güncel oyunlar için en az 16 GB (2×8), tercihen 32 GB (2×16) önerilir.") });
        else if (ramGb == 16)
            a.Add(new Advice { Level = 0, Title = L.T("16 GB RAM yeterli"), Detail = L.T("Çoğu oyun için yeterli. Arka planda tarayıcı/Discord açıkken ağır oyunlarda 32 GB daha akıcı olur.") });
        if (single)
            a.Add(new Advice { Level = 2, Title = L.T("RAM tek kanal çalışıyor"), Detail = L.T("Tek modül takılı. Aynı modelden ikinci bir modül eklemek (çift kanal) FPS'i %10-30 artırabilir.") });
        if (xmpOff)
            a.Add(new Advice { Level = 1, Title = L.T("XMP / EXPO kapalı olabilir"), Detail = L.F("RAM {0} MT/s'de çalışıyor. BIOS'tan XMP (Intel) / EXPO (AMD) profilini açarak bedava performans kazanabilirsin.", h.Ram.Configured) });

        var sd = h.SystemDisk;
        if (sd?.Media == "HDD" || (b?.DiskRead > 0 && b.DiskRead < 200))
            a.Add(new Advice { Level = 2, Title = L.T("Sistem diski yavaş (HDD)"), Detail = L.T("Windows ve oyunlar için bir NVMe SSD, açılış ve yükleme sürelerini birkaç kat kısaltır.") });
        if (h.Disks.Any(d => d.Media == "HDD") && sd?.Media != "HDD")
            a.Add(new Advice { Level = 0, Title = L.T("Bilgisayarda HDD var"), Detail = L.T("Oyunları HDD yerine SSD'ye kurarsan yükleme süreleri ve takılmalar azalır.") });

        if (h.Gpu.DriverDate is DateTime dd && (DateTime.Now - dd).TotalDays > 180)
            a.Add(new Advice { Level = 1, Title = L.T("Ekran kartı sürücüsü eski"), Detail = L.F("Sürücü tarihi {0:dd.MM.yyyy}. Yeni oyunlarda performans/uyumluluk için üreticinin sitesinden güncelle.", dd) });

        if (b != null && b.Throttle >= 10)
            a.Add(new Advice { Level = 1, Title = L.T("İşlemci ısınıyor olabilir"), Detail = L.F("Yük altında hız %{0:0} düştü. Soğutucu, termal macun ve kasa hava akışını kontrol et.", b.Throttle) });

        if (h.Laptop)
            a.Add(new Advice { Level = 0, Title = L.T("Dizüstü bilgisayar"), Detail = L.T("Oyun oynarken şarj aletini tak ve üreticinin performans modunu (ör. Turbo/Performance) seç.") });

        if (r.Cpu.Estimated || r.Gpu.Estimated)
            a.Add(new Advice { Level = 0, Title = L.T("Tahmini puan"), Detail = L.T("Bazı parçalar veritabanında birebir yok; puan model adından tahmin edildi.") });

        if (a.All(x => x.Level == 0))
            a.Insert(0, new Advice { Level = 0, Title = L.T("Yükseltme gerekmiyor"), Detail = L.T("Sistemin dengeli ve güncel oyunlar için güçlü. Şu an parça değiştirmene gerek yok.") });
    }
}
