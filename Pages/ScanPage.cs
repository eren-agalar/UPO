using System.Net;

namespace FpsAtolyesi.Pages;

// ───────────────────────────── 01 · Sistem Taraması ─────────────────────────────
public class ScanPage : PageBase
{
    readonly Gauge gauge = new();
    readonly SegBar bar = new() { Label = L.T("SİSTEM SKORU") };
    readonly ListPanel list = new();
    readonly TextBlock summary = new(L.T("Tarama bekleniyor…"), FSmall);
    readonly HudButton reportBtn;

    public ScanPage()
    {
        var header = new PageHeader("01", L.T("Modül 01 · Sadece okur"), L.T("Sistem Taraması"),
            L.T("Oyun performansını etkileyen Windows ayarlarını ve arka plan yükünü inceler, 100 üzerinden puan verir. Bu modül hiçbir şeyi değiştirmez."));
        var scan = Btn(L.T("TARAMAYI BAŞLAT"), true, () => _ = Run(L.T("Sistem taranıyor"), AppState.Scan));
        reportBtn = Btn(L.T("HTML RAPOR"), false, MakeReport);
        reportBtn.Enabled = false;

        var left = Card(gauge,
            (summary, DockStyle.Bottom), (Gap(10), DockStyle.Bottom), (ButtonRow(scan, reportBtn), DockStyle.Bottom), (Gap(12), DockStyle.Bottom),
            (new Badge(L.T("SIFIR DEĞİŞİKLİK · SADECE OKUR")), DockStyle.Bottom), (Gap(12), DockStyle.Bottom), (bar, DockStyle.Bottom), (Gap(6), DockStyle.Bottom));
        left.Width = P(330);

        var right = Card(list);
        right.Padding = new Padding(P(4), P(8), P(4), P(10));

        Compose(this, right, (header, DockStyle.Top), (Gap(10), DockStyle.Top), (left, DockStyle.Left), (Gap(16), DockStyle.Left));
        AppState.Scanned += Render;
    }

    protected override void OnBusy(bool busy) { if (!busy) reportBtn.Enabled = AppState.Last != null; }

    void Render(ScanResult r)
    {
        gauge.SetValue(r.Score);
        bar.Value = r.Score;
        summary.Text = r.Missing == 0
            ? L.T("Harika! Kontrol edilen tüm ayarlar uygun görünüyor.")
            : L.F("{0} eksik bulundu. Bunları 02 · Tek Tık Optimizasyon ile saniyeler içinde düzeltebilirsiniz.\nSon tarama: {1:HH:mm:ss}", r.Missing, r.Time);
        reportBtn.Enabled = !Main.Busy;

        var rows = new List<Control>();
        foreach (var grp in r.Items.GroupBy(i => i.Cat))
        {
            rows.Add(RowItem.Header(grp.Key));
            foreach (var i in grp.OrderBy(i => i.Ok))
            {
                var row = new RowItem(i.Title, i.Detail) { Status = i.Info ? RowStatus.Info : i.Ok ? RowStatus.Ok : RowStatus.Bad };
                if (i.Info) row.SetTag(L.T("BİLGİ"), Muted);
                else if (i.Ok) row.SetTag(L.T("UYGUN ✓"), Cyan);
                else row.SetTag(L.T("EKSİK"), Red);
                rows.Add(row);
            }
        }
        list.SetRows(rows);
    }

    void MakeReport()
    {
        var r = AppState.Last;
        if (r == null) return;
        try
        {
            var file = Path.Combine(Settings.ReportFolder, $"UPO-Rapor-{r.Time:yyyyMMdd-HHmm}.html");
            File.WriteAllText(file, Report.Html(r), Encoding.UTF8);
            Log.Ok(L.T("[OK] HTML rapor oluşturuldu: ") + file);
            Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
        }
        catch (Exception ex) { Log.Err(L.T("Rapor oluşturulamadı: ") + ex.Message); }
    }
}

public static class Report
{
    static string E(string s) => WebUtility.HtmlEncode(s ?? "");

