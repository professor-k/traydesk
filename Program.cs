using System;
using System.Threading;
using System.Windows.Forms;
using log4net;
using log4net.Config;

namespace TrayDesk
{
    static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            XmlConfigurator.Configure();
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Log.Info("TrayDesk started");
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayDeskApplicationContext());
            Log.Info("TrayDesk shutting down");
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Log.Error("Unhandled exception", e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("Unhandled exception", e.ExceptionObject as Exception);
        }
    }
}
