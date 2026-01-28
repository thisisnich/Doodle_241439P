using System.Drawing;
using System.Drawing.Imaging;

namespace Doodle_241439P
{
    public partial class ImageFilterDialog : Form
    {
        private Bitmap originalImage;
        private Bitmap previewImage;
        private PictureBox previewBox;
        private RadioButton rbNone;
        private RadioButton rbGrayscale;
        private RadioButton rbSepia;
        private RadioButton rbInvert;
        private RadioButton rbBrightness;
        private RadioButton rbContrast;
        private RadioButton rbSaturation;
        private RadioButton rbCustomRGB;
        private TrackBar trackBarBrightness;
        private TrackBar trackBarContrast;
        private TrackBar trackBarSaturation;
        private TrackBar trackBarRed;
        private TrackBar trackBarGreen;
        private TrackBar trackBarBlue;
        private Label lblBrightness;
        private Label lblContrast;
        private Label lblSaturation;
        private Label lblRed;
        private Label lblGreen;
        private Label lblBlue;
        private Button btnReset;
        private Button btnApply;
        private Button btnCancel;
        private Panel panelControls;
        private Panel panelPreview;

        public Bitmap? FilteredImage { get; private set; }

        public ImageFilterDialog(Bitmap sourceImage)
        {
            if (sourceImage == null)
                throw new ArgumentNullException(nameof(sourceImage));

            originalImage = new Bitmap(sourceImage);
            previewImage = new Bitmap(sourceImage);
            FilteredImage = null;

            InitializeComponent();
            UpdatePreview();
        }

        private void InitializeComponent()
        {
            this.Text = "Image Color Filters";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // Main container
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 2;
            mainLayout.RowCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainLayout.Padding = new Padding(10);

            // Left panel - Controls
            panelControls = new Panel();
            panelControls.Dock = DockStyle.Fill;
            panelControls.AutoScroll = true;

            // Right panel - Preview
            panelPreview = new Panel();
            panelPreview.Dock = DockStyle.Fill;
            panelPreview.BorderStyle = BorderStyle.FixedSingle;

            previewBox = new PictureBox();
            previewBox.Dock = DockStyle.Fill;
            previewBox.SizeMode = PictureBoxSizeMode.Zoom;
            previewBox.BackColor = Color.White;
            panelPreview.Controls.Add(previewBox);

            mainLayout.Controls.Add(panelControls, 0, 0);
            mainLayout.Controls.Add(panelPreview, 1, 0);

            // Build controls panel
            BuildControlsPanel();

            // Bottom buttons
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Height = 50;
            buttonPanel.Padding = new Padding(10);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Size = new Size(100, 35);
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnApply = new Button();
            btnApply.Text = "Apply";
            btnApply.Size = new Size(100, 35);
            btnApply.DialogResult = DialogResult.OK;
            btnApply.Click += BtnApply_Click;

            btnReset = new Button();
            btnReset.Text = "Reset";
            btnReset.Size = new Size(100, 35);
            btnReset.Click += BtnReset_Click;

            buttonPanel.Controls.Add(btnApply);
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnReset);

            this.Controls.Add(mainLayout);
            this.Controls.Add(buttonPanel);
            this.AcceptButton = btnApply;
            this.CancelButton = btnCancel;
        }

