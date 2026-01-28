using System.Reflection;
using System.Runtime.InteropServices;
using Doodle_241439P.Properties;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
// WPF imports only used in CreateEmojiBitmap method - using fully qualified names to avoid conflicts

namespace Doodle_241439P
{
    // Brush type enumeration
    public enum BrushType
    {
        Pen,        // Solid, fully opaque pen (default)
        Paintbrush,
        Marker,
        Pencil,
        Airbrush,
        WetBrush  // Speed-based paint brush with variable thickness
    }

    public partial class MainForm_241439P : Form
    {
        Bitmap bm;
        Graphics g;
        System.Drawing.Pen brushPen = new System.Drawing.Pen(Color.Black, 8);  // Thick pen for brush drawing
        System.Drawing.Pen eraserPen = new System.Drawing.Pen(Color.LightGray, 30);  // Pen for eraser drawing
        SolidBrush brush = new SolidBrush(Color.Black);
        Point startP = new Point(0, 0);
        Point endP = new Point(0, 0);
        bool flagDraw = false;
        bool flagErase = false;
        bool flagText = false;
        bool flagBrush = false;
        bool flagFill = false;  // Paint fill/bucket tool
        bool flagLoad = false;  // Load/picture select mode
        bool flagEmoji = false;  // Emoji placement mode
        bool flagLine = false;
        bool flagSquare = false;
        bool flagCircle = false;
        bool flagNgon = false;
        Bitmap? loadedImage = null;  // Currently loaded image to place
        string emojiText = "💀";  // Default emoji text
        List<PlacedImage> placedImages = new List<PlacedImage>();  // All placed images on canvas
        PlacedImage? selectedImage = null;  // Currently selected image for dragging/resizing
        bool isDraggingImage = false;
        Point dragOffset = Point.Empty;
        string strText = "Doodle Painting";
        int brushSize = 30;   // mandatory options: 10,30,50,70
        int eraserSize = 30;  // mandatory options: 10,30,50,70
        BrushType currentBrushType = BrushType.Pen;  // Current brush type (default: solid pen)
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

        // Speed tracking for wet brush
        DateTime lastMouseMoveTime = DateTime.Now;
        Point lastMousePosition = Point.Empty;

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
            Assembly assembly = Assembly.GetExecutingAssembly();
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            Clipboard.SetText(attribute.Value.ToString());
        }

        private void imageFiltersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap sourceImage = null;
            bool applyToSelectedImage = false;

