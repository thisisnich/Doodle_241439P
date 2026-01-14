using System.Reflection;
using System.Runtime.InteropServices;
using Doodle_241439P.Properties;
using System.Drawing.Drawing2D;

namespace Doodle_241439P
{
    public partial class MainForm_241439P : Form
    {
        Bitmap bm;
        Graphics g;
        Pen brushPen = new Pen(Color.Black, 8);  // Thick pen for brush drawing
        Pen eraserPen = new Pen(Color.LightGray, 30);  // Pen for eraser drawing
        SolidBrush brush = new SolidBrush(Color.Black);
        Point startP = new Point(0, 0);
        Point endP = new Point(0, 0);
        bool flagDraw = false;
        bool flagErase = false;
        bool flagText = false;
        bool flagBrush = false;
        bool flagLoad = false;  // Load/picture select mode
        Bitmap? loadedImage = null;  // Currently loaded image to place
        List<PlacedImage> placedImages = new List<PlacedImage>();  // All placed images on canvas
        PlacedImage? selectedImage = null;  // Currently selected image for dragging/resizing
        bool isDraggingImage = false;
        Point dragOffset = Point.Empty;
        string strText = "Doodle Painting";
        int brushSize = 30;   // mandatory options: 10,30,50,70
        int eraserSize = 30;  // mandatory options: 10,30,50,70
        string selectedFontName = "Arial";
        int selectedFontSize = 30;
        Point textPosition = new Point(0, 0);
        bool isDraggingText = false;
        Rectangle textBounds = Rectangle.Empty;
        Point previewMousePos = new Point(-1, -1);
        float imageScale = 1.0f;  // Scale factor for selected image (1.0 = 100%)

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
            else if (flagBrush || flagErase)
            {
                if (e.Button == MouseButtons.Left)
                {
                    flagDraw = true;
                    endP = e.Location; // Initialize endP for continuous drawing
                }
            }
            else if (flagLoad)
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Check if clicking on an existing image
                    selectedImage = GetImageAtPoint(e.Location);

