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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            menuStrip1 = new MenuStrip();
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
            txtBoxText = new TextBox();
            picBoxSave = new PictureBox();
            picBoxClear = new PictureBox();
            picBoxErase = new PictureBox();
            picBoxText = new PictureBox();
            picBoxPen = new PictureBox();
            picBoxBrush = new PictureBox();
            trackBarPenSize = new TrackBar();
            trackBarBrushSize = new TrackBar();
            lblPenSize = new Label();
            lblBrushSize = new Label();
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
            ((System.ComponentModel.ISupportInitialize)picBoxSave).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxClear).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxErase).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxText).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxPen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxBrush).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarPenSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarBrushSize).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 3, 0, 3);
            menuStrip1.Size = new Size(914, 30);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
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
            adminNumberToolStripMenuItem.Size = new Size(247, 26);
            adminNumberToolStripMenuItem.Text = "<Your Admin Number>";
            // 
            // picBoxMain
            // 
            picBoxMain.BackColor = Color.LightGray;
            picBoxMain.Location = new Point(10, 40);
            picBoxMain.Name = "picBoxMain";
            picBoxMain.Size = new Size(724, 300);
            picBoxMain.TabIndex = 1;
            picBoxMain.TabStop = false;
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
            panelColor.Location = new Point(10, 350);
            panelColor.Name = "panelColor";
            panelColor.Size = new Size(310, 70);
            panelColor.TabIndex = 2;
            // 
            // picBoxBrushColor
            // 
            picBoxBrushColor.BackColor = Color.Black;
            picBoxBrushColor.Location = new Point(3, 3);
            picBoxBrushColor.Name = "picBoxBrushColor";
            picBoxBrushColor.Size = new Size(65, 64);
            picBoxBrushColor.TabIndex = 3;
            picBoxBrushColor.TabStop = false;
            // 
            // picBoxBlack
            // 
            picBoxBlack.BackColor = Color.Black;
            picBoxBlack.Location = new Point(74, 3);
            picBoxBlack.Name = "picBoxBlack";
            picBoxBlack.Size = new Size(30, 30);
            picBoxBlack.TabIndex = 4;
            picBoxBlack.TabStop = false;
            picBoxBlack.Click += picBoxBlack_Click;
            // 
            // picBoxRed
            // 
            picBoxRed.BackColor = Color.Red;
            picBoxRed.Location = new Point(74, 37);
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
            // txtBoxText
            // 
            txtBoxText.Location = new Point(326, 356);
            txtBoxText.Name = "txtBoxText";
            txtBoxText.Size = new Size(200, 27);
            txtBoxText.TabIndex = 12;
            txtBoxText.TextChanged += txtBoxText_TextChanged;
            // 
            // picBoxSave
            // 
            picBoxSave.BackColor = Color.Transparent;
            picBoxSave.Image = Properties.Resources.diskette;
            picBoxSave.Location = new Point(852, 96);
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
            picBoxClear.Location = new Point(796, 96);
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
            picBoxErase.Location = new Point(740, 40);
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
            picBoxText.Location = new Point(796, 40);
            picBoxText.Name = "picBoxText";
            picBoxText.Size = new Size(50, 50);
            picBoxText.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxText.TabIndex = 16;
            picBoxText.TabStop = false;
            picBoxText.Click += picBoxText_Click;
            // 
            // picBoxPen
            // 
            picBoxPen.BackColor = Color.Transparent;
            picBoxPen.Image = Properties.Resources.pen;
            picBoxPen.Location = new Point(852, 40);
            picBoxPen.Name = "picBoxPen";
            picBoxPen.Size = new Size(50, 50);
            picBoxPen.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxPen.TabIndex = 17;
            picBoxPen.TabStop = false;
            picBoxPen.Click += picBoxPen_Click;
            // 
            // picBoxBrush
            // 
            picBoxBrush.BackColor = Color.Transparent;
            picBoxBrush.Image = Properties.Resources.paint_brush;
            picBoxBrush.Location = new Point(740, 96);
            picBoxBrush.Name = "picBoxBrush";
            picBoxBrush.Size = new Size(50, 50);
            picBoxBrush.SizeMode = PictureBoxSizeMode.StretchImage;
            picBoxBrush.TabIndex = 18;
            picBoxBrush.TabStop = false;
            picBoxBrush.Click += picBoxBrush_Click;
            picBoxBrush.Paint += picBoxBrush_Paint;
            // 
            // trackBarPenSize
            // 
            trackBarPenSize.Location = new Point(740, 169);
            trackBarPenSize.Maximum = 20;
            trackBarPenSize.Minimum = 1;
            trackBarPenSize.Name = "trackBarPenSize";
            trackBarPenSize.Size = new Size(150, 56);
            trackBarPenSize.TabIndex = 19;
            trackBarPenSize.Value = 2;
            trackBarPenSize.ValueChanged += trackBarPenSize_ValueChanged;
            // 
            // trackBarBrushSize
            // 
            trackBarBrushSize.Location = new Point(740, 225);
            trackBarBrushSize.Maximum = 30;
            trackBarBrushSize.Minimum = 3;
            trackBarBrushSize.Name = "trackBarBrushSize";
            trackBarBrushSize.Size = new Size(150, 56);
            trackBarBrushSize.TabIndex = 20;
            trackBarBrushSize.Value = 8;
            trackBarBrushSize.ValueChanged += trackBarBrushSize_ValueChanged;
            // 
            // lblPenSize
            // 
            lblPenSize.AutoSize = true;
            lblPenSize.Location = new Point(740, 149);
            lblPenSize.Name = "lblPenSize";
            lblPenSize.Size = new Size(63, 20);
            lblPenSize.TabIndex = 21;
            lblPenSize.Text = "Pen: 2px";
            // 
            // lblBrushSize
            // 
            lblBrushSize.AutoSize = true;
            lblBrushSize.Location = new Point(740, 205);
            lblBrushSize.Name = "lblBrushSize";
            lblBrushSize.Size = new Size(76, 20);
            lblBrushSize.TabIndex = 22;
            lblBrushSize.Text = "Brush: 8px";
            // 
            // MainForm_241439P
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(914, 600);
            Controls.Add(lblBrushSize);
            Controls.Add(lblPenSize);
            Controls.Add(trackBarBrushSize);
            Controls.Add(trackBarPenSize);
            Controls.Add(picBoxBrush);
            Controls.Add(picBoxPen);
            Controls.Add(picBoxText);
            Controls.Add(picBoxErase);
            Controls.Add(picBoxClear);
            Controls.Add(picBoxSave);
            Controls.Add(txtBoxText);
            Controls.Add(panelColor);
            Controls.Add(picBoxMain);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm_241439P";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Paint";
            TopMost = true;
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
            ((System.ComponentModel.ISupportInitialize)picBoxSave).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxClear).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxErase).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxText).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxPen).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxBrush).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarPenSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarBrushSize).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adminNumberToolStripMenuItem;
        private System.Windows.Forms.PictureBox picBoxMain;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.PictureBox picBoxBrushColor;
        private System.Windows.Forms.PictureBox picBoxBlack;
        private System.Windows.Forms.PictureBox picBoxRed;
        private System.Windows.Forms.PictureBox picBoxGreen;
        private System.Windows.Forms.PictureBox picBoxBlue;
        private System.Windows.Forms.PictureBox picBoxCyan;
        private System.Windows.Forms.PictureBox picBoxMagenta;
        private System.Windows.Forms.PictureBox picBoxYellow;
        private System.Windows.Forms.PictureBox picBoxOrange;
        private System.Windows.Forms.TextBox txtBoxText;
        private System.Windows.Forms.PictureBox picBoxSave;
        private System.Windows.Forms.PictureBox picBoxClear;
        private System.Windows.Forms.PictureBox picBoxErase;
        private System.Windows.Forms.PictureBox picBoxText;
        private System.Windows.Forms.PictureBox picBoxPen;
        private System.Windows.Forms.PictureBox picBoxBrush;
        private System.Windows.Forms.TrackBar trackBarPenSize;
        private System.Windows.Forms.TrackBar trackBarBrushSize;
        private System.Windows.Forms.Label lblPenSize;
        private System.Windows.Forms.Label lblBrushSize;
    }
}