            // Determine what to filter
            if ((flagLoad || flagEmoji) && selectedImage != null)
            {
                // Filter the selected placed image
                sourceImage = new Bitmap(selectedImage.Image);
                applyToSelectedImage = true;
            }
            else if (bm != null)
            {
                // Filter the entire canvas
                sourceImage = new Bitmap(bm);
                applyToSelectedImage = false;
            }
            else
            {
                MessageBox.Show("No image or canvas content to filter.", "Image Filters", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open filter dialog
            using (ImageFilterDialog filterDialog = new ImageFilterDialog(sourceImage))
            {
                if (filterDialog.ShowDialog(this) == DialogResult.OK && filterDialog.FilteredImage != null)
                {
                    SaveUndoState(); // Save state before applying filter

                    if (applyToSelectedImage && selectedImage != null)
                    {
                        // Apply filter to selected image
                        selectedImage.Image.Dispose();
                        selectedImage.Image = new Bitmap(filterDialog.FilteredImage);
                        RedrawCanvas();
                    }
                    else
                    {
                        // Apply filter to entire canvas
                        if (bm != null)
                        {
                            bm.Dispose();
                        }
                        bm = new Bitmap(filterDialog.FilteredImage);
                        picBoxMain.Image = bm;
                        picBoxMain.Invalidate();
                    }
                }
            }

            sourceImage?.Dispose();
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

            // Initialize brush color display (default to brush with paint brush icon)
            picBoxBrushColor.Image = Properties.Resources.paint_brush;
            picBoxBrushColor.BackColor = Color.Transparent;

            // Initialize text box with default text
            txtBoxText.Text = "Doodle Painting";

            // Initialize emoji text (default)
            emojiText = "💀";

            // Initialize N-gon trackbar
            trackBarNgonSides.Value = 5;
            ngonSides = 5;
            lblNgonSides.Text = "Sides: 5";
            checkBoxShapeFilled.Checked = false;
            shapeFilled = false;

            // Initialize brush type
            currentBrushType = BrushType.Pen;

            // Set brush as default tool on load
            flagBrush = true;
            flagFill = false;
            flagLoad = false;
            flagEmoji = false;
            flagErase = false;
            flagText = false;
            flagLine = false;
            flagSquare = false;
            flagCircle = false;
            flagNgon = false;

            // Create icons for shape buttons (so they're visible in designer)
            CreateShapeButtonIcons();

            // Create custom cursors from icons
            // Hot spot at bottom center (tip of brush/eraser) for better precision
            brushCursor = CreateCursorFromBitmap(Properties.Resources.paint_brush, new Point(16, 28));
            eraserCursor = CreateCursorFromBitmap(Properties.Resources.eraser, new Point(16, 28));

            // Initialize unified slider (defaults to brush size)
            UpdateUnifiedSlider();

            // Initialize unified ComboBox (defaults to brush type) - will show brush type selector since flagBrush is true
            UpdateUnifiedComboBox();

            // Set brush tool as selected (border and cursor)
            SetToolBorder(picBoxBrush);
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;

            // Setup tooltips for all tools
            SetupTooltips();
        }

        // Setup tooltips for all tools
        private void SetupTooltips()
        {
            // Main drawing tools
            toolTip.SetToolTip(picBoxBrush, "Brush Tool - Draw with various brush types");
            toolTip.SetToolTip(picBoxErase, "Eraser Tool - Erase parts of your drawing");
            toolTip.SetToolTip(picBoxText, "Text Tool - Add text to your drawing");
            toolTip.SetToolTip(picBoxFill, "Fill Tool - Fill areas with color");
            toolTip.SetToolTip(picBoxEmoji, "Emoji Tool - Add emoji stamps");
            toolTip.SetToolTip(picBoxLoad, "Load Image - Load an image file");
            toolTip.SetToolTip(picBoxSave, "Save - Save your drawing");
            toolTip.SetToolTip(picBoxClear, "Clear - Clear the entire canvas");

            // Shape tools
            toolTip.SetToolTip(picBoxLine, "Line Tool - Draw straight lines");
            toolTip.SetToolTip(picBoxSquare, "Square Tool - Draw rectangles");
            toolTip.SetToolTip(picBoxCircle, "Circle Tool - Draw circles and ellipses");
            toolTip.SetToolTip(picBoxNgon, "Polygon Tool - Draw polygons with adjustable sides");

            // Color tools
            toolTip.SetToolTip(picBoxBrushColor, "Current Brush Color");
            toolTip.SetToolTip(picBoxBlack, "Black");
            toolTip.SetToolTip(picBoxRed, "Red");
            toolTip.SetToolTip(picBoxGreen, "Green");
            toolTip.SetToolTip(picBoxBlue, "Blue");
            toolTip.SetToolTip(picBoxCyan, "Cyan");
            toolTip.SetToolTip(picBoxMagenta, "Magenta");
            toolTip.SetToolTip(picBoxYellow, "Yellow");
            toolTip.SetToolTip(picBoxOrange, "Orange");
            toolTip.SetToolTip(picBoxWhite, "White");
            toolTip.SetToolTip(picBoxPurple, "Purple");
            toolTip.SetToolTip(picBoxBrown, "Brown");
            toolTip.SetToolTip(picBoxCustom, "Eyedropper - Pick color from canvas");

            // Other controls
            toolTip.SetToolTip(btnStampImage, "Stamp Image - Confirm and place the image");
            toolTip.SetToolTip(trackBarUnified, "Adjust size or scale");
            toolTip.SetToolTip(comboBoxUnified, "Select brush type or font");
            toolTip.SetToolTip(trackBarNgonSides, "Adjust number of polygon sides");
            toolTip.SetToolTip(checkBoxShapeFilled, "Fill shapes with color");
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
            else if (flagFill)
            {
                if (e.Button == MouseButtons.Left)
                {
                    SaveUndoState();  // Save state before filling
                    PerformFloodFill(e.Location);
                    picBoxMain.Invalidate();
                }
            }
            else if (flagBrush || flagErase)
            {
                if (e.Button == MouseButtons.Left)
                {
                    SaveUndoState();  // Save state before drawing/erasing
                    flagDraw = true;
                    endP = e.Location; // Initialize endP for continuous drawing
                    // Initialize speed tracking for wet brush
                    lastMouseMoveTime = DateTime.Now;
                    lastMousePosition = e.Location;
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
            else if (flagLoad || flagEmoji)
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Check if clicking on an existing image/emoji
                    PlacedImage? clickedImage = GetImageAtPoint(e.Location);

                    if (clickedImage != null)
                    {
                        // Clicking on an existing image/emoji
                        Rectangle bounds = GetScaledBounds(clickedImage);
                        if (bounds.Contains(e.Location))
                        {
                            // Clicking on the image - select it and start dragging
                            selectedImage = clickedImage;
                            isDraggingImage = true;
                            // Calculate offset from click position to the visual (scaled) bounds
                            // This ensures dragging works correctly regardless of scale
                            dragOffset = new Point(e.Location.X - bounds.X,
                                                   e.Location.Y - bounds.Y);
                            ShowImageControls(true);
                            UpdateUnifiedSlider(); // Update slider when image is selected
                            picBoxMain.Invalidate();
                        }
                    }
                    else
                    {
                        // Clicking on empty space
                        if (flagLoad && loadedImage != null)
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
                            ShowImageControls(true);
                            UpdateUnifiedSlider(); // Update slider when new image is placed
                        }
                        else if (flagEmoji && !string.IsNullOrEmpty(emojiText))
                        {
                            // For emoji mode, only allow one emoji at a time
                            // Remove any existing emojis first
                            if (placedImages.Count > 0)
                            {
                                // Find and remove all existing emojis (they're in placedImages)
                                // We'll identify them by checking if they're currently selected or by removing all
                                List<PlacedImage> emojisToRemove = new List<PlacedImage>(placedImages);
                                foreach (PlacedImage img in emojisToRemove)
                                {
                                    if (selectedImage == img)
                                    {
                                        selectedImage = null;
                                    }
                                    placedImages.Remove(img);
                                    img.Image.Dispose(); // Clean up the bitmap
                                }
                                RedrawCanvas();
                            }

                            // Convert emoji to bitmap and place it
                            Bitmap emojiBitmap = CreateEmojiBitmap(emojiText, selectedFontSize * 2);
                            PlaceNewImage(emojiBitmap, e.Location);
                            selectedImage = placedImages[placedImages.Count - 1]; // Select the newly placed emoji
                            imageScale = 1.0f; // Reset scale
                            ShowImageControls(true);
                            UpdateUnifiedSlider(); // Update slider when new emoji is placed
                        }
                        else
                        {
                            // No image/emoji to place, just deselect
                            if (selectedImage != null)
                            {
                                selectedImage = null;
                                ShowImageControls(false);
                                UpdateUnifiedSlider();
                                picBoxMain.Invalidate();
                            }
                        }
                    }
                }
            }
        }

