namespace FpsAtolyesi.Pages;

// ───────────────────────────── 02 · Tek Tık Optimizasyon ─────────────────────────────
public class OptimizePage : PageBase
{
    Profile profile = Profile.Balanced;
    readonly HudButton bBal, bComp, bCustom, applyBtn;
    readonly TextBlock desc = new("", FBody, Fg);
    readonly TextBlock info = new("", FSmall);
    readonly ListPanel list = new();
    readonly List<RowItem> rows = new();
    bool loaded;

    static readonly string[] Descs =
    {
        L.T("DENGELİ · Günlük kullanım + oyun. Güvenlik veya pil ömrünü belirgin şekilde etkilemeyen, en güvenli ayarlar."),
        L.T("REKABETÇİ · CS2, VALORANT, LoL gibi oyunlar için en düşük gecikme. Daha fazla güç tüketir; masaüstü bilgisayarlar için önerilir."),
        L.T("ÖZEL · Ayarları tek tek kendiniz seçin. Listeden istediğiniz satıra tıklayın."),
    };

    public OptimizePage()
    {
        var header = new PageHeader("02", L.T("Modül 02 · Tek tık"), L.T("Tek Tık Optimizasyon"),
            L.T("Bir profil seçin ve eksik ayarları saniyeler içinde uygulayın. Her değişiklik önce yedeklenir; Modül 08 ile tek tuşla geri alınır."));
        bBal = Btn(L.T("DENGELİ"), true, () => SetProfile(Profile.Balanced), false);
        bComp = Btn(L.T("REKABETÇİ"), false, () => SetProfile(Profile.Competitive), false);
        bCustom = Btn(L.T("ÖZEL"), false, () => SetProfile(Profile.Custom), false);
        foreach (var b in new[] { bBal, bComp, bCustom }) { b.Width = P(140); }
        applyBtn = Btn(L.T("SEÇİLENLERİ UYGULA"), true, Apply);

        foreach (var t in Tweaks.All)
        {
            var r = new RowItem(t.Title, t.Desc + (t.NeedsRestart ? L.T("  · Yeniden başlatma gerekir") : ""), RowMode.Check) { Data = t };
            r.SetTag("…", Muted);
            r.Toggled += (_, _) => { if (profile != Profile.Custom) { profile = Profile.Custom; UpdateSeg(); } UpdateInfo(); };
            rows.Add(r);
        }
        list.SetRows(rows.Cast<Control>().ToList());

        var top = Card(null, (ButtonRow(bBal, bComp, bCustom), DockStyle.Top), (Gap(10), DockStyle.Top), (desc, DockStyle.Top));
        top.Height = P(124);
        var listCard = Card(list);
        listCard.Padding = new Padding(P(4), P(8), P(4), P(8));
        var bottom = new Panel { Height = P(56), BackColor = Bg };
        Compose(bottom, info, (Gap(10), DockStyle.Top), (ButtonRow(applyBtn), DockStyle.Left), (Gap(14), DockStyle.Left));
        info.Padding = Padding.Empty;

        Compose(this, listCard, (header, DockStyle.Top), (Gap(8), DockStyle.Top), (top, DockStyle.Top), (Gap(12), DockStyle.Top), (bottom, DockStyle.Bottom), (Gap(6), DockStyle.Bottom));
        AppState.Scanned += sr => { if (loaded) _ = RefreshStatus(); };
        Settings.Changed += () => dirty = true;
        SetProfile(Settings.S.DefaultProfile);
    }

    bool dirty;
    public override void OnShow() { if (!loaded || dirty) { loaded = true; dirty = false; _ = RefreshStatus(); } }

    void SetProfile(Profile p)
    {
        profile = p;
        if (p != Profile.Custom)
            foreach (var r in rows) { var t = (Tweak)r.Data; r.Checked = t.InProfile(p) && r.Enabled; }
        UpdateSeg();
        UpdateInfo();
    }

