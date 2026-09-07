using System;
using System.Windows.Forms;

namespace DRS
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                LogWriter.LogOutput(LogLevels.Info, "DRSForm起動");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new DRSForm());
            }
            catch (Exception ex)
            {
                LogWriter.LogOutputSystemError(ex);
                Console.Error.WriteLine(ex);
            }
        }
    }
}
