namespace FpsAtolyesi.UI;

public class HControl : Control
{
    public HControl()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Fg;
    }
}

// ───────────────────────────── Buton ─────────────────────────────
public class HudButton : HControl
{
    public bool Primary { get; set; }
    bool hover, down;

    public HudButton(string text, bool primary = true)
    {
        Text = text; Primary = primary; Font = FBold; Cursor = Cursors.Hand;
        Height = P(38); Margin = new Padding(0, 0, P(10), 0);
        FitWidth();
    }

    public void FitWidth() => Width = G.Measure(Text, Font).Width + P(40);

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = down = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
    protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        var r = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = G.Cut(r, P(9));
        Color fill, border, fg;
        if (!Enabled) { fill = Card2; border = Line; fg = Muted; }
        else if (Primary) { fill = down ? Mix(Yellow, Ink, .15f) : hover ? Mix(Yellow, Color.White, .18f) : Yellow; border = fill; fg = Ink; }
        else { fill = hover ? Alpha(Yellow, 28) : Alpha(Card, 0); border = Yellow; fg = Yellow; }
        using (var b = new SolidBrush(fill)) g.FillPath(b, path);
        using (var p = new Pen(border, P(1.2f))) g.DrawPath(p, path);
        G.Text(g, Text, Font, fg, ClientRectangle, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

// ───────────────────────────── HUD kartı ─────────────────────────────
public class HudPanel : Panel
{
    public Color Border = Line;
    public Color Accent = Yellow;

    public HudPanel()
    {
        DoubleBuffered = true; ResizeRedraw = true;
        BackColor = Card; Padding = new Padding(P(16));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics; G.Hq(g);
        var r = new Rectangle(0, 0, Width - 1, Height - 1);
        int c = P(12);
        using (var path = G.Cut(r, c)) using (var p = new Pen(Border, 1)) g.DrawPath(p, path);
        using var a = new Pen(Accent, P(2));
        int l = P(22);
        g.DrawLine(a, 1, 1, l, 1); g.DrawLine(a, 1, 1, 1, l);
        g.DrawLine(a, r.Right - 1, r.Bottom - 1, r.Right - l, r.Bottom - 1); g.DrawLine(a, r.Right - 1, r.Bottom - 1, r.Right - 1, r.Bottom - l);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Bg);
        G.Hq(e.Graphics);
        using var path = G.Cut(new Rectangle(0, 0, Width - 1, Height - 1), P(12));
        using var b = new SolidBrush(BackColor);
        e.Graphics.FillPath(b, path);
    }
}

// ───────────────────────────── Kaydırılabilir liste ─────────────────────────────
public class ListPanel : Panel
{
    public ListPanel() { AutoScroll = true; BackColor = Card; DoubleBuffered = true; }
    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); Native.DarkScroll(Handle); }

    public void SetRows(IList<Control> rows)
    {
        SuspendLayout();
        var old = Controls.Cast<Control>().ToList();
        Controls.Clear();
        foreach (var c in old) c.Dispose();
        for (int i = rows.Count - 1; i >= 0; i--) { rows[i].Dock = DockStyle.Top; Controls.Add(rows[i]); }
        ResumeLayout();
        AutoScrollPosition = Point.Empty;
    }
}

// ───────────────────────────── Liste satırı ─────────────────────────────
public enum RowStatus { None, Ok, Warn, Bad, Info }
public enum RowMode { Plain, Check, Switch }

public class RowItem : HControl
{
    string title, sub, tag;
    Color tagColor = Muted;
    RowStatus status;
    bool isChecked, hover;

    public RowMode Mode;
    public bool IsHeader;
    public object Data;
    public event EventHandler Toggled;

    public string Title { get => title; set { title = value; Invalidate(); } }
    public string Sub { get => sub; set { sub = value; Invalidate(); } }
    public string Tag2 { get => tag; set { tag = value; Invalidate(); } }
    public Color TagColor { get => tagColor; set { tagColor = value; Invalidate(); } }
    public RowStatus Status { get => status; set { status = value; Invalidate(); } }
    public bool Checked { get => isChecked; set { isChecked = value; Invalidate(); } }

    public void SetTag(string t, Color c) { tag = t; tagColor = c; Invalidate(); }

