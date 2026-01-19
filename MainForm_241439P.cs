using System.Reflection;
using System.Runtime.InteropServices;
using Doodle_241439P.Properties;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Doodle_241439P
{
    // Brush type enumeration
    public enum BrushType
    {
        Paintbrush,
        Crayon,
        Marker,
        Pencil,
        Airbrush,
        PureBlack
    }

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
        bool flagLine = false;
        bool flagSquare = false;
        bool flagCircle = false;
        bool flagNgon = false;
        Bitmap? loadedImage = null;  // Currently loaded image to place
        List<PlacedImage> placedImages = new List<PlacedImage>();  // All placed images on canvas
        PlacedImage? selectedImage = null;  // Currently selected image for dragging/resizing
        bool isDraggingImage = false;
        Point dragOffset = Point.Empty;
        string strText = "Doodle Painting";
        int brushSize = 30;   // mandatory options: 10,30,50,70
        int eraserSize = 30;  // mandatory options: 10,30,50,70
        BrushType currentBrushType = BrushType.Paintbrush;  // Current brush type
        Random random = new Random();  // For crayon texture effect
        string selectedFontName = "Arial";
        int selectedFontSize = 30;
        Point textPosition = new Point(0, 0);
        bool isDraggingText = false;
        Rectangle textBounds = Rectangle.Empty;
        Point previewMousePos = new Point(-1, -1);
        float imageScale = 1.0f;  // Scale factor for selected image (1.0 = 100%)
        Cursor? brushCursor = null;
        Cursor? eraserCursor = null;

        // Shape tool variables
        int ngonSides = 5;  // Default pentagon
        bool shapeFilled = false;  // Filled vs outlined mode
        bool isDrawingShape = false;  // Flag for shape drawing in progress
        Point shapeStartPoint = Point.Empty;  // Start point for shape drawing
        Point shapeEndPoint = Point.Empty;  // End point for shape drawing

        // Undo functionality
        private Stack<Bitmap> undoStack = new Stack<Bitmap>();
        private const int MAX_UNDO_LEVELS = 20;

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

        // Save current bitmap state to undo stack
        private void SaveUndoState()
        {
            if (bm == null) return;

            // Limit undo stack size
            if (undoStack.Count >= MAX_UNDO_LEVELS)
            {
                // Remove oldest item (at bottom of stack)
                var items = undoStack.ToArray();
                undoStack.Clear();
                for (int i = 1; i < items.Length; i++)
                {
                    undoStack.Push(items[i]);
                }
            }

            // Save a copy of current bitmap
            undoStack.Push((Bitmap)bm.Clone());
        }

        // Restore previous bitmap state
        private void PerformUndo()
        {
            if (undoStack.Count == 0)
            {
                MessageBox.Show("Nothing to undo!", "Undo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Restore previous state
            Bitmap previousState = undoStack.Pop();
            if (bm != null)
            {
                bm.Dispose();
            }
            bm = previousState;
            picBoxMain.Image = bm;
            picBoxMain.Invalidate();
        }

        // Override keyboard handler to intercept Ctrl+Z
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Z))
            {
                PerformUndo();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MainForm_241439P_Load(object sender, EventArgs e)
        {
            bm = new Bitmap(picBoxMain.Width, picBoxMain.Height);

            // Fill with initial background color
            using (Graphics g = Graphics.FromImage(bm))
            {
                g.Clear(Color.LightGray);
            }

            picBoxMain.Image = bm;

            // Initialize font and size
            selectedFontName = "Arial";
            selectedFontSize = 30;
            comboBoxFont.SelectedIndex = 0;

            // Initialize brush color display (default to brush with black color)
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            trackBarFontSize.Value = 30;
            lblFontSize.Text = "Size: 30pts";

            // Initialize text box with default text
            txtBoxText.Text = "Doodle Painting";

            // Initialize N-gon trackbar
            trackBarNgonSides.Value = 5;
            ngonSides = 5;
            lblNgonSides.Text = "Sides: 5";
            checkBoxShapeFilled.Checked = false;
            shapeFilled = false;

            // Initialize brush type ComboBox
            comboBoxBrushType.SelectedIndex = 0; // Default to Paintbrush
            currentBrushType = BrushType.Paintbrush;

            // Create icons for shape buttons (so they're visible in designer)
            CreateShapeButtonIcons();

            // Create custom cursors from icons
            // Hot spot at bottom center (tip of brush/eraser) for better precision
            brushCursor = CreateCursorFromBitmap(Properties.Resources.paint_brush, new Point(16, 28));
            eraserCursor = CreateCursorFromBitmap(Properties.Resources.eraser, new Point(16, 28));

            // Clear all borders initially
            ClearAllToolBorders();
            
            // Set initial tool to brush mode
            flagBrush = true;
            if (brushCursor != null)
                picBoxMain.Cursor = brushCursor;
            else
                picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            SetToolBorder(picBoxBrush);
        }

        // P/Invoke declarations for creating custom cursors
        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        private static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        // Helper method to create a cursor from a bitmap with custom hot spot
        private Cursor CreateCursorFromBitmap(Bitmap bitmap, Point hotSpot)
        {
            try
            {
                // Resize bitmap to 32x32 for cursor (standard size)
                Bitmap cursorBitmap = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(cursorBitmap))
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    // Draw the bitmap scaled to fit the cursor size
                    g.DrawImage(bitmap, 0, 0, 32, 32);
                }

                // Get icon handle
                IntPtr hIcon = cursorBitmap.GetHicon();
                IconInfo tmp = new IconInfo();
                GetIconInfo(hIcon, ref tmp);
                tmp.xHotspot = hotSpot.X;
                tmp.yHotspot = hotSpot.Y;
                tmp.fIcon = false; // This is a cursor, not an icon

                // Create cursor with custom hot spot
                IntPtr hCursor = CreateIconIndirect(ref tmp);

                // Clean up
                if (tmp.hbmColor != IntPtr.Zero) DeleteObject(tmp.hbmColor);
                if (tmp.hbmMask != IntPtr.Zero) DeleteObject(tmp.hbmMask);
                DestroyIcon(hIcon);

                return new Cursor(hCursor);
            }
            catch
            {
                // If cursor creation fails, return null to fall back to standard cursor
                return null;
            }
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

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

                    SaveUndoState();  // Save state before placing text
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
                    SaveUndoState();  // Save state before drawing/erasing
                    flagDraw = true;
                    endP = e.Location; // Initialize endP for continuous drawing
                }
            }
            else if (flagLine || flagSquare || flagCircle || flagNgon)
            {
                if (e.Button == MouseButtons.Left)
                {
                    SaveUndoState();  // Save state before drawing shape
                    isDrawingShape = true;
                    shapeStartPoint = e.Location;
                    shapeEndPoint = e.Location;
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
                        // Draw using the selected brush type
                        DrawWithBrushType(g, startP, endP);
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
            else if (isDrawingShape && (flagLine || flagSquare || flagCircle || flagNgon))
            {
                // Update shape preview while dragging
                shapeEndPoint = e.Location;
                picBoxMain.Invalidate(); // Trigger Paint event to show preview
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
            if (isDrawingShape && (flagLine || flagSquare || flagCircle || flagNgon))
            {
                // Finalize shape by drawing to bitmap
                shapeEndPoint = e.Location;
                DrawShapeToCanvas();
                isDrawingShape = false;
                shapeStartPoint = Point.Empty;
                shapeEndPoint = Point.Empty;
                picBoxMain.Invalidate();
            }
            flagDraw = false;
            isDraggingText = false;
            isDraggingImage = false;
        }

        private void picBoxRed_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brushPen.Color = picBoxRed.BackColor;
            brush.Color = picBoxRed.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxBrushColor.Image = null;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            flagErase = false;
        }

        private void picBoxBlack_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brushPen.Color = picBoxBlack.BackColor;
            brush.Color = picBoxBlack.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxBrushColor.Image = null;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            flagErase = false;
        }

        private void picBoxGreen_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brushPen.Color = picBoxGreen.BackColor;
            brush.Color = picBoxGreen.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxBrushColor.Image = null;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            flagErase = false;
        }

        private void picBoxBlue_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brushPen.Color = picBoxBlue.BackColor;
            brush.Color = picBoxBlue.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxBrushColor.Image = null;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            flagErase = false;
        }

        private void picBoxCyan_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brushPen.Color = picBoxCyan.BackColor;
            brush.Color = picBoxCyan.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxBrushColor.Image = null;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            flagErase = false;
        }

        private void picBoxMagenta_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brushPen.Color = picBoxMagenta.BackColor;
            brush.Color = picBoxMagenta.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxBrushColor.Image = null;
            picBoxMain.Cursor = Cursors.Default;
            flagErase = false;
        }

        private void picBoxYellow_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brushPen.Color = picBoxYellow.BackColor;
            brush.Color = picBoxYellow.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxBrushColor.Image = null;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            flagErase = false;
        }

        private void picBoxOrange_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brushPen.Color = picBoxOrange.BackColor;
            brush.Color = picBoxOrange.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxBrushColor.Image = null;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            flagErase = false;
        }

        private void picBoxWhite_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brushPen.Color = picBoxWhite.BackColor;
            brush.Color = picBoxWhite.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxBrushColor.Image = null;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            flagErase = false;
        }

        private void picBoxCustom_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.AllowFullOpen = true;
                colorDialog.FullOpen = true;
                colorDialog.Color = brushPen.Color;

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    brushPen.Color = colorDialog.Color;
                    brush.Color = colorDialog.Color;
                    picBoxBrushColor.BackColor = brushPen.Color;
                    picBoxCustom.BackColor = colorDialog.Color;
                    picBoxBrushColor.Image = null;
                    picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
                    flagErase = false;
                }
            }
        }

        private void picBoxClear_Click(object sender, EventArgs e)
        {
            SaveUndoState();  // Save state before clearing
            g = Graphics.FromImage(bm);
            Rectangle rect = picBoxMain.ClientRectangle;
            g.FillRectangle(new SolidBrush(Color.LightGray), rect);
            g.Dispose();
            picBoxMain.Invalidate();
        }

        private void picBoxErase_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brush = new SolidBrush(picBoxMain.BackColor);
            eraserPen.Color = picBoxMain.BackColor;
            eraserPen.Width = eraserSize;
            picBoxBrushColor.Image = Properties.Resources.eraser;
            picBoxBrushColor.BackColor = Color.Transparent;
            flagErase = true;
            flagText = false;
            flagBrush = false;
            flagLoad = false;
            flagLine = false;
            flagSquare = false;
            flagCircle = false;
            flagNgon = false;
            picBoxMain.Cursor = eraserCursor ?? Cursors.Cross;
            SetToolBorder(picBoxErase);
        }

        private void picBoxText_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            picBoxBrushColor.Image = Properties.Resources.text;
            picBoxBrushColor.BackColor = Color.Transparent;
            flagDraw = false;
            flagText = true;
            flagErase = false;
            flagBrush = false;
            flagLoad = false;
            flagLine = false;
            flagSquare = false;
            flagCircle = false;
            flagNgon = false;
            if (string.IsNullOrEmpty(txtBoxText.Text))
                txtBoxText.Text = "Doodle Painting";
            strText = txtBoxText.Text;
            textBounds = Rectangle.Empty; // Reset text bounds when entering text mode
            picBoxMain.Cursor = Cursors.IBeam;
            SetToolBorder(picBoxText);
            picBoxMain.Invalidate(); // Trigger preview
        }

        private void picBoxSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfdlg = new SaveFileDialog())
            {
                sfdlg.Title = "Save Image";
                sfdlg.Filter = "PNG with Transparency (*.PNG)|*.PNG|GIF Image (*.GIF)|*.GIF|JPEG Image (*.JPG)|*.JPG|All files (*.*)|*.*";
                sfdlg.FilterIndex = 1; // Default to PNG

                if (sfdlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        // Determine format based on filter index or file extension
                        System.Drawing.Imaging.ImageFormat format;
                        string extension = System.IO.Path.GetExtension(sfdlg.FileName).ToLower();
                        bool useTransparency = false;

                        // Determine format from filter index
                        switch (sfdlg.FilterIndex)
                        {
                            case 1: // PNG with Transparency
                                format = System.Drawing.Imaging.ImageFormat.Png;
                                useTransparency = true;
                                break;
                            case 2: // GIF
                                format = System.Drawing.Imaging.ImageFormat.Gif;
                                break;
                            case 3: // JPEG
                                format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                break;
                            default:
                                // Determine from extension
                                if (extension == ".png")
                                {
                                    format = System.Drawing.Imaging.ImageFormat.Png;
                                    useTransparency = true;
                                }
                                else if (extension == ".gif")
                                    format = System.Drawing.Imaging.ImageFormat.Gif;
                                else if (extension == ".jpg" || extension == ".jpeg")
                                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                else
                                    format = System.Drawing.Imaging.ImageFormat.Png;
                                break;
                        }

                        using (Bitmap bmp = new Bitmap(picBoxMain.Width, picBoxMain.Height))
                        {
                            if (useTransparency)
                            {
                                // For PNG with transparency: make unpainted areas transparent
                                using (Graphics g = Graphics.FromImage(bmp))
                                {
                                    g.Clear(Color.Transparent); // Start with transparent background

                                    // Draw the canvas bitmap (which has the gray background and all drawings)
                                    // We need to replace the gray background with transparency
                                    for (int y = 0; y < this.bm.Height; y++)
                                    {
                                        for (int x = 0; x < this.bm.Width; x++)
                                        {
                                            Color pixelColor = this.bm.GetPixel(x, y);
                                            // If pixel is the canvas background color (LightGray), make it transparent
                                            if (pixelColor.R == Color.LightGray.R &&
                                                pixelColor.G == Color.LightGray.G &&
                                                pixelColor.B == Color.LightGray.B)
                                            {
                                                bmp.SetPixel(x, y, Color.Transparent);
                                            }
                                            else
                                            {
                                                bmp.SetPixel(x, y, pixelColor);
                                            }
                                        }
                                    }

                                    // Draw any placed images on top (as overlay)
                                    if (flagLoad && placedImages.Count > 0)
                                    {
                                        g.CompositingMode = CompositingMode.SourceOver;
                                        g.CompositingQuality = CompositingQuality.HighQuality;
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                        foreach (PlacedImage placed in placedImages)
                                        {
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
                                            g.DrawImage(placed.Image, drawBounds);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // For GIF and JPEG: use regular DrawToBitmap (includes canvas and overlays)
                                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                                picBoxMain.DrawToBitmap(bmp, rect);
                            }

                            bmp.Save(sfdlg.FileName, format);
                            MessageBox.Show($"File saved successfully as {format.ToString()}", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                ofd.Filter = "Image Files(*.BMP;*.PNG;*.JPG;*.JPEG;*.GIF)|*.BMP;*.PNG;*.JPG;*.JPEG;*.GIF|BMP Files(*.BMP)|*.BMP|PNG Files(*.PNG)|*.PNG|JPEG Files(*.JPG;*.JPEG)|*.JPG;*.JPEG|GIF Files(*.GIF)|*.GIF|All files (*.*)|*.*";
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
                            // BMP, JPG, JPEG, GIF - load normally
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
                        flagLine = false;
                        flagSquare = false;
                        flagCircle = false;
                        flagNgon = false;
                        picBoxBrushColor.Image = Properties.Resources.image;
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
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            flagBrush = true;
            flagLoad = false;
            flagErase = false;
            flagText = false;
            flagLine = false;
            flagSquare = false;
            flagCircle = false;
            flagNgon = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
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

        // Draw with selected brush type
        private void DrawWithBrushType(Graphics g, Point start, Point end)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            
            switch (currentBrushType)
            {
                case BrushType.Paintbrush:
                    // Soft, painterly effect using rounded caps and ellipses
                    brushPen.Width = brushSize;
                    brushPen.StartCap = LineCap.Round;
                    brushPen.EndCap = LineCap.Round;
                    brushPen.LineJoin = LineJoin.Round;
                    int radius = brushSize / 2;
                    g.FillEllipse(new SolidBrush(brushPen.Color), end.X - radius, end.Y - radius, brushSize, brushSize);
                    g.DrawLine(brushPen, start, end);
                    break;

                case BrushType.Crayon:
                    // Rough, grainy texture with irregular edges
                    brushPen.Width = brushSize;
                    brushPen.StartCap = LineCap.Round;
                    brushPen.EndCap = LineCap.Round;
                    brushPen.LineJoin = LineJoin.Round;
                    // Add some texture by drawing multiple overlapping circles with slight variations
                    for (int i = 0; i < 3; i++)
                    {
                        int offsetX = random.Next(-brushSize / 4, brushSize / 4);
                        int offsetY = random.Next(-brushSize / 4, brushSize / 4);
                        int size = brushSize - random.Next(0, brushSize / 3);
                        g.FillEllipse(new SolidBrush(brushPen.Color), 
                            end.X - size / 2 + offsetX, 
                            end.Y - size / 2 + offsetY, 
                            size, size);
                    }
                    g.DrawLine(brushPen, start, end);
                    break;

                case BrushType.Marker:
                    // Clean, solid lines with sharp edges
                    brushPen.Width = brushSize;
                    brushPen.StartCap = LineCap.Square;
                    brushPen.EndCap = LineCap.Square;
                    brushPen.LineJoin = LineJoin.Miter;
                    g.CompositingMode = CompositingMode.SourceCopy; // Fully opaque
                    g.DrawLine(brushPen, start, end);
                    // Fill at end point
                    radius = brushSize / 2;
                    g.FillEllipse(new SolidBrush(brushPen.Color), end.X - radius, end.Y - radius, brushSize, brushSize);
                    g.CompositingMode = CompositingMode.SourceOver;
                    break;

                case BrushType.Pencil:
                    // Sharp, precise lines with hard edges
                    brushPen.Width = Math.Max(1, brushSize / 3); // Thinner for pencil
                    brushPen.StartCap = LineCap.Flat;
                    brushPen.EndCap = LineCap.Flat;
                    brushPen.LineJoin = LineJoin.Miter;
                    g.SmoothingMode = SmoothingMode.None; // Sharp edges
                    g.DrawLine(brushPen, start, end);
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    break;

                case BrushType.Airbrush:
                    // Very soft, feathered edges with gradient opacity
                    int airbrushSize = brushSize * 2; // Larger for airbrush effect
                    for (int i = airbrushSize; i > 0; i -= 2)
                    {
                        int alpha = (int)(255 * (1.0 - (double)i / airbrushSize));
                        if (alpha > 0)
                        {
                            using (SolidBrush airbrushBrush = new SolidBrush(Color.FromArgb(alpha, brushPen.Color)))
                            {
                                g.FillEllipse(airbrushBrush, 
                                    end.X - i / 2, 
                                    end.Y - i / 2, 
                                    i, i);
                            }
                        }
                    }
                    // Draw connecting line with gradient
                    DrawGradientLine(g, start, end, brushSize);
                    break;

                case BrushType.PureBlack:
                    // Completely opaque, sharp edges
                    brushPen.Width = brushSize;
                    brushPen.StartCap = LineCap.Square;
                    brushPen.EndCap = LineCap.Square;
                    brushPen.LineJoin = LineJoin.Miter;
                    Color pureColor = Color.FromArgb(255, 0, 0, 0); // Pure black
                    if (brushPen.Color.R > 128 || brushPen.Color.G > 128 || brushPen.Color.B > 128)
                    {
                        // If not black, use the selected color but fully opaque
                        pureColor = Color.FromArgb(255, brushPen.Color);
                    }
                    using (Pen purePen = new Pen(pureColor, brushSize))
                    using (SolidBrush pureBrush = new SolidBrush(pureColor))
                    {
                        purePen.StartCap = LineCap.Square;
                        purePen.EndCap = LineCap.Square;
                        purePen.LineJoin = LineJoin.Miter;
                        g.CompositingMode = CompositingMode.SourceCopy;
                        g.DrawLine(purePen, start, end);
                        radius = brushSize / 2;
                        g.FillEllipse(pureBrush, end.X - radius, end.Y - radius, brushSize, brushSize);
                        g.CompositingMode = CompositingMode.SourceOver;
                    }
                    break;
            }
        }

        // Helper method for airbrush gradient line
        private void DrawGradientLine(Graphics g, Point start, Point end, int size)
        {
            int steps = Math.Max(10, Math.Abs(end.X - start.X) + Math.Abs(end.Y - start.Y));
            for (int i = 0; i < steps; i++)
            {
                double t = (double)i / steps;
                int x = (int)(start.X + (end.X - start.X) * t);
                int y = (int)(start.Y + (end.Y - start.Y) * t);
                
                // Draw with decreasing opacity from center
                for (int j = size; j > 0; j -= 2)
                {
                    int alpha = (int)(180 * (1.0 - (double)j / size));
                    if (alpha > 0)
                    {
                        using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, brushPen.Color)))
                        {
                            g.FillEllipse(brush, x - j / 2, y - j / 2, j, j);
                        }
                    }
                }
            }
        }

        // Draw shape to canvas (finalize)
        private void DrawShapeToCanvas()
        {
            if (bm == null) return;

            g = Graphics.FromImage(bm);
            g.SmoothingMode = SmoothingMode.HighQuality;
            // Use brushPen.Width to ensure it matches the current brush size setting
            Pen shapePen = new Pen(brushPen.Color, brushPen.Width);
            SolidBrush shapeBrush = new SolidBrush(brushPen.Color);

            bool shiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            if (flagLine)
            {
                g.DrawLine(shapePen, shapeStartPoint, shapeEndPoint);
            }
            else if (flagSquare)
            {
                Rectangle rect = CalculateRectangle(shapeStartPoint, shapeEndPoint, shiftPressed);
                if (shapeFilled)
                {
                    g.FillRectangle(shapeBrush, rect);
                }
                g.DrawRectangle(shapePen, rect);
            }
            else if (flagCircle)
            {
                Rectangle rect = CalculateRectangle(shapeStartPoint, shapeEndPoint, shiftPressed);
                if (shapeFilled)
                {
                    g.FillEllipse(shapeBrush, rect);
                }
                g.DrawEllipse(shapePen, rect);
            }
            else if (flagNgon)
            {
                // Use Ctrl for regular (equal-sided) polygon, like Shift for square/circle
                bool ctrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                Point[] points = CalculateNgonPoints(shapeStartPoint, shapeEndPoint, ctrlPressed);
                if (points != null && points.Length > 0)
                {
                    if (shapeFilled)
                    {
                        g.FillPolygon(shapeBrush, points);
                    }
                    g.DrawPolygon(shapePen, points);
                }
            }

            shapePen.Dispose();
            shapeBrush.Dispose();
            g.Dispose();
        }

        // Calculate rectangle from two points, optionally making it square if Shift is pressed
        private Rectangle CalculateRectangle(Point start, Point end, bool makeSquare)
        {
            int x = Math.Min(start.X, end.X);
            int y = Math.Min(start.Y, end.Y);
            int width = Math.Abs(end.X - start.X);
            int height = Math.Abs(end.Y - start.Y);

            if (makeSquare)
            {
                int size = Math.Max(width, height);
                width = size;
                height = size;
            }

            return new Rectangle(x, y, width, height);
        }

        // Calculate N-gon points (polygon that fits bounding rectangle, or regular if Ctrl pressed)
        private Point[] CalculateNgonPoints(Point start, Point end, bool makeRegular)
        {
            // Calculate bounding rectangle (same logic as square/circle)
            int x = Math.Min(start.X, end.X);
            int y = Math.Min(start.Y, end.Y);
            int width = Math.Abs(end.X - start.X);
            int height = Math.Abs(end.Y - start.Y);
            
            // Calculate center
            int centerX = x + width / 2;
            int centerY = y + height / 2;
            
            // Calculate radius X and Y (for ellipse-like polygon)
            int radiusX = width / 2;
            int radiusY = height / 2;
            
            // If Ctrl is pressed, make it regular (equal sides) using smaller dimension
            if (makeRegular)
            {
                int size = Math.Min(width, height);
                radiusX = size / 2;
                radiusY = size / 2;
            }
            
            // Ensure minimum size
            if (radiusX < 1) radiusX = 1;
            if (radiusY < 1) radiusY = 1;

            Point[] points = new Point[ngonSides];
            double angleStep = 2 * Math.PI / ngonSides;
            double startAngle = -Math.PI / 2; // Start at top

            for (int i = 0; i < ngonSides; i++)
            {
                double angle = startAngle + i * angleStep;
                // Use radiusX and radiusY to create ellipse-like polygon (or circle if regular)
                int px = centerX + (int)(radiusX * Math.Cos(angle));
                int py = centerY + (int)(radiusY * Math.Sin(angle));
                points[i] = new Point(px, py);
            }

            return points;
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

            // Draw shape preview while dragging
            if (isDrawingShape && (flagLine || flagSquare || flagCircle || flagNgon))
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                // Use brushPen.Width to ensure it matches the current brush size setting
                using (Pen previewPen = new Pen(Color.FromArgb(180, brushPen.Color), brushPen.Width))
                using (SolidBrush previewBrush = new SolidBrush(Color.FromArgb(128, brushPen.Color)))
                {
                    bool shiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

                    if (flagLine)
                    {
                        e.Graphics.DrawLine(previewPen, shapeStartPoint, shapeEndPoint);
                    }
                    else if (flagSquare)
                    {
                        Rectangle rect = CalculateRectangle(shapeStartPoint, shapeEndPoint, shiftPressed);
                        if (shapeFilled)
                        {
                            e.Graphics.FillRectangle(previewBrush, rect);
                        }
                        e.Graphics.DrawRectangle(previewPen, rect);
                    }
                    else if (flagCircle)
                    {
                        Rectangle rect = CalculateRectangle(shapeStartPoint, shapeEndPoint, shiftPressed);
                        if (shapeFilled)
                        {
                            e.Graphics.FillEllipse(previewBrush, rect);
                        }
                        e.Graphics.DrawEllipse(previewPen, rect);
                    }
                    else if (flagNgon)
                    {
                        // Use Ctrl for regular (equal-sided) polygon
                        bool ctrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                        Point[] points = CalculateNgonPoints(shapeStartPoint, shapeEndPoint, ctrlPressed);
                        if (points != null && points.Length > 0)
                        {
                            if (shapeFilled)
                            {
                                e.Graphics.FillPolygon(previewBrush, points);
                            }
                            e.Graphics.DrawPolygon(previewPen, points);
                        }
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

            SaveUndoState();  // Save state before stamping image

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
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
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

        // Auto-stamp image when switching tools from load mode
        private void AutoStampOnToolSwitch()
        {
            if (flagLoad && selectedImage != null)
            {
                StampImage(selectedImage);
            }
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
            // Clear borders on all tool buttons
            picBoxBrush.BorderStyle = BorderStyle.None;
            picBoxLoad.BorderStyle = BorderStyle.None;
            picBoxErase.BorderStyle = BorderStyle.None;
            picBoxText.BorderStyle = BorderStyle.None;
            picBoxLine.BorderStyle = BorderStyle.None;
            picBoxSquare.BorderStyle = BorderStyle.None;
            picBoxCircle.BorderStyle = BorderStyle.None;
            picBoxNgon.BorderStyle = BorderStyle.None;
        }

        private void SetToolBorder(PictureBox tool)
        {
            // Clear all borders first
            ClearAllToolBorders();
            // Set solid border only on the selected tool
            if (tool != null)
            {
                tool.BorderStyle = BorderStyle.FixedSingle;
            }
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
            
            // Update shape preview if drawing a shape
            if (isDrawingShape && (flagLine || flagSquare || flagCircle || flagNgon))
            {
                picBoxMain.Invalidate();
            }
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

        private void comboBoxBrushType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxBrushType.SelectedIndex >= 0)
            {
                currentBrushType = (BrushType)comboBoxBrushType.SelectedIndex;
            }
        }

        private void lblImageScale_Click(object sender, EventArgs e)
        {

        }

        // Shape tool click handlers
        private void picBoxLine_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            flagLine = true;
            flagSquare = false;
            flagCircle = false;
            flagNgon = false;
            flagBrush = false;
            flagLoad = false;
            flagErase = false;
            flagText = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxMain.Cursor = Cursors.Cross;
            SetToolBorder(picBoxLine);
        }

        private void picBoxSquare_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            flagLine = false;
            flagSquare = true;
            flagCircle = false;
            flagNgon = false;
            flagBrush = false;
            flagLoad = false;
            flagErase = false;
            flagText = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxMain.Cursor = Cursors.Cross;
            SetToolBorder(picBoxSquare);
        }

        private void picBoxCircle_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            flagLine = false;
            flagSquare = false;
            flagCircle = true;
            flagNgon = false;
            flagBrush = false;
            flagLoad = false;
            flagErase = false;
            flagText = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxMain.Cursor = Cursors.Cross;
            SetToolBorder(picBoxCircle);
        }

        private void picBoxNgon_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            flagLine = false;
            flagSquare = false;
            flagCircle = false;
            flagNgon = true;
            flagBrush = false;
            flagLoad = false;
            flagErase = false;
            flagText = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxMain.Cursor = Cursors.Cross;
            SetToolBorder(picBoxNgon);
        }

        private void trackBarNgonSides_ValueChanged(object sender, EventArgs e)
        {
            ngonSides = trackBarNgonSides.Value;
            lblNgonSides.Text = $"Sides: {ngonSides}";
        }

        private void checkBoxShapeFilled_CheckedChanged(object sender, EventArgs e)
        {
            shapeFilled = checkBoxShapeFilled.Checked;
        }

        // Create bitmap icons for shape buttons
        private void CreateShapeButtonIcons()
        {
            // Square icon
            Bitmap squareIcon = new Bitmap(30, 30);
            using (Graphics g = Graphics.FromImage(squareIcon))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(5, 5, 20, 20);
                g.DrawRectangle(new Pen(Color.Black, 2), rect);
            }
            picBoxSquare.Image = squareIcon;

            // Circle icon
            Bitmap circleIcon = new Bitmap(30, 30);
            using (Graphics g = Graphics.FromImage(circleIcon))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(5, 5, 20, 20);
                g.DrawEllipse(new Pen(Color.Black, 2), rect);
            }
            picBoxCircle.Image = circleIcon;

            // N-gon icon (pentagon)
            Bitmap ngonIcon = new Bitmap(30, 30);
            using (Graphics g = Graphics.FromImage(ngonIcon))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Point[] points = new Point[]
                {
                    new Point(15, 5),    // Top
                    new Point(25, 12),   // Right
                    new Point(22, 22),   // Bottom right
                    new Point(8, 22),    // Bottom left
                    new Point(5, 12)     // Left
                };
                g.DrawPolygon(new Pen(Color.Black, 2), points);
            }
            picBoxNgon.Image = ngonIcon;
        }

        // Paint handlers for shape icon previews (backup if images fail to load)
        private void picBoxSquare_Paint(object sender, PaintEventArgs e)
        {
            if (picBoxSquare.Image == null)
            {
                Rectangle rect = new Rectangle(5, 5, 20, 20);
                e.Graphics.DrawRectangle(new Pen(Color.Black, 1), rect);
            }
        }

        private void picBoxCircle_Paint(object sender, PaintEventArgs e)
        {
            if (picBoxCircle.Image == null)
            {
                Rectangle rect = new Rectangle(5, 5, 20, 20);
                e.Graphics.DrawEllipse(new Pen(Color.Black, 1), rect);
            }
        }

        private void picBoxNgon_Paint(object sender, PaintEventArgs e)
        {
            if (picBoxNgon.Image == null)
            {
                // Draw a pentagon icon (scaled for 30x30)
                Point[] points = new Point[]
                {
                    new Point(15, 5),    // Top
                    new Point(25, 12),   // Right
                    new Point(22, 22),   // Bottom right
                    new Point(8, 22),    // Bottom left
                    new Point(5, 12)     // Left
                };
                e.Graphics.DrawPolygon(new Pen(Color.Black, 1), points);
            }
        }

        private void lblFont_Click(object sender, EventArgs e)
        {

        }

        private void trackBarImageScale_Scroll(object sender, EventArgs e)
        {

        }

        private void lblFontSize_Click(object sender, EventArgs e)
        {

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
