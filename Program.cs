using System;
using System.Security.Principal;
using System.Windows.Forms;

namespace GTO5_Tool
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (IsElevated == false)
            {
                const string message = "You need to run this program as Administrator !";
                const string title = "Error!";
                MessageBox.Show(message, title);
                Environment.Exit(1);
            }
            Application.Run(new Form1());
            
        }
        private static bool IsElevated
        {
            get
            {
                var id = WindowsIdentity.GetCurrent();
                return id.Owner != id.User;
            }
        }
    }
}