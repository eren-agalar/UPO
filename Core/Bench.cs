using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace FpsAtolyesi.Core;

public class BenchResult
{
    public double CpuSingle, CpuMulti, Throttle;    // puan, puan, % düşüş
    public double RamGBs;
    public double DiskRead, DiskWrite, Disk4k;      // MB/s
    public string DiskSkipped;
    public DateTime Time = DateTime.Now;
}

/// <summary>Yerel donanım testleri. Hiçbir sonuç bilgisayardan dışarı çıkmaz.</summary>
public static class Bench
{
    const int Chunk = 1 << 20;

    /// <summary>Tam sayı + kayan nokta karışık iş yükü (oyun mantığına benzer bağımlı işlemler).</summary>
    static ulong Work(int iters, ulong x, ref double d)
    {
        for (int i = 0; i < iters; i++)
        {
            x ^= x << 13; x ^= x >> 7; x ^= x << 17;
            d = d * 0.9999999 + (x & 0x3FF) * 1e-7;
            if ((x & 0xFFF) == 0) d = Math.Sqrt(d + 1);
        }
        return x;
    }

    public static double CpuSingle(double seconds, CancellationToken ct)
    {
        var t = Thread.CurrentThread;
        var old = t.Priority;
        try
        {
            t.Priority = ThreadPriority.Highest;
            double d = 1; ulong x = 88172645463325252UL;
            var w = Stopwatch.StartNew();
            while (w.Elapsed.TotalSeconds < 0.5) x = Work(Chunk, x, ref d); // ısınma (JIT + turbo)
            long chunks = 0;
            w.Restart();
            while (w.Elapsed.TotalSeconds < seconds) { ct.ThrowIfCancellationRequested(); x = Work(Chunk, x, ref d); chunks++; }
            GC.KeepAlive(x + (ulong)d);
            return chunks * (double)Chunk / w.Elapsed.TotalSeconds / 1e5;
        }
        finally { t.Priority = old; }
    }

    /// <summary>Tüm iş parçacıklarında yük; saniye saniye hız ölçülür (ısınma/kısma tespiti).</summary>
    public static (double score, double throttlePct) CpuMulti(double seconds, Action<double> progress, CancellationToken ct)
    {
        int n = Environment.ProcessorCount;
        long total = 0;
        bool stop = false;
        var threads = new Thread[n];
        for (int i = 0; i < n; i++)
        {
            int seed = i + 1;
            threads[i] = new Thread(() =>
            {
                double d = 1; ulong x = 0x9E3779B97F4A7C15UL * (ulong)seed;
                while (!Volatile.Read(ref stop)) { x = Work(Chunk / 4, x, ref d); Interlocked.Increment(ref total); }
                GC.KeepAlive(x + (ulong)d);
            }) { IsBackground = true, Priority = ThreadPriority.AboveNormal };
            threads[i].Start();
        }
        var samples = new List<double>();
        var w = Stopwatch.StartNew();
        long last = 0;
        double lastT = 0;
        try
        {
            Thread.Sleep(500);
            last = Interlocked.Read(ref total); lastT = w.Elapsed.TotalSeconds;
            while (w.Elapsed.TotalSeconds < seconds + 0.5)
            {
                ct.ThrowIfCancellationRequested();
                Thread.Sleep(500);
                long now = Interlocked.Read(ref total); double tt = w.Elapsed.TotalSeconds;
                samples.Add((now - last) / (tt - lastT));
                last = now; lastT = tt;
                progress?.Invoke(Math.Min(1, (tt - 0.5) / seconds));
            }
        }
        finally
        {
            Volatile.Write(ref stop, true);
            foreach (var th in threads) th.Join(2000);
        }
        if (samples.Count < 4) return (0, 0);
        int k = Math.Max(2, samples.Count / 4);
        double first = samples.Take(k).Average(), end = samples.Skip(samples.Count - k).Average();
        double avg = samples.Average();
        double score = avg * (Chunk / 4.0) / 1e5;
        double drop = first > 0 ? Math.Max(0, (first - end) / first * 100) : 0;
        return (score, drop);
    }

