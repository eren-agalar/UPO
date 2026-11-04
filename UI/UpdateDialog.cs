namespace FpsAtolyesi.UI;

/// <summary>"Yeni sürüm var" penceresi: sürüm notları, indirme ilerlemesi ve kurulum.</summary>
public class UpdateDialog : Form
{
    readonly UpdateManifest m;
    readonly bool installed;
    readonly ProgressRow progress = new(L.T("İndiriliyor")) { BarColor = Cyan, Visible = false };
    readonly HudButton updateBtn, laterBtn, skipBtn;
    CancellationTokenSource cts;
    readonly TextBox notes;

    public UpdateDialog(UpdateManifest manifest)
    {
        m = manifest;
        installed = Updater.IsInstalled();
        Text = "UPO · " + L.T("Güncelleme");
        AutoScaleMode = AutoScaleMode.None;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = MinimizeBox = false; ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent; BackColor = Theme.Card; ForeColor = Fg; Font = FBody;
        ClientSize = new Size(P(580), P(440));
        Padding = new Padding(P(22));

        var title = new TextBlock(L.F("Yeni sürüm hazır: v{0}", m.Version), FTitle, Yellow);
        var sub = new TextBlock(L.F("Şu anki sürüm: v{0} · Yayın tarihi: {1}", Updater.CurrentVersion, m.Published) + "\n" +
            L.T("Ayarların, yedeklerin ve skor geçmişin korunur. İndirilen dosyanın dijital imzası ve SHA-256 değeri doğrulanır."), FSmall);
        var notesText = (L.En && !string.IsNullOrWhiteSpace(m.NotesEn) ? m.NotesEn : m.NotesTr);
        if (string.IsNullOrWhiteSpace(notesText)) notesText = L.T("Bu sürüm için not eklenmemiş.");
        notes = new TextBox
        {
            Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BorderStyle = BorderStyle.None,
            BackColor = Bg, ForeColor = Fg, Font = FBody, Text = notesText.Replace("\r\n", "\n").Replace("\n", "\r\n"),
        };
        var notesWrap = new Panel { BackColor = Bg, Padding = new Padding(P(10)) };
        notesWrap.Controls.Add(notes); notes.Dock = DockStyle.Fill;

        updateBtn = new HudButton(installed ? L.T("ŞİMDİ GÜNCELLE") : L.T("İNDİRME SAYFASINI AÇ"));
        laterBtn = new HudButton(L.T("DAHA SONRA"), false);
        skipBtn = new HudButton(L.T("BU SÜRÜMÜ ATLA"), false);
        updateBtn.Click += async (_, _) => await DoUpdate();
        laterBtn.Click += (_, _) => { cts?.Cancel(); Close(); };
        skipBtn.Click += (_, _) => { Settings.S.SkipVersion = m.Version; Settings.Save(); Log.Dim(L.F("v{0} sürümü atlandı.", m.Version)); Close(); };
        var row = new FlowLayoutPanel { Height = P(46), BackColor = Theme.Card, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        row.Controls.Add(updateBtn); row.Controls.Add(laterBtn); row.Controls.Add(skipBtn);
        var note = new TextBlock(installed ? "" : L.T("Portable sürümde otomatik kurulum yapılmaz. Yeni sürümü indirme sayfasından alabilirsin."), FSmall, Amber);

        Pages.PageBase.Compose(this, notesWrap,
            (title, DockStyle.Top), (Pages.PageBase.Gap(4), DockStyle.Top), (sub, DockStyle.Top), (Pages.PageBase.Gap(12), DockStyle.Top),
            (row, DockStyle.Bottom), (Pages.PageBase.Gap(10), DockStyle.Bottom), (note, DockStyle.Bottom), (progress, DockStyle.Bottom), (Pages.PageBase.Gap(10), DockStyle.Bottom));
    }

    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); Native.DarkTitleBar(Handle); }
    protected override void OnShown(EventArgs e) { base.OnShown(e); notes.SelectionStart = 0; notes.SelectionLength = 0; ActiveControl = null; }
    protected override void OnFormClosing(FormClosingEventArgs e) { cts?.Cancel(); base.OnFormClosing(e); }

    async Task DoUpdate()
    {
        if (!installed)
        {
            try { Process.Start(new ProcessStartInfo(Updater.ReleasesPage) { UseShellExecute = true }); } catch { }
            Close();
            return;
        }
        if (MainForm.I.Busy)
        {
            MessageBox.Show(this, L.T("Şu anda bir işlem sürüyor. Bitince tekrar dene."), "UPO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        updateBtn.Enabled = skipBtn.Enabled = false;
        laterBtn.Text = L.T("İPTAL"); laterBtn.FitWidth();
        progress.Visible = true; progress.Value = 0; progress.Detail = L.T("İndiriliyor…");
        cts = new CancellationTokenSource();
        try
        {
            Log.W(L.F("v{0} indiriliyor…", m.Version));
            var file = await Updater.Download(m, p => BeginInvoke(() => { progress.Value = p; progress.Detail = L.F("İndiriliyor… {0} / {1}", Fmt.Size((long)(m.Size * p / 100)), Fmt.Size(m.Size)); }), cts.Token);
            progress.Value = 100;
            progress.Detail = L.T("İmza ve SHA-256 doğrulandı ✓ Kurulum başlatılıyor; UPO birkaç saniye içinde yeniden açılacak.");
            Log.Ok(L.T("[OK] Güncelleme doğrulandı. Kuruluyor…"));
            await Task.Delay(1200);
            Updater.InstallAndExit(file, m.Sha256);
        }
        catch (OperationCanceledException) { progress.Detail = L.T("İndirme iptal edildi."); Log.Warn(L.T("Güncelleme indirmesi iptal edildi.")); ResetButtons(); }
        catch (Exception ex)
        {
            progress.Detail = L.T("Güncelleme başarısız: ") + ex.Message;
            Log.Err("[X] " + L.T("Güncelleme başarısız: ") + ex.Message);
            ResetButtons();
        }
    }

    void ResetButtons()
    {
        if (IsDisposed) return;
        updateBtn.Enabled = skipBtn.Enabled = true;
        laterBtn.Text = L.T("DAHA SONRA"); laterBtn.FitWidth();
    }
}