        private void picBoxMain_MouseMove(object sender, MouseEventArgs e)
        {
            if ((flagLoad || flagEmoji) && isDraggingImage && selectedImage != null)
            {
                // Calculate new visual position based on mouse location and drag offset
                // Allow dragging outside canvas bounds - no edge snapping
                int newVisualX = e.Location.X - dragOffset.X;
                int newVisualY = e.Location.Y - dragOffset.Y;
                
                // Convert visual position back to actual bounds position
                // If image is scaled, we need to account for the centering offset
                int actualX, actualY;
                if (imageScale != 1.0f)
                {
                    // When scaled, the visual bounds are centered on the actual bounds
                    // So we need to reverse the centering calculation
                    int scaledWidth = (int)(selectedImage.Bounds.Width * imageScale);
                    int scaledHeight = (int)(selectedImage.Bounds.Height * imageScale);
                    actualX = newVisualX - (selectedImage.Bounds.Width - scaledWidth) / 2;
                    actualY = newVisualY - (selectedImage.Bounds.Height - scaledHeight) / 2;
                }
                else
                {
                    actualX = newVisualX;
                    actualY = newVisualY;
                }
                
                selectedImage.Bounds = new Rectangle(actualX, actualY, selectedImage.Bounds.Width, selectedImage.Bounds.Height);

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
                    using (SolidBrush eraserBrush = new SolidBrush(picBoxMain.BackColor))
                    {
                        g.FillEllipse(eraserBrush, endP.X - radius, endP.Y - radius, eraserSize, eraserSize);
                    }
                }
                g.Dispose();
                // Update speed tracking for all brush types (needed for wet brush)
                if (flagBrush && flagDraw)
                {
                    lastMouseMoveTime = DateTime.Now;
                    lastMousePosition = endP;
                }
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
            else if (flagLoad || flagEmoji)
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
            
            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
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
            
            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
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
            
            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
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
            
            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
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
            
            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
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
            
            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
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
            
            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
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
            
            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
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
            
            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
        }