    public RowItem(string title, string sub = null, RowMode mode = RowMode.Plain)
    {
        this.title = title; this.sub = sub; Mode = mode;
        Height = P(string.IsNullOrEmpty(sub) ? 40 : 56);
        if (mode != RowMode.Plain) Cursor = Cursors.Hand;
    }

    public static RowItem Header(string text) => new(text) { IsHeader = true, Height = P(34) };

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        if (Mode == RowMode.Plain || !Enabled || IsHeader) return;
        Checked = !Checked;
        Toggled?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        g.Clear(hover && Mode != RowMode.Plain && Enabled ? Card2 : Card);
        if (IsHeader)
        {
            G.Text(g, Fmt.Upper(title), FTiny, Yellow, new Rectangle(P(14), P(10), Width - P(28), Height - P(12)), TextFormatFlags.Left | TextFormatFlags.Bottom);
            return;
        }
        using (var lp = new Pen(Line)) g.DrawLine(lp, P(10), Height - 1, Width - P(10), Height - 1);

        int x = P(14);
        var dim = !Enabled;
        if (Mode == RowMode.Check)
        {
            int s = P(18);
            var br = new Rectangle(x, (Height - s) / 2, s, s);
            if (isChecked && !dim) { using var b = new SolidBrush(Yellow); g.FillRectangle(b, br); G.Check(g, br, Ink, P(2.2f)); }
            else { using var p = new Pen(dim ? Line : Muted, P(1.3f)); g.DrawRectangle(p, br); if (isChecked) G.Check(g, br, Muted, P(2f)); }
            x += s + P(14);
        }
        if (status != RowStatus.None)
        {
            int d = P(9);
            var c = status switch { RowStatus.Ok => Cyan, RowStatus.Warn => Amber, RowStatus.Bad => Red, _ => Muted };
            using var b = new SolidBrush(dim ? Line : c);
            g.FillEllipse(b, x, (Height - d) / 2, d, d);
            x += d + P(12);
        }

