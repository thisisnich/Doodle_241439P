using System;
using System.Drawing;
using System.Windows.Forms;
using Doodle_241439P.Properties;

namespace Doodle_241439P
{
    public partial class SplashScreen : Form
    {
        private System.Windows.Forms.Timer timer = null!; // Set in InitializeComponent()
        private int displayTime = 2000; // Display for 2 seconds

        public SplashScreen()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.BackColor = Color.LightGray;
            
            // Load splash image from embedded resources
            try
            {
                if (Resources.splash != null)
                {
                    this.BackgroundImage = Resources.splash;
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                    this.Size = Resources.splash.Size;
                }
                else
                {
                    // If image not found, create a default splash
                    this.Size = new Size(600, 400);
                    this.Paint += SplashScreen_Paint;
                }
            }
            catch
            {
                // If image loading fails, use default size
                this.Size = new Size(600, 400);
                this.Paint += SplashScreen_Paint;
            }
            
            // Timer to close splash screen
            timer = new System.Windows.Forms.Timer();
            timer.Interval = displayTime;
            timer.Tick += Timer_Tick;
            timer.Start();
            
            this.ResumeLayout(false);
        }

        private void SplashScreen_Paint(object? sender, PaintEventArgs e)
        {
            // Draw default splash screen if image not found
            Graphics g = e.Graphics;
            g.Clear(Color.LightGray);
            
            // Draw title
            using (Font titleFont = new Font("Arial", 32, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                string title = "Doodle";
                SizeF textSize = g.MeasureString(title, titleFont);
                float x = (this.Width - textSize.Width) / 2;
                float y = (this.Height - textSize.Height) / 2;
                g.DrawString(title, titleFont, brush, x, y);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timer?.Stop();
            timer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