    void UpdateSeg()
    {
        bBal.Primary = profile == Profile.Balanced; bComp.Primary = profile == Profile.Competitive; bCustom.Primary = profile == Profile.Custom;
        bBal.Invalidate(); bComp.Invalidate(); bCustom.Invalidate();
        desc.Text = Descs[(int)profile];
    }

    void UpdateInfo()
    {
        int sel = rows.Count(r => r.Checked && r.Enabled);
        int todo = rows.Count(r => r.Checked && r.Enabled && !Applied.Contains(((Tweak)r.Data).Id));
        info.Text = L.F("{0} ayar seçili · {1} tanesi uygulanacak · Her değişiklik yedeklenir ve geri alınabilir.", sel, todo);
    }

    readonly HashSet<string> Applied = new();

    async Task RefreshStatus()
    {
        var res = await Task.Run(() => Tweaks.All.Select(t => (t, sup: t.Supported, ok: t.Supported && t.IsApplied())).ToList());
        Applied.Clear();
        foreach (var (t, sup, ok) in res)
        {
            var r = rows.First(x => x.Data == t);
            bool never = Settings.S.NeverTouch.Contains(t.Id);
            r.Enabled = sup && !never;
            if (!sup) { r.SetTag(L.T("DESTEKLENMİYOR"), Muted); r.Checked = false; r.Status = RowStatus.None; continue; }
            if (never) { r.SetTag(L.T("DOKUNULMAZ"), Muted); r.Checked = false; r.Status = RowStatus.None; continue; }
            if (ok) { Applied.Add(t.Id); r.SetTag(L.T("UYGULANDI ✓"), Cyan); r.Status = RowStatus.Ok; }
            else { r.SetTag(L.T("BEKLİYOR"), Amber); r.Status = RowStatus.Warn; }
        }
        if (profile != Profile.Custom) SetProfile(profile); else UpdateInfo();
    }

