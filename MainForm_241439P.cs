using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Doodle_241439P.Properties;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms.Integration;
using WpfPoint = System.Windows.Point;
using WpfSize = System.Windows.Size;
using WpfRect = System.Windows.Rect;
using WpfColor = System.Windows.Media.Color;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfHorizontalAlignment = System.Windows.HorizontalAlignment;
using WpfVerticalAlignment = System.Windows.VerticalAlignment;
using WpfPen = System.Windows.Media.Pen;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using Emoji.Wpf;
using DrawingColor = System.Drawing.Color;
using DrawingPen = System.Drawing.Pen;

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
        DrawingPen brushPen = new DrawingPen(DrawingColor.Black, 8);  // Thick pen for brush drawing
        DrawingPen eraserPen = new DrawingPen(DrawingColor.LightGray, 30);  // Pen for eraser drawing
        SolidBrush brush = new SolidBrush(DrawingColor.Black);
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
        float zoomLevel = 1.0f;  // Zoom level for canvas (1.0 = 100%)
        Point zoomCenter = Point.Empty;  // Center point for zooming
        int canvasOffsetX = 0;  // Offset for expanded canvas (negative values allow drawing to the left)
        int canvasOffsetY = 0;  // Offset for expanded canvas (negative values allow drawing above)
        int canvasActualWidth = 0;  // Actual width of canvas (may be larger than picBoxMain)
        int canvasActualHeight = 0;  // Actual height of canvas (may be larger than picBoxMain)
        int viewportOffsetX = 0;  // Viewport pan offset (how much we've scrolled the view)
        int viewportOffsetY = 0;  // Viewport pan offset (how much we've scrolled the view)
        bool isPanning = false;  // Whether user is panning the viewport
        Point panStartPoint = Point.Empty;  // Starting point for panning
        int panStartOffsetX = 0;  // Viewport offset when panning started
        int panStartOffsetY = 0;  // Viewport offset when panning started
        bool isResizingCanvas = false;  // Whether user is dragging to resize canvas
        int canvasResizeEdge = 0;  // Which edge is being resized: 0=none, 1=left, 2=right, 3=top, 4=bottom, 5=top-left, 6=top-right, 7=bottom-left, 8=bottom-right
        Point canvasResizeStart = Point.Empty;  // Starting point for canvas resize
        int canvasResizeStartWidth = 0;  // Canvas width when resize started
        int canvasResizeStartHeight = 0;  // Canvas height when resize started
        int canvasResizeStartOffsetX = 0;  // Canvas offset X when resize started
        int canvasResizeStartOffsetY = 0;  // Canvas offset Y when resize started
        const int RESIZE_HANDLE_SIZE = 8;  // Size of resize handles in pixels
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
        DrawingColor shapeBorderColor = DrawingColor.Black;  // Outside color (border) for shapes
        DrawingColor shapeFillColor = DrawingColor.Black;  // Inside color (fill) for shapes

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

        // Windows message constants for mouse wheel
        private const int WM_MOUSEHWHEEL = 0x020E;
        private const int WM_MOUSEWHEEL = 0x020A;
        
        // Override WndProc to handle horizontal mouse wheel (two-finger horizontal scrolling)
        // and to route vertical wheel to canvas when cursor is over it (so scroll works when canvas doesn't have focus)
        protected override void WndProc(ref Message m)
        {
            // Route vertical mouse wheel to canvas when cursor is over picture box, so scrolling works when zoomed in
            // even if another control has focus (e.g. after clicking a tool)
            if (m.Msg == WM_MOUSEWHEEL && picBoxMain != null)
            {
                Point clientPos = PointToClient(Cursor.Position);
                if (picBoxMain.Visible && picBoxMain.Bounds.Contains(clientPos))
                {
                    int delta = (short)((m.WParam.ToInt64() >> 16) & 0xFFFF);
                    bool ctrlPressed = (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control;
                    bool shiftPressed = (System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift;
                    if (ctrlPressed)
                    {
                        // Zoom: let base deliver to focused control; we handle in picBoxMain_MouseWheel when pic has focus
                        // If we're routing, invoke the same zoom/scroll logic here
                        float zoomFactor = delta > 0 ? 1.1f : 0.9f;
                        float newZoom = zoomLevel * zoomFactor;
                        if (newZoom < 0.1f) newZoom = 0.1f;
                        if (newZoom > 5.0f) newZoom = 5.0f;
                        if (newZoom != zoomLevel)
                        {
                            zoomLevel = newZoom;
                            if (zoomLevel == 1.0f && canvasOffsetX == 0 && canvasOffsetY == 0 && viewportOffsetX == 0 && viewportOffsetY == 0 && bm != null)
                                picBoxMain.Image = bm;
                            else
                                picBoxMain.Image = null;
                            UpdateScrollBars();
                            picBoxMain.Invalidate();
                        }
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    int scrollDelta = -delta;
                    if (shiftPressed)
                        viewportOffsetX += scrollDelta / 3;
                    else
                        viewportOffsetY -= scrollDelta / 3; // Flipped: wheel up = pan up, wheel down = pan down
                    ClampViewportOffset();
                    UpdateScrollBars();
                    picBoxMain.Invalidate();
                    m.Result = IntPtr.Zero;
                    return;
                }
            }
            
            if (m.Msg == WM_MOUSEHWHEEL)
            {
                // Handle horizontal scrolling from two-finger touchpad gestures
                int delta = (short)((m.WParam.ToInt64() >> 16) & 0xFFFF);
                
                // Check if Ctrl is pressed (for zoom)
                bool ctrlPressed = (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control;
                
                if (!ctrlPressed)
                {
                    // Two-finger horizontal scrolling (inverse/natural scrolling)
                    // delta > 0 means scroll right (two fingers right), so viewport moves right, content moves left
                    // We invert for natural scrolling: two fingers right = scroll left
                    int scrollDelta = -delta;
                    viewportOffsetX += scrollDelta / 3; // Divide by 3 for smoother scrolling
                    
                    // Clamp and update scrollbars
                    ClampViewportOffset();
                    UpdateScrollBars();
                    
                    // Redraw with new viewport position
                    picBoxMain.Invalidate();
                    
                    m.Result = IntPtr.Zero;
                    return;
                }
            }
            
            base.WndProc(ref m);
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
            canvasActualWidth = picBoxMain.Width;
            canvasActualHeight = picBoxMain.Height;
            canvasOffsetX = 0;
            canvasOffsetY = 0;

            // Fill with initial background color
            using (Graphics g = Graphics.FromImage(bm))
            {
                g.Clear(DrawingColor.LightGray);
            }

            picBoxMain.Image = bm;
            
            // Initialize scrollbars
            UpdateScrollBars();

            // Initialize font and size
            selectedFontName = "Arial";
            selectedFontSize = 30;

            // Initialize brush color display (default to brush with paint brush icon)
            picBoxBrushColor.Image = Properties.Resources.paint_brush;
            picBoxBrushColor.BackColor = DrawingColor.Transparent;

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

            // Initialize shape colors
            shapeBorderColor = DrawingColor.Black;
            shapeFillColor = DrawingColor.Black;
            picBoxShapeOutsideColor.BackColor = shapeBorderColor;
            picBoxShapeInsideColor.BackColor = shapeFillColor;

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

        private void MainForm_241439P_Resize(object sender, EventArgs e)
        {
            // Only resize if the form is loaded and bitmap is initialized
            if (bm == null || !IsHandleCreated || WindowState == FormWindowState.Minimized) return;

            // Calculate the bottom of the toolbar area
            // Find the lowest Y position + height of all toolbar controls
            int toolbarBottom = menuStrip1.Bottom;

            // Check all toolbar controls to find the bottommost one
            foreach (System.Windows.Forms.Control control in Controls)
            {
                if (control != picBoxMain && control != menuStrip1 && control.Visible)
                {
                    int controlBottom = control.Bottom;
                    if (controlBottom > toolbarBottom)
                    {
                        toolbarBottom = controlBottom;
                    }
                }
            }

            // Add some padding below the toolbars
            int drawingAreaTop = toolbarBottom + 10;

            // Calculate available space for the drawing area
            int margin = picBoxMain.Left; // Maintain left margin (11px)
            int scrollbarWidth = vScrollBarCanvas.Visible ? vScrollBarCanvas.Width : 0;
            int scrollbarHeight = hScrollBarCanvas.Visible ? hScrollBarCanvas.Height : 0;
            int availableWidth = ClientSize.Width - (margin * 2) - scrollbarWidth;
            int availableHeight = ClientSize.Height - drawingAreaTop - 10 - scrollbarHeight; // 10px bottom margin

            // Ensure minimum size
            if (availableWidth < 100) availableWidth = 100;
            if (availableHeight < 100) availableHeight = 100;

            // Only resize if the size actually changed
            if (picBoxMain.Width != availableWidth || picBoxMain.Height != availableHeight || picBoxMain.Top != drawingAreaTop)
            {
                // Save the current bitmap content
                Bitmap oldBitmap = bm;

                // Create new bitmap with new size
                Bitmap newBitmap = new Bitmap(availableWidth, availableHeight);

                // Fill with background color
                using (Graphics g = Graphics.FromImage(newBitmap))
                {
                    g.Clear(DrawingColor.LightGray);

                    // Draw the old bitmap onto the new one
                    if (oldBitmap != null)
                    {
                        // Only scale down if the new size is smaller, otherwise just draw at original size
                        if (availableWidth < oldBitmap.Width || availableHeight < oldBitmap.Height)
                        {
                            // Window got smaller - scale down the drawing
                            // Use high-quality scaling for better image quality
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.CompositingQuality = CompositingQuality.HighQuality;

                            // Scale down to fit the new smaller dimensions
                            g.DrawImage(oldBitmap, 0, 0, availableWidth, availableHeight);
                        }
                        else
                        {
                            // Window got larger - keep drawing at original size, just expand canvas
                            int drawWidth = Math.Min(oldBitmap.Width, availableWidth);
                            int drawHeight = Math.Min(oldBitmap.Height, availableHeight);
                            g.DrawImage(oldBitmap, 0, 0, drawWidth, drawHeight);
                        }
                    }
                }

                // Update the bitmap and PictureBox
                bm = newBitmap;
                picBoxMain.Size = new Size(availableWidth, availableHeight);
                picBoxMain.Location = new Point(margin, drawingAreaTop);
                
                // Update canvas tracking (resize doesn't change offset, just size)
                canvasActualWidth = availableWidth;
                canvasActualHeight = availableHeight;
                
                // Update Image property if no offset and not zoomed
                if (zoomLevel == 1.0f && canvasOffsetX == 0 && canvasOffsetY == 0 && 
                    viewportOffsetX == 0 && viewportOffsetY == 0)
                {
                    picBoxMain.Image = bm;
                }
                else
                {
                    picBoxMain.Image = null; // Draw manually in Paint
                }

                // Dispose old bitmap
                oldBitmap?.Dispose();
                
                // Update scrollbars after resize
                UpdateScrollBars();

                // Invalidate to redraw
                picBoxMain.Invalidate();
            }
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
                    g.Clear(DrawingColor.Transparent);
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
            // Check for panning: Middle mouse button or Space + Left mouse button
            bool spacePressed = (System.Windows.Forms.Control.ModifierKeys & Keys.Space) == Keys.Space;
            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && spacePressed))
            {
                isPanning = true;
                panStartPoint = e.Location;
                panStartOffsetX = viewportOffsetX;
                panStartOffsetY = viewportOffsetY;
                picBoxMain.Cursor = Cursors.Hand;
                return; // Don't process other tools when panning
            }
            
            // Check if clicking on canvas resize handle (only when zoomed out)
            if (zoomLevel < 1.0f && e.Button == MouseButtons.Left)
            {
                int handle = GetCanvasResizeHandle(e.Location);
                if (handle > 0)
                {
                    isResizingCanvas = true;
                    canvasResizeEdge = handle;
                    canvasResizeStart = e.Location;
                    canvasResizeStartWidth = canvasActualWidth;
                    canvasResizeStartHeight = canvasActualHeight;
                    canvasResizeStartOffsetX = canvasOffsetX;
                    canvasResizeStartOffsetY = canvasOffsetY;
                    return; // Don't process other tools when resizing canvas
                }
            }
            
            // Convert screen coordinates to canvas coordinates accounting for zoom
            Point canvasLocation = ScreenToCanvas(e.Location);
            startP = canvasLocation;

            if (flagText)
            {
                if (textBounds != Rectangle.Empty && textBounds.Contains(canvasLocation))
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
                    textPosition = canvasLocation;
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
                    PerformFloodFill(canvasLocation);
                    picBoxMain.Invalidate();
                }
            }
            else if (flagBrush || flagErase)
            {
                if (e.Button == MouseButtons.Left)
                {
                    SaveUndoState();  // Save state before drawing/erasing
                    flagDraw = true;
                    endP = canvasLocation; // Initialize endP for continuous drawing
                    // Initialize speed tracking for wet brush
                    lastMouseMoveTime = DateTime.Now;
                    lastMousePosition = canvasLocation;
                }
            }
            else if (flagLine || flagSquare || flagCircle || flagNgon)
            {
                if (e.Button == MouseButtons.Left)
                {
                    SaveUndoState();  // Save state before drawing shape
                    isDrawingShape = true;
                    shapeStartPoint = canvasLocation;
                    shapeEndPoint = canvasLocation;
                }
            }
            else if (flagLoad || flagEmoji)
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Check if clicking on an existing image/emoji
                    PlacedImage? clickedImage = GetImageAtPoint(canvasLocation);

                    if (clickedImage != null)
                    {
                        // Clicking on an existing image/emoji
                        Rectangle bounds = GetScaledBounds(clickedImage);
                        if (bounds.Contains(canvasLocation))
                        {
                            // Clicking on the image - select it and start dragging
                            selectedImage = clickedImage;
                            isDraggingImage = true;
                            // Calculate offset from click position to the visual (scaled) bounds
                            // This ensures dragging works correctly regardless of scale
                            dragOffset = new Point(canvasLocation.X - bounds.X,
                                                   canvasLocation.Y - bounds.Y);
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
                            PlaceNewImage(loadedImage, canvasLocation);
                            selectedImage = placedImages[placedImages.Count - 1]; // Select the newly placed image
                            imageScale = 1.0f; // Reset scale
                            ShowImageControls(true);
                            UpdateUnifiedSlider(); // Update slider when new image is placed
                        }
                        else if (flagEmoji && !string.IsNullOrEmpty(emojiText))
                        {
                            // Check if there's a selected image and if click is outside its bounding box
                            if (selectedImage != null)
                            {
                                Rectangle bounds = GetScaledBounds(selectedImage);
                                if (!bounds.Contains(canvasLocation))
                                {
                                    // Clicking outside the bounding box - stamp the selected emoji
                                    StampImage(selectedImage);
                                    return; // Don't place a new emoji after stamping
                                }
                            }

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
                            PlaceNewImage(emojiBitmap, canvasLocation);
                            selectedImage = placedImages[placedImages.Count - 1]; // Select the newly placed emoji
                            imageScale = 1.0f; // Reset scale
                            ShowImageControls(true);
                            UpdateUnifiedSlider(); // Update slider when new emoji is placed
                        }
                        else
                        {
                            // No image/emoji to place
                            if (selectedImage != null)
                            {
                                // In emoji mode, clicking outside the drag box should stamp the image
                                if (flagEmoji)
                                {
                                    StampImage(selectedImage);
                                }
                                else
                                {
                                    // Just deselect for other modes
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
        }

        private void picBoxMain_MouseMove(object sender, MouseEventArgs e)
        {
            // Handle viewport panning
            if (isPanning)
            {
                int deltaX = e.Location.X - panStartPoint.X;
                int deltaY = e.Location.Y - panStartPoint.Y;
                
                viewportOffsetX = panStartOffsetX + deltaX;
                viewportOffsetY = panStartOffsetY + deltaY;
                
                // Clamp and update scrollbars
                ClampViewportOffset();
                UpdateScrollBars();
                
                picBoxMain.Invalidate();
                return;
            }
            
            // Handle canvas resizing
            if (isResizingCanvas)
            {
                int deltaX = e.Location.X - canvasResizeStart.X;
                int deltaY = e.Location.Y - canvasResizeStart.Y;
                
                // Convert screen delta to canvas delta
                int canvasDeltaX = (int)(deltaX / zoomLevel);
                int canvasDeltaY = (int)(deltaY / zoomLevel);
                
                int newWidth = canvasResizeStartWidth;
                int newHeight = canvasResizeStartHeight;
                int newOffsetX = canvasResizeStartOffsetX;
                int newOffsetY = canvasResizeStartOffsetY;
                
                // Adjust based on which edge is being dragged
                switch (canvasResizeEdge)
                {
                    case 1: // Left edge
                        newOffsetX -= canvasDeltaX;
                        newWidth += canvasDeltaX;
                        break;
                    case 2: // Right edge
                        newWidth += canvasDeltaX;
                        break;
                    case 3: // Top edge
                        newOffsetY -= canvasDeltaY;
                        newHeight += canvasDeltaY;
                        break;
                    case 4: // Bottom edge
                        newHeight += canvasDeltaY;
                        break;
                    case 5: // Top-left corner
                        newOffsetX -= canvasDeltaX;
                        newWidth += canvasDeltaX;
                        newOffsetY -= canvasDeltaY;
                        newHeight += canvasDeltaY;
                        break;
                    case 6: // Top-right corner
                        newWidth += canvasDeltaX;
                        newOffsetY -= canvasDeltaY;
                        newHeight += canvasDeltaY;
                        break;
                    case 7: // Bottom-left corner
                        newOffsetX -= canvasDeltaX;
                        newWidth += canvasDeltaX;
                        newHeight += canvasDeltaY;
                        break;
                    case 8: // Bottom-right corner
                        newWidth += canvasDeltaX;
                        newHeight += canvasDeltaY;
                        break;
                }
                
                // Ensure minimum size
                if (newWidth < 100) newWidth = 100;
                if (newHeight < 100) newHeight = 100;
                
                // Update canvas (temporarily, will be finalized in MouseUp)
                ResizeCanvas(newWidth, newHeight, newOffsetX, newOffsetY, false);
                picBoxMain.Invalidate();
                return;
            }
            
            // Update cursor when hovering over resize handles
            if (zoomLevel < 1.0f)
            {
                int handle = GetCanvasResizeHandle(e.Location);
                if (handle > 0)
                {
                    // Set appropriate cursor based on handle
                    Cursor cursor = Cursors.Default;
                    switch (handle)
                    {
                        case 1: case 2: cursor = Cursors.SizeWE; break;
                        case 3: case 4: cursor = Cursors.SizeNS; break;
                        case 5: case 8: cursor = Cursors.SizeNWSE; break;
                        case 6: case 7: cursor = Cursors.SizeNESW; break;
                    }
                    picBoxMain.Cursor = cursor;
                    return;
                }
                else
                {
                    picBoxMain.Cursor = Cursors.Default;
                }
            }
            
            // Convert screen coordinates to canvas coordinates accounting for zoom
            Point canvasLocation = ScreenToCanvas(e.Location);
            
            if ((flagLoad || flagEmoji) && isDraggingImage && selectedImage != null)
            {
                // Calculate new visual position based on mouse location and drag offset
                // Allow dragging outside canvas bounds - no edge snapping
                int newVisualX = canvasLocation.X - dragOffset.X;
                int newVisualY = canvasLocation.Y - dragOffset.Y;

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
                textPosition = canvasLocation;
                DrawTextOnCanvas();
                picBoxMain.Invalidate();
            }
            else if (flagDraw == true && (flagBrush == true || flagErase == true))
            {
                // Always update endP to track pen position (even outside bounds)
                endP = canvasLocation;
                
                // Convert to bitmap coordinates
                Point bitmapStartP = CanvasToBitmap(startP);
                Point bitmapEndP = CanvasToBitmap(endP);
                
                // Set up graphics with clipping to bitmap bounds
                // This allows drawing to continue tracking outside bounds but only renders visible parts
                g = Graphics.FromImage(bm);
                g.SetClip(new Rectangle(0, 0, bm.Width, bm.Height));
                
                if (flagErase == false)
                {
                    if (flagBrush == true)
                    {
                        // Draw using the selected brush type (will be clipped to bounds)
                        DrawWithBrushType(g, bitmapStartP, bitmapEndP);
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
                    // Draw line to connect points smoothly (will be clipped to bounds)
                    g.DrawLine(eraserPen, bitmapStartP, bitmapEndP);
                    // Fill ellipse at end point for complete coverage (will be clipped to bounds)
                    int radius = eraserSize / 2;
                    using (SolidBrush eraserBrush = new SolidBrush(picBoxMain.BackColor))
                    {
                        g.FillEllipse(eraserBrush, bitmapEndP.X - radius, bitmapEndP.Y - radius, eraserSize, eraserSize);
                    }
                }
                g.ResetClip(); // Reset clipping
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
                // Update preview position when hovering in text mode (use canvas coordinates)
                previewMousePos = canvasLocation;
                picBoxMain.Invalidate();
            }
            else if (isDrawingShape && (flagLine || flagSquare || flagCircle || flagNgon))
            {
                // Update shape preview while dragging
                shapeEndPoint = canvasLocation;
                picBoxMain.Invalidate(); // Trigger Paint event to show preview
            }
            else if (flagLoad || flagEmoji)
            {
                // Update cursor when hovering over images/resize handles
                UpdateCursorForImageMode(canvasLocation);
            }
            startP = endP;
        }

        private void picBoxMain_MouseUp(object sender, MouseEventArgs e)
        {
            // Finalize viewport panning
            if (isPanning)
            {
                isPanning = false;
                picBoxMain.Cursor = Cursors.Default;
                return;
            }
            
            // Finalize canvas resize
            if (isResizingCanvas)
            {
                int deltaX = e.Location.X - canvasResizeStart.X;
                int deltaY = e.Location.Y - canvasResizeStart.Y;
                
                // Convert screen delta to canvas delta
                int canvasDeltaX = (int)(deltaX / zoomLevel);
                int canvasDeltaY = (int)(deltaY / zoomLevel);
                
                int newWidth = canvasResizeStartWidth;
                int newHeight = canvasResizeStartHeight;
                int newOffsetX = canvasResizeStartOffsetX;
                int newOffsetY = canvasResizeStartOffsetY;
                
                // Adjust based on which edge is being dragged
                switch (canvasResizeEdge)
                {
                    case 1: // Left edge
                        newOffsetX -= canvasDeltaX;
                        newWidth += canvasDeltaX;
                        break;
                    case 2: // Right edge
                        newWidth += canvasDeltaX;
                        break;
                    case 3: // Top edge
                        newOffsetY -= canvasDeltaY;
                        newHeight += canvasDeltaY;
                        break;
                    case 4: // Bottom edge
                        newHeight += canvasDeltaY;
                        break;
                    case 5: // Top-left corner
                        newOffsetX -= canvasDeltaX;
                        newWidth += canvasDeltaX;
                        newOffsetY -= canvasDeltaY;
                        newHeight += canvasDeltaY;
                        break;
                    case 6: // Top-right corner
                        newWidth += canvasDeltaX;
                        newOffsetY -= canvasDeltaY;
                        newHeight += canvasDeltaY;
                        break;
                    case 7: // Bottom-left corner
                        newOffsetX -= canvasDeltaX;
                        newWidth += canvasDeltaX;
                        newHeight += canvasDeltaY;
                        break;
                    case 8: // Bottom-right corner
                        newWidth += canvasDeltaX;
                        newHeight += canvasDeltaY;
                        break;
                }
                
                // Ensure minimum size
                if (newWidth < 100) newWidth = 100;
                if (newHeight < 100) newHeight = 100;
                
                // Finalize canvas resize
                ResizeCanvas(newWidth, newHeight, newOffsetX, newOffsetY, true);
                isResizingCanvas = false;
                canvasResizeEdge = 0;
                picBoxMain.Cursor = Cursors.Default;
                picBoxMain.Invalidate();
                return;
            }
            
            // Convert screen coordinates to canvas coordinates accounting for zoom
            Point canvasLocation = ScreenToCanvas(e.Location);
            
            if (isDrawingShape && (flagLine || flagSquare || flagCircle || flagNgon))
            {
                // Finalize shape by drawing to bitmap
                shapeEndPoint = canvasLocation;
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
        
        // Resize the canvas bitmap
        private void ResizeCanvas(int newWidth, int newHeight, int newOffsetX, int newOffsetY, bool saveUndo)
        {
            if (bm == null) return;
            
            if (saveUndo)
            {
                SaveUndoState(); // Save before resizing
            }
            
            // Create new bitmap with new size
            Bitmap newBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.Clear(DrawingColor.LightGray);
                
                // Calculate where to draw the old bitmap in the new bitmap
                int drawX = canvasOffsetX - newOffsetX;
                int drawY = canvasOffsetY - newOffsetY;
                
                // Draw old bitmap at the correct position
                g.DrawImage(bm, drawX, drawY);
            }
            
            // Update bitmap and tracking variables
            bm.Dispose();
            bm = newBitmap;
            canvasOffsetX = newOffsetX;
            canvasOffsetY = newOffsetY;
            canvasActualWidth = newWidth;
            canvasActualHeight = newHeight;
            
            // Update Image property
            if (zoomLevel == 1.0f && canvasOffsetX == 0 && canvasOffsetY == 0 && 
                viewportOffsetX == 0 && viewportOffsetY == 0)
            {
                picBoxMain.Image = bm;
            }
            else
            {
                picBoxMain.Image = null; // Draw manually in Paint
            }
            
            // Update scrollbars after canvas resize
            UpdateScrollBars();
        }
        
        // Clamp viewport offset to prevent scrolling beyond canvas bounds
        // 
        // Coordinate transformation analysis:
        // In Paint: TranslateTransform(viewportOffsetX, viewportOffsetY) then ScaleTransform(zoomLevel) then DrawImage(bm, canvasOffsetX, canvasOffsetY)
        //
        // Graphics transformations work on the coordinate system:
        // - TranslateTransform(viewportOffsetX, viewportOffsetY): moves origin, so point (x,y) appears at screen (viewportOffsetX + x, viewportOffsetY + y)
        // - ScaleTransform(zoomLevel): scales coordinate system, so point (x,y) appears at screen (viewportOffsetX + x*zoomLevel, viewportOffsetY + y*zoomLevel)
        //
        // Canvas point (0,0) is at bitmap position (canvasOffsetX, canvasOffsetY)
        // So canvas (0,0) appears at screen: (viewportOffsetX + canvasOffsetX * zoomLevel, viewportOffsetY + canvasOffsetY * zoomLevel)
        //
        // For canvas (0,0) to be at screen (0,0):
        //   viewportOffsetX + canvasOffsetX * zoomLevel = 0
        //   viewportOffsetX = -canvasOffsetX * zoomLevel
        //
        // Maximum scroll right: canvas right edge at viewport right edge
        // Canvas right edge in canvas coords: canvasOffsetX + canvasActualWidth
        // Canvas right edge in screen coords: viewportOffsetX + (canvasOffsetX + canvasActualWidth) * zoomLevel
        // For right edge at viewport right: viewportOffsetX + (canvasOffsetX + canvasActualWidth) * zoomLevel = visibleWidth
        // Therefore: viewportOffsetX = visibleWidth - (canvasOffsetX + canvasActualWidth) * zoomLevel
        private void ClampViewportOffset()
        {
            if (bm == null) return;
            
            // Calculate the visible canvas size
            int visibleWidth = picBoxMain.Width;
            int visibleHeight = picBoxMain.Height;
            
            // Calculate where canvas (0,0) appears on screen
            // Canvas (0,0) is at bitmap position (canvasOffsetX, canvasOffsetY)
            // After transformations: screen position = viewportOffsetX + canvasOffsetX * zoomLevel
            // For canvas (0,0) to be at screen (0,0): viewportOffsetX = -canvasOffsetX * zoomLevel
            int minScrollX = (int)(-canvasOffsetX * zoomLevel);
            int minScrollY = (int)(-canvasOffsetY * zoomLevel);
            
            // Calculate maximum scroll (canvas right/bottom edge at viewport right/bottom edge)
            // Canvas right edge in canvas coords: canvasOffsetX + canvasActualWidth
            // Canvas right edge in screen coords: viewportOffsetX + (canvasOffsetX + canvasActualWidth) * zoomLevel
            // For right edge at viewport right: viewportOffsetX + (canvasOffsetX + canvasActualWidth) * zoomLevel = visibleWidth
            int canvasRightEdgeCanvas = canvasOffsetX + canvasActualWidth;
            int canvasBottomEdgeCanvas = canvasOffsetY + canvasActualHeight;
            
            int maxScrollX = visibleWidth - (int)(canvasRightEdgeCanvas * zoomLevel);
            int maxScrollY = visibleHeight - (int)(canvasBottomEdgeCanvas * zoomLevel);
            
            // Clamp viewport offsets to valid range. When zoomed in, maxScroll < minScroll (e.g. maxScrollX negative),
            // so the valid range is [maxScroll, minScroll]. Use min/max of the pair so it works for both zoom in and out.
            int lowX = Math.Min(minScrollX, maxScrollX);
            int highX = Math.Max(minScrollX, maxScrollX);
            int lowY = Math.Min(minScrollY, maxScrollY);
            int highY = Math.Max(minScrollY, maxScrollY);
            viewportOffsetX = Math.Max(lowX, Math.Min(highX, viewportOffsetX));
            viewportOffsetY = Math.Max(lowY, Math.Min(highY, viewportOffsetY));
        }
        
        // Update scrollbar ranges and values based on canvas size and viewport
        private void UpdateScrollBars()
        {
            if (bm == null) return;
            
            // Clamp viewport offset first to ensure it's within bounds
            ClampViewportOffset();
            
            // Calculate the visible canvas size
            int visibleWidth = picBoxMain.Width;
            int visibleHeight = picBoxMain.Height;
            
            // Calculate canvas edges in canvas coordinates
            int canvasLeftEdgeCanvas = canvasOffsetX;
            int canvasTopEdgeCanvas = canvasOffsetY;
            int canvasRightEdgeCanvas = canvasOffsetX + canvasActualWidth;
            int canvasBottomEdgeCanvas = canvasOffsetY + canvasActualHeight;
            
            // Calculate canvas size in screen coordinates (after zoom)
            int canvasWidthScreen = (int)(canvasActualWidth * zoomLevel);
            int canvasHeightScreen = (int)(canvasActualHeight * zoomLevel);
            
            // Calculate scroll limits
            // Minimum: canvas (0,0) at screen (0,0)
            // Screen position of canvas (0,0) = viewportOffsetX + canvasOffsetX * zoomLevel
            // For canvas (0,0) at screen (0,0): viewportOffsetX = -canvasOffsetX * zoomLevel
            int minScrollX = (int)(-canvasOffsetX * zoomLevel);
            int minScrollY = (int)(-canvasOffsetY * zoomLevel);
            
            // Maximum: canvas right/bottom edge at viewport right/bottom edge
            // Screen position of canvas right edge = viewportOffsetX + canvasRightEdgeCanvas * zoomLevel
            // For right edge at viewport right: viewportOffsetX + canvasRightEdgeCanvas * zoomLevel = visibleWidth
            // Therefore: viewportOffsetX = visibleWidth - canvasRightEdgeCanvas * zoomLevel
            int maxScrollX = visibleWidth - (int)(canvasRightEdgeCanvas * zoomLevel);
            int maxScrollY = visibleHeight - (int)(canvasBottomEdgeCanvas * zoomLevel);
            
            // Show/hide scrollbars based on whether canvas is larger than viewport
            // We need scrolling when zoomed in (canvas larger than viewport) or when we have a valid scroll range
            bool needHScroll = canvasWidthScreen > visibleWidth || minScrollX != maxScrollX;
            bool needVScroll = canvasHeightScreen > visibleHeight || minScrollY != maxScrollY;
            
            hScrollBarCanvas.Visible = needHScroll;
            vScrollBarCanvas.Visible = needVScroll;
            
            if (needHScroll)
            {
                // Scrollbar range: when zoomed in, maxScrollX < minScrollX, so valid range is [maxScrollX, minScrollX].
                // Always set Minimum <= Maximum for the control.
                int rangeMinX = Math.Min(minScrollX, maxScrollX);
                int rangeMaxX = Math.Max(minScrollX, maxScrollX);
                int scrollRange = rangeMaxX - rangeMinX;
                
                hScrollBarCanvas.Minimum = rangeMinX;
                hScrollBarCanvas.Maximum = rangeMaxX;
                hScrollBarCanvas.LargeChange = Math.Max(1, Math.Min(visibleWidth, scrollRange));
                hScrollBarCanvas.SmallChange = Math.Max(1, visibleWidth / 10);
                hScrollBarCanvas.Value = Math.Max(rangeMinX, Math.Min(rangeMaxX, viewportOffsetX));
            }
            
            if (needVScroll)
            {
                int rangeMinY = Math.Min(minScrollY, maxScrollY);
                int rangeMaxY = Math.Max(minScrollY, maxScrollY);
                int scrollRange = rangeMaxY - rangeMinY;
                
                vScrollBarCanvas.Minimum = rangeMinY;
                vScrollBarCanvas.Maximum = rangeMaxY;
                vScrollBarCanvas.LargeChange = Math.Max(1, Math.Min(visibleHeight, scrollRange));
                vScrollBarCanvas.SmallChange = Math.Max(1, visibleHeight / 10);
                vScrollBarCanvas.Value = Math.Max(rangeMinY, Math.Min(rangeMaxY, viewportOffsetY));
            }
        }
        
        // Handle horizontal scrollbar scroll
        private void hScrollBarCanvas_Scroll(object sender, ScrollEventArgs e)
        {
            // Update viewport offset from scrollbar value
            viewportOffsetX = e.NewValue;
            // Clamp to ensure it's within bounds (scrollbar might allow slight out-of-range values)
            ClampViewportOffset();
            // Update scrollbar to reflect any clamping (but don't call UpdateScrollBars to avoid recursion)
            if (hScrollBarCanvas.Visible)
            {
                hScrollBarCanvas.Value = viewportOffsetX;
            }
            // Redraw with new viewport position
            picBoxMain.Invalidate();
        }
        
        // Handle vertical scrollbar scroll
        private void vScrollBarCanvas_Scroll(object sender, ScrollEventArgs e)
        {
            // Update viewport offset from scrollbar value
            viewportOffsetY = e.NewValue;
            // Clamp to ensure it's within bounds (scrollbar might allow slight out-of-range values)
            ClampViewportOffset();
            // Update scrollbar to reflect any clamping (but don't call UpdateScrollBars to avoid recursion)
            if (vScrollBarCanvas.Visible)
            {
                vScrollBarCanvas.Value = viewportOffsetY;
            }
            // Redraw with new viewport position
            picBoxMain.Invalidate();
        }

        // Helper method to handle color selection
        private void HandleColorSelection(DrawingColor selectedColor)
        {
            // If a shape tool is active, update shape border color
            if (flagLine || flagSquare || flagCircle || flagNgon)
            {
                shapeBorderColor = selectedColor;
                picBoxShapeOutsideColor.BackColor = shapeBorderColor;
                if (isDrawingShape)
                {
                    picBoxMain.Invalidate();
                }
            }
            else
            {
                brushPen.Color = selectedColor;
                brush.Color = selectedColor;
                picBoxBrushColor.BackColor = brushPen.Color;
                picBoxBrushColor.Image = null;
                picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
                flagErase = false;
            }

            // Update text preview if in text mode
            if (flagText)
            {
                picBoxMain.Invalidate();
            }
        }

        private void picBoxRed_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();
            HandleColorSelection(picBoxRed.BackColor);
        }

        private void picBoxBlack_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            HandleColorSelection(picBoxBlack.BackColor);
        }

        private void picBoxGreen_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            HandleColorSelection(picBoxGreen.BackColor);
        }

        private void picBoxBlue_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            HandleColorSelection(picBoxBlue.BackColor);
        }

        private void picBoxCyan_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            HandleColorSelection(picBoxCyan.BackColor);
        }

        private void picBoxMagenta_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            HandleColorSelection(picBoxMagenta.BackColor);
        }

        private void picBoxYellow_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            HandleColorSelection(picBoxYellow.BackColor);
        }

        private void picBoxOrange_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            HandleColorSelection(picBoxOrange.BackColor);
        }

        private void picBoxWhite_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            HandleColorSelection(picBoxWhite.BackColor);
        }

        private void picBoxPurple_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            HandleColorSelection(picBoxPurple.BackColor);
        }

        private void picBoxBrown_Click(object sender, EventArgs e)
        {
            AutoStampOnToolSwitch();
            HandleColorSelection(picBoxBrown.BackColor);
        }

        private void picBoxCustom_Click(object sender, EventArgs e)
        {
            // Auto-stamp if switching from load mode
            AutoStampOnToolSwitch();

            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.AllowFullOpen = true;
                colorDialog.FullOpen = true;

                // If a shape tool is active, show current border color, otherwise show brush color
                colorDialog.Color = (flagLine || flagSquare || flagCircle || flagNgon) ? shapeBorderColor : brushPen.Color;

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    if (flagLine || flagSquare || flagCircle || flagNgon)
                    {
                        shapeBorderColor = colorDialog.Color;
                        picBoxShapeOutsideColor.BackColor = shapeBorderColor;
                        if (isDrawingShape)
                        {
                            picBoxMain.Invalidate();
                        }
                    }
                    else
                    {
                        brushPen.Color = colorDialog.Color;
                        brush.Color = colorDialog.Color;
                        picBoxBrushColor.BackColor = brushPen.Color;
                        picBoxCustom.BackColor = colorDialog.Color;
                        picBoxBrushColor.Image = null;
                        picBoxMain.Cursor = brushCursor ?? Cursors.Cross;
                        flagErase = false;
                    }

                    // Update text preview if in text mode
                    if (flagText)
                    {
                        picBoxMain.Invalidate();
                    }
                }
            }
        }

        private void picBoxShapeOutsideColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.AllowFullOpen = true;
                colorDialog.FullOpen = true;
                colorDialog.Color = shapeBorderColor;

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    shapeBorderColor = colorDialog.Color;
                    picBoxShapeOutsideColor.BackColor = shapeBorderColor;

                    // Update shape preview if drawing
                    if (isDrawingShape)
                    {
                        picBoxMain.Invalidate();
                    }
                }
            }
        }

        private void picBoxShapeInsideColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.AllowFullOpen = true;
                colorDialog.FullOpen = true;
                colorDialog.Color = shapeFillColor;

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    shapeFillColor = colorDialog.Color;
                    picBoxShapeInsideColor.BackColor = shapeFillColor;

                    // Update shape preview if drawing
                    if (isDrawingShape)
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
            g.FillRectangle(new SolidBrush(DrawingColor.LightGray), rect);
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
            picBoxBrushColor.BackColor = DrawingColor.Transparent;
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
            picBoxBrushColor.BackColor = DrawingColor.Transparent;
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
                                    g.Clear(DrawingColor.Transparent); // Start with transparent background

                                    // Draw the canvas bitmap (which has the gray background and all drawings)
                                    // We need to replace the gray background with transparency
                                    for (int y = 0; y < this.bm.Height; y++)
                                    {
                                        for (int x = 0; x < this.bm.Width; x++)
                                        {
                                            DrawingColor pixelColor = this.bm.GetPixel(x, y);
                                            // If pixel is the canvas background color (LightGray), make it transparent
                                            if (pixelColor.R == DrawingColor.LightGray.R &&
                                                pixelColor.G == DrawingColor.LightGray.G &&
                                                pixelColor.B == DrawingColor.LightGray.B)
                                            {
                                                bmp.SetPixel(x, y, DrawingColor.Transparent);
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
                        picBoxBrushColor.BackColor = DrawingColor.Transparent;
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
            picBoxBrushColor.BackColor = DrawingColor.Transparent;
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
            picBoxBrushColor.BackColor = DrawingColor.Transparent;
            picBoxMain.Cursor = Cursors.Hand; // Hand cursor for fill tool
            SetToolBorder(picBoxFill);
            UpdateUnifiedSlider();
            UpdateUnifiedComboBox();
        }

        // Flood fill algorithm
        private void PerformFloodFill(Point startPoint)
        {
            if (bm == null) return;

            // Convert to bitmap coordinates
            Point bitmapPoint = CanvasToBitmap(startPoint);

            // Ensure point is within bitmap bounds (don't auto-expand)
            if (bitmapPoint.X < 0 || bitmapPoint.X >= bm.Width ||
                bitmapPoint.Y < 0 || bitmapPoint.Y >= bm.Height)
                return;

            DrawingColor targetColor = bm.GetPixel(bitmapPoint.X, bitmapPoint.Y);
            DrawingColor fillColor = brushPen.Color;

            // If clicking on the same color, don't do anything
            if (targetColor.ToArgb() == fillColor.ToArgb())
                return;

            // Use a queue-based flood fill algorithm
            Queue<Point> queue = new Queue<Point>();
            HashSet<Point> visited = new HashSet<Point>();

            queue.Enqueue(bitmapPoint);
            visited.Add(bitmapPoint);

            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();

                // Check if this pixel matches the target color
                if (current.X < 0 || current.X >= bm.Width ||
                    current.Y < 0 || current.Y >= bm.Height)
                    continue;

                DrawingColor pixelColor = bm.GetPixel(current.X, current.Y);

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

            // Convert to bitmap coordinates
            Point bitmapTextPos = CanvasToBitmap(textPosition);
            
            // Check if within bounds (don't auto-expand)
            if (bitmapTextPos.X < 0 || bitmapTextPos.Y < 0 ||
                bitmapTextPos.X >= bm.Width || bitmapTextPos.Y >= bm.Height)
                return;

            g = Graphics.FromImage(bm);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Font font = new Font(selectedFontName, selectedFontSize);
            SolidBrush textBrush = new SolidBrush(brushPen.Color);

            // Draw text
            g.DrawString(strText, font, textBrush, bitmapTextPos.X, bitmapTextPos.Y);

            // Calculate text bounds for dragging (in canvas coordinates)
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
                            tempG.Clear(DrawingColor.Transparent);
                            using (DrawingPen paintPen = new DrawingPen(brushPen.Color, paintBrushSize))
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
                            tempG.Clear(DrawingColor.Transparent);
                            using (DrawingPen markerPen = new DrawingPen(brushPen.Color, markerBrushSize))
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
                    DrawingColor pencilColor = DrawingColor.FromArgb(178, brushPen.Color); // 178/255 ≈ 70% opacity
                    using (DrawingPen pencilPen = new DrawingPen(pencilColor, pencilWidth))
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
                            using (SolidBrush airbrushBrush = new SolidBrush(DrawingColor.FromArgb(alpha, brushPen.Color)))
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
                    DrawingColor wetColor = DrawingColor.FromArgb(204, brushPen.Color); // 204/255 ≈ 80% opacity
                    using (DrawingPen wetPen = new DrawingPen(wetColor, wetBrushSize))
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
                        using (SolidBrush brush = new SolidBrush(DrawingColor.FromArgb(alpha, brushPen.Color)))
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

            // Convert to bitmap coordinates
            Point bitmapStart = CanvasToBitmap(shapeStartPoint);
            Point bitmapEnd = CanvasToBitmap(shapeEndPoint);
            
            // Check if within bounds (don't auto-expand)
            // Shapes can partially extend outside, so we'll allow drawing but clip to bounds

            g = Graphics.FromImage(bm);
            g.SmoothingMode = SmoothingMode.HighQuality;
            // Use brushPen.Width for border thickness, shapeBorderColor for border color
            DrawingPen shapePen = new DrawingPen(shapeBorderColor, brushPen.Width);
            SolidBrush shapeBrush = new SolidBrush(shapeFillColor);

            bool shiftPressed = (System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            if (flagLine)
            {
                g.DrawLine(shapePen, bitmapStart, bitmapEnd);
            }
            else if (flagSquare)
            {
                Rectangle rect = CalculateRectangle(bitmapStart, bitmapEnd, shiftPressed);
                if (shapeFilled)
                {
                    g.FillRectangle(shapeBrush, rect);
                }
                g.DrawRectangle(shapePen, rect);
            }
            else if (flagCircle)
            {
                Rectangle rect = CalculateRectangle(bitmapStart, bitmapEnd, shiftPressed);
                if (shapeFilled)
                {
                    g.FillEllipse(shapeBrush, rect);
                }
                g.DrawEllipse(shapePen, rect);
            }
            else if (flagNgon)
            {
                // Use Ctrl for regular (equal-sided) polygon, like Shift for square/circle
                bool ctrlPressed = (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control;
                Point[] points = CalculateNgonPoints(bitmapStart, bitmapEnd, ctrlPressed);
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

        private void picBoxMain_MouseWheel(object sender, MouseEventArgs e)
        {
            bool ctrlPressed = (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control;
            bool shiftPressed = (System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift;
            
            // Zoom when Ctrl is pressed
            if (ctrlPressed)
            {
                // Determine zoom direction
                float zoomFactor = e.Delta > 0 ? 1.1f : 0.9f;
                float newZoom = zoomLevel * zoomFactor;

                // Limit zoom range (10% to 500%)
                if (newZoom < 0.1f) newZoom = 0.1f;
                if (newZoom > 5.0f) newZoom = 5.0f;

                if (newZoom != zoomLevel)
                {
                    zoomLevel = newZoom;

                    // Update Image property based on zoom level and offset
                    // When zoomed or offset, we draw manually in Paint
                    if (zoomLevel == 1.0f && canvasOffsetX == 0 && canvasOffsetY == 0 && 
                        viewportOffsetX == 0 && viewportOffsetY == 0 && bm != null)
                    {
                        picBoxMain.Image = bm;
                    }
                    else
                    {
                        picBoxMain.Image = null; // Draw manually in Paint when zoomed or offset
                    }

                    // Update scrollbars after zoom change
                    UpdateScrollBars();
                    
                    // Redraw with new zoom level
                    picBoxMain.Invalidate();
                }
            }
            // Two-finger scrolling (inverse/natural scrolling): pan the viewport
            else
            {
                // Inverse scrolling: two fingers up = scroll down, two fingers right = scroll left
                // So we invert the delta direction
                int scrollDelta = -e.Delta; // Invert for natural horizontal
                
                if (shiftPressed)
                {
                    // Horizontal scrolling via Shift+MouseWheel (fallback for systems that don't send WM_MOUSEHWHEEL)
                    viewportOffsetX += scrollDelta / 3;
                }
                else
                {
                    // Vertical scrolling: flipped so wheel up = pan up, wheel down = pan down
                    viewportOffsetY -= scrollDelta / 3;
                }
                
                // Clamp and update scrollbars
                ClampViewportOffset();
                UpdateScrollBars();
                
                // Redraw with new viewport position
                picBoxMain.Invalidate();
            }
        }

        // Convert screen coordinates to canvas coordinates accounting for zoom and viewport pan
        private Point ScreenToCanvas(Point screenPoint)
        {
            // Account for viewport pan offset
            int canvasX = screenPoint.X - viewportOffsetX;
            int canvasY = screenPoint.Y - viewportOffsetY;
            
            // Account for zoom
            if (zoomLevel != 1.0f)
            {
                canvasX = (int)(canvasX / zoomLevel);
                canvasY = (int)(canvasY / zoomLevel);
            }
            
            return new Point(canvasX, canvasY);
        }

        // Expand canvas bitmap if drawing outside current bounds
        // point is in canvas coordinates (logical canvas space)
        private void ExpandCanvasIfNeeded(Point point, int padding = 50)
        {
            if (bm == null) return;

            // Convert canvas coordinates to bitmap coordinates
            int bitmapX = point.X - canvasOffsetX;
            int bitmapY = point.Y - canvasOffsetY;

            // Check if we need to expand
            bool needExpand = false;
            int newOffsetX = canvasOffsetX;
            int newOffsetY = canvasOffsetY;
            int newWidth = canvasActualWidth;
            int newHeight = canvasActualHeight;

            // Check if point is to the left of current bitmap
            if (bitmapX < -padding)
            {
                int expandLeft = -bitmapX + padding;
                newOffsetX -= expandLeft; // Move origin further left
                newWidth += expandLeft;
                needExpand = true;
            }
            // Check if point is to the right of current bitmap
            else if (bitmapX >= canvasActualWidth + padding)
            {
                int expandRight = bitmapX - canvasActualWidth + padding;
                newWidth += expandRight;
                needExpand = true;
            }

            // Check if point is above current bitmap
            if (bitmapY < -padding)
            {
                int expandTop = -bitmapY + padding;
                newOffsetY -= expandTop; // Move origin further up
                newHeight += expandTop;
                needExpand = true;
            }
            // Check if point is below current bitmap
            else if (bitmapY >= canvasActualHeight + padding)
            {
                int expandBottom = bitmapY - canvasActualHeight + padding;
                newHeight += expandBottom;
                needExpand = true;
            }

            if (needExpand)
            {
                SaveUndoState(); // Save before expanding

                // Create new larger bitmap
                Bitmap newBitmap = new Bitmap(newWidth, newHeight);
                using (Graphics g = Graphics.FromImage(newBitmap))
                {
                    g.Clear(DrawingColor.LightGray);
                    
                    // Draw old bitmap at the position that maintains canvas (0,0) at the same logical position
                    // The old bitmap was drawn at (canvasOffsetX - oldOffsetX, ...) in the old bitmap
                    // Now we need to draw it at (newOffsetX - oldOffsetX, ...) in the new bitmap
                    // But since canvasOffsetX changed, we need to adjust
                    int oldBitmapX = canvasOffsetX - newOffsetX;
                    int oldBitmapY = canvasOffsetY - newOffsetY;
                    g.DrawImage(bm, oldBitmapX, oldBitmapY);
                }

                // Update bitmap and tracking variables
                bm.Dispose();
                bm = newBitmap;
                canvasOffsetX = newOffsetX;
                canvasOffsetY = newOffsetY;
                canvasActualWidth = newWidth;
                canvasActualHeight = newHeight;

                // Update Image property if not zoomed and no offset (for performance)
                // If we have an offset, we'll draw manually in Paint
                if (zoomLevel == 1.0f && canvasOffsetX == 0 && canvasOffsetY == 0)
                {
                    picBoxMain.Image = bm;
                }
                else
                {
                    picBoxMain.Image = null; // Draw manually in Paint
                }
            }
        }

        // Convert canvas coordinates to bitmap coordinates (accounting for offset)
        private Point CanvasToBitmap(Point canvasPoint)
        {
            return new Point(
                canvasPoint.X - canvasOffsetX,
                canvasPoint.Y - canvasOffsetY
            );
        }

        // Get canvas bounds in screen coordinates (accounting for viewport pan and zoom)
        private Rectangle GetCanvasBoundsScreen()
        {
            int screenX, screenY, screenWidth, screenHeight;
            
            if (zoomLevel == 1.0f)
            {
                screenX = viewportOffsetX;
                screenY = viewportOffsetY;
                screenWidth = canvasActualWidth;
                screenHeight = canvasActualHeight;
            }
            else
            {
                // When zoomed, canvas appears smaller, and we need to account for viewport pan
                screenX = viewportOffsetX + (int)(canvasOffsetX * zoomLevel);
                screenY = viewportOffsetY + (int)(canvasOffsetY * zoomLevel);
                screenWidth = (int)(canvasActualWidth * zoomLevel);
                screenHeight = (int)(canvasActualHeight * zoomLevel);
            }
            
            return new Rectangle(screenX, screenY, screenWidth, screenHeight);
        }

        // Check if point is on a canvas resize handle
        private int GetCanvasResizeHandle(Point screenPoint)
        {
            if (zoomLevel == 1.0f) return 0; // No resize handles when not zoomed
            
            Rectangle canvasBounds = GetCanvasBoundsScreen();
            int handleSize = RESIZE_HANDLE_SIZE;
            
            // Check corners first (they take priority)
            if (Math.Abs(screenPoint.X - canvasBounds.Left) <= handleSize && 
                Math.Abs(screenPoint.Y - canvasBounds.Top) <= handleSize)
                return 5; // Top-left
            if (Math.Abs(screenPoint.X - canvasBounds.Right) <= handleSize && 
                Math.Abs(screenPoint.Y - canvasBounds.Top) <= handleSize)
                return 6; // Top-right
            if (Math.Abs(screenPoint.X - canvasBounds.Left) <= handleSize && 
                Math.Abs(screenPoint.Y - canvasBounds.Bottom) <= handleSize)
                return 7; // Bottom-left
            if (Math.Abs(screenPoint.X - canvasBounds.Right) <= handleSize && 
                Math.Abs(screenPoint.Y - canvasBounds.Bottom) <= handleSize)
                return 8; // Bottom-right
            
            // Check edges
            if (Math.Abs(screenPoint.X - canvasBounds.Left) <= handleSize && 
                screenPoint.Y >= canvasBounds.Top && screenPoint.Y <= canvasBounds.Bottom)
                return 1; // Left
            if (Math.Abs(screenPoint.X - canvasBounds.Right) <= handleSize && 
                screenPoint.Y >= canvasBounds.Top && screenPoint.Y <= canvasBounds.Bottom)
                return 2; // Right
            if (Math.Abs(screenPoint.Y - canvasBounds.Top) <= handleSize && 
                screenPoint.X >= canvasBounds.Left && screenPoint.X <= canvasBounds.Right)
                return 3; // Top
            if (Math.Abs(screenPoint.Y - canvasBounds.Bottom) <= handleSize && 
                screenPoint.X >= canvasBounds.Left && screenPoint.X <= canvasBounds.Right)
                return 4; // Bottom
            
            return 0; // Not on a handle
        }

        private void picBoxMain_Paint(object sender, PaintEventArgs e)
        {
            // Save graphics state for overlays
            GraphicsState overlayState = e.Graphics.Save();
            
            // Apply viewport pan offset first (translate before zoom)
            // Always apply, even if 0, to maintain consistent transformation order
            e.Graphics.TranslateTransform(viewportOffsetX, viewportOffsetY);
            
            // Apply zoom transformation if zoomed
            if (zoomLevel != 1.0f)
            {
                e.Graphics.ScaleTransform(zoomLevel, zoomLevel);
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor; // Use nearest neighbor for pixel-perfect zoom
                
                // Draw the bitmap manually when zoomed (Image property is set to null)
                // Position it so canvas (0,0) aligns with PictureBox (0,0) after viewport offset
                if (bm != null)
                {
                    e.Graphics.DrawImage(bm, canvasOffsetX, canvasOffsetY);
                }
            }
            // When not zoomed, check if we need to draw manually
            else if (canvasOffsetX != 0 || canvasOffsetY != 0 || viewportOffsetX != 0 || viewportOffsetY != 0)
            {
                // If we have any offset (canvas or viewport), draw manually
                if (bm != null)
                {
                    e.Graphics.DrawImage(bm, canvasOffsetX, canvasOffsetY);
                }
            }
            // When no zoom and no offsets, Image property handles drawing automatically

            // Draw all placed images as temporary overlay (not yet stamped to bitmap)
            // MUST be drawn before restoring graphics state so they're in canvas coordinates
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

            // Draw border around selected image in load/emoji mode (also in canvas coordinates)
            if ((flagLoad || flagEmoji) && selectedImage != null && !isDraggingImage)
            {
                Rectangle bounds = GetScaledBounds(selectedImage);
                using (DrawingPen borderPen = new DrawingPen(DrawingColor.Blue, 2))
                {
                    e.Graphics.DrawRectangle(borderPen, bounds);
                }
            }
            
            // Restore graphics state for overlays (drawn in screen coordinates)
            e.Graphics.Restore(overlayState);
            
            // Draw canvas bounds and resize handles when zoomed out (in screen coordinates)
            if (zoomLevel < 1.0f)
            {
                Rectangle canvasBounds = GetCanvasBoundsScreen();
                
                // Draw canvas border
                using (DrawingPen borderPen = new DrawingPen(DrawingColor.Blue, 2))
                {
                    e.Graphics.DrawRectangle(borderPen, canvasBounds);
                }
                
                // Draw resize handles
                int handleSize = RESIZE_HANDLE_SIZE;
                using (SolidBrush handleBrush = new SolidBrush(DrawingColor.Orange))
                {
                    // Corner handles
                    e.Graphics.FillRectangle(handleBrush, 
                        canvasBounds.Left - handleSize/2, canvasBounds.Top - handleSize/2, handleSize, handleSize);
                    e.Graphics.FillRectangle(handleBrush, 
                        canvasBounds.Right - handleSize/2, canvasBounds.Top - handleSize/2, handleSize, handleSize);
                    e.Graphics.FillRectangle(handleBrush, 
                        canvasBounds.Left - handleSize/2, canvasBounds.Bottom - handleSize/2, handleSize, handleSize);
                    e.Graphics.FillRectangle(handleBrush, 
                        canvasBounds.Right - handleSize/2, canvasBounds.Bottom - handleSize/2, handleSize, handleSize);
                    
                    // Edge handles
                    int midX = (canvasBounds.Left + canvasBounds.Right) / 2;
                    int midY = (canvasBounds.Top + canvasBounds.Bottom) / 2;
                    e.Graphics.FillRectangle(handleBrush, 
                        canvasBounds.Left - handleSize/2, midY - handleSize/2, handleSize, handleSize);
                    e.Graphics.FillRectangle(handleBrush, 
                        canvasBounds.Right - handleSize/2, midY - handleSize/2, handleSize, handleSize);
                    e.Graphics.FillRectangle(handleBrush, 
                        midX - handleSize/2, canvasBounds.Top - handleSize/2, handleSize, handleSize);
                    e.Graphics.FillRectangle(handleBrush, 
                        midX - handleSize/2, canvasBounds.Bottom - handleSize/2, handleSize, handleSize);
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
                    using (SolidBrush textBrush = new SolidBrush(DrawingColor.FromArgb(128, brushPen.Color)))
                    {
                        e.Graphics.DrawString(previewText, font, textBrush, previewMousePos.X, previewMousePos.Y);
                    }
                }
            }

            // Draw shape preview while dragging
            if (isDrawingShape && (flagLine || flagSquare || flagCircle || flagNgon))
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                // Use brushPen.Width for border thickness, shapeBorderColor for border, shapeFillColor for fill
                using (DrawingPen previewPen = new DrawingPen(DrawingColor.FromArgb(180, shapeBorderColor), brushPen.Width))
                using (SolidBrush previewBrush = new SolidBrush(DrawingColor.FromArgb(128, shapeFillColor)))
                {
                    bool shiftPressed = (System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.Shift;

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
                        bool ctrlPressed = (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control;
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
                Bitmap bmp = new Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var bitmapData = bmp.LockBits(
                    new Rectangle(0, 0, bitmapWidth, bitmapHeight),
                    ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                renderBitmap.CopyPixels(
                    System.Windows.Int32Rect.Empty,
                    bitmapData.Scan0,
                    bitmapData.Height * bitmapData.Stride,
                    bitmapData.Stride);

                bmp.UnlockBits(bitmapData);

                // Make white background transparent
                bmp.MakeTransparent(DrawingColor.White);

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
                    g.Clear(DrawingColor.Transparent);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    float x = (bitmapWidth - textSize.Width) / 2;
                    float y = (bitmapHeight - textSize.Height) / 2;
                    g.DrawString(emoji, emojiFont, System.Drawing.Brushes.Black, x, y);
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
                picBoxBrushColor.BackColor = DrawingColor.Transparent;
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

                System.Windows.Forms.TextBox inputBox = new System.Windows.Forms.TextBox();
                inputBox.Location = new Point(30, 30);
                inputBox.Size = new Size(340, 80);
                inputBox.Multiline = true;
                inputBox.Text = emojiText;
                inputBox.Font = new Font("Segoe UI Emoji", 20);
                inputBox.SelectAll();


                System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
                okButton.Text = "OK";
                okButton.DialogResult = DialogResult.OK;
                okButton.Location = new Point(120, 130);
                okButton.Size = new Size(100, 35);

                System.Windows.Forms.Button cancelButton = new System.Windows.Forms.Button();
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
                        picBoxBrushColor.BackColor = DrawingColor.Transparent;
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
                if (System.Windows.Forms.Control.ModifierKeys == Keys.Control)
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
                if (System.Windows.Forms.Control.ModifierKeys == Keys.Control)
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
                if (System.Windows.Forms.Control.ModifierKeys == Keys.Control)
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
                g.Clear(DrawingColor.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(5, 5, 20, 20);
                g.DrawRectangle(new DrawingPen(DrawingColor.Black, 2), rect);
            }
            picBoxSquare.Image = squareIcon;

            // Circle icon
            Bitmap circleIcon = new Bitmap(30, 30);
            using (Graphics g = Graphics.FromImage(circleIcon))
            {
                g.Clear(DrawingColor.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(5, 5, 20, 20);
                g.DrawEllipse(new DrawingPen(DrawingColor.Black, 2), rect);
            }
            picBoxCircle.Image = circleIcon;

            // N-gon icon (pentagon)
            Bitmap ngonIcon = new Bitmap(30, 30);
            using (Graphics g = Graphics.FromImage(ngonIcon))
            {
                g.Clear(DrawingColor.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Point[] points = new Point[]
                {
                    new Point(15, 5),    // Top
                    new Point(25, 12),   // Right
                    new Point(22, 22),   // Bottom right
                    new Point(8, 22),    // Bottom left
                    new Point(5, 12)     // Left
                };
                g.DrawPolygon(new DrawingPen(DrawingColor.Black, 2), points);
            }
            picBoxNgon.Image = ngonIcon;
        }

        // Paint handlers for shape icon previews (backup if images fail to load)
        private void picBoxSquare_Paint(object sender, PaintEventArgs e)
        {
            if (picBoxSquare.Image == null)
            {
                Rectangle rect = new Rectangle(5, 5, 20, 20);
                e.Graphics.DrawRectangle(new DrawingPen(DrawingColor.Black, 1), rect);
            }
        }

        private void picBoxCircle_Paint(object sender, PaintEventArgs e)
        {
            if (picBoxCircle.Image == null)
            {
                Rectangle rect = new Rectangle(5, 5, 20, 20);
                e.Graphics.DrawEllipse(new DrawingPen(DrawingColor.Black, 1), rect);
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
                e.Graphics.DrawPolygon(new DrawingPen(DrawingColor.Black, 1), points);
            }
        }

        private void lblFont_Click(object sender, EventArgs e)
        {

        }

        private void lblUnifiedCombo_Click(object sender, EventArgs e)
        {

        }

        private void trackBarNgonSides_Scroll(object sender, EventArgs e)
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
