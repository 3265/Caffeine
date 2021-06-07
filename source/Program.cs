using System;
using System.Threading;
using System.Windows.Forms;

namespace Caffeine
{
    static class Program
    {
        private static Mutex _Mutex;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew;
            _Mutex = new Mutex(true, "EB06A900-686A-45A0-B2EE-30B8A8A0981A", out createdNew); // Allow only one instance to run
            if (!createdNew)
            {
                MessageBox.Show("Caffeine is already running!", "Caffeine", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainWindow());
            }
        }
    }
}
