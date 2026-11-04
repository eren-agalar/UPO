namespace FpsAtolyesi.Pages;

/// <summary>Bir donanım parçasının puan kartı.</summary>
public class CompCard : HControl
{
    CompScore s;
    public string Title;
    public CompCard(string title) { Title = title; }
    public void Set(CompScore score) { s = score; Invalidate(); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        var r = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var path = G.Cut(r, P(12)))
        {
            using var b = new SolidBrush(Card); g.FillPath(b, path);
            using var p = new Pen(Line); g.DrawPath(p, path);
        }
        var col = s?.Color ?? Muted;
        using (var a = new Pen(col, P(2))) { g.DrawLine(a, 1, 1, P(22), 1); g.DrawLine(a, 1, 1, 1, P(22)); }

        int x = P(18), w = Width - P(36);
        G.Text(g, Title, FTiny, Yellow, new Rectangle(x, P(14), w, P(16)), TextFormatFlags.Left);
        if (s == null) { G.Text(g, L.T("Okunuyor…"), FBody, Muted, new Rectangle(x, P(36), w, P(24))); return; }

        var num = s.Points.ToString();
        var ns = G.Measure(num, FNum);
        G.Text(g, num, FNum, col, new Rectangle(Width - P(18) - ns.Width - P(4), P(8), ns.Width + P(4), P(44)), TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        G.Text(g, s.Model, FHead, Fg, new Rectangle(x, P(32), w - ns.Width - P(12), P(24)), TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        G.Text(g, s.Verdict, FTiny, col, new Rectangle(x, P(58), w, P(16)), TextFormatFlags.Left);

        int seg = P(6), gap = P(3), y = P(80), h = P(10);
        int n = Math.Max(1, (w + gap) / (seg + gap)), on = (int)Math.Round(n * s.Points / 100.0);
        using (var bOn = new SolidBrush(col)) using (var bOff = new SolidBrush(Card2))
            for (int i = 0; i < n; i++) g.FillRectangle(i < on ? bOn : bOff, x + i * (seg + gap), y, seg, h);

        int ly = P(102);
        foreach (var d in s.Details)
        {
            if (ly + P(20) > Height - P(6)) break;
            G.Text(g, "› " + d, FSmall, Muted, new Rectangle(x, ly, w, P(20)), TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            ly += P(21);
        }
    }
}

// ───────────────────────────── 09 · Donanım Testi ─────────────────────────────
public class HardwarePage : PageBase
{
    readonly Gauge gauge = new() { Label = L.T("DONANIM SKORU"), Width = P(200) };
    readonly TextBlock summary = new(L.T("Donanım okunuyor…"), FHead, Fg);
    readonly TextBlock info = new("", FSmall);
    readonly ProgressRow progress = new(L.T("Test bekliyor")) { BarColor = Amber };
    readonly CompCard cpu = new(L.T("İŞLEMCİ")), gpu = new(L.T("EKRAN KARTI")), ram = new(L.T("BELLEK (RAM)")), disk = new(L.T("SİSTEM DİSKİ"));
    readonly Panel adviceHost = new() { BackColor = Bg };
    readonly HudButton stopBtn;
    HwInfo hw;
    CancellationTokenSource cts;
    bool loaded;

    public HardwarePage()
    {
        var header = new PageHeader("09", L.T("Modül 09 · Donanım"), L.T("Donanım Testi"),
            L.T("İşlemci, RAM ve diski gerçek testlerle ölçer; ekran kartıyla birlikte bugünün oyunlarına göre puanlar ve yükseltme gerekip gerekmediğini söyler."));
        var start = Btn(L.T("TESTİ BAŞLAT"), true, StartTest);
        stopBtn = Btn(L.T("DURDUR"), false, () => cts?.Cancel(), false);
        stopBtn.Enabled = false;
        var secs = Settings.BenchSeconds;
        info.Text = L.F("Test yaklaşık {0} saniye sürer; bu sırada ağır bir program çalıştırmayın. Sonuçlar yalnızca bu bilgisayarda kalır. Veritabanı: {1}.", (int)(secs.single + secs.multi + secs.ram + 18), HwDb.Version);

        var right = new Panel { BackColor = Color.Transparent };
        Compose(right, null, (summary, DockStyle.Top), (Gap(4), DockStyle.Top), (info, DockStyle.Top), (Gap(10), DockStyle.Top),
            (progress, DockStyle.Top), (ButtonRow(start, stopBtn), DockStyle.Top));
        var top = Card(right, (gauge, DockStyle.Left), (Gap(20), DockStyle.Left));
        top.Height = P(236);

        var grid = new TableLayoutPanel { ColumnCount = 2, RowCount = 2, Height = P(400), BackColor = Bg, Margin = Padding.Empty, Padding = Padding.Empty };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        int m = P(6);
        foreach (var (c, col, row) in new[] { (gpu, 0, 0), (cpu, 1, 0), (ram, 0, 1), (disk, 1, 1) })
        {
            c.Dock = DockStyle.Fill;
            c.Margin = new Padding(col == 0 ? 0 : m, row == 0 ? 0 : m, col == 0 ? m : 0, row == 0 ? m : 0);
            grid.Controls.Add(c, col, row);
        }

        var scroll = new ScrollHost();
        Compose(scroll, null, (top, DockStyle.Top), (Gap(14), DockStyle.Top), (grid, DockStyle.Top), (Gap(14), DockStyle.Top), (adviceHost, DockStyle.Top), (Gap(10), DockStyle.Top));
        Compose(this, scroll, (header, DockStyle.Top), (Gap(8), DockStyle.Top));
    }

    class ScrollHost : Panel
    {
        public ScrollHost() { AutoScroll = true; BackColor = Bg; }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); Native.DarkScroll(Handle); }
    }