    public static string Html(ScanResult r)
    {
        var sb = new StringBuilder();
        var col = r.Score < 60 ? "#f25454" : r.Score < 85 ? "#f7c523" : "#28e2d2";
        sb.Append($@"<!doctype html><html lang=""{(L.En ? "en" : "tr")}""><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<title>{E(L.T("UPO Raporu"))}</title><style>
:root{{--bg:#0a0d12;--card:#10141b;--line:#2a3240;--fg:#e8ecf2;--mut:#8490a2;--y:#f7c523;--c:#28e2d2;--r:#f25454}}
*{{box-sizing:border-box}}body{{margin:0;background:var(--bg);color:var(--fg);font:15px/1.5 'Segoe UI',system-ui,sans-serif;padding:32px 16px}}
.w{{max-width:920px;margin:0 auto}}h1{{font-family:Bahnschrift,'Segoe UI';color:var(--y);margin:0;font-size:30px;letter-spacing:.5px}}
.sub{{color:var(--mut);margin:4px 0 24px}}.top{{display:flex;gap:16px;flex-wrap:wrap}}
.card{{background:var(--card);border:1px solid var(--line);padding:18px 20px;border-radius:4px;flex:1;min-width:240px}}
.score{{font-family:Bahnschrift,'Segoe UI';font-size:64px;font-weight:700;color:{col};line-height:1}}.k{{color:var(--mut);font-size:12px;letter-spacing:1.5px;font-weight:700}}
.bar{{height:10px;background:#18202b;margin-top:12px}}.bar i{{display:block;height:100%;width:{r.Score}%;background:{col}}}
h2{{font-size:12px;letter-spacing:1.5px;color:var(--y);margin:28px 0 8px}}table{{width:100%;border-collapse:collapse;background:var(--card);border:1px solid var(--line)}}
td{{padding:10px 14px;border-top:1px solid var(--line);vertical-align:top}}td.s{{width:96px;font-weight:700;font-size:12px}}.ok{{color:var(--c)}}.no{{color:var(--r)}}.in{{color:var(--mut)}}
.d{{color:var(--mut);font-size:13px}}.f{{color:var(--mut);font-size:12px;margin-top:28px}}</style></head><body><div class=""w"">
<h1>{E(L.T("UPO · SİSTEM RAPORU"))}</h1><div class=""sub"">{r.Time:dd.MM.yyyy HH:mm} · {E(L.T("Bu rapor yalnızca bilgisayarınızda oluşturuldu."))}</div>
<div class=""top""><div class=""card""><div class=""k"">{E(L.T("PERFORMANS SKORU"))}</div><div class=""score"">{r.Score}<span style=""font-size:22px;color:var(--mut)""> /100</span></div><div class=""bar""><i></i></div>
<div class=""d"" style=""margin-top:10px"">{E(L.F("{0} eksik ayar bulundu.", r.Missing))}</div></div>
<div class=""card""><div class=""k"">{E(L.T("DONANIM"))}</div><div>{E(r.Sys?.Cpu)}</div><div>{E(r.Sys?.Gpu)}</div><div class=""d"">{E(r.Sys?.Os)} · {E(r.Sys?.RamText)}</div></div></div>");
        foreach (var g in r.Items.GroupBy(i => i.Cat))
        {
            sb.Append($"<h2>{E(Fmt.Upper(g.Key))}</h2><table>");
            foreach (var i in g.OrderBy(i => i.Ok))
            {
                var (cls, txt) = i.Info ? ("in", L.T("BİLGİ")) : i.Ok ? ("ok", L.T("✓ UYGUN")) : ("no", L.T("✗ EKSİK"));
                sb.Append($"<tr><td class=\"s {cls}\">{txt}</td><td><b>{E(i.Title)}</b><div class=\"d\">{E(i.Detail)}</div></td></tr>");
            }
            sb.Append("</table>");
        }
        sb.Append(L.T("<div class=\"f\">Skor, oyun performansını etkileyen ayarların durumuna göre hesaplanır. Gerçek FPS artışı donanıma ve oyuna göre değişir.</div></div></body></html>"));
        return sb.ToString();
    }
}