    void Apply()
    {
        var todo = rows.Where(r => r.Checked && r.Enabled && !Applied.Contains(((Tweak)r.Data).Id)).ToList();
        if (todo.Count == 0)
        {
            MessageBox.Show(Main, L.T("Seçili ayarların hepsi zaten uygulanmış görünüyor."), "UPO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var names = string.Join("\n", todo.Take(12).Select(r => "• " + r.Title)) + (todo.Count > 12 ? L.F("\n… ve {0} ayar daha", todo.Count - 12) : "");
        if (!Main.Confirm(L.F("Şu {0} ayar uygulanacak:\n\n{1}\n\nHer değişikliğin orijinal değeri yedeklenecek. Devam edilsin mi?", todo.Count, names), "Optimizasyon")) return;

        var prof = profile;
        _ = Run(L.T("Optimizasyon uygulanıyor"), async () =>
        {
            if (State.Data.BeforeScore == null) await AppState.Scan();
            await Backup.EnsureRestorePoint();
            Log.W(L.F("Profil: {0} · {1} ayar uygulanıyor…", (prof == Profile.Balanced ? L.T("Dengeli") : prof == Profile.Competitive ? L.T("Rekabetçi") : L.T("Özel")), todo.Count));
            int ok = 0; bool restart = false;
            foreach (var r in todo)
            {
                var t = (Tweak)r.Data;
                r.SetTag(L.T("UYGULANIYOR…"), Amber);
                try
                {
                    await Task.Run(() => t.Apply(prof));
                    r.SetTag(L.T("UYGULANDI ✓"), Cyan); r.Status = RowStatus.Ok; Applied.Add(t.Id);
                    Log.Ok("  [OK] " + t.Title);
                    ok++; restart |= t.NeedsRestart;
                }
                catch (Exception ex) { r.SetTag(L.T("HATA"), Red); r.Status = RowStatus.Bad; Log.Err($"  [X] {t.Title}: {ex.Message}"); }
            }
            Log.Ok(L.F("[OK] {0}/{1} ayar uygulandı. Yeniden tarama yapılıyor…", ok, todo.Count));
            await AppState.Scan();
            if (restart) Main.AskRestart(L.T("Bazı ayarların (ör. GPU zamanlaması) tam olarak geçerli olması için yeniden başlatma gerekiyor."));
        });
    }
}

// ───────────────────────────── 03 · Başlangıç Uygulamaları ─────────────────────────────
public class StartupPage : PageBase
{
    readonly ListPanel list = new();
    readonly TextBlock info = new("", FSmall);
    List<StartupEntry> entries = new();
    bool loaded;

    public StartupPage()
    {
        var header = new PageHeader("03", L.T("Modül 03 · FPS çalan arka plan"), L.T("Başlangıç Uygulamaları"),
            L.T("Bilgisayar açılırken kendiliğinden başlayan ve arka planda kaynak tüketen uygulamalar. Kapatmak uygulamayı silmez; istediğinizde elle açabilirsiniz."));
        var warn = new HudPanel { Height = P(64), Border = Alpha(Amber, 120) };
        Compose(warn, new TextBlock(L.T("Dikkat: Ses, ekran kartı, dokunmatik yüzey, antivirüs ve anti-cheat (Vanguard, FACEIT vb.) girişleri \"DOKUNMAYIN\" olarak işaretlenir. Bunları kapatırsanız oyunlar açılmayabilir."), FSmall, Fg));
        warn.Padding = new Padding(P(16), P(12), P(16), P(8));

        var refresh = Btn(L.T("YENİLE"), false, () => _ = LoadList());
        var rec = Btn(L.T("ÖNERİLENLERİ KAPAT"), true, DisableRecommended);
        var bottom = new Panel { Height = P(56), BackColor = Bg };
        Compose(bottom, info, (Gap(10), DockStyle.Top), (ButtonRow(rec, refresh), DockStyle.Left), (Gap(14), DockStyle.Left));
        var listCard = Card(list);
        listCard.Padding = new Padding(P(4), P(8), P(4), P(8));
        Compose(this, listCard, (header, DockStyle.Top), (Gap(8), DockStyle.Top), (warn, DockStyle.Top), (Gap(12), DockStyle.Top), (bottom, DockStyle.Bottom), (Gap(6), DockStyle.Bottom));
    }

    public override void OnShow() { if (!loaded) { loaded = true; _ = LoadList(); } }

    async Task LoadList()
    {
        entries = await Task.Run(Startup.Load);
        var rows = new List<Control>();
        foreach (var e in entries)
        {
            var r = new RowItem(e.Name, $"{e.Source} · {Short(e.Command)}", RowMode.Switch) { Checked = e.Enabled, Data = e };
            switch (e.Class)
            {
                case StartupClass.Recommended: r.SetTag("KAPATILABİLİR", Amber); break;
                case StartupClass.Protected: r.SetTag("DOKUNMAYIN", Red); break;
                case StartupClass.Whitelisted: r.SetTag("BEYAZ LİSTE", Cyan); break;
            }
            r.Toggled += (_, _) => Toggle(r);
            rows.Add(r);
        }
        if (rows.Count == 0) rows.Add(new RowItem(L.T("Başlangıçta çalışan uygulama bulunamadı."), L.T("Tebrikler, açılışınız tertemiz.")));
        list.SetRows(rows);
        UpdateInfo();
        Log.Dim(L.F("Başlangıç: {0} açık, {1} kapalı uygulama.", entries.Count(x => x.Enabled), entries.Count(x => !x.Enabled)));
    }

    void UpdateInfo()
    {
        int on = entries.Count(x => x.Enabled), rec = entries.Count(x => x.Enabled && x.Class == StartupClass.Recommended);
        info.Text = L.F("{0} uygulama açılışta başlıyor · {1} tanesi güvenle kapatılabilir · Değişiklik bir sonraki açılışta geçerli olur.", on, rec);
    }

    static string Short(string cmd)
    {
        if (string.IsNullOrEmpty(cmd)) return "";
        var m = Regex.Match(cmd, "^\"([^\"]+)\"");
        var path = m.Success ? m.Groups[1].Value : cmd.Split(' ')[0];
        return path.Length > 90 ? "…" + path[^88..] : path;
    }

    void Toggle(RowItem r)
    {
        var e = (StartupEntry)r.Data;
        bool want = r.Checked;
        if (!want && e.Class == StartupClass.Protected &&
            !Main.Confirm(L.F("\"{0}\" bir sürücü, güvenlik veya anti-cheat bileşeni olabilir.\nKapatırsanız ses/görüntü/oyunlar düzgün çalışmayabilir.\n\nYine de kapatılsın mı?", e.Name), "Dikkat", true))
        { r.Checked = true; return; }
        try
        {
            Startup.SetEnabled(e, want);
            Log.Ok($"  {(want ? L.T("[OK] Açıldı") : L.T("[OK] Kapatıldı"))}: {e.Name}");
        }
        catch (Exception ex) { r.Checked = !want; Log.Err($"  [X] {e.Name}: {ex.Message}"); }
        UpdateInfo();
    }

    void DisableRecommended()
    {
        var todo = entries.Where(e => e.Enabled && e.Class == StartupClass.Recommended).ToList();
        if (todo.Count == 0) { MessageBox.Show(Main, L.T("Kapatılması önerilen açık bir başlangıç uygulaması yok."), "UPO"); return; }
        if (!Main.Confirm(L.T("Şu uygulamalar açılışta başlamayacak:\n\n") + string.Join("\n", todo.Select(e => "• " + e.Name)) + "\n\nDevam edilsin mi?", L.T("Başlangıç"))) return;
        foreach (var e in todo)
        {
            try { Startup.SetEnabled(e, false); Log.Ok(L.T("  [OK] Kapatıldı: ") + e.Name); }
            catch (Exception ex) { Log.Err($"  [X] {e.Name}: {ex.Message}"); }
        }
        foreach (RowItem r in list.Controls.OfType<RowItem>()) if (r.Data is StartupEntry se) r.Checked = se.Enabled;
        UpdateInfo();
    }
}

// ───────────────────────────── 04 · Temizlik Merkezi ─────────────────────────────
public class CleanPage : PageBase
{
    readonly ListPanel list = new();
    readonly TextBlock info = new("", FSmall);
    readonly List<(CleanTarget t, RowItem r)> items = new();
    bool loaded;

    public CleanPage()
    {
        var header = new PageHeader("04", L.T("Modül 04 · Kalıntılar"), L.T("Temizlik Merkezi"),
            L.T("Geçici dosyaları, shader önbelleklerini ve eski güncelleme dosyalarını temizler. Kullanımdaki dosyalar atlanır; kişisel dosyalarınıza dokunulmaz."));
        foreach (var t in Cleaner.Targets())
        {
            var r = new RowItem(t.Title, t.Desc, RowMode.Check) { Checked = t.DefaultOn, Data = t };
            r.SetTag(t.Dism ? L.T("UZUN SÜRER") : L.T("HESAPLANIYOR…"), Muted);
            items.Add((t, r));
        }
        list.SetRows(items.Select(i => (Control)i.r).ToList());
        var calc = Btn(L.T("YENİDEN HESAPLA"), false, () => _ = Run(L.T("Boyutlar hesaplanıyor"), Calc));
        var clean = Btn(L.T("SEÇİLENLERİ TEMİZLE"), true, Clean);
        var bottom = new Panel { Height = P(56), BackColor = Bg };
        Compose(bottom, info, (Gap(10), DockStyle.Top), (ButtonRow(clean, calc), DockStyle.Left), (Gap(14), DockStyle.Left));
        var note = new TextBlock(L.T("Silinen dosyalar geri getirilemez; ancak bunlar yalnızca önbellek ve geçici dosyalardır, Windows ve oyunlar gerektiğinde yeniden oluşturur."), FSmall);
        var listCard = Card(list);
        listCard.Padding = new Padding(P(4), P(8), P(4), P(8));
        Compose(this, listCard, (header, DockStyle.Top), (Gap(8), DockStyle.Top), (note, DockStyle.Top), (Gap(10), DockStyle.Top), (bottom, DockStyle.Bottom), (Gap(6), DockStyle.Bottom));
    }

    public override void OnShow() { if (!loaded) { loaded = true; _ = Run(L.T("Boyutlar hesaplanıyor"), Calc); } }

    async Task Calc()
    {
        long total = 0;
        foreach (var (t, r) in items)
        {
            if (t.Dism) continue;
            r.SetTag(L.T("HESAPLANIYOR…"), Muted);
            t.Size = await Task.Run(() => Cleaner.Size(t));
            total += t.Size;
            r.SetTag(Fmt.Size(t.Size), t.Size > 200L << 20 ? Amber : Cyan);
        }
        info.Text = $"Temizlenebilir toplam: {Fmt.Size(total)}";
    }

    void Clean()
    {
        var sel = items.Where(i => i.r.Checked).ToList();
        if (sel.Count == 0) return;
        if (!Main.Confirm(L.T("Seçili öğeler temizlenecek:\n\n") + string.Join("\n", sel.Select(i => "• " + i.t.Title)) +
                          L.T("\n\nBu işlem geri alınamaz (yalnızca geçici/önbellek dosyaları silinir). Devam edilsin mi?"), L.T("Temizlik"))) return;
        _ = Run(L.T("Temizlik yapılıyor"), async () =>
        {
            long freed = 0;
            foreach (var (t, r) in sel)
            {
                r.SetTag(L.T("TEMİZLENİYOR…"), Amber);
                Log.W(L.T("Temizleniyor: ") + t.Title);
                if (t.Dism)
                {
                    int lastPct = -1;
                    var code = await ProcRunner.RunAsync("dism.exe", "/Online /Cleanup-Image /StartComponentCleanup", line =>
                    {
                        var m = Regex.Match(line, @"(\d+(?:[.,]\d+)?)\s*%");
                        if (m.Success && int.TryParse(m.Groups[1].Value.Split('.', ',')[0], out var pct) && pct != lastPct)
                        { lastPct = pct; Main.BeginInvoke(() => r.SetTag(Fmt.Pct(pct), Amber)); }
                    });
                    r.SetTag(code == 0 ? L.T("TAMAM ✓") : $"HATA ({code})", code == 0 ? Cyan : Red);
                    continue;
                }
                long f = 0; int skipped = 0;
                await Task.Run(() =>
                {
                    if (t.StopUpdate) { ProcRunner.Capture("net.exe", "stop wuauserv /y", 60000); ProcRunner.Capture("net.exe", "stop bits /y", 60000); }
                    foreach (var p in t.Paths.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        var (fr, _, sk) = Cleaner.CleanDir(p);
                        f += fr; skipped += sk;
                    }
                    if (t.StopUpdate) { ProcRunner.Capture("net.exe", "start bits", 30000); ProcRunner.Capture("net.exe", "start wuauserv", 30000); }
                });
                freed += f;
                r.SetTag(L.F("{0} BOŞALDI ✓", Fmt.Size(f)), Cyan);
                Log.Ok(L.F("  [OK] {0}: {1} boşaldı", t.Title, Fmt.Size(f)) + (skipped > 0 ? L.F(" ({0} kullanımdaki dosya atlandı)", skipped) : ""));
            }
            Log.Ok(L.F("[OK] Temizlik bitti · Toplam {0} boşaldı.", Fmt.Size(freed)));
            info.Text = L.F("Toplam {0} boşaldı.", Fmt.Size(freed));
            await Task.Delay(1500);
            await Calc();
        });
    }
}
