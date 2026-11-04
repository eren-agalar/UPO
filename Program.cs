using System.Security.Principal;

namespace FpsAtolyesi;

public static class Program
{
    static Mutex mutex;

    [STAThread]
    static void Main(string[] args)
    {
        Settings.Load();
        L.Init();
        // Yeniden açılışta eski pencerenin kapanmasını bekle (en fazla 15 sn).
        bool restart = args.Contains("--restart");
        mutex = new Mutex(false, @"Global\UPOUltimatePerformanceOptimizer");
        bool first;
        try { first = mutex.WaitOne(restart ? 15000 : 0); } catch (AbandonedMutexException) { first = true; }
        if (!first)
        {
            MessageBox.Show(L.T("UPO zaten açık."), "UPO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.ThreadException += (_, e) => Fatal(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Fatal(e.ExceptionObject as Exception);

        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            MessageBox.Show(L.T("UPO'nun Windows ayarlarını değiştirebilmesi için yönetici olarak çalıştırılması gerekir.\n\nDosyaya sağ tıklayıp \"Yönetici olarak çalıştır\"ı seçin."),
                "UPO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Theme.Init();
        State.Load();
        Backup.Load();
        MainForm.StartArgs = args.Where(a => a != "--restart").ToArray();
        Application.Run(new MainForm());
        try { mutex.ReleaseMutex(); } catch { }
    }

    /// <summary>Uygulamayı kapatıp yeniden açar (dil/renk değişikliği için). Yönetici yetkisi korunur.</summary>
    /// <summary>Güncelleme kurulumu için uygulamayı hemen kapatır.</summary>
    public static void ExitNow()
    {
        try { mutex.ReleaseMutex(); } catch { }
        Environment.Exit(0);
    }

    public static void Restart()
    {
        try
        {
            Process.Start(new ProcessStartInfo(Environment.ProcessPath, "--restart") { UseShellExecute = false });
            try { mutex.ReleaseMutex(); } catch { }
            Environment.Exit(0);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "UPO", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    static void Fatal(Exception ex)
    {
        try { File.AppendAllText(Path.Combine(Paths.Data, "hata.log"), $"[{DateTime.Now}] {ex}\n\n"); } catch { }
        try { Log.Err(L.T("Beklenmeyen hata: ") + ex?.Message); } catch { }
        MessageBox.Show(L.T("Beklenmeyen bir hata oluştu:\n\n") + ex?.Message + L.T("\n\nAyrıntılar: ") + Path.Combine(Paths.Data, "hata.log"),
            "UPO", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
