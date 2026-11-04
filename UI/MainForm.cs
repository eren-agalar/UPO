using FpsAtolyesi.Pages;

namespace FpsAtolyesi.UI;

/// <summary>Uygulama genelinde paylaşılan son tarama sonucu.</summary>
public static class AppState
{
    public static ScanResult Last;
    public static SysInfo Sys;
    public static event Action<ScanResult> Scanned;

    public static void SetScan(ScanResult r)
    {
        Last = r;
        var items = r.Items.Where(i => !i.Info).ToDictionary(i => i.Id, i => i.Ok);
        if (State.Data.BeforeScore == null)
        {
            State.Data.BeforeScore = r.Score; State.Data.BeforeTime = r.Time; State.Data.BeforeItems = items;
        }
        State.Data.LastScore = r.Score; State.Data.LastTime = r.Time; State.Data.LastItems = items;
        State.Save();
        Scanned?.Invoke(r);
    }

    public static async Task Scan()
    {
        Log.W(L.T("Sistem taranıyor… (hiçbir ayar değiştirilmez)"));
        var r = await Task.Run(() => Scanner.Run(Sys));
        SetScan(r);
        Log.Ok(L.F("[OK] Tarama bitti · Skor {0}/100 · {1} eksik bulundu", r.Score, r.Missing));
    }
}