                    if (selectedImage != null)
                    {
                        // Check if clicking on the selected image to drag it
                        if (GetScaledBounds(selectedImage).Contains(e.Location))
                        {
                            // Start dragging
                            isDraggingImage = true;
                            dragOffset = new Point(e.Location.X - selectedImage.Bounds.X,
                                                   e.Location.Y - selectedImage.Bounds.Y);
                        }
                        else
                        {
                            // Clicked outside selected image - deselect it
                            selectedImage = null;
                            ShowImageControls(false);
                            picBoxMain.Invalidate();
                        }
                    }
                    else if (loadedImage != null)
                    {
                        // Stamp any existing selected image first
                        if (selectedImage != null)
                        {
                            StampImage(selectedImage);
                        }

                        // Place new image at click position
                        PlaceNewImage(loadedImage, e.Location);
                        selectedImage = placedImages[placedImages.Count - 1]; // Select the newly placed image
                        imageScale = 1.0f; // Reset scale
                        trackBarImageScale.Value = 100;
                        ShowImageControls(true);
                    }
                }
            }
        }

        private void picBoxMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (flagLoad && isDraggingImage && selectedImage != null)
            {
                // Update image position
                int newX = e.Location.X - dragOffset.X;
                int newY = e.Location.Y - dragOffset.Y;
                selectedImage.Bounds = new Rectangle(newX, newY, selectedImage.Bounds.Width, selectedImage.Bounds.Height);
                
                // Redraw all placed images (preserves stamped content and other drawings)
                // This will redraw the dragged image at its new position
                RedrawCanvas();
            }
            else if (flagText && isDraggingText)
            {
                // Drag text to new position
                textPosition = e.Location;
                DrawTextOnCanvas();
                picBoxMain.Invalidate();
            }
            else if (flagDraw == true && (flagBrush == true || flagErase == true))
            {
                endP = e.Location;
                g = Graphics.FromImage(bm);
                if (flagErase == false)
                {
                    if (flagBrush == true)
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
            else if (flagLoad)
            {
                // Update cursor when hovering over images/resize handles
                UpdateCursorForImageMode(e.Location);
            }
            startP = endP;
        }

        private void picBoxMain_MouseUp(object sender, MouseEventArgs e)
        {
            flagDraw = false;
            isDraggingText = false;
            isDraggingImage = false;
        }

        private void picBoxRed_Click(object sender, EventArgs e)
        {
            brushPen.Color = picBoxRed.BackColor;
            brush.Color = picBoxRed.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxBlack_Click(object sender, EventArgs e)
        {
            brushPen.Color = picBoxBlack.BackColor;
            brush.Color = picBoxBlack.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxGreen_Click(object sender, EventArgs e)
        {
            brushPen.Color = picBoxGreen.BackColor;
            brush.Color = picBoxGreen.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxBlue_Click(object sender, EventArgs e)
        {
            brushPen.Color = picBoxBlue.BackColor;
            brush.Color = picBoxBlue.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxCyan_Click(object sender, EventArgs e)
        {
            brushPen.Color = picBoxCyan.BackColor;
            brush.Color = picBoxCyan.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxMagenta_Click(object sender, EventArgs e)
        {
            brushPen.Color = picBoxMagenta.BackColor;
            brush.Color = picBoxMagenta.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxYellow_Click(object sender, EventArgs e)
        {
            brushPen.Color = picBoxYellow.BackColor;
            brush.Color = picBoxYellow.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            if (!flagText)
                picBoxBrushColor.Image = null;
            flagErase = false;
        }

        private void picBoxOrange_Click(object sender, EventArgs e)
        {
            brushPen.Color = picBoxOrange.BackColor;
            brush.Color = picBoxOrange.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
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
            flagLoad = false;
            SetToolBorder(picBoxErase);
        }

        private void picBoxText_Click(object sender, EventArgs e)
        {
            picBoxBrushColor.Image = Properties.Resources.text;
            flagDraw = false;
            flagText = true;
            flagErase = false;
            flagBrush = false;
            flagLoad = false;
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

        private void picBoxLoad_Click(object sender, EventArgs e)
        {
            // Open file dialog to select image
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Load Image";
                ofd.Filter = "Image Files(*.BMP;*.PNG;*.JPG;*.JPEG)|*.BMP;*.PNG;*.JPG;*.JPEG|BMP Files(*.BMP)|*.BMP|PNG Files(*.PNG)|*.PNG|JPEG Files(*.JPG;*.JPEG)|*.JPG;*.JPEG|All files (*.*)|*.*";
                ofd.FilterIndex = 1;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Dispose previous image if exists
                        if (loadedImage != null)
                        {
                            loadedImage.Dispose();
                            loadedImage = null;
                        }

                        // Load image with transparency support for PNG
                        string filePath = ofd.FileName;
                        string extension = System.IO.Path.GetExtension(filePath).ToLower();

                        Bitmap originalImage;
                        if (extension == ".png")
                        {
                            // PNG with transparency support
                            originalImage = new Bitmap(filePath);
                        }
                        else
                        {
                            // BMP, JPG, JPEG - load normally
                            originalImage = new Bitmap(filePath);
                        }

                        // Shrink to fit canvas by default
                        loadedImage = ShrinkImageToFit(originalImage);
                        if (loadedImage != originalImage)
                        {
                            originalImage.Dispose(); // Dispose original if it was resized
                        }

                        flagLoad = true;
                        flagBrush = false;
                        flagErase = false;
                        flagText = false;
                        picBoxBrushColor.Image = null;
                        picBoxBrushColor.BackColor = Color.Transparent;
                        SetToolBorder(picBoxLoad);

                        MessageBox.Show("Image loaded. Click on canvas to place it.", "Load Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        flagLoad = false;
                    }
                }
            }
        }

        private void picBoxBrush_Click(object sender, EventArgs e)
        {
            flagBrush = true;
            flagLoad = false;
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
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Font font = new Font(selectedFontName, selectedFontSize);
            SolidBrush textBrush = new SolidBrush(brushPen.Color);

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
            // Draw all placed images as temporary overlay (not yet stamped to bitmap)
            if (flagLoad && placedImages.Count > 0)
            {
                e.Graphics.CompositingMode = CompositingMode.SourceOver;
                e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                
                foreach (PlacedImage placed in placedImages)
                {
                    // Apply scaling if this is the selected image
                    Rectangle drawBounds;
                    if (placed == selectedImage && imageScale != 1.0f)
                    {
                        int scaledWidth = (int)(placed.Bounds.Width * imageScale);
                        int scaledHeight = (int)(placed.Bounds.Height * imageScale);
                        int scaledX = placed.Bounds.X + (placed.Bounds.Width - scaledWidth) / 2;
                        int scaledY = placed.Bounds.Y + (placed.Bounds.Height - scaledHeight) / 2;
                        drawBounds = new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight);
                    }
                    else
                    {
                        drawBounds = placed.Bounds;
                    }
                    e.Graphics.DrawImage(placed.Image, drawBounds);
                }
            }
            
            // Draw border around selected image in load mode
            if (flagLoad && selectedImage != null && !isDraggingImage)
            {
                Rectangle bounds = GetScaledBounds(selectedImage);
                using (Pen borderPen = new Pen(Color.Blue, 2))
                {
                    e.Graphics.DrawRectangle(borderPen, bounds);
                }
            }

            // Draw preview when in text mode (only when no text is placed yet or hovering)
            if (flagText && !isDraggingText && previewMousePos.X >= 0 && previewMousePos.Y >= 0)
            {
                string previewText = string.IsNullOrEmpty(txtBoxText.Text) ? "Doodle Painting" : txtBoxText.Text;

                if (previewMousePos.X >= 0 && previewMousePos.Y >= 0 &&
                    previewMousePos.X < picBoxMain.Width && previewMousePos.Y < picBoxMain.Height)
                {
                    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    using (Font font = new Font(selectedFontName, selectedFontSize))
                    using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(128, brushPen.Color)))
                    {
                        e.Graphics.DrawString(previewText, font, textBrush, previewMousePos.X, previewMousePos.Y);
                    }
                }
            }
        }


        // Helper method to shrink image to fit canvas
        private Bitmap ShrinkImageToFit(Bitmap originalImage)
        {
            int maxWidth = picBoxMain.Width;
            int maxHeight = picBoxMain.Height;

            double scaleX = (double)maxWidth / originalImage.Width;
            double scaleY = (double)maxHeight / originalImage.Height;
            double scale = Math.Min(scaleX, scaleY);

            // Only shrink if image is larger than canvas
            if (scale < 1.0)
            {
                int newWidth = (int)(originalImage.Width * scale);
                int newHeight = (int)(originalImage.Height * scale);

                Bitmap resized = new Bitmap(newWidth, newHeight);
                using (Graphics g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                }
                return resized;
            }

            return new Bitmap(originalImage);
        }

        // Place a new image on the canvas
        private void PlaceNewImage(Bitmap image, Point position)
        {
            // Shrink image to fit canvas if needed
            Bitmap resizedImage = ShrinkImageToFit(image);

            Rectangle bounds = new Rectangle(position.X, position.Y, resizedImage.Width, resizedImage.Height);
            PlacedImage placed = new PlacedImage(resizedImage, bounds);
            placedImages.Add(placed);

            RedrawCanvas();
        }

        // Get image at a specific point
        private PlacedImage? GetImageAtPoint(Point point)
        {
            // Check from back to front (last added is on top)
            for (int i = placedImages.Count - 1; i >= 0; i--)
            {
                Rectangle bounds = GetScaledBounds(placedImages[i]);
                if (bounds.Contains(point))
                {
                    return placedImages[i];
                }
            }
            return null;
        }

        // Get scaled bounds for an image (if selected and scaled)
        private Rectangle GetScaledBounds(PlacedImage image)
        {
            if (image == selectedImage && imageScale != 1.0f)
            {
                int scaledWidth = (int)(image.Bounds.Width * imageScale);
                int scaledHeight = (int)(image.Bounds.Height * imageScale);
                int scaledX = image.Bounds.X + (image.Bounds.Width - scaledWidth) / 2;
                int scaledY = image.Bounds.Y + (image.Bounds.Height - scaledHeight) / 2;
                return new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight);
            }
            return image.Bounds;
        }


        // Invalidate canvas to trigger Paint event (placed images are drawn in Paint, not directly to bitmap)
        private void RedrawCanvas()
        {
            // Just invalidate - placed images will be drawn in Paint event as temporary overlay
            picBoxMain.Invalidate();
        }

        // Stamp image permanently onto the canvas (draw RGB pixels directly)
        private void StampImage(PlacedImage image)
        {
            if (bm == null || image == null) return;

            // Calculate the actual bounds to draw (with scaling if selected)
            Rectangle drawBounds;
            if (image == selectedImage && imageScale != 1.0f)
            {
                int scaledWidth = (int)(image.Bounds.Width * imageScale);
                int scaledHeight = (int)(image.Bounds.Height * imageScale);
                int scaledX = image.Bounds.X + (image.Bounds.Width - scaledWidth) / 2;
                int scaledY = image.Bounds.Y + (image.Bounds.Height - scaledHeight) / 2;
                drawBounds = new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight);
            }
            else
            {
                drawBounds = image.Bounds;
            }

            // Draw image pixels directly onto the canvas bitmap - this permanently adds RGB pixels
            g = Graphics.FromImage(bm);
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            
            // Draw the image - this permanently adds RGB pixels to the canvas
            g.DrawImage(image.Image, drawBounds);
            g.Dispose();

            // IMPORTANT: Remove from placed images list so it can't be selected/moved anymore
            placedImages.Remove(image);
            
            if (selectedImage == image)
            {
                selectedImage = null;
                ShowImageControls(false);
            }

            // Switch back to brush mode after stamping
            flagLoad = false;
            flagBrush = true;
            flagErase = false;
            flagText = false;
            SetToolBorder(picBoxBrush);
            
            // Invalidate to refresh display (stamped image is now part of the bitmap)
            picBoxMain.Invalidate();
        }

        // Show/hide image controls (scale slider and OK button)
        private void ShowImageControls(bool show)
        {
            trackBarImageScale.Visible = show;
            lblImageScale.Visible = show;
            btnStampImage.Visible = show;
        }

        // Handle image scale slider change
        private void trackBarImageScale_ValueChanged(object sender, EventArgs e)
        {
            imageScale = trackBarImageScale.Value / 100.0f;
            lblImageScale.Text = $"Scale: {trackBarImageScale.Value}%";
            if (selectedImage != null)
            {
                RedrawCanvas();
            }
        }

        // Handle OK/Stamp button click
        private void btnStampImage_Click(object sender, EventArgs e)
        {
            if (selectedImage != null)
            {
                StampImage(selectedImage);
            }
        }

        // Update cursor based on what's under mouse
        private void UpdateCursorForImageMode(Point location)
        {
            PlacedImage? img = GetImageAtPoint(location);
            if (img != null)
            {
                picBoxMain.Cursor = Cursors.SizeAll;
            }
            else
            {
                picBoxMain.Cursor = Cursors.Default;
            }
        }

        private void ClearAllToolBorders()
        {
            picBoxBrush.BorderStyle = BorderStyle.None;
            picBoxLoad.BorderStyle = BorderStyle.None;
            picBoxErase.BorderStyle = BorderStyle.None;
            picBoxText.BorderStyle = BorderStyle.None;
        }

        private void SetToolBorder(PictureBox tool)
        {
            ClearAllToolBorders();
            tool.BorderStyle = BorderStyle.FixedSingle;
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

    // Class to represent a placed image on the canvas
    public class PlacedImage
    {
        public Bitmap Image { get; set; }
        public Rectangle Bounds { get; set; }
        
        public PlacedImage(Bitmap image, Rectangle bounds)
        {
            Image = image;
            Bounds = bounds;
        }
    }
    
    // Enum for resize handles
    public enum ResizeHandle
    {
        None,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Top,
        Bottom,
        Left,
        Right
    }
}
