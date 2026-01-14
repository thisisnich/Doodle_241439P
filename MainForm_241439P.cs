using System.Reflection;
using System.Runtime.InteropServices;
using Doodle_241439P.Properties;

namespace Doodle_241439P
{
    public partial class MainForm_241439P : Form
    {
        Bitmap bm;
        Graphics g;
        Pen pen = new Pen(Color.Black, 2);  // Thin pen for precise drawing
        Pen brushPen = new Pen(Color.Black, 8);  // Thick pen for brush drawing
        SolidBrush brush = new SolidBrush(Color.Black);
        Point startP = new Point(0, 0);
        Point endP = new Point(0, 0);
        bool flagDraw = false;
        bool flagErase = false;
        bool flagText = false;
        bool flagBrush = false;
        bool flagPen = false;
        string strText;
        int penSize = 2;
        int brushSize = 8;

        public MainForm_241439P()
        {
            InitializeComponent();
        }

        private void nameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            Clipboard.SetText(attribute.Value.ToString());
        }

        private void adminNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Handler for admin number menu item
        }

        private void MainForm_241439P_Load(object sender, EventArgs e)
        {
            bm = new Bitmap(picBoxMain.Width, picBoxMain.Height);
            picBoxMain.Image = bm;
        }

        private void picBoxMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (flagBrush == false && flagPen == false)
                return;

            startP = e.Location;
            if (flagText == false)
            {
                if (e.Button == MouseButtons.Left)
                    flagDraw = true;
            }
            else
            {
                strText = txtBoxText.Text;
                g = Graphics.FromImage(bm);
                Font font = new Font("Arial", 12);
                SolidBrush textBrush = new SolidBrush(pen.Color);
                g.DrawString(strText, font, textBrush, startP.X, startP.Y);
                textBrush.Dispose();
                g.Dispose();
                picBoxMain.Invalidate();
            }
        }

        private void picBoxMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (flagDraw == true && (flagBrush == true || flagPen == true))
            {
                endP = e.Location;
                g = Graphics.FromImage(bm);
                if (flagErase == false)
                {
                    if (flagPen == true)
                    {
                        // Pen: Sharp, precise lines with square caps
                        pen.Width = penSize;
                        pen.StartCap = System.Drawing.Drawing2D.LineCap.Square;
                        pen.EndCap = System.Drawing.Drawing2D.LineCap.Square;
                        g.DrawLine(pen, startP, endP);
                    }
                    else if (flagBrush == true)
                    {
                        // Brush: Soft, painterly effect using rounded caps and ellipses
                        brushPen.Width = brushSize;
                        brushPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                        brushPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                        brushPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                        // Draw with ellipses for a more brush-like, painterly effect
                        int radius = brushSize / 2;
                        g.FillEllipse(new SolidBrush(brushPen.Color), endP.X - radius, endP.Y - radius, brushSize, brushSize);
                        g.DrawLine(brushPen, startP, endP);
                    }
                }
                else
                {
                    g.FillRectangle(brush, endP.X, endP.Y, 20, 20);
                }
                g.Dispose();
                picBoxMain.Invalidate();
            }
            startP = endP;
        }

        private void picBoxMain_MouseUp(object sender, MouseEventArgs e)
        {
            flagDraw = false;
        }

        private void picBoxRed_Click(object sender, EventArgs e)
        {
            pen.Color = picBoxRed.BackColor;
            brushPen.Color = picBoxRed.BackColor;
            brush.Color = picBoxRed.BackColor;
            picBoxBrushColor.BackColor = pen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxBlack_Click(object sender, EventArgs e)
        {
            pen.Color = picBoxBlack.BackColor;
            brushPen.Color = picBoxBlack.BackColor;
            brush.Color = picBoxBlack.BackColor;
            picBoxBrushColor.BackColor = pen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxGreen_Click(object sender, EventArgs e)
        {
            pen.Color = picBoxGreen.BackColor;
            brushPen.Color = picBoxGreen.BackColor;
            brush.Color = picBoxGreen.BackColor;
            picBoxBrushColor.BackColor = pen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxBlue_Click(object sender, EventArgs e)
        {
            pen.Color = picBoxBlue.BackColor;
            brushPen.Color = picBoxBlue.BackColor;
            brush.Color = picBoxBlue.BackColor;
            picBoxBrushColor.BackColor = pen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxCyan_Click(object sender, EventArgs e)
        {
            pen.Color = picBoxCyan.BackColor;
            brushPen.Color = picBoxCyan.BackColor;
            brush.Color = picBoxCyan.BackColor;
            picBoxBrushColor.BackColor = pen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxMagenta_Click(object sender, EventArgs e)
        {
            pen.Color = picBoxMagenta.BackColor;
            brushPen.Color = picBoxMagenta.BackColor;
            brush.Color = picBoxMagenta.BackColor;
            picBoxBrushColor.BackColor = pen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxYellow_Click(object sender, EventArgs e)
        {
            pen.Color = picBoxYellow.BackColor;
            brushPen.Color = picBoxYellow.BackColor;
            brush.Color = picBoxYellow.BackColor;
            picBoxBrushColor.BackColor = pen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxOrange_Click(object sender, EventArgs e)
        {
            pen.Color = picBoxOrange.BackColor;
            brushPen.Color = picBoxOrange.BackColor;
            brush.Color = picBoxOrange.BackColor;
            picBoxBrushColor.BackColor = pen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxClear_Click(object sender, EventArgs e)
        {
            g = Graphics.FromImage(bm);
            Rectangle rect = picBoxMain.ClientRectangle;
            g.FillRectangle(new SolidBrush(Color.LightGray), rect);
            g.Dispose();
            picBoxMain.Invalidate();
        }

        private void picBoxErase_Click(object sender, EventArgs e)
        {
            brush = new SolidBrush(picBoxMain.BackColor);
            picBoxBrushColor.Image = Properties.Resources.eraser;
            flagErase = true;
            flagText = false;
            SetToolBorder(picBoxErase);
        }

        private void picBoxText_Click(object sender, EventArgs e)
        {
            picBoxBrushColor.Image = Properties.Resources.text;
            flagDraw = false;
            flagText = true;
            flagErase = false;
            SetToolBorder(picBoxText);
        }

        private void picBoxSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfdlg = new SaveFileDialog())
            {
                sfdlg.Title = "Save Dialog";
                sfdlg.Filter = "Image Files(*.BMP)|*.BMP|All files (*.*)|*.*";
                if (sfdlg.ShowDialog(this) == DialogResult.OK)
                {
                    using (Bitmap bmp = new Bitmap(picBoxMain.Width, picBoxMain.Height))
                    {
                        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                        picBoxMain.DrawToBitmap(bmp, rect);
                        bmp.Save(sfdlg.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        MessageBox.Show("File Saved Successfully");
                    }
                }
            }
        }

        private void picBoxPen_Click(object sender, EventArgs e)
        {
            flagPen = true;
            flagBrush = false;
            flagErase = false;
            flagText = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = pen.Color;
            SetToolBorder(picBoxPen);
        }

        private void picBoxBrush_Click(object sender, EventArgs e)
        {
            flagBrush = true;
            flagPen = false;
            flagErase = false;
            flagText = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            SetToolBorder(picBoxBrush);
        }

        private void txtBoxText_TextChanged(object sender, EventArgs e)
        {

        }

        private void picBoxBrush_Paint(object sender, PaintEventArgs e)
        {
            // Draw "BRUSH" text on the Brush tool
            using (Font font = new Font("Arial", 8, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                e.Graphics.DrawString("BRUSH", font, textBrush, 5, 15);
            }
        }

        private void ClearAllToolBorders()
        {
            picBoxBrush.BorderStyle = BorderStyle.None;
            picBoxPen.BorderStyle = BorderStyle.None;
            picBoxErase.BorderStyle = BorderStyle.None;
            picBoxText.BorderStyle = BorderStyle.None;
        }

        private void SetToolBorder(PictureBox tool)
        {
            ClearAllToolBorders();
            tool.BorderStyle = BorderStyle.FixedSingle;
        }

        private void trackBarPenSize_ValueChanged(object sender, EventArgs e)
        {
            penSize = trackBarPenSize.Value;
            lblPenSize.Text = $"Pen: {penSize}px";
            pen.Width = penSize;
        }

        private void trackBarBrushSize_ValueChanged(object sender, EventArgs e)
        {
            brushSize = trackBarBrushSize.Value;
            lblBrushSize.Text = $"Brush: {brushSize}px";
            brushPen.Width = brushSize;
        }
    }
}
