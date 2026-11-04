namespace FpsAtolyesi.UI;

public static class Theme
{
    public static readonly Color Bg = Color.FromArgb(10, 13, 18);
    public static readonly Color Card = Color.FromArgb(16, 20, 27);
    public static readonly Color Card2 = Color.FromArgb(24, 30, 40);
    public static readonly Color Line = Color.FromArgb(42, 50, 64);
    public static readonly Color Fg = Color.FromArgb(232, 236, 242);
    public static readonly Color Muted = Color.FromArgb(132, 144, 162);
    /// <summary>Vurgu rengi (Ayarlar'dan değiştirilebilir).</summary>
    public static Color Yellow = Color.FromArgb(247, 197, 35);
    /// <summary>Uyarılar ve orta skorlar için sabit sarı/amber.</summary>
    public static readonly Color Amber = Color.FromArgb(247, 197, 35);

    public static readonly (string id, string name, Color c)[] Accents =
    {
        ("yellow", L.T("SARI"), Color.FromArgb(247, 197, 35)),
        ("red", L.T("KIRMIZI"), Color.FromArgb(255, 86, 72)),
        ("blue", L.T("MAVİ"), Color.FromArgb(64, 156, 255)),
        ("green", L.T("YEŞİL"), Color.FromArgb(64, 222, 128)),
        ("purple", L.T("MOR"), Color.FromArgb(176, 120, 255)),
    };
    public static readonly Color Cyan = Color.FromArgb(40, 226, 210);
    public static readonly Color Red = Color.FromArgb(242, 84, 84);
    public static readonly Color Green = Color.FromArgb(76, 217, 123);
    public static readonly Color Ink = Color.FromArgb(14, 14, 14);

    public static float S = 1f;
    public static int P(float v) => (int)Math.Round(v * S);

    public static Font FBody, FBold, FSmall, FSmallBold, FTiny, FHead, FTitle, FHuge, FMono, FNum;

    public static void Init()
    {
        using (var g = Graphics.FromHwnd(IntPtr.Zero)) S = g.DpiX / 96f;
        foreach (var a in Accents) if (a.id == Settings.S.Accent) Yellow = a.c;
        string head = Has("Bahnschrift") ? "Bahnschrift" : "Segoe UI";
        string mono = Has("Cascadia Mono") ? "Cascadia Mono" : "Consolas";
        FBody = new Font("Segoe UI", 9.75f);
        FBold = new Font("Segoe UI", 9.75f, FontStyle.Bold);
        FSmall = new Font("Segoe UI", 8.5f);
        FSmallBold = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        FTiny = new Font("Segoe UI", 7.25f, FontStyle.Bold);
        FHead = new Font(head, 11.5f, FontStyle.Bold);
        FTitle = new Font(head, 19f, FontStyle.Bold);
        FNum = new Font(head, 26f, FontStyle.Bold);
        FHuge = new Font(head, 40f, FontStyle.Bold);
        FMono = new Font(mono, 9f);
    }

    static bool Has(string n) { try { using var f = new FontFamily(n); return true; } catch { return false; } }

    public static Color Mix(Color a, Color b, float t) =>
        Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
    public static Color Alpha(Color c, int a) => Color.FromArgb(a, c);
    public static Color ScoreColor(int v) => v < 60 ? Red : v < 85 ? Amber : Cyan;
}

static class G
{
    public static void Hq(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
    }

    /// <summary>HUD tarzı köşesi kesik dikdörtgen.</summary>
    public static GraphicsPath Cut(Rectangle r, int c)
    {
        var p = new GraphicsPath();
        p.AddPolygon(new[]
        {
            new Point(r.Left, r.Top), new Point(r.Right - c, r.Top), new Point(r.Right, r.Top + c),
            new Point(r.Right, r.Bottom), new Point(r.Left + c, r.Bottom), new Point(r.Left, r.Bottom - c),
        });
        return p;
    }

    public static GraphicsPath Round(RectangleF r, float rad)
    {
        var p = new GraphicsPath();
        float d = rad * 2;
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    public static void Text(Graphics g, string s, Font f, Color c, Rectangle r, TextFormatFlags fl = TextFormatFlags.Left | TextFormatFlags.VerticalCenter)
        => TextRenderer.DrawText(g, s ?? "", f, r, c, fl | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);

    public static Size Measure(string s, Font f) => TextRenderer.MeasureText(s ?? "", f, Size.Empty, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);

    public static void Check(Graphics g, Rectangle r, Color c, float w)
    {
        using var pen = new Pen(c, w) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        g.DrawLines(pen, new[]
        {
            new PointF(r.Left + r.Width * .2f, r.Top + r.Height * .52f),
            new PointF(r.Left + r.Width * .42f, r.Top + r.Height * .74f),
            new PointF(r.Left + r.Width * .8f, r.Top + r.Height * .3f),
        });
    }
}