public class NavItem : HControl
{
    public readonly string Num, Title;
    bool selected, hover;
    public bool Selected { get => selected; set { selected = value; Invalidate(); } }
    public NavItem(string num, string title) { Num = num; Title = title; Height = P(48); Cursor = Cursors.Hand; }
    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        g.Clear(selected ? Card2 : hover ? Mix(Card, Card2, .5f) : Card);
        if (selected) using (var b = new SolidBrush(Yellow)) g.FillRectangle(b, 0, 0, P(4), Height);
        var nb = new Rectangle(P(18), (Height - P(28)) / 2, P(34), P(28));
        using (var path = G.Cut(nb, P(6)))
        {
            if (selected) { using var b = new SolidBrush(Yellow); g.FillPath(b, path); }
            else { using var p = new Pen(Line, P(1.2f)); g.DrawPath(p, path); }
        }
        G.Text(g, Num, FSmallBold, selected ? Ink : Muted, nb, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        G.Text(g, Title, selected ? FBold : FBody, selected ? Fg : Mix(Muted, Fg, .4f), new Rectangle(P(64), 0, Width - P(70), Height),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

public class HeaderBar : HControl
{
    public string Hw1 = L.T("Donanım okunuyor…"), Hw2 = "";
    public int Score = -1;
    public HeaderBar() { Height = P(76); BackColor = Card; }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        g.Clear(Card);
        using (var lb = new LinearGradientBrush(new Rectangle(0, Height - P(2), Width, P(2)), Yellow, Alpha(Cyan, 0), 0f))
            g.FillRectangle(lb, 0, Height - P(2), Width, P(2));

        int x = P(24);
        var t1 = "UPO";
        var s1 = G.Measure(t1, FTitle);
        G.Text(g, t1, FTitle, Yellow, new Rectangle(x, 0, s1.Width + P(4), Height), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        x += s1.Width + P(14);
        var t2 = "ULTIMATE PERFORMANCE OPTIMIZER";
        var s2 = G.Measure(t2, FSmallBold);
        var b2 = new Rectangle(x, (Height - P(24)) / 2, s2.Width + P(20), P(24));
        using (var path = G.Cut(b2, P(6))) using (var b = new SolidBrush(Yellow)) g.FillPath(b, path);
        G.Text(g, t2, FSmallBold, Ink, b2, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        x = b2.Right + P(8);
        var vText = "v" + MainForm.AppVersion;
        var b3 = new Rectangle(x, b2.Y, G.Measure(vText, FSmallBold).Width + P(16), P(24));
        using (var p = new Pen(Cyan)) g.DrawRectangle(p, b3);
        G.Text(g, vText, FSmallBold, Cyan, b3, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        // Skor çipi
        int cw = P(120);
        var chip = new Rectangle(Width - cw - P(22), (Height - P(48)) / 2, cw, P(48));
        var sc = Score < 0 ? Muted : ScoreColor(Score);
        using (var path = G.Cut(chip, P(9)))
        {
            using var b = new SolidBrush(Alpha(sc, 22)); g.FillPath(b, path);
            using var p = new Pen(sc, P(1.3f)); g.DrawPath(p, path);
        }
        G.Text(g, L.T("SKOR"), FTiny, Muted, new Rectangle(chip.X + P(12), chip.Y, P(40), chip.Height), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        G.Text(g, Score < 0 ? "--" : Score.ToString(), FNum, sc, new Rectangle(chip.X, chip.Y, chip.Width - P(12), chip.Height), TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

        int hx = b3.Right + P(24), hr = chip.Left - P(20);
        G.Text(g, Hw1, FSmallBold, Fg, new Rectangle(hx, P(18), hr - hx, P(20)), TextFormatFlags.Right | TextFormatFlags.EndEllipsis);
        G.Text(g, Hw2, FSmall, Muted, new Rectangle(hx, P(40), hr - hx, P(20)), TextFormatFlags.Right | TextFormatFlags.EndEllipsis);
    }
}

public class LogTitle : HControl
{
    readonly DateTime start = DateTime.Now;
    readonly System.Windows.Forms.Timer t = new() { Interval = 1000 };
    bool blink;
    public LogTitle() { Height = P(28); t.Tick += (_, _) => { blink = !blink; Invalidate(); }; t.Start(); }
    protected override void Dispose(bool d) { if (d) t.Dispose(); base.Dispose(d); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        using (var b = new SolidBrush(blink ? Red : Alpha(Red, 90))) g.FillEllipse(b, P(14), (Height - P(8)) / 2, P(8), P(8));
        G.Text(g, L.T("CANLI KAYIT"), FTiny, Fg, new Rectangle(P(28), 0, P(200), Height));
        var el = DateTime.Now - start;
        G.Text(g, $"REC {(int)el.TotalHours:00}:{el.Minutes:00}:{el.Seconds:00}", FTiny, Cyan, new Rectangle(0, 0, Width - P(14), Height), TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }
}

public class StatusStrip2 : HControl
{
    string busy;
    int dots;
    readonly System.Windows.Forms.Timer t = new() { Interval = 350 };
    public string Busy { get => busy; set { busy = value; if (value != null) t.Start(); else t.Stop(); Invalidate(); } }
    public StatusStrip2() { Height = P(28); t.Tick += (_, _) => { dots = (dots + 1) % 4; Invalidate(); }; }
    protected override void Dispose(bool d) { if (d) t.Dispose(); base.Dispose(d); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Card);
        using (var p = new Pen(Line)) g.DrawLine(p, 0, 0, Width, 0);
        G.Text(g, (Settings.S.AutoUpdate && Updater.Configured ? L.F("Sıfır veri toplama · Yalnızca güncelleme kontrolü · Her değişiklik yedeklenir ({0} kayıt)", Backup.Entries.Count) : L.F("Sıfır veri toplama · İnternete bağlanmaz · Her değişiklik yedeklenir ({0} kayıt)", Backup.Entries.Count)),
            FSmall, Muted, new Rectangle(P(16), 0, Width / 2 + P(120), Height));
        if (busy != null)
            G.Text(g, busy + new string('.', dots), FSmallBold, Amber, new Rectangle(0, 0, Width - P(16), Height), TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        else
            G.Text(g, L.T("Hazır"), FSmallBold, Cyan, new Rectangle(0, 0, Width - P(16), Height), TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }
}

public class MainForm : Form
{
    public static MainForm I;
    public const string Designer = "Eren Ağalar";
    /// <summary>Komut satırı: --page N (1-10) veya bir .upobackup dosyası.</summary>
    public static string[] StartArgs = Array.Empty<string>();
    public static readonly string AppVersion = Updater.CurrentVersion;
    public bool Busy { get; private set; }
    public event Action BusyChanged;
    string busyLabel;

    readonly HeaderBar header = new();
    readonly StatusStrip2 status = new();
    readonly RichTextBox log = new();
    readonly Panel body = new();
    readonly List<NavItem> navs = new();
    readonly PageBase[] pages;
    PageBase current;

    public MainForm()
    {
        I = this;
        Text = "UPO · Ultimate Performance Optimizer";
        AutoScaleMode = AutoScaleMode.None;
        BackColor = Bg; ForeColor = Fg; Font = FBody;
        StartPosition = FormStartPosition.CenterScreen;
        var wa = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1040);
        Size = new Size(Math.Min(P(1260), wa.Width), Math.Min(P(840), wa.Height));
        MinimumSize = new Size(Math.Min(P(1000), wa.Width), Math.Min(P(660), wa.Height));
        if (P(1260) > wa.Width || P(840) > wa.Height) WindowState = FormWindowState.Maximized;
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        pages = new PageBase[]
        {
            new ScanPage(), new OptimizePage(), new StartupPage(), new CleanPage(),
            new RepairPage(), new ComparePage(), new VbsPage(), new RestorePage(), new HardwarePage(), new SettingsPage(),
        };
        string[] names = { L.T("Sistem Taraması"), L.T("Tek Tık Optimizasyon"), L.T("Başlangıç Uygulamaları"), L.T("Temizlik Merkezi"),
                           L.T("Windows Onarımı"), L.T("Önce / Sonra"), L.T("Bellek Bütünlüğü (VBS)"), L.T("Tümünü Geri Al"), L.T("Donanım Testi"), L.T("Ayarlar") };

        // Kenar çubuğu
        var side = new Panel { BackColor = Card, Width = P(250), Padding = new Padding(0, P(14), 0, P(10)) };
        var sideFoot = new TextBlock(L.T("Tüm işlemler yalnızca bu bilgisayarda yapılır. Hiçbir veri toplanmaz veya gönderilmez."), FSmall);
        var navHost = new Panel { BackColor = Card };
        for (int i = 0; i < pages.Length; i++)
        {
            var n = new NavItem((i + 1).ToString("00"), names[i]);
            int idx = i;
            n.Click += (_, _) => Show(idx);
            navs.Add(n);
        }
        for (int i = navs.Count - 1; i >= 0; i--) { navs[i].Dock = DockStyle.Top; navHost.Controls.Add(navs[i]); }
        sideFoot.Margin = Padding.Empty;
        var footWrap = new Panel { BackColor = Card, Height = P(70), Padding = new Padding(P(18), P(8), P(14), 0) };
        footWrap.Controls.Add(sideFoot); sideFoot.Dock = DockStyle.Fill;
        PageBase.Compose(side, navHost, (footWrap, DockStyle.Bottom));
        // Ekran kısaysa alttaki açıklama menü öğelerinin üstüne binmesin.
        side.Resize += (_, _) => footWrap.Visible = side.ClientSize.Height - footWrap.Height >= navs.Count * navs[0].Height + side.Padding.Vertical + P(8);

        // Canlı kayıt
        logWrap = new Panel { BackColor = Card, Height = P(150), Padding = new Padding(P(14), 0, P(6), P(6)) };
        log.BackColor = Color.FromArgb(8, 10, 14); log.ForeColor = Cyan; log.BorderStyle = BorderStyle.None; log.ReadOnly = true;
        log.Font = FMono; log.DetectUrls = false; log.ScrollBars = RichTextBoxScrollBars.Vertical; log.HideSelection = false;
        var logTitle = new LogTitle();
        var logInner = new Panel { BackColor = log.BackColor, Padding = new Padding(P(10), P(6), P(4), P(4)) };
        logInner.Controls.Add(log); log.Dock = DockStyle.Fill;
        PageBase.Compose(logWrap, logInner, (logTitle, DockStyle.Top));

        body.BackColor = Bg;
        sep = new Panel { BackColor = Line, Height = 1 };
        PageBase.Compose(this, body, (header, DockStyle.Top), (status, DockStyle.Bottom), (logWrap, DockStyle.Bottom), (sep, DockStyle.Bottom), (side, DockStyle.Left));

        Log.Line += OnLog;
        SetLogVisible(Settings.S.ShowLog);
        Show(0);
    }

    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); Native.DarkTitleBar(Handle); Native.DarkScroll(log.Handle); }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Log.W(L.F("UPO · Ultimate Performance Optimizer v{0} başlatıldı.", AppVersion));
        // Güncellemeden sonraki ilk açılışta haber ver.
        if (State.Data.LastVersion != AppVersion)
        {
            if (State.Data.LastVersion != null && UpdateManifest.IsNewer(AppVersion, State.Data.LastVersion))
                Log.Ok(L.F("[OK] UPO v{0} sürümüne güncellendi (önceki: v{1}).", AppVersion, State.Data.LastVersion));
            State.Data.LastVersion = AppVersion; State.Save();
        }
        Log.Dim(L.F("Yedek klasörü: {0}", Paths.Data));
        if (Backup.Entries.Count > 0) Log.Dim(L.F("Önceki oturumlardan {0} yedek kaydı var (Modül 08 ile geri alınabilir).", Backup.Entries.Count));
        AppState.Scanned += r => { header.Score = r.Score; header.Invalidate(); status.Invalidate(); };
        await RunBusy(L.T("Donanım okunuyor"), async () =>
        {
            AppState.Sys = await Task.Run(SysInfo.Load);
            var s = AppState.Sys;
            header.Hw1 = $"{s.Cpu}  |  {s.Gpu}";
            header.Hw2 = $"{s.Os}  ·  {s.RamText}";
            header.Invalidate();
            Log.Dim($"{s.Cpu} | {s.Gpu} | {s.RamText} | {s.Os}");
        });
        if (Settings.S.AutoScan) await RunBusy(L.T("Sistem taranıyor"), AppState.Scan);
        else Log.Dim(L.T("Açılışta otomatik tarama kapalı (Ayarlar). Taramak için Modül 01."));

        // Komut satırı: kaldırıcı "--page 8" ile açar; .upobackup dosyasına çift tıklanınca dosya yolu gelir.
        int pi = Array.IndexOf(StartArgs, "--page");
        if (pi >= 0 && pi + 1 < StartArgs.Length && int.TryParse(StartArgs[pi + 1], out var page) && page >= 1 && page <= pages.Length) Show(page - 1);
        var file = StartArgs.FirstOrDefault(a => a.EndsWith(".upobackup", StringComparison.OrdinalIgnoreCase) && File.Exists(a));
        if (Settings.S.AutoUpdate) _ = CheckForUpdates(false);
        if (file != null && pages.OfType<SettingsPage>().FirstOrDefault() is SettingsPage sp)
        {
            Show(Array.IndexOf(pages, sp));
            sp.ImportFile(file);
        }
    }

    void OnLog(string s, LogKind k)
    {
        if (!IsHandleCreated || IsDisposed) return;
        if (InvokeRequired) { BeginInvoke(() => OnLog(s, k)); return; }
        if (log.TextLength > 300_000) { log.Select(0, 150_000); log.SelectedText = ""; }
        log.SelectionStart = log.TextLength;
        log.SelectionLength = 0;
        log.SelectionColor = Muted;
        log.AppendText($"[{DateTime.Now:HH:mm:ss}] ");
        log.SelectionColor = k switch { LogKind.Ok => Cyan, LogKind.Warn => Amber, LogKind.Error => Red, LogKind.Dim => Muted, _ => Fg };
        log.AppendText(s + "\n");
        log.ScrollToCaret();
        status.Invalidate();
    }

    public void Show(int idx)
    {
        for (int i = 0; i < navs.Count; i++) navs[i].Selected = i == idx;
        var p = pages[idx];
        if (current == p) return;
        body.SuspendLayout();
        if (current != null) body.Controls.Remove(current);
        current = p;
        p.Dock = DockStyle.Fill;
        body.Controls.Add(p);
        body.ResumeLayout();
        p.OnShow();
    }

    public async Task<bool> RunBusy(string label, Func<Task> work)
    {
        if (Busy)
        {
            MessageBox.Show(this, L.F("Şu anda başka bir işlem sürüyor: {0}.\nLütfen bitmesini bekleyin.", busyLabel), "UPO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
        Busy = true; busyLabel = label; status.Busy = label; BusyChanged?.Invoke();
        try { await work(); return true; }
        catch (OperationCanceledException) { Log.Warn(L.T("İşlem durduruldu.")); return false; }
        catch (Exception ex)
        {
            Log.Err(L.T("Hata: ") + ex.Message);
            MessageBox.Show(this, ex.Message, "UPO · Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        finally { Busy = false; busyLabel = null; status.Busy = null; BusyChanged?.Invoke(); }
    }

    public bool Confirm(string msg, string title = null, bool warn = false) =>
        MessageBox.Show(this, msg, "UPO · " + (title ?? L.T("Onay")), MessageBoxButtons.YesNo, warn ? MessageBoxIcon.Warning : MessageBoxIcon.Question) == DialogResult.Yes;

    bool updateDialogOpen;

    /// <summary>Güncelleme kontrolü. manual=false ise sessizdir (hata ve "güncel" mesajı göstermez).</summary>
    public async Task CheckForUpdates(bool manual)
    {
        if (!Updater.Configured)
        {
            if (manual) MessageBox.Show(this, L.T("Güncelleme sunucusu henüz ayarlanmamış."), "UPO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        try
        {
            if (manual) Log.W(L.T("Güncellemeler kontrol ediliyor…"));
            var m = await Updater.Check();
            Settings.S.LastUpdateCheck = DateTime.Now; Settings.Save();
            if (m == null)
            {
                if (manual) { Log.Ok(L.F("[OK] UPO güncel (v{0}).", Updater.CurrentVersion)); MessageBox.Show(this, L.F("UPO güncel (v{0}).", Updater.CurrentVersion), "UPO"); }
                return;
            }
            Log.Ok(L.F("[OK] Yeni sürüm bulundu: v{0}", m.Version));
            if (!manual && m.Version == Settings.S.SkipVersion) { Log.Dim(L.F("v{0} atlanacak sürüm olarak işaretli.", m.Version)); return; }
            // Bir işlem sürüyorsa bitmesini bekle, sonra göster.
            while (Busy && !manual) await Task.Delay(2000);
            if (updateDialogOpen) return;
            updateDialogOpen = true;
            try { using var d = new UpdateDialog(m); d.ShowDialog(this); }
            finally { updateDialogOpen = false; }
        }
        catch (Exception ex)
        {
            Log.Dim(L.T("Güncelleme kontrolü yapılamadı: ") + ex.Message);
            if (manual) MessageBox.Show(this, L.T("Güncelleme kontrolü yapılamadı: ") + ex.Message, "UPO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    Panel logWrap, sep;
    public void SetLogVisible(bool on) { logWrap.Visible = on; sep.Visible = on; }

    public void AskRestart(string why)
    {
        if (Settings.S.Restart == "never")
        {
            Log.Warn("[!] " + why + L.T(" Uygun olduğunuzda bilgisayarı yeniden başlatın."));
            return;
        }
        // Varsayılan seçenek "Hayır": başka bir yerde yazarken basılan Enter bilgisayarı yeniden başlatmasın.
        if (MessageBox.Show(this, why + L.T("\n\nBilgisayar şimdi yeniden başlatılsın mı? (Açık işlerinizi kaydedin.)\n\nDaha sonra kendiniz de yeniden başlatabilirsiniz."),
                L.T("UPO · Yeniden başlatma"), MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
        {
            Log.Dim(L.T("Yeniden başlatma ertelendi. Uygun olduğunuzda bilgisayarı yeniden başlatın."));
            return;
        }
        ProcRunner.Capture("shutdown.exe", "/r /t 60 /c \"UPO: 60 saniye icinde yeniden baslatilacak. Iptal etmek icin uygulamadaki Iptal dugmesine basin.\"");
        Log.Warn(L.T("[!] Bilgisayar 60 saniye içinde yeniden başlatılacak."));
        // Son bir şans: varsayılan seçenek "İptal" olduğundan yanlışlıkla basılan Enter yeniden başlatmayı durdurur.
        if (MessageBox.Show(this, L.T("Bilgisayar 60 saniye içinde yeniden başlatılacak.\n\nİşlerinizi kaydettiyseniz Tamam'a basın.\nVazgeçmek için İptal'e basın."),
                L.T("UPO · Yeniden başlatma"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.OK)
        {
            ProcRunner.Capture("shutdown.exe", "/a");
            Log.Ok(L.T("[OK] Yeniden başlatma iptal edildi."));
        }
    }
}

/// <summary>"EVET" yazarak onaylama penceresi (riskli işlemler için).</summary>
public class TypeConfirm : Form
{
    public TypeConfirm(string title, string message)
    {
        Text = "UPO · " + title;
        AutoScaleMode = AutoScaleMode.None;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = MinimizeBox = false; ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent; BackColor = Card; ForeColor = Fg; Font = FBody;
        ClientSize = new Size(P(520), P(300));
        var msg = new TextBlock(message, FBody, Fg);
        var hint = new TextBlock(L.T("Devam etmek için aşağıya EVET yazın:"), FSmallBold, Yellow);
        var tb = new TextBox { BackColor = Bg, ForeColor = Fg, BorderStyle = BorderStyle.FixedSingle, Font = FHead, CharacterCasing = CharacterCasing.Upper };
        var ok = new HudButton(L.T("ONAYLA")) { Enabled = false };
        var cancel = new HudButton(L.T("VAZGEÇ"), false);
        tb.TextChanged += (_, _) => ok.Enabled = tb.Text.Trim() == L.T("EVET");
        ok.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        var row = new FlowLayoutPanel { Height = P(46), BackColor = Card, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
        row.Controls.Add(cancel); row.Controls.Add(ok);
        Padding = new Padding(P(20));
        PageBase.Compose(this, null, (msg, DockStyle.Top), (PageBase.Gap(12), DockStyle.Top), (hint, DockStyle.Top), (PageBase.Gap(6), DockStyle.Top), (tb, DockStyle.Top), (row, DockStyle.Bottom));
        AcceptButton = null;
        tb.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter && ok.Enabled) { DialogResult = DialogResult.OK; Close(); } };
    }
    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); Native.DarkTitleBar(Handle); }
}
