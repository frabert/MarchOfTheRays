namespace MarchOfTheRays
{
    partial class RenderingSettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.GroupBox groupBox2;
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.upDirection = new MarchOfTheRays.Vector3Input();
            this.cameraTarget = new MarchOfTheRays.Vector3Input();
            this.cameraPos = new MarchOfTheRays.Vector3Input();
            this.numIterations = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.renderingDistance = new System.Windows.Forms.NumericUpDown();
            this.epsilon = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.stepSize = new System.Windows.Forms.NumericUpDown();
            this.cancel = new System.Windows.Forms.Button();
            this.apply = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIterations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.renderingDistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.epsilon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stepSize)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this.label3);
            groupBox1.Controls.Add(this.label2);
            groupBox1.Controls.Add(this.label1);
            groupBox1.Controls.Add(this.upDirection);
            groupBox1.Controls.Add(this.cameraTarget);
            groupBox1.Controls.Add(this.cameraPos);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(377, 118);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Camera settings";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Camera position:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Camera target:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Up direction:";
            // 
            // upDirection
            // 
            this.upDirection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.upDirection.Location = new System.Drawing.Point(99, 83);
            this.upDirection.Name = "upDirection";
            this.upDirection.Size = new System.Drawing.Size(272, 26);
            this.upDirection.TabIndex = 2;
            // 
            // cameraTarget
            // 
            this.cameraTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraTarget.Location = new System.Drawing.Point(99, 51);
            this.cameraTarget.Name = "cameraTarget";
            this.cameraTarget.Size = new System.Drawing.Size(272, 26);
            this.cameraTarget.TabIndex = 1;
            // 
            // cameraPos
            // 
            this.cameraPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraPos.Location = new System.Drawing.Point(99, 19);
            this.cameraPos.Name = "cameraPos";
            this.cameraPos.Size = new System.Drawing.Size(272, 26);
            this.cameraPos.TabIndex = 0;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(this.label7);
            groupBox2.Controls.Add(this.stepSize);
            groupBox2.Controls.Add(this.label6);
            groupBox2.Controls.Add(this.epsilon);
            groupBox2.Controls.Add(this.renderingDistance);
            groupBox2.Controls.Add(this.label5);
            groupBox2.Controls.Add(this.label4);
            groupBox2.Controls.Add(this.numIterations);
            groupBox2.Location = new System.Drawing.Point(12, 136);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(377, 127);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Rendering engine settings";
            // 
            // numIterations
            // 
            this.numIterations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numIterations.Location = new System.Drawing.Point(251, 19);
            this.numIterations.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numIterations.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numIterations.Name = "numIterations";
            this.numIterations.Size = new System.Drawing.Size(120, 20);
            this.numIterations.TabIndex = 0;
            this.numIterations.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(149, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Maximum number of iterations:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(144, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Maximum rendering distance:";
            // 
            // renderingDistance
            // 
            this.renderingDistance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.renderingDistance.DecimalPlaces = 3;
            this.renderingDistance.Location = new System.Drawing.Point(251, 45);
            this.renderingDistance.Name = "renderingDistance";
            this.renderingDistance.Size = new System.Drawing.Size(120, 20);
            this.renderingDistance.TabIndex = 3;
            // 
            // epsilon
            // 
            this.epsilon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.epsilon.DecimalPlaces = 5;
            this.epsilon.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.epsilon.Location = new System.Drawing.Point(251, 71);
            this.epsilon.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.epsilon.Name = "epsilon";
            this.epsilon.Size = new System.Drawing.Size(120, 20);
            this.epsilon.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 73);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Epsilon:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 99);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Step size:";
            // 
            // stepSize
            // 
            this.stepSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.stepSize.DecimalPlaces = 2;
            this.stepSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.stepSize.Location = new System.Drawing.Point(251, 97);
            this.stepSize.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.stepSize.Name = "stepSize";
            this.stepSize.Size = new System.Drawing.Size(120, 20);
            this.stepSize.TabIndex = 6;
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(314, 282);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 2;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // apply
            // 
            this.apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.apply.Location = new System.Drawing.Point(233, 282);
            this.apply.Name = "apply";
            this.apply.Size = new System.Drawing.Size(75, 23);
            this.apply.TabIndex = 3;
            this.apply.Text = "Apply";
            this.apply.UseVisualStyleBackColor = true;
            this.apply.Click += new System.EventHandler(this.apply_Click);
            // 
            // RenderingSettingsDialog
            // 
            this.AcceptButton = this.apply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(401, 317);
            this.Controls.Add(this.apply);
            this.Controls.Add(this.cancel);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RenderingSettingsDialog";
            this.ShowInTaskbar = false;
            this.Text = "Rendering settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIterations)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.renderingDistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.epsilon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stepSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Vector3Input upDirection;
        private Vector3Input cameraPos;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numIterations;
        private System.Windows.Forms.NumericUpDown renderingDistance;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown epsilon;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown stepSize;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button apply;
        private Vector3Input cameraTarget;
    }
}