    public static double RamCopy(double seconds, CancellationToken ct)
    {
        const int size = 256 << 20;
        var a = GC.AllocateUninitializedArray<byte>(size);
        var b = GC.AllocateUninitializedArray<byte>(size);
        new Random(1).NextBytes(a.AsSpan(0, 1 << 20));
        int parts = Math.Clamp(Environment.ProcessorCount / 2, 1, 8);
        int seg = size / parts;
        long bytes = 0;
        var w = Stopwatch.StartNew();
        while (w.Elapsed.TotalSeconds < seconds)
        {
            ct.ThrowIfCancellationRequested();
            Parallel.For(0, parts, new ParallelOptions { MaxDegreeOfParallelism = parts }, p =>
                a.AsSpan(p * seg, seg).CopyTo(b.AsSpan(p * seg, seg)));
            bytes += size;
        }
        double gbs = bytes / w.Elapsed.TotalSeconds / (1L << 30);
        GC.KeepAlive(a); GC.KeepAlive(b);
        return gbs;
    }

    const FileOptions NoBuffering = (FileOptions)0x20000000;

    /// <summary>Önbelleği atlayarak sıralı okuma/yazma ve 4K rastgele okuma testi. Geçici dosya sonra silinir.</summary>
    public static unsafe (double read, double write, double rand4k, string skipped) Disk(string dir, Action<string, double> stage, CancellationToken ct)
    {
        const int block = 4 << 20;
        long fileSize = 1L << 30;
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(dir));
            if (drive.AvailableFreeSpace < 5L << 30) return (0, 0, 0, L.T("Disk testi atlandı: en az 5 GB boş alan gerekli."));
        }
        catch { }
        var path = Path.Combine(dir, "upo-disktest.tmp");
        byte* buf = (byte*)NativeMemory.AlignedAlloc(block, 4096);
        try
        {
            var span = new Span<byte>(buf, block);
            new Random(7).NextBytes(span); // sıkıştırılamayan veri

            stage?.Invoke(L.T("Disk yazma testi…"), 70);
            double write;
            using (var h = File.OpenHandle(path, FileMode.Create, FileAccess.Write, FileShare.None, NoBuffering | FileOptions.WriteThrough, fileSize))
            {
                var w = Stopwatch.StartNew();
                for (long off = 0; off < fileSize; off += block) { ct.ThrowIfCancellationRequested(); RandomAccess.Write(h, span, off); }
                write = fileSize / w.Elapsed.TotalSeconds / (1 << 20);
            }

            stage?.Invoke(L.T("Disk okuma testi…"), 82);
            double read, r4k;
            using (var h = File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.None, NoBuffering))
            {
                var w = Stopwatch.StartNew();
                for (long off = 0; off < fileSize; off += block) { ct.ThrowIfCancellationRequested(); RandomAccess.Read(h, span, off); }
                read = fileSize / w.Elapsed.TotalSeconds / (1 << 20);

                stage?.Invoke(L.T("Disk 4K rastgele okuma testi…"), 92);
                var rnd = new Random(3);
                var small = new Span<byte>(buf, 4096);
                long blocks = fileSize / 4096, n = 0;
                w.Restart();
                while (w.Elapsed.TotalSeconds < 3) { ct.ThrowIfCancellationRequested(); RandomAccess.Read(h, small, rnd.NextInt64(blocks) * 4096); n++; }
                r4k = n * 4096.0 / w.Elapsed.TotalSeconds / (1 << 20);
            }
            return (read, write, r4k, null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return (0, 0, 0, L.T("Disk testi yapılamadı: ") + ex.Message);
        }
        finally
        {
            NativeMemory.AlignedFree(buf);
            try { File.Delete(path); } catch { }
        }
    }
}