        int right = Width - P(14);
        if (Mode == RowMode.Switch)
        {
            int sw = P(70), sh = P(26);
            var sr = new RectangleF(right - sw, (Height - sh) / 2f, sw, sh);
            using (var path = G.Round(sr, sh / 2f))
            {
                using var b = new SolidBrush(isChecked ? Alpha(Cyan, 50) : Card2);
                g.FillPath(b, path);
                using var p = new Pen(isChecked ? Cyan : Line, P(1.2f));
                g.DrawPath(p, path);
            }
            float k = sh - P(8);
            float kx = isChecked ? sr.Right - k - P(4) : sr.X + P(4);
            using (var kb = new SolidBrush(isChecked ? Cyan : Muted)) g.FillEllipse(kb, kx, sr.Y + P(4), k, k);
            var txtR = isChecked ? new Rectangle((int)sr.X + P(8), (int)sr.Y, sw - (int)k - P(12), sh) : new Rectangle((int)(sr.X + k + P(8)), (int)sr.Y, sw - (int)k - P(12), sh);
            G.Text(g, isChecked ? L.T("AÇIK") : L.T("KAPALI"), FTiny, isChecked ? Cyan : Muted, txtR, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            right -= sw + P(14);
        }
        if (!string.IsNullOrEmpty(tag))
        {
            var ts = G.Measure(tag, FTiny);
            var tr = new Rectangle(right - ts.Width - P(16), (Height - P(22)) / 2, ts.Width + P(16), P(22));
            using (var p = new Pen(dim ? Line : Alpha(tagColor, 160))) g.DrawRectangle(p, tr);
            G.Text(g, tag, FTiny, dim ? Muted : tagColor, tr, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            right = tr.Left - P(12);
        }

        var fg = dim ? Muted : Fg;
        if (string.IsNullOrEmpty(sub))
            G.Text(g, title, FBold, fg, new Rectangle(x, 0, right - x, Height), TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        else
        {
            G.Text(g, title, FBold, fg, new Rectangle(x, P(9), right - x, P(20)), TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
            G.Text(g, sub, FSmall, Muted, new Rectangle(x, P(30), right - x, P(18)), TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
        }
    }
}

// ───────────────────────────── Skor ibresi ─────────────────────────────
public class Gauge : HControl
{
    float val, target = -1;
    readonly System.Windows.Forms.Timer timer = new() { Interval = 16 };
    public string Label = L.T("SİSTEM SKORU");

    public Gauge()
    {
        timer.Tick += (_, _) =>
        {
            if (target < 0) { timer.Stop(); return; }
            val += (target - val) * 0.12f;
            if (Math.Abs(target - val) < 0.3f) { val = target; timer.Stop(); }
            Invalidate();
        };
    }

    public void SetValue(int v) { if (target < 0) val = 0; target = v; timer.Start(); }
    public void Reset() { target = -1; val = 0; Invalidate(); }

    protected override void Dispose(bool disposing) { if (disposing) timer.Dispose(); base.Dispose(disposing); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        int size = Math.Min(Width, Height) - P(16);
        var r = new RectangleF((Width - size) / 2f, (Height - size) / 2f + P(6), size, size);
        float th = P(12);
        var arc = RectangleF.Inflate(r, -th, -th);

        using (var p = new Pen(Card2, th) { StartCap = LineCap.Flat, EndCap = LineCap.Flat }) g.DrawArc(p, arc, 135, 270);
        // Tikler
        var cx = r.X + r.Width / 2; var cy = r.Y + r.Height / 2;
        for (int i = 0; i <= 50; i++)
        {
            double a = (135 + 270.0 * i / 50) * Math.PI / 180;
            float r1 = arc.Width / 2 - th, r2 = r1 - (i % 5 == 0 ? P(9) : P(4));
            using var tp = new Pen(i % 5 == 0 ? Muted : Line, i % 5 == 0 ? P(1.6f) : 1);
            g.DrawLine(tp, cx + (float)Math.Cos(a) * r1, cy + (float)Math.Sin(a) * r1, cx + (float)Math.Cos(a) * r2, cy + (float)Math.Sin(a) * r2);
        }
        var col = target < 0 ? Muted : ScoreColor((int)Math.Round(val));
        if (target >= 0 && val > 0.5f)
        {
            using var vp = new Pen(col, th) { StartCap = LineCap.Flat, EndCap = LineCap.Round };
            g.DrawArc(vp, arc, 135, 270 * val / 100f);
            using var glow = new Pen(Alpha(col, 40), th * 2.2f);
            g.DrawArc(glow, arc, 135, 270 * val / 100f);
        }
        var txt = target < 0 ? "--" : ((int)Math.Round(val)).ToString();
        var c = new Rectangle((int)r.X, (int)(cy - P(42)), (int)r.Width, P(64));
        G.Text(g, txt, FHuge, target < 0 ? Muted : Fg, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        G.Text(g, "/ 100", FSmallBold, Muted, new Rectangle((int)r.X, (int)cy + P(22), (int)r.Width, P(18)), TextFormatFlags.HorizontalCenter);
        G.Text(g, Label, FTiny, col, new Rectangle((int)r.X, (int)cy + P(42), (int)r.Width, P(16)), TextFormatFlags.HorizontalCenter);
    }
}

// ───────────────────────────── Segmentli çubuk ─────────────────────────────
public class SegBar : HControl
{
    int value = -1;
    public string Label = "";
    public Color? FixedColor;
    public string RightText;
    public int Value { get => value; set { this.value = value; Invalidate(); } }

    public SegBar() { Height = P(46); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var col = FixedColor ?? (value < 0 ? Muted : ScoreColor(value));
        G.Text(g, Label, FTiny, Muted, new Rectangle(0, 0, Width, P(16)), TextFormatFlags.Left);
        G.Text(g, RightText ?? (value < 0 ? "--/100" : value + "/100"), FBold, Fg, new Rectangle(0, 0, Width, P(18)), TextFormatFlags.Right);
        int seg = P(7), gap = P(3), y = P(24), h = P(14);
        int n = Math.Max(1, (Width + gap) / (seg + gap));
        int on = value < 0 ? 0 : (int)Math.Round(n * value / 100.0);
        using var bOn = new SolidBrush(col);
        using var bOff = new SolidBrush(Card2);
        for (int i = 0; i < n; i++) g.FillRectangle(i < on ? bOn : bOff, i * (seg + gap), y, seg, h);
    }
}

// ───────────────────────────── İlerleme satırı ─────────────────────────────
public class ProgressRow : HControl
{
    double value;
    string detail = "", title;
    public string Title { get => title; set { title = value; Invalidate(); } }
    public Color BarColor = Cyan;
    public double Value { get => value; set { this.value = Math.Clamp(value, 0, 100); Invalidate(); } }
    public string Detail { get => detail; set { detail = value; Invalidate(); } }

    public ProgressRow(string title) { this.title = title; Height = P(70); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        G.Text(g, title, FHead, Fg, new Rectangle(0, 0, Width, P(24)), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        G.Text(g, Fmt.Pct(Math.Round(value)), FHead, BarColor, new Rectangle(0, 0, Width, P(24)), TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        var br = new RectangleF(0, P(30), Width - 1, P(12));
        using (var b = new SolidBrush(Card2)) g.FillRectangle(b, br);
        if (value > 0)
        {
            var fr = new RectangleF(br.X, br.Y, (float)(br.Width * value / 100), br.Height);
            using var lb = new LinearGradientBrush(new RectangleF(br.X, br.Y, br.Width + 1, br.Height), Mix(BarColor, Bg, .35f), BarColor, 0f);
            g.FillRectangle(lb, fr);
        }
        G.Text(g, detail, FSmall, Muted, new Rectangle(0, P(48), Width, P(20)), TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
    }
}

// ───────────────────────────── Başlık ─────────────────────────────
public class PageHeader : HControl
{
    readonly string num, kicker, title, desc;

    public PageHeader(string num, string kicker, string title, string desc)
    { this.num = num; this.kicker = kicker; this.title = title; this.desc = desc; Height = P(96); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        var nb = new Rectangle(0, P(4), P(68), P(68));
        using (var path = G.Cut(nb, P(12)))
        {
            using var b = new SolidBrush(Alpha(Yellow, 22)); g.FillPath(b, path);
            using var p = new Pen(Yellow, P(1.5f)); g.DrawPath(p, path);
        }
        G.Text(g, num, FNum, Yellow, nb, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        int x = P(88);
        G.Text(g, Fmt.Upper(kicker), FTiny, Cyan, new Rectangle(x, P(2), Width - x, P(16)), TextFormatFlags.Left);
        G.Text(g, Fmt.Upper(title), FTitle, Fg, new Rectangle(x, P(16), Width - x, P(36)), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        G.Text(g, desc, FBody, Muted, new Rectangle(x, P(54), Width - x, P(40)), TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
    }
}

// ───────────────────────────── Metin bloğu (otomatik yükseklik) ─────────────────────────────
public class TextBlock : HControl
{
    public Color TextColor = Muted;
    public TextBlock(string text, Font f = null, Color? color = null) { Text = text; Font = f ?? FBody; if (color.HasValue) TextColor = color.Value; Height = P(20); }
    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Fit(); Invalidate(); }
    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); Fit(); }
    void Fit()
    {
        if (Width <= 0) return;
        int h = TextRenderer.MeasureText(Text ?? "", Font, new Size(Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + P(4);
        if (h != Height) Height = h;
    }
    protected override void OnPaint(PaintEventArgs e) =>
        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, TextColor, TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.Top);
}

// ───────────────────────────── Rozet ─────────────────────────────
public class Badge : HControl
{
    public Color Color = Cyan;
    public Badge(string text, Color? c = null) { Text = text; if (c.HasValue) Color = c.Value; Height = P(32); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        var w = Math.Min(Width - 1, G.Measure(Text, FTiny).Width + P(46));
        var r = new Rectangle(0, 0, w, Height - 1);
        using (var b = new SolidBrush(Alpha(Color, 22))) g.FillRectangle(b, r);
        using (var p = new Pen(Alpha(Color, 150))) g.DrawRectangle(p, r);
        G.Check(g, new Rectangle(P(8), (Height - P(16)) / 2, P(16), P(16)), Color, P(2f));
        G.Text(g, Text, FTiny, Color, new Rectangle(P(30), 0, w - P(30), Height), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }
}

// ───────────────────────────── Uyarı şeridi ─────────────────────────────
public class HazardStripe : HControl
{
    public HazardStripe(string text) { Text = text; Height = P(34); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        var r = ClientRectangle;
        using (var b = new SolidBrush(Alpha(Amber, 18))) g.FillRectangle(b, r);
        g.SetClip(r);
        using (var yb = new SolidBrush(Alpha(Amber, 200)))
            for (int x = -Height; x < Width + Height; x += P(22))
                g.FillPolygon(yb, new[] { new Point(x, Height), new Point(x + P(10), Height), new Point(x + P(10) + Height, 0), new Point(x + Height, 0) });
        g.ResetClip();
        var ts = G.Measure(Text, FSmallBold);
        var tr = new Rectangle((Width - ts.Width) / 2 - P(14), P(5), ts.Width + P(28), Height - P(10));
        using (var bb = new SolidBrush(Bg)) g.FillRectangle(bb, tr);
        G.Text(g, Text, FSmallBold, Amber, tr, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

// ───────────────────────────── Büyük anahtar ─────────────────────────────
public class BigToggle : HControl
{
    bool on;
    public bool On { get => on; set { on = value; Invalidate(); } }
    public string Caption = "";
    public event EventHandler Requested;
    public BigToggle() { Height = P(64); Cursor = Cursors.Hand; }
    protected override void OnClick(EventArgs e) { base.OnClick(e); if (Enabled) Requested?.Invoke(this, EventArgs.Empty); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        var col = !Enabled ? Muted : on ? Red : Cyan;
        var sr = new RectangleF(1, P(8), P(130), P(48));
        using (var path = G.Round(sr, sr.Height / 2))
        {
            using var b = new SolidBrush(Alpha(col, 36)); g.FillPath(b, path);
            using var p = new Pen(col, P(2)); g.DrawPath(p, path);
        }
        float k = sr.Height - P(10);
        float kx = on ? sr.Right - k - P(5) : sr.X + P(5);
        using (var kb = new SolidBrush(Fg)) g.FillEllipse(kb, kx, sr.Y + P(5), k, k);
        var tr = on ? new Rectangle((int)sr.X + P(10), (int)sr.Y, (int)(sr.Width - k - P(16)), (int)sr.Height)
                    : new Rectangle((int)(sr.X + k + P(10)), (int)sr.Y, (int)(sr.Width - k - P(16)), (int)sr.Height);
        G.Text(g, on ? L.T("AÇIK") : L.T("KAPALI"), FHead, col, tr, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        G.Text(g, Caption, FHead, Fg, new Rectangle((int)sr.Right + P(20), 0, Width - (int)sr.Right - P(20), Height), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }
}

// ───────────────────────────── Kalkan ─────────────────────────────
public class Shield : HControl
{
    public Shield() { Height = P(150); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; G.Hq(g);
        float h = Height - P(12), w = h * .82f, x = (Width - w) / 2, y = P(6);
        var pts = new[]
        {
            new PointF(x + w / 2, y), new PointF(x + w, y + h * .16f), new PointF(x + w, y + h * .5f),
            new PointF(x + w / 2, y + h), new PointF(x, y + h * .5f), new PointF(x, y + h * .16f),
        };
        using (var gp = new GraphicsPath())
        {
            gp.AddPolygon(pts);
            using var b = new LinearGradientBrush(new RectangleF(x, y, w, h), Card2, Bg, 90f);
            g.FillPath(b, gp);
            using var glow = new Pen(Alpha(Yellow, 45), P(9)); g.DrawPath(glow, gp);
            using var p = new Pen(Yellow, P(3)); g.DrawPath(p, gp);
        }
        float lw = w * .34f, lh = lw * .8f, lx = x + (w - lw) / 2, ly = y + h * .44f;
        using (var sp = new Pen(Fg, P(5))) g.DrawArc(sp, lx + lw * .18f, ly - lw * .5f, lw * .64f, lw * .7f, 180, 180);
        using (var lb = new SolidBrush(Yellow)) g.FillRectangle(lb, lx, ly, lw, lh);
        using (var kb = new SolidBrush(Ink)) g.FillEllipse(kb, lx + lw / 2 - P(5), ly + lh * .3f, P(10), P(10));
    }
}
