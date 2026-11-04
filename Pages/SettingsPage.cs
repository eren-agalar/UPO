namespace FpsAtolyesi.Pages;

/// <summary>Başlık + açıklama solda, seçenek düğmeleri sağda.</summary>
public class ChoiceRow : HControl
{
    readonly string title, sub;
    readonly string[] options;
    readonly Color[] swatches;
    int selected, hover = -1;
    Rectangle[] rects = Array.Empty<Rectangle>();
    public event Action<int> Changed;
    public int Selected { get => selected; set { selected = value; Invalidate(); } }

    public ChoiceRow(string title, string sub, string[] options, int selected, Color[] swatches = null)
    {
        this.title = title; this.sub = sub; this.options = options; this.selected = Math.Max(0, selected); this.swatches = swatches;
        Height = P(62); Cursor = Cursors.Hand;
    }

    void Layout2()
    {
        rects = new Rectangle[options.Length];
        int x = Width - P(14), h = P(30), y = (Height - h) / 2;
        for (int i = options.Length - 1; i >= 0; i--)
        {
            int w = G.Measure(options[i], FTiny).Width + P(swatches != null ? 40 : 26);
            x -= w;
            rects[i] = new Rectangle(x, y, w, h);
            x -= P(6);
        }
    }

    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); Layout2(); }
    protected override void OnMouseMove(MouseEventArgs e) { int h = Array.FindIndex(rects, r => r.Contains(e.Location)); if (h != hover) { hover = h; Invalidate(); } base.OnMouseMove(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = -1; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        int i = Array.FindIndex(rects, r => r.Contains(e.Location));
        if (i >= 0 && i != selected) { selected = i; Invalidate(); Changed?.Invoke(i); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        g.Clear(Card);
        using (var lp = new Pen(Line)) g.DrawLine(lp, P(10), Height - 1, Width - P(10), Height - 1);
        if (rects.Length != options.Length) Layout2();
        int right = (rects.Length > 0 ? rects[0].Left : Width) - P(14);
        G.Text(g, title, FBold, Fg, new Rectangle(P(14), P(10), right - P(14), P(20)), TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
        G.Text(g, sub, FSmall, Muted, new Rectangle(P(14), P(32), right - P(14), P(18)), TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
        for (int i = 0; i < options.Length; i++)
        {
            var r = rects[i];
            bool sel = i == selected;
            using (var path = G.Cut(r, P(6)))
            {
                using var b = new SolidBrush(sel ? Yellow : i == hover ? Card2 : Card);
                g.FillPath(b, path);
                using var p = new Pen(sel ? Yellow : Line, P(1.2f));
                g.DrawPath(p, path);
            }
            int tx = r.X;
            if (swatches != null)
            {
                int d = P(12);
                using var sb = new SolidBrush(swatches[i]);
                g.FillEllipse(sb, r.X + P(10), r.Y + (r.Height - d) / 2, d, d);
                if (sel) using (var op = new Pen(Ink, P(1.5f))) g.DrawEllipse(op, r.X + P(10), r.Y + (r.Height - d) / 2, d, d);
                tx += P(16);
            }
            G.Text(g, options[i], FTiny, sel ? Ink : Fg, new Rectangle(tx, r.Y, r.Right - tx, r.Height), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}

// ───────────────────────────── 10 · Ayarlar ─────────────────────────────
public class SettingsPage : PageBase
{
    readonly HudPanel restartBanner;
    readonly HudPanel whitelistCard;
    readonly Panel host = new() { BackColor = Bg };
    readonly string startAccent = Settings.S.Accent, startLang = Settings.S.Lang;
    bool loaded;

    static AppSettings S => Settings.S;

    public SettingsPage()
    {
        var header = new PageHeader("10", L.T("Ayarlar"), L.T("Ayarlar"),
            L.T("UPO'yu kendine göre ayarla. Değişiklikler anında kaydedilir; renk ve dil uygulama yeniden açılınca geçerli olur."));

        // Yeniden açma uyarısı
        var reopen = Btn(L.T("ŞİMDİ YENİDEN AÇ"), true, () => Program.Restart(), false);
        restartBanner = new HudPanel { Border = Yellow, Visible = false, Padding = new Padding(P(16), P(10), P(16), P(10)) };
        Compose(restartBanner, new TextBlock(L.T("Renk veya dil değişikliğinin geçerli olması için UPO'nun yeniden açılması gerekiyor."), FBold, Fg),
            (ButtonRow(reopen), DockStyle.Right));
        restartBanner.Height = P(66);

        // GENEL VE GÖRÜNÜM
        var autoScan = Switch(L.T("Açılışta otomatik tarama"), L.T("UPO açılınca sistemi tarayıp skoru gösterir (hiçbir ayarı değiştirmez)."), S.AutoScan, v => S.AutoScan = v);
        var accentIdx = Array.FindIndex(Accents, a => a.id == S.Accent);
        var accent = new ChoiceRow(L.T("Vurgu rengi"), L.T("Arayüzün ana rengi. Yeniden açınca geçerli olur."),
            Accents.Select(a => L.T(a.name)).ToArray(), accentIdx, Accents.Select(a => a.c).ToArray());
        accent.Changed += i => { S.Accent = Accents[i].id; Settings.Save(); CheckRestart(); };
        var showLog = Switch(L.T("Canlı kayıt panelini göster"), L.T("Alttaki kayıt paneli. Gizlersen sayfalara daha fazla yer kalır."), S.ShowLog, v => { S.ShowLog = v; Main.SetLogVisible(v); });
        var lang = new ChoiceRow("Dil / Language", L.T("Arayüz dili. Yeniden açınca geçerli olur."), new[] { L.T("TÜRKÇE"), "ENGLISH" }, S.Lang == "en" ? 1 : 0);
        lang.Changed += i => { S.Lang = i == 1 ? "en" : "tr"; Settings.Save(); CheckRestart(); };

        // GÜVENLİK VE YEDEK
        var autoUpd = Switch(L.T("Güncellemeleri otomatik kontrol et"), L.T("UPO açılınca GitHub'dan yeni sürüm var mı diye bakar. Hiçbir kişisel veya donanım bilgisi gönderilmez."), S.AutoUpdate, v => S.AutoUpdate = v);
        var upd = ActionRow(L.F("Sürüm v{0}", Updater.CurrentVersion),
            S.LastUpdateCheck is DateTime lc ? L.F("Son kontrol: {0:dd.MM.yyyy HH:mm}", lc) : L.T("Henüz kontrol edilmedi."),
            Btn(L.T("ŞİMDİ KONTROL ET"), false, () => _ = Main.CheckForUpdates(true), false));
        var rp = Switch(L.T("Değişiklikten önce geri yükleme noktası oluştur"), L.T("İlk ayar değişikliğinden önce Windows sistem geri yükleme noktası oluşturmayı dener."), S.RestorePoint, v => S.RestorePoint = v);
        var restart = new ChoiceRow(L.T("Yeniden başlatma"), L.T("Bazı işlemlerden sonra yeniden başlatma gerekir. UPO sorsun mu?"), new[] { L.T("SOR"), L.T("HİÇ SORMA") }, S.Restart == "never" ? 1 : 0);
        restart.Changed += i => { S.Restart = i == 1 ? "never" : "ask"; Settings.Save(); };
        var exp = ActionRow(L.T("Yedeği dışa / içe aktar"), L.T("Yedeği dosyaya kaydet; format sonrası veya başka bilgisayarda Modül 08 ile geri al."),
            Btn(L.T("DIŞA AKTAR"), false, ExportBackup), Btn(L.T("İÇE AKTAR"), false, ImportBackup));
        var folder = ActionRow(L.T("Uygulama verileri"), Paths.Data,
            Btn(L.T("KLASÖRÜ AÇ"), false, () => { try { Process.Start("explorer.exe", Paths.Data); } catch { } }, false), Btn(L.T("VERİLERİ SIFIRLA"), false, ResetData));

        // MODÜLLER
        var prof = new ChoiceRow(L.T("Tek Tık varsayılan profili"), L.T("02 · Tek Tık Optimizasyon sayfası bu profille açılır."),
            new[] { L.T("DENGELİ"), L.T("REKABETÇİ"), L.T("ÖZEL") }, (int)S.DefaultProfile);
        prof.Changed += i => { S.DefaultProfile = (Profile)i; Settings.Save(); };
        var bench = new ChoiceRow(L.T("Donanım testi süresi"), L.T("Uzun test, ısınma kaynaklı yavaşlamayı daha iyi yakalar."),
            new[] { L.T("KISA ~20 SN"), L.T("NORMAL ~40 SN"), L.T("UZUN ~80 SN") }, S.BenchLength == "short" ? 0 : S.BenchLength == "long" ? 2 : 1);
        bench.Changed += i => { S.BenchLength = i == 0 ? "short" : i == 2 ? "long" : "normal"; Settings.Save(); };
        var report = new ChoiceRow(L.T("HTML rapor kayıt yeri"), L.T("01 · Sistem Taraması raporunun kaydedileceği klasör."), new[] { L.T("MASAÜSTÜ"), L.T("BELGELER") }, S.ReportDir == "documents" ? 1 : 0);
        report.Changed += i => { S.ReportDir = i == 1 ? "documents" : "desktop"; Settings.Save(); };

        // ASLA DOKUNMA LİSTESİ
        var neverRows = Tweaks.All.Select(t =>
        {
            var r = new RowItem(t.Title, t.Desc, RowMode.Check) { Checked = S.NeverTouch.Contains(t.Id), Data = t.Id };
            r.Toggled += (_, _) =>
            {
                S.NeverTouch.Remove(t.Id);
                if (r.Checked) S.NeverTouch.Add(t.Id);
                Settings.Save();
                Log.Dim(r.Checked ? L.F("  Dokunma listesine eklendi: {0}", t.Title) : L.F("  Dokunma listesinden çıkarıldı: {0}", t.Title));
            };
            return (Control)r;
        }).ToArray();

        whitelistCard = Section(L.T("Başlangıç beyaz listesi"), new Control[] { new RowItem(L.T("Yükleniyor…")) });

        var about = new HudPanel { Padding = new Padding(P(18), P(14), P(18), P(14)) };
        var aboutText = new TextBlock(
            L.F("UPO · Ultimate Performance Optimizer · Sürüm {0}", MainForm.AppVersion) + "\n" +
            L.F("Tasarım: {0}", MainForm.Designer) + "  ·  © 2026" + "\n\n" +
            L.T("Gizlilik: UPO hiçbir veri toplamaz veya göndermez. Tüm işlemler ve sonuçlar yalnızca bu bilgisayarda kalır. İnternete yalnızca güncelleme kontrolü için (GitHub, kapatılabilir) bağlanır. (Windows'un kendi DISM onarımı Windows Update'ten dosya indirebilir.)") + "\n\n" +
            L.T("Güvenlik: Her ayar değişikliğinin orijinal değeri yedeklenir ve 08 · Tümünü Geri Al ile geri alınabilir. Temizlik ve onarım dosya işlemi olduğu için geri alınamaz.") + "\n\n" +
            L.T("Kısa kullanım rehberi:") + "\n" +
            L.T("1) 01 · Sistem Taraması ile skoru gör.  2) 02 · Tek Tık ile eksikleri uygula.  3) 03 · Başlangıç'ta gereksiz uygulamaları kapat.  4) 04 · Temizlik ile önbellekleri boşalt.  5) Sorun varsa 05 · Onarım.  6) 06 · Önce/Sonra ile farkı gör.  7) 09 · Donanım Testi ile parçalarını puanla.  Beğenmezsen 08 · Tümünü Geri Al."),
            FSmall, Fg);
        about.Controls.Add(aboutText); aboutText.Dock = DockStyle.Top;
        about.Height = P(290);

        var scroll = new ScrollPanel();
        Compose(scroll, null,
            (restartBanner, DockStyle.Top), (Gap(10), DockStyle.Top),
            (Section(L.T("Genel ve görünüm"), autoScan, accent, showLog, lang), DockStyle.Top), (Gap(14), DockStyle.Top),
            (Section(L.T("Güncelleme"), autoUpd, upd), DockStyle.Top), (Gap(14), DockStyle.Top),
            (Section(L.T("Güvenlik ve yedek"), rp, restart, exp, folder), DockStyle.Top), (Gap(14), DockStyle.Top),
            (Section(L.T("Modüller"), prof, bench, report), DockStyle.Top), (Gap(14), DockStyle.Top),
            (Section(L.T("Asla dokunma listesi · işaretlenen ayarlar hiçbir profilde uygulanmaz ve skora katılmaz"), neverRows), DockStyle.Top), (Gap(14), DockStyle.Top),
            (whitelistCard, DockStyle.Top), (Gap(14), DockStyle.Top),
            (SectionTitle(L.T("Hakkında")), DockStyle.Top), (about, DockStyle.Top), (Gap(14), DockStyle.Top));
        Compose(this, scroll, (header, DockStyle.Top), (Gap(8), DockStyle.Top));
    }

    class ScrollPanel : Panel
    {
        public ScrollPanel() { AutoScroll = true; BackColor = Bg; }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); Native.DarkScroll(Handle); }
    }

    static Control SectionTitle(string t) => new BgHeader(t);

    class BgHeader : HControl
    {
        public BgHeader(string t) { Text = t; Height = P(30); }
        protected override void OnPaint(PaintEventArgs e) =>
            G.Text(e.Graphics, Fmt.Upper(Text), FTiny, Yellow, new Rectangle(P(4), 0, Width, Height - P(6)), TextFormatFlags.Left | TextFormatFlags.Bottom);
    }

    static HudPanel Section(string title, params Control[] rows)
    {
        var p = new HudPanel { Padding = new Padding(P(4), P(6), P(4), P(8)) };
        var all = new List<Control> { RowItem.Header(title) };
        all.AddRange(rows);
        for (int i = all.Count - 1; i >= 0; i--) { all[i].Dock = DockStyle.Top; p.Controls.Add(all[i]); }
        p.Height = all.Sum(c => c.Height) + P(16);
        return p;
    }

    RowItem Switch(string title, string sub, bool value, Action<bool> set)
    {
        var r = new RowItem(title, sub, RowMode.Switch) { Checked = value };
        r.Toggled += (_, _) => { set(r.Checked); Settings.Save(); };
        return r;
    }

    static Control ActionRow(string title, string sub, params HudButton[] buttons)
    {
        var p = new Panel { Height = P(62), BackColor = Theme.Card };
        var row = ButtonRow(buttons);
        row.Padding = new Padding(0, P(12), P(8), 0);
        Compose(p, new RowItem(title, sub), (row, DockStyle.Right));
        return p;
    }

    void CheckRestart() => restartBanner.Visible = S.Accent != startAccent || S.Lang != startLang;

    public override void OnShow()
    {
        if (loaded) return;
        loaded = true;
        _ = LoadWhitelist();
    }

    async Task LoadWhitelist()
    {
        var entries = await Task.Run(Startup.Load);
        var rows = new List<Control>();
        foreach (var e in entries.OrderBy(e => e.Name))
        {
            var r = new RowItem(e.Name, L.T("İşaretlenirse \"kapatılabilir\" önerilmez ve skoru düşürmez") + " · " + e.Source, RowMode.Check) { Checked = Settings.IsWhitelisted(e.Name) };
            r.Toggled += (_, _) =>
            {
                S.StartupWhitelist.RemoveAll(n => string.Equals(n, e.Name, StringComparison.OrdinalIgnoreCase));
                if (r.Checked) S.StartupWhitelist.Add(e.Name);
                Settings.Save();
            };
            rows.Add(r);
        }
        if (rows.Count == 0) rows.Add(new RowItem(L.T("Başlangıçta çalışan uygulama bulunamadı.")));
        rows.Insert(0, RowItem.Header(L.T("Başlangıç beyaz listesi")));
        whitelistCard.SuspendLayout();
        foreach (var c in whitelistCard.Controls.Cast<Control>().ToList()) c.Dispose();
        whitelistCard.Controls.Clear();
        for (int i = rows.Count - 1; i >= 0; i--) { rows[i].Dock = DockStyle.Top; whitelistCard.Controls.Add(rows[i]); }
        whitelistCard.Height = rows.Sum(c => c.Height) + P(16);
        whitelistCard.ResumeLayout();
    }

    void ExportBackup()
    {
        if (Backup.Entries.Count == 0) { MessageBox.Show(Main, L.T("Dışa aktarılacak yedek yok. Henüz hiçbir ayar değiştirilmemiş."), "UPO"); return; }
        using var d = new SaveFileDialog { Filter = L.T("UPO yedeği") + " (*.upobackup)|*.upobackup", FileName = $"UPO-Yedek-{Environment.MachineName}-{DateTime.Now:yyyyMMdd-HHmm}.upobackup" };
        if (d.ShowDialog(Main) != DialogResult.OK) return;
        try { int n = Backup.Export(d.FileName); Log.Ok(L.F("[OK] {0} yedek kaydı dışa aktarıldı: {1}", n, d.FileName)); }
        catch (Exception ex) { Log.Err("[X] " + ex.Message); MessageBox.Show(Main, ex.Message, "UPO"); }
    }

    void ImportBackup()
    {
        using var d = new OpenFileDialog { Filter = L.T("UPO yedeği") + " (*.upobackup)|*.upobackup|JSON (*.json)|*.json" };
        if (d.ShowDialog(Main) != DialogResult.OK) return;
        ImportFile(d.FileName);
    }

    /// <summary>Yedek dosyasını içeri alır (diyalogdan veya .upobackup dosyasına çift tıklanınca).</summary>
    public void ImportFile(string file)
    {
        if (!Main.Confirm(L.T("Yedek içeri alınacak. Bu işlem hiçbir ayarı hemen değiştirmez; kayıtlar 08 · Tümünü Geri Al listesine eklenir.\n\nNot: Başka bir bilgisayardan alınan yedek, o bilgisayarın orijinal ayarlarını içerir.\n\nDevam edilsin mi?"), L.T("İçe aktar"))) return;
        try
        {
            var (add, skip, machine) = Backup.Import(file);
            Log.Ok(L.F("[OK] Yedek içeri alındı ({0}): {1} kayıt eklendi, {2} kayıt zaten vardı.", machine, add, skip));
            MessageBox.Show(Main, L.F("{0} kayıt eklendi. Geri almak için 08 · Tümünü Geri Al sayfasını kullan.", add), "UPO");
        }
        catch (Exception ex) { Log.Err("[X] " + ex.Message); MessageBox.Show(Main, ex.Message, "UPO", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    void ResetData()
    {
        if (Backup.Entries.Count > 0)
        {
            MessageBox.Show(Main, L.F("Bekleyen {0} yedek kaydı var. Verileri sıfırlarsan bu değişiklikleri geri alamazsın.\n\nÖnce 08 · Tümünü Geri Al ile ayarları eski hâline döndür (veya yedeği dışa aktar).", Backup.Entries.Count),
                "UPO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!Main.Confirm(L.T("Önce/sonra skorları, donanım testi sonuçları ve tüm ayarlar silinecek; UPO yeniden açılacak. Devam edilsin mi?"), L.T("Verileri sıfırla"), true)) return;
        foreach (var f in new[] { "state.json", "settings.json", "hata.log" })
            try { File.Delete(Path.Combine(Paths.Data, f)); } catch { }
        Program.Restart();
    }
}
