namespace Doodle_241439P
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Show splash screen
            using (SplashScreen splash = new SplashScreen())
            {
                splash.Show();
                Application.DoEvents(); // Process splash screen display
            
                // Load main form while splash is showing
                MainForm_241439P mainForm = new MainForm_241439P();
                
                // Wait a bit for splash to display, then show main form
                System.Threading.Thread.Sleep(500);
                splash.Hide();
                
                // Show main form
                Application.Run(mainForm);
            }
        }
    }
}