        private void picBoxPurple_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brushPen.Color = picBoxPurple.BackColor;
            brush.Color = picBoxPurple.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxBrushColor.Image = null;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            flagErase = false;
            
            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
        }

        private void picBoxBrown_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            brushPen.Color = picBoxBrown.BackColor;
            brush.Color = picBoxBrown.BackColor;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxBrushColor.Image = null;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            flagErase = false;
            
            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
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
                    
                    // Update text preview if in text mode
                    if (flagText)
                    {
                        picBoxMain.Invalidate();
                    }
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
            flagFill = false;
            flagLoad = false;
            flagEmoji = false;
            flagLine = false;
            flagSquare = false;
            flagCircle = false;
            flagNgon = false;
            picBoxMain.Cursor = eraserCursor ?? Cursors.Cross;
            SetToolBorder(picBoxErase);
            UpdateUnifiedSlider();
            UpdateUnifiedComboBox();
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
            flagFill = false;
            flagLoad = false;
            flagEmoji = false;
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
            UpdateUnifiedSlider();
            UpdateUnifiedComboBox();
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
            // Auto-stamp if switching from emoji mode
            AutoStampOnToolSwitch();

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
                        flagEmoji = false;
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
                        UpdateUnifiedSlider();
                        UpdateUnifiedComboBox();

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
            // Auto-stamp if switching from load/emoji mode
            AutoStampOnToolSwitch();

            flagBrush = true;
            flagFill = false;
            flagLoad = false;
            flagEmoji = false;
            flagErase = false;
            flagText = false;
            flagLine = false;
            flagSquare = false;
            flagCircle = false;
            flagNgon = false;
            picBoxBrushColor.Image = Properties.Resources.paint_brush;
            picBoxBrushColor.BackColor = Color.Transparent;
            picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
            SetToolBorder(picBoxBrush);
            UpdateUnifiedSlider();
            UpdateUnifiedComboBox();
        }

        // Fill tool click handler
        private void picBoxFill_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load/emoji mode
            AutoStampOnToolSwitch();

            flagFill = true;
            flagBrush = false;
            flagLoad = false;
            flagEmoji = false;
            flagErase = false;
            flagText = false;
            flagLine = false;
            flagSquare = false;
            flagCircle = false;
            flagNgon = false;
            picBoxBrushColor.Image = Properties.Resources.bucket;
            picBoxBrushColor.BackColor = Color.Transparent;
            picBoxMain.Cursor = Cursors.Hand; // Hand cursor for fill tool
            SetToolBorder(picBoxFill);
            UpdateUnifiedSlider();
            UpdateUnifiedComboBox();
        }

        // Flood fill algorithm
        private void PerformFloodFill(Point startPoint)
        {
            if (bm == null) return;

            // Ensure point is within bounds
            if (startPoint.X < 0 || startPoint.X >= bm.Width || 
                startPoint.Y < 0 || startPoint.Y >= bm.Height)
                return;

            Color targetColor = bm.GetPixel(startPoint.X, startPoint.Y);
            Color fillColor = brushPen.Color;

            // If clicking on the same color, don't do anything
            if (targetColor.ToArgb() == fillColor.ToArgb())
                return;

            // Use a queue-based flood fill algorithm
            Queue<Point> queue = new Queue<Point>();
            HashSet<Point> visited = new HashSet<Point>();
            
            queue.Enqueue(startPoint);
            visited.Add(startPoint);

            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();
                
                // Check if this pixel matches the target color
                if (current.X < 0 || current.X >= bm.Width || 
                    current.Y < 0 || current.Y >= bm.Height)
                    continue;

                Color pixelColor = bm.GetPixel(current.X, current.Y);
                
                // Use tolerance for color matching (helps with anti-aliasing)
                if (ColorsMatch(pixelColor, targetColor))
                {
                    bm.SetPixel(current.X, current.Y, fillColor);

                    // Add neighbors to queue
                    Point[] neighbors = new Point[]
                    {
                        new Point(current.X + 1, current.Y),
                        new Point(current.X - 1, current.Y),
                        new Point(current.X, current.Y + 1),
                        new Point(current.X, current.Y - 1)
                    };

                    foreach (Point neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor) && 
                            neighbor.X >= 0 && neighbor.X < bm.Width &&
                            neighbor.Y >= 0 && neighbor.Y < bm.Height)
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }

        // Helper method to check if two colors match (with tolerance)
        private bool ColorsMatch(System.Drawing.Color c1, System.Drawing.Color c2, int tolerance = 5)
        {
            return Math.Abs(c1.R - c2.R) <= tolerance &&
                   Math.Abs(c1.G - c2.G) <= tolerance &&
                   Math.Abs(c1.B - c2.B) <= tolerance;
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
                case BrushType.Pen:
                    // Solid, fully opaque pen - simple and clean
                    brushPen.Width = brushSize;
                    brushPen.StartCap = LineCap.Round;
                    brushPen.EndCap = LineCap.Round;
                    brushPen.LineJoin = LineJoin.Round;
                    g.DrawLine(brushPen, start, end);
                    // Fill ellipse at end point for complete coverage
                    int radius = brushSize / 2;
                    using (SolidBrush penBrush = new SolidBrush(brushPen.Color))
                    {
                        g.FillEllipse(penBrush, end.X - radius, end.Y - radius, brushSize, brushSize);
                    }
                    break;

                case BrushType.Paintbrush:
                    // Soft, painterly effect using rounded caps, slightly transparent
                    // Draw to temp bitmap first to prevent opacity accumulation
                    int paintBrushSize = brushSize;
                    Rectangle paintBounds = GetStrokeBounds(start, end, paintBrushSize);
                    using (Bitmap tempBitmap = new Bitmap(paintBounds.Width, paintBounds.Height))
                    {
                        using (Graphics tempG = Graphics.FromImage(tempBitmap))
                        {
                            tempG.SmoothingMode = SmoothingMode.HighQuality;
                            tempG.Clear(Color.Transparent);
                            using (Pen paintPen = new Pen(brushPen.Color, paintBrushSize))
                            {
                                paintPen.StartCap = LineCap.Round;
                                paintPen.EndCap = LineCap.Round;
                                paintPen.LineJoin = LineJoin.Round;
                                // Draw at full opacity to temp bitmap
                                tempG.DrawLine(paintPen, 
                                    start.X - paintBounds.X, start.Y - paintBounds.Y,
                                    end.X - paintBounds.X, end.Y - paintBounds.Y);
                            }
                        }
                        // Composite temp bitmap onto main with desired opacity (70%)
                        DrawBitmapWithOpacity(g, tempBitmap, paintBounds.Location, 0.7f);
                    }
                    break;


                case BrushType.Marker:
                    // Clean, solid lines with square caps, slightly transparent
                    // Draw to temp bitmap first to prevent opacity accumulation
                    int markerBrushSize = brushSize;
                    Rectangle markerBounds = GetStrokeBounds(start, end, markerBrushSize);
                    using (Bitmap tempBitmap = new Bitmap(markerBounds.Width, markerBounds.Height))
                    {
                        using (Graphics tempG = Graphics.FromImage(tempBitmap))
                        {
                            tempG.SmoothingMode = SmoothingMode.HighQuality;
                            tempG.Clear(Color.Transparent);
                            using (Pen markerPen = new Pen(brushPen.Color, markerBrushSize))
                            {
                                markerPen.StartCap = LineCap.Square;
                                markerPen.EndCap = LineCap.Square;
                                markerPen.LineJoin = LineJoin.Miter;
                                // Draw at full opacity to temp bitmap
                                tempG.DrawLine(markerPen,
                                    start.X - markerBounds.X, start.Y - markerBounds.Y,
                                    end.X - markerBounds.X, end.Y - markerBounds.Y);
                            }
                        }
                        // Composite temp bitmap onto main with desired opacity (80%)
                        DrawBitmapWithOpacity(g, tempBitmap, markerBounds.Location, 0.8f);
                    }
                    break;

                case BrushType.Pencil:
                    // Sharp, precise lines with hard edges, semi-transparent to remove less paint
                    int pencilWidth = Math.Max(1, brushSize / 3); // Thinner for pencil
                    // Use semi-transparent color (about 70% opacity) so it removes less paint
                    Color pencilColor = Color.FromArgb(178, brushPen.Color); // 178/255 ≈ 70% opacity
                    using (Pen pencilPen = new Pen(pencilColor, pencilWidth))
                    {
                        pencilPen.StartCap = LineCap.Flat;
                        pencilPen.EndCap = LineCap.Flat;
                        pencilPen.LineJoin = LineJoin.Miter;
                        g.SmoothingMode = SmoothingMode.None; // Sharp edges
                        g.DrawLine(pencilPen, start, end);
                    }
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    break;

                case BrushType.Airbrush:
                    // Very soft, feathered edges with more gradual gradient opacity
                    int airbrushSize = brushSize * 2; // Larger for airbrush effect
                    for (int i = airbrushSize; i > 0; i -= 2)
                    {
                        // More gradual falloff using square root for smoother, longer gradient
                        double ratio = (double)i / airbrushSize;
                        int alpha = (int)(255 * (1.0 - Math.Sqrt(ratio))); // Square root for more gradual falloff
                        if (alpha > 3) // Lower threshold for more gradual effect
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
                    // Draw connecting line with more gradual gradient
                    DrawGradientLine(g, start, end, brushSize);
                    break;

                case BrushType.WetBrush:
                    // Speed-based paint brush - slightly opaque, thickness varies with speed
                    // Calculate speed based on distance and time from last position
                    double distance = 0;
                    double speed = 0;
                    if (lastMousePosition != Point.Empty)
                    {
                        distance = Math.Sqrt(Math.Pow(end.X - lastMousePosition.X, 2) + Math.Pow(end.Y - lastMousePosition.Y, 2));
                        TimeSpan timeElapsed = DateTime.Now - lastMouseMoveTime;
                        speed = timeElapsed.TotalMilliseconds > 0 ? distance / timeElapsed.TotalMilliseconds : 0;
                    }
                    else
                    {
                        // First point - use distance from start
                        distance = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
                        speed = 0; // Default to slow (thick) for first point
                    }
                    
                    // Faster speed = thinner brush, slower speed = thicker brush
                    // Speed range: 0-5 pixels/ms, map to brush size variation: 0.6x to 1.4x
                    // Clamp speed to reasonable range
                    speed = Math.Min(5.0, speed);
                    double speedFactor = Math.Max(0.6, Math.Min(1.4, 1.4 - (speed * 0.16)));
                    int wetBrushSize = (int)(brushSize * speedFactor);
                    wetBrushSize = Math.Max(1, wetBrushSize); // Ensure minimum size
                    
                    // Slightly transparent (about 80% opacity)
                    Color wetColor = Color.FromArgb(204, brushPen.Color); // 204/255 ≈ 80% opacity
                    using (Pen wetPen = new Pen(wetColor, wetBrushSize))
                    using (SolidBrush wetBrush = new SolidBrush(wetColor))
                    {
                        wetPen.StartCap = LineCap.Round;
                        wetPen.EndCap = LineCap.Round;
                        wetPen.LineJoin = LineJoin.Round;
                        g.DrawLine(wetPen, start, end);
                        int wetRadius = wetBrushSize / 2;
                        g.FillEllipse(wetBrush, end.X - wetRadius, end.Y - wetRadius, wetBrushSize, wetBrushSize);
                    }
                    break;
            }
        }

        // Helper method to calculate bounding rectangle for a stroke
        private Rectangle GetStrokeBounds(Point start, Point end, int brushSize)
        {
            int minX = Math.Min(start.X, end.X) - brushSize;
            int minY = Math.Min(start.Y, end.Y) - brushSize;
            int maxX = Math.Max(start.X, end.X) + brushSize;
            int maxY = Math.Max(start.Y, end.Y) + brushSize;
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        // Helper method to draw a bitmap with specified opacity
        private void DrawBitmapWithOpacity(Graphics g, Bitmap bitmap, Point location, float opacity)
        {
            if (opacity >= 1.0f)
            {
                g.DrawImage(bitmap, location);
            }
            else
            {
                // Create color matrix for opacity
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = opacity; // Alpha channel
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(bitmap,
                    new Rectangle(location.X, location.Y, bitmap.Width, bitmap.Height),
                    0, 0, bitmap.Width, bitmap.Height,
                    GraphicsUnit.Pixel,
                    attributes);
                attributes.Dispose();
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

                // Draw with decreasing opacity from center - use square root for more gradual falloff
                for (int j = size; j > 0; j -= 2)
                {
                    double ratio = (double)j / size;
                    int alpha = (int)(180 * (1.0 - Math.Sqrt(ratio))); // Square root for more gradual falloff
                    if (alpha > 3) // Lower threshold for more gradual effect
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
            if ((flagLoad || flagEmoji) && placedImages.Count > 0)
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

            // Draw border around selected image in load/emoji mode
            if ((flagLoad || flagEmoji) && selectedImage != null && !isDraggingImage)
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
            flagEmoji = false;
            flagBrush = true;
            flagErase = false;
            flagText = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            SetToolBorder(picBoxBrush);

            // Invalidate to refresh display (stamped image is now part of the bitmap)
            picBoxMain.Invalidate();
        }

        // Show/hide image controls (OK button - unified slider is handled by UpdateUnifiedSlider)
        private void ShowImageControls(bool show)
        {
            btnStampImage.Visible = show;
            if (show)
            {
                UpdateUnifiedSlider(); // Update unified slider when image is selected
            }
        }

        // Auto-stamp image when switching tools from load/emoji mode
        private void AutoStampOnToolSwitch()
        {
            if ((flagLoad || flagEmoji) && selectedImage != null)
            {
                StampImage(selectedImage);
            }
        }

        // Create a bitmap from emoji text with COLORED emoji support
        // Uses WPF interop with Emoji.Wpf library to render colored emojis
        // Solution based on: https://stackoverflow.com/questions/49721440/display-colored-emoji-instead-of-black-and-white
        private Bitmap CreateEmojiBitmap(string emoji, int size)
        {
            // First, measure the text to determine bitmap size
            Font emojiFont = new Font("Segoe UI Emoji", size * 0.7f, FontStyle.Regular, GraphicsUnit.Pixel);
            SizeF textSize;
            using (Graphics measureG = this.CreateGraphics())
            {
                textSize = measureG.MeasureString(emoji, emojiFont);
            }
            
            int bitmapWidth = Math.Max(size, (int)textSize.Width + 20);
            int bitmapHeight = Math.Max(size, (int)textSize.Height + 20);
            
            try
            {
                // Use WPF with Emoji.Wpf to render colored emojis
                // Emoji.Wpf provides TextBlock that supports colored emoji rendering
                var emojiTextBlock = new Emoji.Wpf.TextBlock
                {
                    Text = emoji,
                    FontSize = size * 0.7,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Background = System.Windows.Media.Brushes.White, // White background for transparency key
                    Foreground = System.Windows.Media.Brushes.Black // Text color (doesn't affect emoji colors)
                };
                
                // Create a container to host the TextBlock
                var container = new System.Windows.Controls.Border
                {
                    Width = bitmapWidth,
                    Height = bitmapHeight,
                    Background = System.Windows.Media.Brushes.White,
                    Child = emojiTextBlock
                };
                
                // Measure and arrange
                container.Measure(new System.Windows.Size(bitmapWidth, bitmapHeight));
                container.Arrange(new System.Windows.Rect(0, 0, bitmapWidth, bitmapHeight));
                container.UpdateLayout();
                
                // Create a RenderTargetBitmap to capture the WPF control
                var dpiScale = 96.0; // Standard DPI
                var renderBitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(
                    bitmapWidth, 
                    bitmapHeight, 
                    dpiScale, 
                    dpiScale, 
                    System.Windows.Media.PixelFormats.Pbgra32);
                
                // Render the container to the bitmap
                renderBitmap.Render(container);
                renderBitmap.Freeze(); // Freeze for thread safety
                
                // Convert WPF BitmapSource to System.Drawing.Bitmap
                Bitmap bmp = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format32bppArgb);
                var bitmapData = bmp.LockBits(
                    new Rectangle(0, 0, bitmapWidth, bitmapHeight),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);
                
                renderBitmap.CopyPixels(
                    System.Windows.Int32Rect.Empty,
                    bitmapData.Scan0,
                    bitmapData.Height * bitmapData.Stride,
                    bitmapData.Stride);
                
                bmp.UnlockBits(bitmapData);
                
                // Make white background transparent
                bmp.MakeTransparent(Color.White);
                
                emojiFont.Dispose();
                return bmp;
            }
            catch (Exception ex)
            {
                // Fallback to black emoji if WPF rendering fails
                System.Diagnostics.Debug.WriteLine($"Emoji.Wpf rendering failed: {ex.Message}, falling back to GDI+");
                
                Bitmap bmp = new Bitmap(bitmapWidth, bitmapHeight);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    float x = (bitmapWidth - textSize.Width) / 2;
                    float y = (bitmapHeight - textSize.Height) / 2;
                    g.DrawString(emoji, emojiFont, Brushes.Black, x, y);
                }
                emojiFont.Dispose();
                return bmp;
            }
        }

        // Emoji tool click handler
        private void picBoxEmoji_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load/fill mode (but not if already in emoji mode)
            if ((flagLoad || flagFill) && !flagEmoji)
            {
                AutoStampOnToolSwitch();
            }
            
            // If emojiText is already set and we're switching back to emoji mode,
            // just re-enable emoji mode without showing the dialog
            if (!string.IsNullOrEmpty(emojiText) && !flagEmoji)
            {
                // Re-enable emoji mode with existing emoji
                flagEmoji = true;
                flagLoad = false;
                flagFill = false;
                flagBrush = false;
                flagErase = false;
                flagText = false;
                flagLine = false;
                flagSquare = false;
                flagCircle = false;
                flagNgon = false;
                picBoxBrushColor.Image = Properties.Resources.happy_face;
                picBoxBrushColor.BackColor = Color.Transparent;
                picBoxMain.Cursor = Cursors.Default;
                SetToolBorder(picBoxEmoji);
                UpdateUnifiedSlider();
                UpdateUnifiedComboBox();
                return;
            }
            
            // If already in emoji mode, show dialog to allow changing emoji
            // Otherwise, prompt user to enter emoji (first time or empty emojiText)
            using (Form emojiInputForm = new Form())
            {
                emojiInputForm.Text = "Enter Emoji";
                emojiInputForm.Size = new Size(400, 220);
                emojiInputForm.StartPosition = FormStartPosition.CenterParent;
                emojiInputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                emojiInputForm.MaximizeBox = false;
                emojiInputForm.MinimizeBox = false;
                emojiInputForm.ShowInTaskbar = false;
                emojiInputForm.TopMost = true; // Show above TopMost main form
                emojiInputForm.Owner = this; // Set owner to ensure proper z-order

                TextBox inputBox = new TextBox();
                inputBox.Location = new Point(30, 30);
                inputBox.Size = new Size(340, 80);
                inputBox.Multiline = true;
                inputBox.Text = emojiText;
                inputBox.Font = new Font("Segoe UI Emoji", 20);
                inputBox.SelectAll();


                Button okButton = new Button();
                okButton.Text = "OK";
                okButton.DialogResult = DialogResult.OK;
                okButton.Location = new Point(120, 130);
                okButton.Size = new Size(100, 35);

                Button cancelButton = new Button();
                cancelButton.Text = "Cancel";
                cancelButton.DialogResult = DialogResult.Cancel;
                cancelButton.Location = new Point(230, 130);
                cancelButton.Size = new Size(100, 35);

                emojiInputForm.Controls.Add(inputBox);
                emojiInputForm.Controls.Add(okButton);
                emojiInputForm.Controls.Add(cancelButton);
                emojiInputForm.AcceptButton = okButton;
                emojiInputForm.CancelButton = cancelButton;

                if (emojiInputForm.ShowDialog(this) == DialogResult.OK)
                {
                    string input = inputBox.Text.Trim();
                    if (!string.IsNullOrEmpty(input))
                    {
                        emojiText = input;
                        // Enable emoji mode
                        flagEmoji = true;
                        flagLoad = false;
                        flagFill = false;
                        flagBrush = false;
                        flagErase = false;
                        flagText = false;
                        flagLine = false;
                        flagSquare = false;
                        flagCircle = false;
                        flagNgon = false;
                        picBoxBrushColor.Image = Properties.Resources.happy_face;
                        picBoxBrushColor.BackColor = Color.Transparent;
                        picBoxMain.Cursor = Cursors.Default;
                        SetToolBorder(picBoxEmoji);
                        UpdateUnifiedSlider();
                        UpdateUnifiedComboBox();
                    }
                    else
                    {
                        // User cancelled or entered empty, don't enable emoji mode
                        return;
                    }
                }
                else
                {
                    // User cancelled, don't enable emoji mode
                    return;
                }
            }
        }

        // Old handler removed - image scale is now handled by trackBarUnified_ValueChanged

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
            picBoxFill.BorderStyle = BorderStyle.None;
            picBoxLoad.BorderStyle = BorderStyle.None;
            picBoxErase.BorderStyle = BorderStyle.None;
            picBoxText.BorderStyle = BorderStyle.None;
            picBoxEmoji.BorderStyle = BorderStyle.None;
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

        // Old handlers removed - functionality moved to comboBoxUnified_SelectedIndexChanged

        // Update unified ComboBox based on selected tool
        private void UpdateUnifiedComboBox()
        {
            // Temporarily remove handler to avoid recursion
            comboBoxUnified.SelectedIndexChanged -= comboBoxUnified_SelectedIndexChanged;

            if (flagBrush || flagLine || flagSquare || flagCircle || flagNgon)
            {
                // Brush or shape tools - show brush types
                comboBoxUnified.Items.Clear();
                comboBoxUnified.Items.AddRange(new object[] { "Pen", "Paintbrush", "Marker", "Pencil", "Airbrush", "Wet Brush" });
                comboBoxUnified.SelectedIndex = (int)currentBrushType;
                lblUnifiedCombo.Text = "Brush Type:";
                comboBoxUnified.Visible = true;
                lblUnifiedCombo.Visible = true;
            }
            else if (flagText)
            {
                // Text tool - show fonts
                comboBoxUnified.Items.Clear();
                comboBoxUnified.Items.AddRange(new object[] { "Arial", "Times New Roman", "Courier New", "Comic Sans MS", "Verdana" });
                // Find and select current font
                int fontIndex = comboBoxUnified.Items.IndexOf(selectedFontName);
                comboBoxUnified.SelectedIndex = fontIndex >= 0 ? fontIndex : 0;
                lblUnifiedCombo.Text = "Font:";
                comboBoxUnified.Visible = true;
                lblUnifiedCombo.Visible = true;
            }
            else
            {
                // Other tools - hide ComboBox
                comboBoxUnified.Visible = false;
                lblUnifiedCombo.Visible = false;
            }

            // Re-add handler
            comboBoxUnified.SelectedIndexChanged += comboBoxUnified_SelectedIndexChanged;
        }

        // Unified ComboBox value changed handler
        private void comboBoxUnified_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (comboBoxUnified.SelectedIndex < 0) return;

            if (flagBrush || flagLine || flagSquare || flagCircle || flagNgon)
            {
                // Brush or shape tools - update brush type
                currentBrushType = (BrushType)comboBoxUnified.SelectedIndex;
            }
            else if (flagText)
            {
                // Text tool - update font
                selectedFontName = comboBoxUnified.SelectedItem?.ToString() ?? "Arial";
                if (flagText)
                {
                    picBoxMain.Invalidate(); // Update preview
                }
            }
        }

        // Update unified slider based on selected tool
        private void UpdateUnifiedSlider()
        {
            // Temporarily remove handler to avoid recursion
            trackBarUnified.ValueChanged -= trackBarUnified_ValueChanged;

            if (flagBrush || flagLine || flagSquare || flagCircle || flagNgon)
            {
                // Brush or shape tools - use brush size
                trackBarUnified.Minimum = 10;
                trackBarUnified.Maximum = 70;
                trackBarUnified.TickFrequency = 10;
                trackBarUnified.Value = brushSize;
                lblUnified.Text = $"Brush: {brushSize}pts";
            }
            else if (flagErase)
            {
                // Eraser tool - use eraser size
                trackBarUnified.Minimum = 10;
                trackBarUnified.Maximum = 70;
                trackBarUnified.TickFrequency = 10;
                trackBarUnified.Value = eraserSize;
                lblUnified.Text = $"Eraser: {eraserSize}pts";
            }
            else if (flagText)
            {
                // Text tool - use font size
                trackBarUnified.Minimum = 1;
                trackBarUnified.Maximum = 70;
                trackBarUnified.TickFrequency = 10;
                trackBarUnified.Value = selectedFontSize;
                lblUnified.Text = $"Font Size: {selectedFontSize}pts";
            }
            else if ((flagLoad || flagEmoji) && selectedImage != null)
            {
                // Image load/emoji tool with selected image - use image scale
                trackBarUnified.Minimum = 50;
                trackBarUnified.Maximum = 200;
                trackBarUnified.TickFrequency = 25;
                trackBarUnified.Value = (int)(imageScale * 100);
                lblUnified.Text = $"Scale: {(int)(imageScale * 100)}%";
            }
            else
            {
                // Default to brush size
                trackBarUnified.Minimum = 10;
                trackBarUnified.Maximum = 70;
                trackBarUnified.TickFrequency = 10;
                trackBarUnified.Value = brushSize;
                lblUnified.Text = $"Brush: {brushSize}pts";
            }

            // Re-add handler
            trackBarUnified.ValueChanged += trackBarUnified_ValueChanged;
        }

        // Unified slider value changed handler
        private void trackBarUnified_ValueChanged(object? sender, EventArgs e)
        {
            int value = trackBarUnified.Value;

            if (flagBrush || flagLine || flagSquare || flagCircle || flagNgon)
            {
                // Brush or shape tools - update brush size
                if (Control.ModifierKeys == Keys.Control)
                {
                    value = SnapToPresetSize(value);
                    trackBarUnified.ValueChanged -= trackBarUnified_ValueChanged;
                    trackBarUnified.Value = value;
                    trackBarUnified.ValueChanged += trackBarUnified_ValueChanged;
                }
                brushSize = value;
                brushPen.Width = brushSize;
                lblUnified.Text = $"Brush: {brushSize}pts";

                // Update shape preview if drawing
                if (isDrawingShape && (flagLine || flagSquare || flagCircle || flagNgon))
                {
                    picBoxMain.Invalidate();
                }
            }
            else if (flagErase)
            {
                // Eraser tool - update eraser size
                if (Control.ModifierKeys == Keys.Control)
                {
                    value = SnapToPresetSize(value);
                    trackBarUnified.ValueChanged -= trackBarUnified_ValueChanged;
                    trackBarUnified.Value = value;
                    trackBarUnified.ValueChanged += trackBarUnified_ValueChanged;
                }
                eraserSize = value;
                eraserPen.Width = eraserSize;
                lblUnified.Text = $"Eraser: {eraserSize}pts";
            }
            else if (flagText)
            {
                // Text tool - update font size
                if (Control.ModifierKeys == Keys.Control)
                {
                    value = SnapToPresetSize(value);
                    trackBarUnified.ValueChanged -= trackBarUnified_ValueChanged;
                    trackBarUnified.Value = value;
                    trackBarUnified.ValueChanged += trackBarUnified_ValueChanged;
                }
                selectedFontSize = value;
                lblUnified.Text = $"Font Size: {selectedFontSize}pts";
                if (flagText)
                {
                    picBoxMain.Invalidate(); // Update preview
                }
            }
            else if ((flagLoad || flagEmoji) && selectedImage != null)
            {
                // Image load/emoji tool - update image scale
                imageScale = value / 100.0f;
                lblUnified.Text = $"Scale: {value}%";
                if (selectedImage != null)
                {
                    RedrawCanvas();
                }
            }
        }

        // Removed - lblImageScale no longer exists

        // Shape tool click handlers
        private void picBoxLine_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            flagLine = true;
            flagSquare = false;
            flagCircle = false;
            flagNgon = false;
            flagBrush = false;
            flagFill = false;
            flagLoad = false;
            flagEmoji = false;
            flagErase = false;
            flagText = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxMain.Cursor = Cursors.Cross;
            SetToolBorder(picBoxLine);
            UpdateUnifiedSlider();
            UpdateUnifiedComboBox();
        }

        private void picBoxSquare_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            flagLine = false;
            flagSquare = true;
            flagCircle = false;
            flagNgon = false;
            flagBrush = false;
            flagFill = false;
            flagLoad = false;
            flagEmoji = false;
            flagErase = false;
            flagText = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxMain.Cursor = Cursors.Cross;
            SetToolBorder(picBoxSquare);
            UpdateUnifiedSlider();
            UpdateUnifiedComboBox();
        }

        private void picBoxCircle_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            flagLine = false;
            flagSquare = false;
            flagCircle = true;
            flagNgon = false;
            flagBrush = false;
            flagFill = false;
            flagLoad = false;
            flagEmoji = false;
            flagErase = false;
            flagText = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxMain.Cursor = Cursors.Cross;
            SetToolBorder(picBoxCircle);
            UpdateUnifiedSlider();
            UpdateUnifiedComboBox();
        }

        private void picBoxNgon_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            flagLine = false;
            flagSquare = false;
            flagCircle = false;
            flagNgon = true;
            flagBrush = false;
            flagFill = false;
            flagLoad = false;
            flagEmoji = false;
            flagErase = false;
            flagText = false;
            picBoxBrushColor.Image = null;
            picBoxBrushColor.BackColor = brushPen.Color;
            picBoxMain.Cursor = Cursors.Cross;
            SetToolBorder(picBoxNgon);
            UpdateUnifiedSlider();
            UpdateUnifiedComboBox();
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

        private void lblUnifiedCombo_Click(object sender, EventArgs e)
        {

        }

        // Removed - trackBarImageScale no longer exists

        // Removed - lblFontSize no longer exists
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
