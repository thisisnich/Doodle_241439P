namespace Doodle_241439P
{
    partial class MainForm_241439P
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                // Dispose custom cursors
                brushCursor?.Dispose();
                eraserCursor?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            menuStrip1 = new MenuStrip();
            imageFiltersToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            nameToolStripMenuItem = new ToolStripMenuItem();
            adminNumberToolStripMenuItem = new ToolStripMenuItem();
            picBoxMain = new PictureBox();
            panelColor = new Panel();
            picBoxBrushColor = new PictureBox();
            picBoxBlack = new PictureBox();
            picBoxRed = new PictureBox();
            picBoxGreen = new PictureBox();
            picBoxBlue = new PictureBox();
            picBoxCyan = new PictureBox();
            picBoxOrange = new PictureBox();
            picBoxMagenta = new PictureBox();
            picBoxYellow = new PictureBox();
            picBoxWhite = new PictureBox();
            picBoxCustom = new PictureBox();
            picBoxPurple = new PictureBox();
            picBoxBrown = new PictureBox();
            panelShapes = new Panel();
            txtBoxText = new TextBox();
            picBoxLine = new PictureBox();
            picBoxSquare = new PictureBox();
            picBoxCircle = new PictureBox();
            picBoxNgon = new PictureBox();
            lblNgonSides = new Label();
            checkBoxShapeFilled = new CheckBox();
            trackBarNgonSides = new TrackBar();
            picBoxSave = new PictureBox();
            picBoxClear = new PictureBox();
            picBoxErase = new PictureBox();
            picBoxText = new PictureBox();
            picBoxLoad = new PictureBox();
            picBoxBrush = new PictureBox();
            picBoxEmoji = new PictureBox();
            picBoxFill = new PictureBox();
            btnStampImage = new Button();
            trackBarUnified = new TrackBar();
            lblUnified = new Label();
            comboBoxUnified = new ComboBox();
            lblUnifiedCombo = new Label();
            toolTip = new ToolTip(components);
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picBoxMain).BeginInit();
            panelColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picBoxBrushColor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxBlack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxRed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxGreen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxBlue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxCyan).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxOrange).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxMagenta).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxYellow).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxWhite).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxCustom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxPurple).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxBrown).BeginInit();
            panelShapes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picBoxLine).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxSquare).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxCircle).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxNgon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarNgonSides).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxSave).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxClear).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxErase).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxText).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxLoad).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxBrush).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxEmoji).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxFill).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarUnified).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { imageFiltersToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 3, 0, 3);
            menuStrip1.Size = new Size(885, 30);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // imageFiltersToolStripMenuItem
            // 
            imageFiltersToolStripMenuItem.Name = "imageFiltersToolStripMenuItem";
            imageFiltersToolStripMenuItem.Size = new Size(108, 24);
            imageFiltersToolStripMenuItem.Text = "Image Filters";
            imageFiltersToolStripMenuItem.Click += imageFiltersToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { nameToolStripMenuItem });
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(64, 24);
            aboutToolStripMenuItem.Text = "About";
            // 
            // nameToolStripMenuItem
            // 
            nameToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { adminNumberToolStripMenuItem });
            nameToolStripMenuItem.Name = "nameToolStripMenuItem";
            nameToolStripMenuItem.Size = new Size(187, 26);
            nameToolStripMenuItem.Text = "Nicholas Dubs";
            nameToolStripMenuItem.Click += nameToolStripMenuItem_Click;
            // 
            // adminNumberToolStripMenuItem
            // 
            adminNumberToolStripMenuItem.Name = "adminNumberToolStripMenuItem";
            adminNumberToolStripMenuItem.Size = new Size(148, 26);
            adminNumberToolStripMenuItem.Text = "241439P";
            adminNumberToolStripMenuItem.Click += adminNumberToolStripMenuItem_Click;
            // 
            // picBoxMain
            // 
            picBoxMain.BackColor = Color.LightGray;
            picBoxMain.Location = new Point(11, 151);
            picBoxMain.Name = "picBoxMain";
            picBoxMain.Size = new Size(870, 410);
            picBoxMain.TabIndex = 1;
            picBoxMain.TabStop = false;
            picBoxMain.Paint += picBoxMain_Paint;
            picBoxMain.MouseDown += picBoxMain_MouseDown;
            picBoxMain.MouseMove += picBoxMain_MouseMove;
            picBoxMain.MouseUp += picBoxMain_MouseUp;
            // 
            // panelColor
            // 
            panelColor.Controls.Add(picBoxBrushColor);
            panelColor.Controls.Add(picBoxBlack);
            panelColor.Controls.Add(picBoxRed);
            panelColor.Controls.Add(picBoxGreen);
            panelColor.Controls.Add(picBoxBlue);
            panelColor.Controls.Add(picBoxCyan);
            panelColor.Controls.Add(picBoxOrange);
            panelColor.Controls.Add(picBoxMagenta);
            panelColor.Controls.Add(picBoxYellow);
            panelColor.Controls.Add(picBoxWhite);
            panelColor.Controls.Add(picBoxCustom);
            panelColor.Controls.Add(picBoxPurple);
            panelColor.Controls.Add(picBoxBrown);
            panelColor.Location = new Point(12, 33);
            panelColor.Name = "panelColor";
            panelColor.Size = new Size(253, 112);
            panelColor.TabIndex = 2;
            // 
            // picBoxBrushColor
            // 
            picBoxBrushColor.BackColor = Color.Black;
            picBoxBrushColor.Location = new Point(3, 3);
            picBoxBrushColor.Name = "picBoxBrushColor";
            picBoxBrushColor.Size = new Size(101, 100);
            picBoxBrushColor.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxBrushColor.TabIndex = 3;
            picBoxBrushColor.TabStop = false;
            // 
            // picBoxBlack
            // 
            picBoxBlack.BackColor = Color.Black;
            picBoxBlack.Location = new Point(146, 73);
            picBoxBlack.Name = "picBoxBlack";
            picBoxBlack.Size = new Size(30, 30);
            picBoxBlack.TabIndex = 4;
            picBoxBlack.TabStop = false;
            picBoxBlack.Click += picBoxBlack_Click;
            // 
            // picBoxRed
            // 
            picBoxRed.BackColor = Color.Red;
            picBoxRed.Location = new Point(110, 73);
            picBoxRed.Name = "picBoxRed";
            picBoxRed.Size = new Size(30, 30);
            picBoxRed.TabIndex = 5;
            picBoxRed.TabStop = false;
            picBoxRed.Click += picBoxRed_Click;
            // 
            // picBoxGreen
            // 
            picBoxGreen.BackColor = Color.Green;
            picBoxGreen.Location = new Point(110, 3);
            picBoxGreen.Name = "picBoxGreen";
            picBoxGreen.Size = new Size(30, 30);
            picBoxGreen.TabIndex = 6;
            picBoxGreen.TabStop = false;
            picBoxGreen.Click += picBoxGreen_Click;
            // 
            // picBoxBlue
            // 
            picBoxBlue.BackColor = Color.Blue;
            picBoxBlue.Location = new Point(110, 37);
            picBoxBlue.Name = "picBoxBlue";
            picBoxBlue.Size = new Size(30, 30);
            picBoxBlue.TabIndex = 7;
            picBoxBlue.TabStop = false;
            picBoxBlue.Click += picBoxBlue_Click;
            // 
            // picBoxCyan
            // 
            picBoxCyan.BackColor = Color.Cyan;
            picBoxCyan.Location = new Point(146, 3);
            picBoxCyan.Name = "picBoxCyan";
            picBoxCyan.Size = new Size(30, 30);
            picBoxCyan.TabIndex = 8;
            picBoxCyan.TabStop = false;
            picBoxCyan.Click += picBoxCyan_Click;
            // 
            // picBoxOrange
            // 
            picBoxOrange.BackColor = Color.Orange;
            picBoxOrange.Location = new Point(182, 37);
            picBoxOrange.Name = "picBoxOrange";
            picBoxOrange.Size = new Size(30, 30);
            picBoxOrange.TabIndex = 11;
            picBoxOrange.TabStop = false;
            picBoxOrange.Click += picBoxOrange_Click;
            // 
            // picBoxMagenta
            // 
            picBoxMagenta.BackColor = Color.Magenta;
            picBoxMagenta.Location = new Point(146, 37);
            picBoxMagenta.Name = "picBoxMagenta";
            picBoxMagenta.Size = new Size(30, 30);
            picBoxMagenta.TabIndex = 9;
            picBoxMagenta.TabStop = false;
            picBoxMagenta.Click += picBoxMagenta_Click;
            // 
            // picBoxYellow
            // 
            picBoxYellow.BackColor = Color.Yellow;
            picBoxYellow.Location = new Point(182, 3);
            picBoxYellow.Name = "picBoxYellow";
            picBoxYellow.Size = new Size(30, 30);
            picBoxYellow.TabIndex = 10;
            picBoxYellow.TabStop = false;
            picBoxYellow.Click += picBoxYellow_Click;
            // 
            // picBoxWhite
            // 
            picBoxWhite.BackColor = Color.White;
            picBoxWhite.BorderStyle = BorderStyle.FixedSingle;
            picBoxWhite.Location = new Point(182, 73);
            picBoxWhite.Name = "picBoxWhite";
            picBoxWhite.Size = new Size(30, 30);
            picBoxWhite.TabIndex = 12;
            picBoxWhite.TabStop = false;
            picBoxWhite.Click += picBoxWhite_Click;
            // 
            // picBoxCustom
            // 
            picBoxCustom.BackColor = Color.FromArgb(128, 128, 128);
            picBoxCustom.BorderStyle = BorderStyle.FixedSingle;
            picBoxCustom.Image = Properties.Resources.eyedropper;
            picBoxCustom.Location = new Point(218, 73);
            picBoxCustom.Name = "picBoxCustom";
            picBoxCustom.Size = new Size(30, 30);
            picBoxCustom.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxCustom.TabIndex = 13;
            picBoxCustom.TabStop = false;
            picBoxCustom.Click += picBoxCustom_Click;
            // 
            // picBoxPurple
            // 
            picBoxPurple.BackColor = Color.Purple;
            picBoxPurple.Location = new Point(218, 3);
            picBoxPurple.Name = "picBoxPurple";
            picBoxPurple.Size = new Size(30, 30);
            picBoxPurple.TabIndex = 14;
            picBoxPurple.TabStop = false;
            picBoxPurple.Click += picBoxPurple_Click;
            // 
            // picBoxBrown
            // 
            picBoxBrown.BackColor = Color.Brown;
            picBoxBrown.Location = new Point(218, 39);
            picBoxBrown.Name = "picBoxBrown";
            picBoxBrown.Size = new Size(30, 30);
            picBoxBrown.TabIndex = 15;
            picBoxBrown.TabStop = false;
            picBoxBrown.Click += picBoxBrown_Click;
            // 
            // panelShapes
            // 
            panelShapes.Controls.Add(txtBoxText);
            panelShapes.Controls.Add(picBoxLine);
            panelShapes.Controls.Add(picBoxSquare);
            panelShapes.Controls.Add(picBoxCircle);
            panelShapes.Controls.Add(picBoxNgon);
            panelShapes.Controls.Add(lblNgonSides);
            panelShapes.Controls.Add(checkBoxShapeFilled);
            panelShapes.Controls.Add(trackBarNgonSides);
            panelShapes.Location = new Point(271, 33);
            panelShapes.Name = "panelShapes";
            panelShapes.Size = new Size(215, 112);
            panelShapes.TabIndex = 36;
            // 
            // txtBoxText
            // 
            txtBoxText.Location = new Point(3, 82);
            txtBoxText.Name = "txtBoxText";
            txtBoxText.Size = new Size(209, 27);
            txtBoxText.TabIndex = 12;
            txtBoxText.TextChanged += txtBoxText_TextChanged;
            // 
            // picBoxLine
            // 
            picBoxLine.BackColor = Color.Transparent;
            picBoxLine.Image = Properties.Resources.diagonal_line;
            picBoxLine.Location = new Point(3, 3);
            picBoxLine.Name = "picBoxLine";
            picBoxLine.Size = new Size(30, 30);
            picBoxLine.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxLine.TabIndex = 29;
            picBoxLine.TabStop = false;
            picBoxLine.Click += picBoxLine_Click;
            // 
            // picBoxSquare
            // 
            picBoxSquare.BackColor = Color.Transparent;
            picBoxSquare.Image = Properties.Resources.square_1_;
            picBoxSquare.Location = new Point(39, 3);
            picBoxSquare.Name = "picBoxSquare";
            picBoxSquare.Size = new Size(30, 30);
            picBoxSquare.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxSquare.TabIndex = 30;
            picBoxSquare.TabStop = false;
            picBoxSquare.Click += picBoxSquare_Click;
            picBoxSquare.Paint += picBoxSquare_Paint;
            // 
            // picBoxCircle
            // 
            picBoxCircle.BackColor = Color.Transparent;
            picBoxCircle.Image = Properties.Resources.dry_clean;
            picBoxCircle.Location = new Point(75, 3);
            picBoxCircle.Name = "picBoxCircle";
            picBoxCircle.Size = new Size(30, 30);
            picBoxCircle.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxCircle.TabIndex = 31;
            picBoxCircle.TabStop = false;
            picBoxCircle.Click += picBoxCircle_Click;
            picBoxCircle.Paint += picBoxCircle_Paint;
            // 
            // picBoxNgon
            // 
            picBoxNgon.BackColor = Color.Transparent;
            picBoxNgon.Image = Properties.Resources.pentagon;
            picBoxNgon.Location = new Point(111, 3);
            picBoxNgon.Name = "picBoxNgon";
            picBoxNgon.Size = new Size(30, 30);
            picBoxNgon.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxNgon.TabIndex = 32;
            picBoxNgon.TabStop = false;
            picBoxNgon.Click += picBoxNgon_Click;
            picBoxNgon.Paint += picBoxNgon_Paint;
            // 
            // lblNgonSides
            // 
            lblNgonSides.AutoSize = true;
            lblNgonSides.Location = new Point(147, 7);
            lblNgonSides.Name = "lblNgonSides";
            lblNgonSides.Size = new Size(59, 20);
            lblNgonSides.TabIndex = 34;
            lblNgonSides.Text = "Sides: 5";
            // 
            // checkBoxShapeFilled
            // 
            checkBoxShapeFilled.AutoSize = true;
            checkBoxShapeFilled.Location = new Point(147, 43);
            checkBoxShapeFilled.Name = "checkBoxShapeFilled";
            checkBoxShapeFilled.Size = new Size(67, 24);
            checkBoxShapeFilled.TabIndex = 35;
            checkBoxShapeFilled.Text = "Filled";
            checkBoxShapeFilled.UseVisualStyleBackColor = true;
            checkBoxShapeFilled.CheckedChanged += checkBoxShapeFilled_CheckedChanged;
            // 
            // trackBarNgonSides
            // 
            trackBarNgonSides.Location = new Point(3, 47);
            trackBarNgonSides.Maximum = 20;
            trackBarNgonSides.Minimum = 3;
            trackBarNgonSides.Name = "trackBarNgonSides";
            trackBarNgonSides.Size = new Size(138, 56);
            trackBarNgonSides.TabIndex = 33;
            trackBarNgonSides.Value = 5;
            trackBarNgonSides.ValueChanged += trackBarNgonSides_ValueChanged;
            // 
            // picBoxSave
            // 
            picBoxSave.BackColor = Color.Transparent;
            picBoxSave.Image = Properties.Resources.diskette;
            picBoxSave.Location = new Point(606, 95);
            picBoxSave.Name = "picBoxSave";
            picBoxSave.Size = new Size(50, 50);
            picBoxSave.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxSave.TabIndex = 13;
            picBoxSave.TabStop = false;
            picBoxSave.Click += picBoxSave_Click;
            // 
            // picBoxClear
            // 
            picBoxClear.BackColor = Color.Transparent;
            picBoxClear.Image = Properties.Resources.trash;
            picBoxClear.Location = new Point(550, 95);
            picBoxClear.Name = "picBoxClear";
            picBoxClear.Size = new Size(50, 50);
            picBoxClear.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxClear.TabIndex = 14;
            picBoxClear.TabStop = false;
            picBoxClear.Click += picBoxClear_Click;
            // 
            // picBoxErase
            // 
            picBoxErase.BackColor = Color.Transparent;
            picBoxErase.Image = Properties.Resources.eraser;
            picBoxErase.Location = new Point(494, 33);
            picBoxErase.Name = "picBoxErase";
            picBoxErase.Size = new Size(50, 50);
            picBoxErase.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxErase.TabIndex = 15;
            picBoxErase.TabStop = false;
            picBoxErase.Click += picBoxErase_Click;
            // 
            // picBoxText
            // 
            picBoxText.BackColor = Color.Transparent;
            picBoxText.Image = Properties.Resources.text;
            picBoxText.Location = new Point(550, 33);
            picBoxText.Name = "picBoxText";
            picBoxText.Size = new Size(50, 50);
            picBoxText.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxText.TabIndex = 16;
            picBoxText.TabStop = false;
            picBoxText.Click += picBoxText_Click;
            // 
            // picBoxLoad
            // 
            picBoxLoad.BackColor = Color.Transparent;
            picBoxLoad.Image = Properties.Resources.image;
            picBoxLoad.Location = new Point(606, 33);
            picBoxLoad.Name = "picBoxLoad";
            picBoxLoad.Size = new Size(50, 50);
            picBoxLoad.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxLoad.TabIndex = 17;
            picBoxLoad.TabStop = false;
            picBoxLoad.Click += picBoxLoad_Click;
            // 
            // picBoxBrush
            // 
            picBoxBrush.BackColor = Color.Transparent;
            picBoxBrush.Image = Properties.Resources.paint_brush;
            picBoxBrush.Location = new Point(494, 95);
            picBoxBrush.Name = "picBoxBrush";
            picBoxBrush.Size = new Size(50, 50);
            picBoxBrush.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxBrush.TabIndex = 18;
            picBoxBrush.TabStop = false;
            picBoxBrush.Click += picBoxBrush_Click;
            // 
            // picBoxEmoji
            // 
            picBoxEmoji.BackColor = Color.Transparent;
            picBoxEmoji.Image = Properties.Resources.happy_face;
            picBoxEmoji.Location = new Point(663, 33);
            picBoxEmoji.Name = "picBoxEmoji";
            picBoxEmoji.Size = new Size(50, 50);
            picBoxEmoji.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxEmoji.TabIndex = 19;
            picBoxEmoji.TabStop = false;
            picBoxEmoji.Click += picBoxEmoji_Click;
            // 
            // picBoxFill
            // 
            picBoxFill.BackColor = Color.Transparent;
            picBoxFill.Image = Properties.Resources.bucket;
            picBoxFill.Location = new Point(663, 95);
            picBoxFill.Name = "picBoxFill";
            picBoxFill.Size = new Size(50, 50);
            picBoxFill.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxFill.TabIndex = 20;
            picBoxFill.TabStop = false;
            picBoxFill.Click += picBoxFill_Click;
            // 
            // btnStampImage
            // 
            btnStampImage.Location = new Point(719, 105);
            btnStampImage.Name = "btnStampImage";
            btnStampImage.Size = new Size(82, 35);
            btnStampImage.TabIndex = 0;
            btnStampImage.Text = "OK (Stamp)";
            btnStampImage.UseVisualStyleBackColor = true;
            btnStampImage.Visible = false;
            btnStampImage.Click += btnStampImage_Click;
            // 
            // trackBarUnified
            // 
            trackBarUnified.Location = new Point(719, 55);
            trackBarUnified.Maximum = 70;
            trackBarUnified.Minimum = 10;
            trackBarUnified.Name = "trackBarUnified";
            trackBarUnified.Size = new Size(162, 56);
            trackBarUnified.TabIndex = 38;
            trackBarUnified.TickFrequency = 10;
            trackBarUnified.Value = 30;
            trackBarUnified.ValueChanged += trackBarUnified_ValueChanged;
            // 
            // lblUnified
            // 
            lblUnified.AutoSize = true;
            lblUnified.Location = new Point(719, 30);
            lblUnified.Name = "lblUnified";
            lblUnified.Size = new Size(88, 20);
            lblUnified.TabIndex = 39;
            lblUnified.Text = "Brush: 30pts";
            // 
            // comboBoxUnified
            // 
            comboBoxUnified.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxUnified.FormattingEnabled = true;
            comboBoxUnified.Items.AddRange(new object[] { "Pen", "Paintbrush", "Marker", "Pencil", "Airbrush", "Wet Brush" });
            comboBoxUnified.Location = new Point(719, 105);
            comboBoxUnified.Name = "comboBoxUnified";
            comboBoxUnified.Size = new Size(162, 28);
            comboBoxUnified.TabIndex = 41;
            comboBoxUnified.SelectedIndexChanged += comboBoxUnified_SelectedIndexChanged;
            // 
            // lblUnifiedCombo
            // 
            lblUnifiedCombo.AutoSize = true;
            lblUnifiedCombo.Location = new Point(719, 82);
            lblUnifiedCombo.Name = "lblUnifiedCombo";
            lblUnifiedCombo.Size = new Size(83, 20);
            lblUnifiedCombo.TabIndex = 40;
            lblUnifiedCombo.Text = "Brush Type:";
            lblUnifiedCombo.Click += lblUnifiedCombo_Click;
            // 
            // MainForm_241439P
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(885, 577);
            Controls.Add(panelShapes);
            Controls.Add(btnStampImage);
            Controls.Add(comboBoxUnified);
            Controls.Add(lblUnifiedCombo);
            Controls.Add(lblUnified);
            Controls.Add(trackBarUnified);
            Controls.Add(picBoxBrush);
            Controls.Add(picBoxFill);
            Controls.Add(picBoxLoad);
            Controls.Add(picBoxText);
            Controls.Add(picBoxEmoji);
            Controls.Add(picBoxErase);
            Controls.Add(picBoxClear);
            Controls.Add(picBoxSave);
            Controls.Add(panelColor);
            Controls.Add(picBoxMain);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm_241439P";
            StartPosition = FormStartPosition.CenterScreen;
            Text = " ";
            Load += MainForm_241439P_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picBoxMain).EndInit();
            panelColor.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picBoxBrushColor).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxBlack).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxRed).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxGreen).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxBlue).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxCyan).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxOrange).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxMagenta).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxYellow).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxWhite).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxCustom).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxPurple).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxBrown).EndInit();
            panelShapes.ResumeLayout(false);
            panelShapes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picBoxLine).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxSquare).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxCircle).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxNgon).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarNgonSides).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxSave).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxClear).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxErase).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxText).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxLoad).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxBrush).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxEmoji).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxFill).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarUnified).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem imageFiltersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adminNumberToolStripMenuItem;
        private System.Windows.Forms.PictureBox picBoxMain;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Panel panelShapes;
        private System.Windows.Forms.PictureBox picBoxBrushColor;
        private System.Windows.Forms.PictureBox picBoxBlack;
        private System.Windows.Forms.PictureBox picBoxRed;
        private System.Windows.Forms.PictureBox picBoxGreen;
        private System.Windows.Forms.PictureBox picBoxBlue;
        private System.Windows.Forms.PictureBox picBoxCyan;
        private System.Windows.Forms.PictureBox picBoxMagenta;
        private System.Windows.Forms.PictureBox picBoxYellow;
        private System.Windows.Forms.PictureBox picBoxOrange;
        private System.Windows.Forms.PictureBox picBoxWhite;
        private System.Windows.Forms.PictureBox picBoxCustom;
        private System.Windows.Forms.PictureBox picBoxPurple;
        private System.Windows.Forms.PictureBox picBoxBrown;
        private System.Windows.Forms.TextBox txtBoxText;
        private System.Windows.Forms.PictureBox picBoxSave;
        private System.Windows.Forms.PictureBox picBoxClear;
        private System.Windows.Forms.PictureBox picBoxErase;
        private System.Windows.Forms.PictureBox picBoxText;
        private System.Windows.Forms.PictureBox picBoxLoad;
        private System.Windows.Forms.PictureBox picBoxBrush;
        private System.Windows.Forms.PictureBox picBoxFill;
        private System.Windows.Forms.PictureBox picBoxEmoji;
        private System.Windows.Forms.TrackBar trackBarUnified;
        private System.Windows.Forms.Label lblUnified;
        private System.Windows.Forms.ComboBox comboBoxUnified;
        private System.Windows.Forms.Label lblUnifiedCombo;
        private System.Windows.Forms.Button btnStampImage;
        private System.Windows.Forms.PictureBox picBoxLine;
        private System.Windows.Forms.PictureBox picBoxSquare;
        private System.Windows.Forms.PictureBox picBoxCircle;
        private System.Windows.Forms.PictureBox picBoxNgon;
        private System.Windows.Forms.TrackBar trackBarNgonSides;
        private System.Windows.Forms.Label lblNgonSides;
        private System.Windows.Forms.CheckBox checkBoxShapeFilled;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