        private void BuildControlsPanel()
        {
            int yPos = 10;
            int spacing = 35;

            // Filter Presets Label
            Label lblPresets = new Label();
            lblPresets.Text = "Filter Presets:";
            lblPresets.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPresets.Location = new Point(10, yPos);
            lblPresets.AutoSize = true;
            panelControls.Controls.Add(lblPresets);
            yPos += 25;

            // Preset Radio Buttons
            rbNone = CreateRadioButton("None (Original)", 10, yPos);
            rbNone.Checked = true;
            yPos += spacing;

            rbGrayscale = CreateRadioButton("Grayscale", 10, yPos);
            yPos += spacing;

            rbSepia = CreateRadioButton("Sepia", 10, yPos);
            yPos += spacing;

            rbInvert = CreateRadioButton("Invert", 10, yPos);
            yPos += spacing;

            rbBrightness = CreateRadioButton("Brightness", 10, yPos);
            yPos += spacing;

            rbContrast = CreateRadioButton("Contrast", 10, yPos);
            yPos += spacing;

            rbSaturation = CreateRadioButton("Saturation", 10, yPos);
            yPos += spacing;

            rbCustomRGB = CreateRadioButton("Custom RGB", 10, yPos);
            yPos += spacing + 10;

            // Brightness controls
            lblBrightness = new Label();
            lblBrightness.Text = "Brightness: 0";
            lblBrightness.Location = new Point(10, yPos);
            lblBrightness.AutoSize = true;
            panelControls.Controls.Add(lblBrightness);
            yPos += 20;

            trackBarBrightness = new TrackBar();
            trackBarBrightness.Location = new Point(10, yPos);
            trackBarBrightness.Size = new Size(300, 45);
            trackBarBrightness.Minimum = -100;
            trackBarBrightness.Maximum = 100;
            trackBarBrightness.Value = 0;
            trackBarBrightness.TickFrequency = 20;
            trackBarBrightness.ValueChanged += (s, e) =>
            {
                lblBrightness.Text = $"Brightness: {trackBarBrightness.Value}";
                if (rbBrightness.Checked) UpdatePreview();
            };
            panelControls.Controls.Add(trackBarBrightness);
            yPos += 50;

            // Contrast controls
            lblContrast = new Label();
            lblContrast.Text = "Contrast: 0";
            lblContrast.Location = new Point(10, yPos);
            lblContrast.AutoSize = true;
            panelControls.Controls.Add(lblContrast);
            yPos += 20;

            trackBarContrast = new TrackBar();
            trackBarContrast.Location = new Point(10, yPos);
            trackBarContrast.Size = new Size(300, 45);
            trackBarContrast.Minimum = -100;
            trackBarContrast.Maximum = 100;
            trackBarContrast.Value = 0;
            trackBarContrast.TickFrequency = 20;
            trackBarContrast.ValueChanged += (s, e) =>
            {
                lblContrast.Text = $"Contrast: {trackBarContrast.Value}";
                if (rbContrast.Checked) UpdatePreview();
            };
            panelControls.Controls.Add(trackBarContrast);
            yPos += 50;

            // Saturation controls
            lblSaturation = new Label();
            lblSaturation.Text = "Saturation: 0";
            lblSaturation.Location = new Point(10, yPos);
            lblSaturation.AutoSize = true;
            panelControls.Controls.Add(lblSaturation);
            yPos += 20;

            trackBarSaturation = new TrackBar();
            trackBarSaturation.Location = new Point(10, yPos);
            trackBarSaturation.Size = new Size(300, 45);
            trackBarSaturation.Minimum = -100;
            trackBarSaturation.Maximum = 100;
            trackBarSaturation.Value = 0;
            trackBarSaturation.TickFrequency = 20;
            trackBarSaturation.ValueChanged += (s, e) =>
            {
                lblSaturation.Text = $"Saturation: {trackBarSaturation.Value}";
                if (rbSaturation.Checked) UpdatePreview();
            };
            panelControls.Controls.Add(trackBarSaturation);
            yPos += 50;

            // Custom RGB Label
            Label lblRGB = new Label();
            lblRGB.Text = "Custom RGB (0-200%):";
            lblRGB.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRGB.Location = new Point(10, yPos);
            lblRGB.AutoSize = true;
            panelControls.Controls.Add(lblRGB);
            yPos += 25;

            // Red channel
            lblRed = new Label();
            lblRed.Text = "Red: 100%";
            lblRed.Location = new Point(10, yPos);
            lblRed.AutoSize = true;
            panelControls.Controls.Add(lblRed);
            yPos += 20;

            trackBarRed = new TrackBar();
            trackBarRed.Location = new Point(10, yPos);
            trackBarRed.Size = new Size(300, 45);
            trackBarRed.Minimum = 0;
            trackBarRed.Maximum = 200;
            trackBarRed.Value = 100;
            trackBarRed.TickFrequency = 25;
            trackBarRed.ValueChanged += (s, e) =>
            {
                lblRed.Text = $"Red: {trackBarRed.Value}%";
                if (rbCustomRGB.Checked) UpdatePreview();
            };
            panelControls.Controls.Add(trackBarRed);
            yPos += 50;

            // Green channel
            lblGreen = new Label();
            lblGreen.Text = "Green: 100%";
            lblGreen.Location = new Point(10, yPos);
            lblGreen.AutoSize = true;
            panelControls.Controls.Add(lblGreen);
            yPos += 20;

            trackBarGreen = new TrackBar();
            trackBarGreen.Location = new Point(10, yPos);
            trackBarGreen.Size = new Size(300, 45);
            trackBarGreen.Minimum = 0;
            trackBarGreen.Maximum = 200;
            trackBarGreen.Value = 100;
            trackBarGreen.TickFrequency = 25;
            trackBarGreen.ValueChanged += (s, e) =>
            {
                lblGreen.Text = $"Green: {trackBarGreen.Value}%";
                if (rbCustomRGB.Checked) UpdatePreview();
            };
            panelControls.Controls.Add(trackBarGreen);
            yPos += 50;

            // Blue channel
            lblBlue = new Label();
            lblBlue.Text = "Blue: 100%";
            lblBlue.Location = new Point(10, yPos);
            lblBlue.AutoSize = true;
            panelControls.Controls.Add(lblBlue);
            yPos += 20;

            trackBarBlue = new TrackBar();
            trackBarBlue.Location = new Point(10, yPos);
            trackBarBlue.Size = new Size(300, 45);
            trackBarBlue.Minimum = 0;
            trackBarBlue.Maximum = 200;
            trackBarBlue.Value = 100;
            trackBarBlue.TickFrequency = 25;
            trackBarBlue.ValueChanged += (s, e) =>
            {
                lblBlue.Text = $"Blue: {trackBarBlue.Value}%";
                if (rbCustomRGB.Checked) UpdatePreview();
            };
            panelControls.Controls.Add(trackBarBlue);

            // Wire up radio button events
            rbNone.CheckedChanged += (s, e) => { if (rbNone.Checked) UpdatePreview(); };
            rbGrayscale.CheckedChanged += (s, e) => { if (rbGrayscale.Checked) UpdatePreview(); };
            rbSepia.CheckedChanged += (s, e) => { if (rbSepia.Checked) UpdatePreview(); };
            rbInvert.CheckedChanged += (s, e) => { if (rbInvert.Checked) UpdatePreview(); };
            rbBrightness.CheckedChanged += (s, e) => { if (rbBrightness.Checked) UpdatePreview(); };
            rbContrast.CheckedChanged += (s, e) => { if (rbContrast.Checked) UpdatePreview(); };
            rbSaturation.CheckedChanged += (s, e) => { if (rbSaturation.Checked) UpdatePreview(); };
            rbCustomRGB.CheckedChanged += (s, e) => { if (rbCustomRGB.Checked) UpdatePreview(); };
        }