    public override void OnShow()
    {
        if (loaded) return;
        loaded = true;
        _ = Run(L.T("Donanım okunuyor"), async () =>
        {
            hw = await Task.Run(() => HwInfo.Detect(AppState.Sys ?? SysInfo.Load()));
            var last = State.Data.LastBench;
            Render(HwScorer.Score(hw, last));
            if (last != null) progress.Detail = L.F("Son test: {0:dd.MM.yyyy HH:mm}. Yeniden ölçmek için TESTİ BAŞLAT.", last.Time);
            else progress.Detail = L.T("Puanlar şimdilik model bilgisine göre. Gerçek ölçüm için TESTİ BAŞLAT.");
        });
    }

    void Render(HwReport r)
    {
        gauge.SetValue(r.Overall);
        summary.Text = r.Summary;
        cpu.Set(r.Cpu); gpu.Set(r.Gpu); ram.Set(r.Ram); disk.Set(r.Disk);

        var rows = new List<Control> { RowItem.Header(L.T("Öneriler")) };
        foreach (var a in r.Advices.OrderByDescending(a => a.Level))
        {
            var row = new RowItem(a.Title, a.Detail) { Status = a.Level == 2 ? RowStatus.Bad : a.Level == 1 ? RowStatus.Warn : RowStatus.Ok };
            row.SetTag(a.Level == 2 ? L.T("ÖNEMLİ") : a.Level == 1 ? L.T("ÖNERİ") : L.T("BİLGİ"), a.Level == 2 ? Red : a.Level == 1 ? Amber : Cyan);
            rows.Add(row);
        }
        var card = new HudPanel { Padding = new Padding(P(4), P(8), P(4), P(10)) };
        var inner = new ListPanel { AutoScroll = false };
        inner.SetRows(rows);
        card.Controls.Add(inner); inner.Dock = DockStyle.Fill;
        card.Height = rows.Sum(c => c.Height) + P(22);
        adviceHost.SuspendLayout();
        foreach (Control c in adviceHost.Controls.Cast<Control>().ToList()) c.Dispose();
        adviceHost.Controls.Clear();
        adviceHost.Controls.Add(card); card.Dock = DockStyle.Fill;
        adviceHost.Height = card.Height;
        adviceHost.ResumeLayout();
    }

    void StartTest()
    {
        if (hw == null) return;
        cts = new CancellationTokenSource();
        var ct = cts.Token;
        stopBtn.Enabled = true;
        _ = Run(L.T("Donanım test ediliyor"), async () =>
        {
            try
            {
                var b = new BenchResult();
                var (sSec, mSec, rSec) = Settings.BenchSeconds;
                void Stage(string t, double pct) => Main.BeginInvoke(() => { progress.Detail = t; progress.Value = pct; });
                Log.W(L.T("Donanım testi başladı…"));
                progress.Title = L.T("Test sürüyor");

                Stage(L.T("İşlemci · tek çekirdek testi…"), 2);
                b.CpuSingle = await Task.Run(() => Bench.CpuSingle(sSec, ct), ct);
                Log.Dim(L.F("  Tek çekirdek: {0:N0} puan", b.CpuSingle));

                Stage(L.T("İşlemci · çok çekirdek testi (tüm çekirdekler %100)…"), 20);
                (b.CpuMulti, b.Throttle) = await Task.Run(() => Bench.CpuMulti(mSec, f => Stage(L.T("İşlemci · çok çekirdek testi (tüm çekirdekler %100)…"), 20 + f * 35), ct), ct);
                Log.Dim(L.F("  Çok çekirdek: {0:N0} puan · yük altında düşüş %{1:0}", b.CpuMulti, b.Throttle));

                Stage(L.T("Bellek (RAM) hız testi…"), 58);
                b.RamGBs = await Task.Run(() => Bench.RamCopy(rSec, ct), ct);
                Log.Dim($"  RAM: {b.RamGBs:0.0} GB/s");
                GC.Collect();

                Stage(L.T("Disk testi hazırlanıyor…"), 66);
                var (rd, wr, r4, skip) = await Task.Run(() => Bench.Disk(Paths.Data, (s, pct) => Stage(s, pct), ct), ct);
                b.DiskRead = rd; b.DiskWrite = wr; b.Disk4k = r4; b.DiskSkipped = skip;
                Log.Dim(skip ?? $"  Disk: okuma {rd:N0} MB/s · yazma {wr:N0} MB/s · 4K {r4:0.0} MB/s");

                Stage(L.T("Tamamlandı ✓"), 100);
                b.Time = DateTime.Now;
                State.Data.LastBench = b; State.Save();
                var r = HwScorer.Score(hw, b);
                Render(r);
                progress.Title = L.T("Test tamamlandı");
                progress.Detail = L.F("Tamamlandı ✓ · {0:HH:mm}", b.Time);
                Log.Ok(L.F("[OK] Donanım skoru {0}/100 · GPU {1} · CPU {2} · RAM {3} · Disk {4}", r.Overall, r.Gpu.Points, r.Cpu.Points, r.Ram.Points, r.Disk.Points));
            }
            finally
            {
                stopBtn.Enabled = false;
                if (progress.Title == L.T("Test sürüyor")) { progress.Title = L.T("Test durduruldu"); progress.Value = 0; }
            }
        });
    }
}
