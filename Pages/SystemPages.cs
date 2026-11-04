namespace FpsAtolyesi.Pages;

// ───────────────────────────── 05 · Windows Onarımı ─────────────────────────────
public class RepairPage : PageBase
{
    readonly ProgressRow dism = new(L.T("DISM · Windows görüntüsünü onar")) { BarColor = Cyan };
    readonly ProgressRow sfc = new(L.T("SFC · Sistem dosyalarını denetle")) { BarColor = Amber };
    readonly HudButton stopBtn;
    CancellationTokenSource cts;

    public RepairPage()
    {
        var header = new PageHeader("05", L.T("Modül 05 · Onarım"), L.T("SFC + DISM Onarım"),
            L.T("Bozuk Windows sistem dosyalarını bulur ve onarır. Oyun çökmeleri, donmalar ve açılmayan programlar için faydalıdır. 15–40 dakika sürebilir; bu sırada bilgisayarı kullanabilirsiniz."));
        dism.Detail = L.T("Bekliyor · Windows Update'ten sağlam dosyaları indirir (internet gerekir)");
        sfc.Detail = L.T("Bekliyor · Tüm korumalı sistem dosyalarını denetler ve onarır");
        var start = Btn(L.T("ONARIMI BAŞLAT"), true, Start);
        stopBtn = Btn(L.T("DURDUR"), false, () => cts?.Cancel(), false);
        stopBtn.Enabled = false;
        var note = new TextBlock(L.T("Sıra: önce DISM (onarım kaynağını düzeltir), sonra SFC (dosyaları onarır). Ayrıntılı günlük: C:\\Windows\\Logs\\CBS\\CBS.log"), FSmall);
        var card = Card(null, (dism, DockStyle.Top), (Gap(16), DockStyle.Top), (sfc, DockStyle.Top), (Gap(16), DockStyle.Top), (note, DockStyle.Top));
        card.Height = P(250);
        card.Padding = new Padding(P(22));
        Compose(this, new Panel { BackColor = Bg }, (header, DockStyle.Top), (Gap(8), DockStyle.Top), (card, DockStyle.Top), (Gap(14), DockStyle.Top), (ButtonRow(start, stopBtn), DockStyle.Top));
    }

    void Start()
    {
        if (!Main.Confirm(L.T("Windows onarımı başlatılsın mı?\n\nDISM ve SFC birlikte 15–40 dakika sürebilir. Dizüstü bilgisayarlarda şarj aletini takın."), L.T("Onarım"))) return;
        cts = new CancellationTokenSource();
        stopBtn.Enabled = true;
        _ = Run(L.T("Windows onarılıyor"), async () =>
        {
            try
            {
                dism.Value = 0; sfc.Value = 0;
                dism.Detail = L.T("Başlatılıyor…");
                Log.W(L.T("DISM /RestoreHealth başlatıldı…"));
                int dc = await Exec("dism.exe", "/Online /Cleanup-Image /RestoreHealth", dism, null);
                dism.Value = dc == 0 ? 100 : dism.Value;
                dism.Detail = dc == 0 ? L.T("Tamamlandı ✓ Windows görüntüsü sağlıklı.") : L.F("Hata kodu {0}. İnternet bağlantınızı kontrol edin.", dc);
                (dc == 0 ? (Action<string>)Log.Ok : Log.Warn)(L.T("DISM: ") + dism.Detail);

                sfc.Detail = L.T("Başlatılıyor…");
                Log.W(L.T("SFC /scannow başlatıldı…"));
                int sc = await Exec("sfc.exe", "/scannow", sfc, Encoding.Unicode);
                sfc.Value = 100;
                sfc.Detail = sc == 0 ? L.T("Tamamlandı ✓ Ayrıntılar canlı kayıtta.") : L.F("Bitti (kod {0}). Ayrıntılar canlı kayıtta ve CBS.log'da.", sc);
                Log.Ok(L.T("SFC: ") + sfc.Detail);
                Main.AskRestart(L.T("Onarım tamamlandı. Onarılan dosyaların geçerli olması için yeniden başlatmanız önerilir."));
            }
            finally { stopBtn.Enabled = false; }
        });
    }