        private RadioButton CreateRadioButton(string text, int x, int y)
        {
            RadioButton rb = new RadioButton();
            rb.Text = text;
            rb.Location = new Point(x, y);
            rb.AutoSize = true;
            panelControls.Controls.Add(rb);
            return rb;
        }

        private void UpdatePreview()
        {
            if (previewImage != null)
            {
                previewImage.Dispose();
            }

            previewImage = ApplyFilter(originalImage);
            previewBox.Image = previewImage;
        }

        private Bitmap ApplyFilter(Bitmap source)
        {
            Bitmap result = new Bitmap(source.Width, source.Height);
            ColorMatrix matrix = GetColorMatrix();

            using (Graphics g = Graphics.FromImage(result))
            {
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                    0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
                attributes.Dispose();
            }

            return result;
        }

        private ColorMatrix GetColorMatrix()
        {
            ColorMatrix matrix = new ColorMatrix();

            if (rbNone.Checked)
            {
                // Identity matrix (no change)
                return new ColorMatrix(new float[][]
                {
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
            }
            else if (rbGrayscale.Checked)
            {
                // Grayscale conversion
                float grayScaleR = 0.299f;
                float grayScaleG = 0.587f;
                float grayScaleB = 0.114f;
                return new ColorMatrix(new float[][]
                {
                    new float[] {grayScaleR, grayScaleR, grayScaleR, 0, 0},
                    new float[] {grayScaleG, grayScaleG, grayScaleG, 0, 0},
                    new float[] {grayScaleB, grayScaleB, grayScaleB, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
            }
            else if (rbSepia.Checked)
            {
                // Sepia tone
                return new ColorMatrix(new float[][]
                {
                    new float[] {0.393f, 0.769f, 0.189f, 0, 0},
                    new float[] {0.349f, 0.686f, 0.168f, 0, 0},
                    new float[] {0.272f, 0.534f, 0.131f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
            }
            else if (rbInvert.Checked)
            {
                // Invert colors
                return new ColorMatrix(new float[][]
                {
                    new float[] {-1, 0, 0, 0, 0},
                    new float[] {0, -1, 0, 0, 0},
                    new float[] {0, 0, -1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {1, 1, 1, 0, 1}
                });
            }
            else if (rbBrightness.Checked)
            {
                // Brightness adjustment
                float brightness = trackBarBrightness.Value / 100.0f;
                return new ColorMatrix(new float[][]
                {
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {brightness, brightness, brightness, 0, 1}
                });
            }
            else if (rbContrast.Checked)
            {
                // Contrast adjustment
                float contrast = (trackBarContrast.Value + 100) / 100.0f;
                float translate = (1.0f - contrast) / 2.0f;
                return new ColorMatrix(new float[][]
                {
                    new float[] {contrast, 0, 0, 0, 0},
                    new float[] {0, contrast, 0, 0, 0},
                    new float[] {0, 0, contrast, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {translate, translate, translate, 0, 1}
                });
            }
            else if (rbSaturation.Checked)
            {
                // Saturation adjustment
                float saturation = (trackBarSaturation.Value + 100) / 100.0f;
                float grayScaleR = 0.299f;
                float grayScaleG = 0.587f;
                float grayScaleB = 0.114f;
                float s = saturation;
                float sr = (1.0f - s) * grayScaleR;
                float sg = (1.0f - s) * grayScaleG;
                float sb = (1.0f - s) * grayScaleB;
                return new ColorMatrix(new float[][]
                {
                    new float[] {sr + s, sr, sr, 0, 0},
                    new float[] {sg, sg + s, sg, 0, 0},
                    new float[] {sb, sb, sb + s, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
            }
            else if (rbCustomRGB.Checked)
            {
                // Custom RGB adjustment
                float red = trackBarRed.Value / 100.0f;
                float green = trackBarGreen.Value / 100.0f;
                float blue = trackBarBlue.Value / 100.0f;
                return new ColorMatrix(new float[][]
                {
                    new float[] {red, 0, 0, 0, 0},
                    new float[] {0, green, 0, 0, 0},
                    new float[] {0, 0, blue, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
            }

            // Default: identity matrix
            return new ColorMatrix();
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            if (previewImage != null)
            {
                FilteredImage = new Bitmap(previewImage);
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            rbNone.Checked = true;
            trackBarBrightness.Value = 0;
            trackBarContrast.Value = 0;
            trackBarSaturation.Value = 0;
            trackBarRed.Value = 100;
            trackBarGreen.Value = 100;
            trackBarBlue.Value = 100;
            UpdatePreview();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                originalImage?.Dispose();
                previewImage?.Dispose();
                FilteredImage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
