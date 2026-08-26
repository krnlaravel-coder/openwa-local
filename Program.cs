using System;
using System.Windows.Forms;
using System.Threading;

namespace WhtsApi
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Start the Local API Server in the background
            LocalApiServer apiServer = new LocalApiServer();
            Thread apiThread = new Thread(new ThreadStart(apiServer.Start));
            apiThread.IsBackground = true;
            apiThread.Start();

            // Run the Main Form
            Application.Run(new MainForm());
        }
    }
}