    async Task<int> Exec(string exe, string args, ProgressRow row, Encoding enc)
    {
        string last = null;
        var sw = Stopwatch.StartNew();
        var lastChange = sw.Elapsed;
        double lastVal = -1;
        bool isDism = exe.StartsWith("dism", StringComparison.OrdinalIgnoreCase);
        // Her saniye geçen süreyi göster; ilerleme uzun süre değişmezse bunun normal olduğunu açıkla.
        using var tick = new System.Windows.Forms.Timer { Interval = 1000 };
        tick.Tick += (_, _) =>
        {
            var el = sw.Elapsed;
            var still = el - lastChange;
            var txt = L.F("Çalışıyor… {0:00}:{1:00} geçti", (int)el.TotalMinutes, el.Seconds);
            if (still.TotalSeconds > 90)
                txt += isDism
                    ? L.F(" · %{0} aşamasında {1} dk'dır bekliyor. Bu normaldir; DISM bu sırada Windows Update'ten dosya indirip karşılaştırır (10–30 dk sürebilir).", Math.Round(row.Value), (int)still.TotalMinutes)
                    : L.F(" · {0} dk'dır aynı aşamada. SFC bazı dosyalarda uzun süre bekleyebilir, bu normaldir.", (int)still.TotalMinutes);
            row.Detail = txt;
        };
        tick.Start();
        try
        {
            return await ProcRunner.RunAsync(exe, args, line =>
            {
                var m = Regex.Match(line, @"(\d{1,3}(?:[.,]\d+)?)\s*%");
                if (m.Success && double.TryParse(m.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
                {
                    if (Math.Abs(v - lastVal) >= 0.1) { lastVal = v; lastChange = sw.Elapsed; }
                    Main.BeginInvoke(() => row.Value = v);
                    return;
                }
                if (line.Length > 3 && line != last && !line.All(c => c == '=' || c == '[' || c == ']' || c == ' '))
                {
                    last = line;
                    lastChange = sw.Elapsed;
                    Log.Dim("  " + line);
                }
            }, enc, cts.Token);
        }
        finally { tick.Stop(); }
    }
}

// ───────────────────────────── 06 · Önce / Sonra ─────────────────────────────
public class ComparePage : PageBase
{
    readonly SegBar before = new() { Label = L.T("ÖNCE"), FixedColor = Yellow };
    readonly SegBar after = new() { Label = L.T("SONRA"), FixedColor = Cyan };
    readonly TextBlock delta = new("", FTitle, Fg);
    readonly TextBlock when = new("", FSmall);
    readonly ListPanel list = new();

    public ComparePage()
    {
        var header = new PageHeader("06", L.T("Modül 06 · Kıyas"), L.T("Önce / Sonra"),
            L.T("İlk taramadaki skorunuzu şimdiki durumla karşılaştırır ve hangi ayarların düzeldiğini gösterir."));
        var rescan = Btn(L.T("YENİDEN TARA"), true, () => _ = Run(L.T("Sistem taranıyor"), AppState.Scan));
        var reset = Btn(L.T("ÖNCE DEĞERİNİ SIFIRLA"), false, () =>
        {
            if (AppState.Last == null || !Main.Confirm(L.T("\"Önce\" değeri şimdiki skor olarak ayarlansın mı?"), L.T("Sıfırla"))) return;
            State.Data.BeforeScore = null; AppState.SetScan(AppState.Last);
        });
        var note = new TextBlock(L.T("Skor, oyun performansını etkileyen ayarların durumuna göre hesaplanır. Gerçek FPS artışı donanıma, oyuna ve sürücüye göre değişir."), FSmall);
        var top = Card(null, (new TextBlock(L.T("PERFORMANS SKORU · KIYAS"), FTiny, Yellow), DockStyle.Top), (Gap(10), DockStyle.Top),
            (before, DockStyle.Top), (Gap(10), DockStyle.Top), (after, DockStyle.Top), (Gap(12), DockStyle.Top),
            (delta, DockStyle.Top), (when, DockStyle.Top), (Gap(4), DockStyle.Top), (note, DockStyle.Top));
        top.Height = P(244);
        var listCard = Card(list);
        listCard.Padding = new Padding(P(4), P(8), P(4), P(8));
        Compose(this, listCard, (header, DockStyle.Top), (Gap(8), DockStyle.Top), (top, DockStyle.Top), (Gap(10), DockStyle.Top),
            (ButtonRow(rescan, reset), DockStyle.Top), (Gap(10), DockStyle.Top));
        AppState.Scanned += _ => Render();
    }

    public override void OnShow() => Render();

    void Render()
    {
        var d = State.Data;
        before.Value = d.BeforeScore ?? -1;
        after.Value = d.LastScore ?? -1;
        before.RightText = d.BeforeScore.HasValue ? d.BeforeScore + "/100" : "--";
        after.RightText = d.LastScore.HasValue ? d.LastScore + "/100" : "--";
        if (d.BeforeScore.HasValue && d.LastScore.HasValue)
        {
            int diff = d.LastScore.Value - d.BeforeScore.Value;
            delta.Text = diff > 0 ? L.F("+{0} puan iyileşme", diff) : diff == 0 ? L.T("Henüz değişiklik yok") : L.F("{0} puan", diff);
            delta.TextColor = diff > 0 ? Cyan : diff == 0 ? Muted : Red;
            when.Text = L.F("Önce: {0:dd.MM.yyyy HH:mm}   ·   Sonra: {1:dd.MM.yyyy HH:mm}", d.BeforeTime, d.LastTime);
        }
        else { delta.Text = L.T("Tarama bekleniyor"); delta.TextColor = Muted; when.Text = ""; }

        var rows = new List<Control>();
        if (AppState.Last != null)
        {
            var fixedOnes = AppState.Last.Items.Where(i => !i.Info && i.Ok && d.BeforeItems.TryGetValue(i.Id, out var b) && !b).ToList();
            var still = AppState.Last.Items.Where(i => !i.Info && !i.Ok).ToList();
            if (fixedOnes.Count > 0)
            {
                rows.Add(RowItem.Header(L.F("Düzelenler ({0})", fixedOnes.Count)));
                foreach (var i in fixedOnes) { var r = new RowItem(i.Title) { Status = RowStatus.Ok }; r.SetTag(L.T("EKSİK → UYGUN ✓"), Cyan); rows.Add(r); }
            }
            if (still.Count > 0)
            {
                rows.Add(RowItem.Header(L.F("Hâlâ eksik ({0})", still.Count)));
                foreach (var i in still) { var r = new RowItem(i.Title) { Status = RowStatus.Bad }; r.SetTag(L.T("EKSİK"), Red); rows.Add(r); }
            }
            if (rows.Count == 0) rows.Add(new RowItem(L.T("Tüm ayarlar uygun."), L.T("Kontrol edilen her şey yerli yerinde.")) { Status = RowStatus.Ok });
        }
        list.SetRows(rows);
    }
}

// ───────────────────────────── 07 · Bellek Bütünlüğü (VBS) ─────────────────────────────
public class VbsPage : PageBase
{
    readonly BigToggle toggle = new() { Caption = L.T("Bellek Bütünlüğü (Çekirdek yalıtımı)") };
    readonly ListPanel status = new() { AutoScroll = false };
    readonly TextBlock pending = new("", FSmallBold, Amber);

    public VbsPage()
    {
        var header = new PageHeader("07", L.T("Modül 07 · Gelişmiş"), L.T("VBS · Bellek Bütünlüğü"),
            L.T("Windows'un sanallaştırma tabanlı güvenlik özelliği. Bazı sistemlerde oyunlarda belirgin FPS kaybına yol açabilir; kapatmak ise güvenliği azaltır."));
        var stripe = new HazardStripe(L.T("GÜVENLİK  ·  PERFORMANS DENGESİ"));
        var text = new TextBlock(
            L.T("Bellek Bütünlüğü (HVCI), kötü amaçlı yazılımların Windows çekirdeğine sızmasını zorlaştırır. Bazı bilgisayarlarda oyunlarda %5–10'a varan FPS kaybına neden olabilir.\n\n") +
            L.T("• Kapatmak bilgisayarınızın güvenliğini azaltır. Bankacılık/iş bilgisayarında kapatmanızı önermeyiz.\n") +
            L.T("• Bazı oyunlar ve anti-cheat sistemleri bu özelliğin açık olmasını isteyebilir; oyun açılmazsa buradan tekrar açın.\n") +
            L.T("• Değişiklik yeniden başlattıktan sonra geçerli olur ve her zaman geri alınabilir."), FBody, Fg);
        status.Height = P(124);
        toggle.Requested += (_, _) => Toggle();
        var card = Card(null, (toggle, DockStyle.Top), (pending, DockStyle.Top), (Gap(12), DockStyle.Top), (stripe, DockStyle.Top), (Gap(14), DockStyle.Top), (text, DockStyle.Top));
        card.Height = P(380);
        card.Padding = new Padding(P(22));
        var stCard = Card(status);
        stCard.Height = P(150);
        stCard.Padding = new Padding(P(4), P(12), P(4), P(10));
        Compose(this, new Panel { BackColor = Bg }, (header, DockStyle.Top), (Gap(8), DockStyle.Top), (stCard, DockStyle.Top), (Gap(12), DockStyle.Top), (card, DockStyle.Top));
        Actions.Add(toggle);
    }

    public override void OnShow() => _ = RefreshState();

    async Task RefreshState()
    {
        var (vbs, hvciRun, conf, policy) = await Task.Run(() => { var rt = Vbs.Runtime(); return (rt.vbs, rt.hvciRunning, Vbs.HvciConfigured(), Vbs.PolicyManaged()); });
        toggle.On = conf;
        var rows = new List<Control>();
        var r1 = new RowItem(L.T("Sanallaştırma tabanlı güvenlik (VBS)")) { Status = vbs == 2 ? RowStatus.Warn : RowStatus.Ok };
        r1.SetTag(vbs switch { 2 => L.T("ÇALIŞIYOR"), 1 => L.T("ETKİN · ÇALIŞMIYOR"), 0 => L.T("KAPALI"), _ => L.T("OKUNAMADI") }, vbs == 2 ? Amber : Cyan);
        var r2 = new RowItem(L.T("Bellek Bütünlüğü (HVCI) · şu anki durum")) { Status = hvciRun ? RowStatus.Warn : RowStatus.Ok };
        r2.SetTag(hvciRun ? L.T("ÇALIŞIYOR") : L.T("ÇALIŞMIYOR"), hvciRun ? Amber : Cyan);
        var r3 = new RowItem(L.T("Bellek Bütünlüğü (HVCI) · ayar")) { Status = RowStatus.Info };
        r3.SetTag(conf ? L.T("AÇIK") : L.T("KAPALI"), conf ? Amber : Cyan);
        rows.Add(r1); rows.Add(r2); rows.Add(r3);
        status.SetRows(rows);
        pending.Text = conf != hvciRun && vbs != -1 ? L.T("Yeniden başlatma bekleniyor: ayar değişti ama henüz geçerli değil.") :
                       policy ? L.T("Not: Bu ayar bir grup ilkesiyle yönetiliyor; değişiklik geçerli olmayabilir.") : "";
    }

    void Toggle()
    {
        if (toggle.On)
        {
            using var dlg = new TypeConfirm(L.T("Bellek Bütünlüğü"),
                L.T("Bellek Bütünlüğü kapatılacak.\n\nBu, bazı oyunlarda FPS'i artırabilir ama bilgisayarınızın kötü amaçlı yazılımlara karşı korumasını azaltır. Değişiklik yedeklenir ve buradan tekrar açılabilir."));
            if (dlg.ShowDialog(Main) != DialogResult.OK) return;
            _ = Run(L.T("Bellek Bütünlüğü kapatılıyor"), async () =>
            {
                await Backup.EnsureRestorePoint();
                await Task.Run(Vbs.Disable);
                Log.Ok(L.T("[OK] Bellek Bütünlüğü kapatıldı (yeniden başlatma sonrası geçerli)."));
                await RefreshState();
                Main.AskRestart(L.T("Bellek Bütünlüğü ayarı yeniden başlatmadan sonra geçerli olur."));
            });
        }
        else
        {
            if (!Main.Confirm(L.T("Bellek Bütünlüğü yeniden açılsın mı? (Önerilen güvenli ayar)"), L.T("Bellek Bütünlüğü"))) return;
            _ = Run(L.T("Bellek Bütünlüğü açılıyor"), async () =>
            {
                await Task.Run(Vbs.Enable);
                Log.Ok(L.T("[OK] Bellek Bütünlüğü açıldı (yeniden başlatma sonrası geçerli)."));
                await RefreshState();
                Main.AskRestart(L.T("Bellek Bütünlüğü ayarı yeniden başlatmadan sonra geçerli olur."));
            });
        }
    }
}

// ───────────────────────────── 08 · Tümünü Geri Al ─────────────────────────────
public class RestorePage : PageBase
{
    readonly ListPanel list = new();
    readonly TextBlock info = new("", FSmall);

    public RestorePage()
    {
        var header = new PageHeader("08", L.T("Modül 08 · Güvenlik kalkanı"), L.T("Tümünü Geri Al"),
            L.T("Uygulamanın yaptığı her ayar değişikliği orijinal değeriyle yedeklenir. İstediğiniz an tek tuşla bilgisayarınızı ilk hâline döndürebilirsiniz."));
        var shield = new Shield { Width = P(200) };
        var badges = new Panel { BackColor = Color.Transparent };
        Compose(badges, null, (new Badge(L.T("SIFIR VERİ TOPLAMA")), DockStyle.Top), (Gap(8), DockStyle.Top),
            (new Badge(L.T("%100 GERİ ALINABİLİR YEDEKLEME")), DockStyle.Top), (Gap(8), DockStyle.Top),
            (new Badge(L.T("ORİJİNAL DEĞERLER SAKLANIR"), Amber), DockStyle.Top), (Gap(12), DockStyle.Top),
            (new TextBlock(L.T("Not: Temizlik ve onarım işlemleri ayar değil dosya işlemidir, bu yüzden geri alma listesinde yer almaz. İlk değişiklikten önce ayrıca bir Windows geri yükleme noktası oluşturulmaya çalışılır."), FSmall), DockStyle.Top));
        var top = Card(badges, (shield, DockStyle.Left), (Gap(20), DockStyle.Left));
        top.Height = P(186);
        var sel = Btn(L.T("SEÇİLENLERİ GERİ AL"), false, () => DoRestore(false));
        var all = Btn(L.T("TÜMÜNÜ GERİ AL"), true, () => DoRestore(true));
        var bottom = new Panel { Height = P(56), BackColor = Bg };
        Compose(bottom, info, (Gap(10), DockStyle.Top), (ButtonRow(all, sel), DockStyle.Left), (Gap(14), DockStyle.Left));
        var listCard = Card(list);
        listCard.Padding = new Padding(P(4), P(8), P(4), P(8));
        Compose(this, listCard, (header, DockStyle.Top), (Gap(8), DockStyle.Top), (top, DockStyle.Top), (Gap(12), DockStyle.Top), (bottom, DockStyle.Bottom), (Gap(6), DockStyle.Bottom));
    }

    public override void OnShow() => Render();

    void Render()
    {
        var rows = new List<Control>();
        var groups = Backup.Groups().ToList();
        foreach (var g in groups)
        {
            var r = new RowItem(Tweaks.GroupTitle(g.Key), L.F("{0} kayıt · {1:dd.MM.yyyy HH:mm}", g.Count(), g.Min(e => e.Time)), RowMode.Check) { Data = g.Key, Checked = true };
            r.SetTag(L.T("YEDEKLİ"), Cyan);
            rows.Add(r);
        }
        if (rows.Count == 0) rows.Add(new RowItem(L.T("Geri alınacak değişiklik yok."), L.T("Bilgisayarınız şu an orijinal ayarlarında (bu uygulama açısından).")) { Status = RowStatus.Ok });
        list.SetRows(rows);
        info.Text = groups.Count == 0 ? "" : L.F("{0} ayar grubu · {1} yedek kaydı", groups.Count, groups.Sum(g => g.Count()));
    }

    void DoRestore(bool all)
    {
        var chosen = all ? Backup.Groups().Select(g => g.Key).ToHashSet()
                         : list.Controls.OfType<RowItem>().Where(r => r.Checked && r.Data is string).Select(r => (string)r.Data).ToHashSet();
        if (chosen.Count == 0) { MessageBox.Show(Main, L.T("Geri alınacak bir şey seçilmedi."), "UPO"); return; }
        if (!Main.Confirm(L.F("{0} ayar grubu orijinal hâline döndürülecek. Devam edilsin mi?", chosen.Count), "Geri al")) return;
        _ = Run(L.T("Geri alınıyor"), async () =>
        {
            Log.W(L.T("Geri alma başladı…"));
            var (ok, fail) = await Task.Run(() => Backup.Restore(e => chosen.Contains(e.Group)));
            Log.Ok(L.F("[OK] Tek tuşla ilk hâline döndü · {0} kayıt geri yüklendi", ok) + (fail > 0 ? L.F(", {0} hata", fail) : ""));
            Render();
            await AppState.Scan();
            Main.AskRestart(L.T("Bazı ayarların tamamen eski hâline dönmesi için yeniden başlatma önerilir."));
        });
    }
}
