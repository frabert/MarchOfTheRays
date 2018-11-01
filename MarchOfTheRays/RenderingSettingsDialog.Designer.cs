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
            this.cameraGroup = new System.Windows.Forms.GroupBox();
            this.lblUp = new System.Windows.Forms.Label();
            this.lblTarget = new System.Windows.Forms.Label();
            this.lblPosition = new System.Windows.Forms.Label();
            this.upDirection = new MarchOfTheRays.Vector3Input();
            this.cameraTarget = new MarchOfTheRays.Vector3Input();
            this.cameraPos = new MarchOfTheRays.Vector3Input();
            this.engineGroup = new System.Windows.Forms.GroupBox();
            this.lblStep = new System.Windows.Forms.Label();
            this.stepSize = new System.Windows.Forms.NumericUpDown();
            this.lblEps = new System.Windows.Forms.Label();
            this.epsilon = new System.Windows.Forms.NumericUpDown();
            this.renderingDistance = new System.Windows.Forms.NumericUpDown();
            this.lblMaxDist = new System.Windows.Forms.Label();
            this.lblMaxIter = new System.Windows.Forms.Label();
            this.numIterations = new System.Windows.Forms.NumericUpDown();
            this.cancel = new System.Windows.Forms.Button();
            this.apply = new System.Windows.Forms.Button();
            this.cameraGroup.SuspendLayout();
            this.engineGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stepSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.epsilon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.renderingDistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numIterations)).BeginInit();
            this.SuspendLayout();
            // 
            // cameraGroup
            // 
            this.cameraGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraGroup.Controls.Add(this.lblUp);
            this.cameraGroup.Controls.Add(this.lblTarget);
            this.cameraGroup.Controls.Add(this.lblPosition);
            this.cameraGroup.Controls.Add(this.upDirection);
            this.cameraGroup.Controls.Add(this.cameraTarget);
            this.cameraGroup.Controls.Add(this.cameraPos);
            this.cameraGroup.Location = new System.Drawing.Point(12, 12);
            this.cameraGroup.Name = "cameraGroup";
            this.cameraGroup.Size = new System.Drawing.Size(377, 118);
            this.cameraGroup.TabIndex = 0;
            this.cameraGroup.TabStop = false;
            this.cameraGroup.Text = "Camera settings";
            // 
            // lblUp
            // 
            this.lblUp.AutoSize = true;
            this.lblUp.Location = new System.Drawing.Point(6, 89);
            this.lblUp.Name = "lblUp";
            this.lblUp.Size = new System.Drawing.Size(67, 13);
            this.lblUp.TabIndex = 5;
            this.lblUp.Text = "Up direction:";
            // 
            // lblTarget
            // 
            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(6, 57);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(76, 13);
            this.lblTarget.TabIndex = 4;
            this.lblTarget.Text = "Camera target:";
            // 
            // lblPosition
            // 
            this.lblPosition.AutoSize = true;
            this.lblPosition.Location = new System.Drawing.Point(6, 25);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(85, 13);
            this.lblPosition.TabIndex = 3;
            this.lblPosition.Text = "Camera position:";
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
            // engineGroup
            // 
            this.engineGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.engineGroup.Controls.Add(this.lblStep);
            this.engineGroup.Controls.Add(this.stepSize);
            this.engineGroup.Controls.Add(this.lblEps);
            this.engineGroup.Controls.Add(this.epsilon);
            this.engineGroup.Controls.Add(this.renderingDistance);
            this.engineGroup.Controls.Add(this.lblMaxDist);
            this.engineGroup.Controls.Add(this.lblMaxIter);
            this.engineGroup.Controls.Add(this.numIterations);
            this.engineGroup.Location = new System.Drawing.Point(12, 136);
            this.engineGroup.Name = "engineGroup";
            this.engineGroup.Size = new System.Drawing.Size(377, 127);
            this.engineGroup.TabIndex = 1;
            this.engineGroup.TabStop = false;
            this.engineGroup.Text = "Rendering engine settings";
            // 
            // lblStep
            // 
            this.lblStep.AutoSize = true;
            this.lblStep.Location = new System.Drawing.Point(6, 99);
            this.lblStep.Name = "lblStep";
            this.lblStep.Size = new System.Drawing.Size(53, 13);
            this.lblStep.TabIndex = 7;
            this.lblStep.Text = "Step size:";
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
            // lblEps
            // 
            this.lblEps.AutoSize = true;
            this.lblEps.Location = new System.Drawing.Point(6, 73);
            this.lblEps.Name = "lblEps";
            this.lblEps.Size = new System.Drawing.Size(44, 13);
            this.lblEps.TabIndex = 5;
            this.lblEps.Text = "Epsilon:";
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
            // renderingDistance
            // 
            this.renderingDistance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.renderingDistance.DecimalPlaces = 3;
            this.renderingDistance.Location = new System.Drawing.Point(251, 45);
            this.renderingDistance.Name = "renderingDistance";
            this.renderingDistance.Size = new System.Drawing.Size(120, 20);
            this.renderingDistance.TabIndex = 3;
            // 
            // lblMaxDist
            // 
            this.lblMaxDist.AutoSize = true;
            this.lblMaxDist.Location = new System.Drawing.Point(6, 47);
            this.lblMaxDist.Name = "lblMaxDist";
            this.lblMaxDist.Size = new System.Drawing.Size(144, 13);
            this.lblMaxDist.TabIndex = 2;
            this.lblMaxDist.Text = "Maximum rendering distance:";
            // 
            // lblMaxIter
            // 
            this.lblMaxIter.AutoSize = true;
            this.lblMaxIter.Location = new System.Drawing.Point(6, 21);
            this.lblMaxIter.Name = "lblMaxIter";
            this.lblMaxIter.Size = new System.Drawing.Size(149, 13);
            this.lblMaxIter.TabIndex = 1;
            this.lblMaxIter.Text = "Maximum number of iterations:";
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
            this.Controls.Add(this.engineGroup);
            this.Controls.Add(this.cameraGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RenderingSettingsDialog";
            this.ShowInTaskbar = false;
            this.Text = "Rendering settings";
            this.cameraGroup.ResumeLayout(false);
            this.cameraGroup.PerformLayout();
            this.engineGroup.ResumeLayout(false);
            this.engineGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stepSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.epsilon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.renderingDistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numIterations)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Vector3Input upDirection;
        private Vector3Input cameraPos;
        private System.Windows.Forms.Label lblUp;
        private System.Windows.Forms.Label lblPosition;
        private System.Windows.Forms.Label lblMaxIter;
        private System.Windows.Forms.NumericUpDown numIterations;
        private System.Windows.Forms.NumericUpDown renderingDistance;
        private System.Windows.Forms.Label lblMaxDist;
        private System.Windows.Forms.Label lblEps;
        private System.Windows.Forms.NumericUpDown epsilon;
        private System.Windows.Forms.Label lblStep;
        private System.Windows.Forms.NumericUpDown stepSize;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button apply;
        private Vector3Input cameraTarget;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.GroupBox cameraGroup;
        private System.Windows.Forms.GroupBox engineGroup;
    }
}