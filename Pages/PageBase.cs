namespace FpsAtolyesi.Pages;

public class PageBase : UserControl
{
    /// <summary>İşlem sürerken devre dışı kalacak butonlar.</summary>
    protected readonly List<Control> Actions = new();

    public PageBase()
    {
        BackColor = Bg; ForeColor = Fg; Font = FBody; DoubleBuffered = true;
        Padding = new Padding(P(26), P(20), P(26), P(16));
        MainForm.I.BusyChanged += () =>
        {
            if (IsDisposed) return;
            foreach (var a in Actions) a.Enabled = !MainForm.I.Busy;
            OnBusy(MainForm.I.Busy);
        };
    }

    protected MainForm Main => MainForm.I;
    public virtual void OnShow() { }
    protected virtual void OnBusy(bool busy) { }

    protected Task<bool> Run(string label, Func<Task> work) => Main.RunBusy(label, work);

    /// <summary>
    /// Dock düzeni: önce "fill" eklenir, ardından dıştan içe sıralanmış kenar kontrolleri.
    /// Listede ilk sıradaki kontrol en dıştadır.
    /// </summary>
    public static void Compose(Control parent, Control fill, params (Control c, DockStyle d)[] outerToInner)
    {
        parent.SuspendLayout();
        if (fill != null) { fill.Dock = DockStyle.Fill; parent.Controls.Add(fill); }
        for (int i = outerToInner.Length - 1; i >= 0; i--)
        {
            var (c, d) = outerToInner[i];
            c.Dock = d;
            parent.Controls.Add(c);
        }
        parent.ResumeLayout();
    }

    public static Control Gap(int h) => new Panel { Height = P(h), Width = P(h), BackColor = Color.Transparent };

    public static FlowLayoutPanel ButtonRow(params Control[] items)
    {
        var f = new FlowLayoutPanel
        {
            Height = P(46), BackColor = Color.Transparent, FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
            Padding = new Padding(0, P(4), 0, 0), Margin = Padding.Empty,
        };
        foreach (var c in items) f.Controls.Add(c);
        f.Width = items.Sum(c => c.Width + c.Margin.Horizontal) + P(4);
        return f;
    }

    protected HudButton Btn(string text, bool primary, Action onClick, bool action = true)
    {
        var b = new HudButton(text, primary);
        b.Click += (_, _) => onClick();
        if (action) Actions.Add(b);
        return b;
    }

    protected static HudPanel Card(Control fill, params (Control c, DockStyle d)[] outerToInner)
    {
        var p = new HudPanel();
        Compose(p, fill, outerToInner);
        return p;
    }
}
