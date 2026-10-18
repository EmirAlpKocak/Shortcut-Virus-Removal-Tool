using System;
using System.Windows.Forms;

namespace Shortcut_Virus_Removal_Tool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Lang.Initialize();
            string osLanguage = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            Lang.culture = osLanguage.ToLower();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
