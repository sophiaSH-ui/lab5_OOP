namespace lab5_Voronoi
{
    partial class Form1
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
            pbCanvas = new PictureBox();
            panel1 = new Panel();
            label1 = new Label();
            numPoints = new NumericUpDown();
            btnGenerate = new Button();
            chkMultiThread = new CheckBox();
            cmbMetrics = new ComboBox();
            btnClear = new Button();
            numRemovePercent = new NumericUpDown();
            btnRemoveSmallest = new Button();
            lblStats = new Label();
            label2 = new Label();
            label3 = new Label();
            ((System.ComponentModel.ISupportInitialize)pbCanvas).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPoints).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRemovePercent).BeginInit();
            SuspendLayout();
            // 
            // pbCanvas
            // 
            pbCanvas.Location = new Point(346, 12);
            pbCanvas.Name = "pbCanvas";
            pbCanvas.Size = new Size(442, 426);
            pbCanvas.TabIndex = 0;
            pbCanvas.TabStop = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(numPoints);
            panel1.Controls.Add(btnGenerate);
            panel1.Controls.Add(chkMultiThread);
            panel1.Controls.Add(cmbMetrics);
            panel1.Controls.Add(btnClear);
            panel1.Controls.Add(numRemovePercent);
            panel1.Controls.Add(btnRemoveSmallest);
            panel1.Controls.Add(lblStats);
            panel1.Location = new Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(328, 426);
            panel1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 38);
            label1.Name = "label1";
            label1.Size = new Size(95, 15);
            label1.TabIndex = 8;
            label1.Text = "% до вилучення";
            // 
            // numPoints
            // 
            numPoints.Location = new Point(12, 230);
            numPoints.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numPoints.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numPoints.Name = "numPoints";
            numPoints.Size = new Size(303, 23);
            numPoints.TabIndex = 7;
            numPoints.Value = new decimal(new int[] { 500, 0, 0, 0 });
            // 
            // btnGenerate
            // 
            btnGenerate.Location = new Point(12, 186);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(303, 23);
            btnGenerate.TabIndex = 6;
            btnGenerate.Text = "Згенерувати";
            btnGenerate.UseVisualStyleBackColor = true;
            // 
            // chkMultiThread
            // 
            chkMultiThread.AutoSize = true;
            chkMultiThread.Checked = true;
            chkMultiThread.CheckState = CheckState.Checked;
            chkMultiThread.Location = new Point(12, 161);
            chkMultiThread.Name = "chkMultiThread";
            chkMultiThread.Size = new Size(162, 19);
            chkMultiThread.TabIndex = 5;
            chkMultiThread.Text = "Багатопотоковий режим";
            chkMultiThread.UseVisualStyleBackColor = true;
            // 
            // cmbMetrics
            // 
            cmbMetrics.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMetrics.FormattingEnabled = true;
            cmbMetrics.Items.AddRange(new object[] { "Евклідова", "Манхеттенська", "Чебишова" });
            cmbMetrics.Location = new Point(12, 132);
            cmbMetrics.Name = "cmbMetrics";
            cmbMetrics.Size = new Size(303, 23);
            cmbMetrics.TabIndex = 4;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(12, 86);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(303, 23);
            btnClear.TabIndex = 3;
            btnClear.Text = "Очистити";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // numRemovePercent
            // 
            numRemovePercent.Location = new Point(12, 57);
            numRemovePercent.Maximum = new decimal(new int[] { 90, 0, 0, 0 });
            numRemovePercent.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRemovePercent.Name = "numRemovePercent";
            numRemovePercent.Size = new Size(303, 23);
            numRemovePercent.TabIndex = 2;
            numRemovePercent.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // btnRemoveSmallest
            // 
            btnRemoveSmallest.Location = new Point(12, 12);
            btnRemoveSmallest.Name = "btnRemoveSmallest";
            btnRemoveSmallest.Size = new Size(303, 23);
            btnRemoveSmallest.TabIndex = 1;
            btnRemoveSmallest.Text = "Видалити % найменших";
            btnRemoveSmallest.UseVisualStyleBackColor = true;
            // 
            // lblStats
            // 
            lblStats.AutoSize = true;
            lblStats.Location = new Point(12, 335);
            lblStats.Name = "lblStats";
            lblStats.Size = new Size(77, 15);
            lblStats.TabIndex = 0;
            lblStats.Text = "Статистика...";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 112);
            label2.Name = "label2";
            label2.Size = new Size(103, 15);
            label2.TabIndex = 9;
            label2.Text = "Метрика відстані:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 212);
            label3.Name = "label3";
            label3.Size = new Size(106, 15);
            label3.TabIndex = 10;
            label3.Text = "Кількість вершин:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel1);
            Controls.Add(pbCanvas);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pbCanvas).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPoints).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRemovePercent).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pbCanvas;
        private Panel panel1;
        private Label lblStats;
        private Button btnRemoveSmallest;
        private NumericUpDown numRemovePercent;
        private ComboBox cmbMetrics;
        private Button btnClear;
        private NumericUpDown numPoints;
        private Button btnGenerate;
        private CheckBox chkMultiThread;
        private Label label1;
        private Label label3;
        private Label label2;
    }
}
