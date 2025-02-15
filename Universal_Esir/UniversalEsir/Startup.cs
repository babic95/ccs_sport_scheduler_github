using UniversalEsir_Logging;
using UniversalEsir_Mutex;
using UniversalEsir_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UniversalEsir
{
    public class Startup
    {
        [STAThread]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.0.0")]
        public static void Main()
        {
            using (var appLock = new SingleInstanceApplicationLock())
            {
                if (!appLock.TryAcquireExclusiveLock())
                {
                    MessageBox.Show("CCS UniversalEsir je već pokrenut!", "", MessageBoxButton.OK, MessageBoxImage.Information);

                    return;
                }

                Logger.ConfigureLog(SettingsManager.Instance.GetLoggingFolderPath());
                App app = new App();
                app.InitializeComponent();
                app.Run(new MainWindow());
            }
        }
    }
}
