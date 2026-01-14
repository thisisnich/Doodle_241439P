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
        Pen eraserPen = new Pen(Color.LightGray, 30);  // Pen for eraser drawing
        SolidBrush brush = new SolidBrush(Color.Black);
        Point startP = new Point(0, 0);
        Point endP = new Point(0, 0);
        bool flagDraw = false;
        bool flagErase = false;
        bool flagText = false;
        bool flagBrush = false;
        bool flagPen = false;
        string strText = "Doodle Painting";
        int penSize = 2;
        int brushSize = 30;   // mandatory options: 10,30,50,70
        int eraserSize = 30;  // mandatory options: 10,30,50,70
        string selectedFontName = "Arial";
        int selectedFontSize = 30;
        Point textPosition = new Point(0, 0);
        bool isDraggingText = false;
        Rectangle textBounds = Rectangle.Empty;
        Point previewMousePos = new Point(-1, -1);

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
            
            // Initialize font and size
            selectedFontName = "Arial";
            selectedFontSize = 30;
            comboBoxFont.SelectedIndex = 0;
            trackBarFontSize.Value = 30;
            lblFontSize.Text = "Size: 30pts";
        }

        private void picBoxMain_MouseDown(object sender, MouseEventArgs e)
        {
            startP = e.Location;
            
            if (flagText)
            {
                if (textBounds != Rectangle.Empty && textBounds.Contains(e.Location))
                {
                    // Start dragging existing text
                    isDraggingText = true;
                }
                else
                {
                    // Place new text
                    strText = txtBoxText.Text;
                    if (string.IsNullOrEmpty(strText))
                        strText = "Doodle Painting";
                    
                    textPosition = e.Location;
                    DrawTextOnCanvas();
                    picBoxMain.Invalidate();
                    
                    // After placing text, allow drawing symbols
                    // User can switch to Brush or Pen tool to draw
                }
            }
            else if (flagBrush || flagPen || flagErase)
            {
                if (e.Button == MouseButtons.Left)
                {
                    flagDraw = true;
                    endP = e.Location; // Initialize endP for continuous drawing
                }
            }
        }

        private void picBoxMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (flagText && isDraggingText)
            {
                // Drag text to new position
                textPosition = e.Location;
                DrawTextOnCanvas();
                picBoxMain.Invalidate();
            }
            else if (flagDraw == true && (flagBrush == true || flagPen == true || flagErase == true))
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
                    // Eraser: Use line drawing to avoid gaps when moving fast
                    eraserPen.Width = eraserSize;
                    eraserPen.Color = picBoxMain.BackColor; // Use canvas background color
                    eraserPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    eraserPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    eraserPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                    // Draw line to connect points smoothly
                    g.DrawLine(eraserPen, startP, endP);
                    // Fill ellipse at end point for complete coverage
                    int radius = eraserSize / 2;
                    g.FillEllipse(new SolidBrush(picBoxMain.BackColor), endP.X - radius, endP.Y - radius, eraserSize, eraserSize);
                }
                g.Dispose();
                picBoxMain.Invalidate();
            }
            else if (flagText && !isDraggingText)
            {
                // Update preview position when hovering in text mode
                previewMousePos = e.Location;
                picBoxMain.Invalidate();
            }
            startP = endP;
        }

        private void picBoxMain_MouseUp(object sender, MouseEventArgs e)
        {
            flagDraw = false;
            isDraggingText = false;
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
            eraserPen.Color = picBoxMain.BackColor;
            eraserPen.Width = eraserSize;
            picBoxBrushColor.Image = Properties.Resources.eraser;
            flagErase = true;
            flagText = false;
            flagBrush = false;
            flagPen = false;
            SetToolBorder(picBoxErase);
        }

        private void picBoxText_Click(object sender, EventArgs e)
        {
            picBoxBrushColor.Image = Properties.Resources.text;
            flagDraw = false;
            flagText = true;
            flagErase = false;
            flagBrush = false;
            flagPen = false;
            if (string.IsNullOrEmpty(txtBoxText.Text))
                txtBoxText.Text = "Doodle Painting";
            strText = txtBoxText.Text;
            textBounds = Rectangle.Empty; // Reset text bounds when entering text mode
            SetToolBorder(picBoxText);
            picBoxMain.Invalidate(); // Trigger preview
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
            strText = txtBoxText.Text;
            if (flagText)
            {
                picBoxMain.Invalidate(); // Update preview
            }
        }

        private void DrawTextOnCanvas()
        {
            if (bm == null || string.IsNullOrEmpty(strText))
                return;

            g = Graphics.FromImage(bm);
            Font font = new Font(selectedFontName, selectedFontSize);
            SolidBrush textBrush = new SolidBrush(pen.Color);
            
            // Draw text
            g.DrawString(strText, font, textBrush, textPosition.X, textPosition.Y);
            
            // Calculate text bounds for dragging
            SizeF textSize = g.MeasureString(strText, font);
            Rectangle textRect = new Rectangle(textPosition.X, textPosition.Y, 
                (int)textSize.Width, (int)textSize.Height);
            textBounds = textRect;
            
            textBrush.Dispose();
            font.Dispose();
            g.Dispose();
        }

        private void picBoxMain_Paint(object sender, PaintEventArgs e)
        {
            // Draw preview when in text mode (only when no text is placed yet or hovering)
            if (flagText && !isDraggingText && previewMousePos.X >= 0 && previewMousePos.Y >= 0)
            {
                string previewText = string.IsNullOrEmpty(txtBoxText.Text) ? "Doodle Painting" : txtBoxText.Text;
                
                if (previewMousePos.X >= 0 && previewMousePos.Y >= 0 && 
                    previewMousePos.X < picBoxMain.Width && previewMousePos.Y < picBoxMain.Height)
                {
                    using (Font font = new Font(selectedFontName, selectedFontSize))
                    using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(128, pen.Color)))
                    {
                        e.Graphics.DrawString(previewText, font, textBrush, previewMousePos.X, previewMousePos.Y);
                    }
                }
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
            int value = trackBarBrushSize.Value;
            
            // If Control is held, snap to nearest preset (10, 30, 50, 70)
            if (Control.ModifierKeys == Keys.Control)
            {
                value = SnapToPresetSize(value);
                // Temporarily remove handler to avoid recursion
                trackBarBrushSize.ValueChanged -= trackBarBrushSize_ValueChanged;
                trackBarBrushSize.Value = value;
                trackBarBrushSize.ValueChanged += trackBarBrushSize_ValueChanged;
            }
            
            brushSize = value;
            lblBrushSize.Text = $"Brush: {brushSize}pts";
            brushPen.Width = brushSize;
        }

        private void trackBarEraserSize_ValueChanged(object sender, EventArgs e)
        {
            int value = trackBarEraserSize.Value;
            
            // If Control is held, snap to nearest preset (10, 30, 50, 70)
            if (Control.ModifierKeys == Keys.Control)
            {
                value = SnapToPresetSize(value);
                // Temporarily remove handler to avoid recursion
                trackBarEraserSize.ValueChanged -= trackBarEraserSize_ValueChanged;
                trackBarEraserSize.Value = value;
                trackBarEraserSize.ValueChanged += trackBarEraserSize_ValueChanged;
            }
            
            eraserSize = value;
            eraserPen.Width = eraserSize; // Update eraser pen width
            lblEraserSize.Text = $"Eraser: {eraserSize}pts";
        }

        private void trackBarFontSize_ValueChanged(object sender, EventArgs e)
        {
            int value = trackBarFontSize.Value;
            
            // If Control is held, snap to nearest preset (10, 30, 50, 70)
            if (Control.ModifierKeys == Keys.Control)
            {
                value = SnapToPresetSize(value);
                // Temporarily remove handler to avoid recursion
                trackBarFontSize.ValueChanged -= trackBarFontSize_ValueChanged;
                trackBarFontSize.Value = value;
                trackBarFontSize.ValueChanged += trackBarFontSize_ValueChanged;
            }
            
            selectedFontSize = value;
            lblFontSize.Text = $"Size: {selectedFontSize}pts";
            if (flagText)
            {
                picBoxMain.Invalidate(); // Update preview
            }
        }

        private static int SnapToPresetSize(int value)
        {
            // Preset sizes: 10, 30, 50, 70
            int[] presets = { 10, 30, 50, 70 };
            int nearest = presets[0];
            int minDiff = Math.Abs(value - presets[0]);
            
            foreach (int preset in presets)
            {
                int diff = Math.Abs(value - preset);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearest = preset;
                }
            }
            
            return nearest;
        }

        private void comboBoxFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxFont.SelectedItem != null)
            {
                selectedFontName = comboBoxFont.SelectedItem.ToString() ?? "Arial";
                if (flagText)
                {
                    picBoxMain.Invalidate(); // Update preview
                }
            }
        }

    }
}
