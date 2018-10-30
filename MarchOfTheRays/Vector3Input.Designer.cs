namespace MarchOfTheRays
{
    partial class Vector3Input
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            this.txtX = new System.Windows.Forms.TextBox();
            this.txtY = new System.Windows.Forms.TextBox();
            this.txtZ = new System.Windows.Forms.TextBox();
            this.epX = new System.Windows.Forms.ErrorProvider(this.components);
            this.epY = new System.Windows.Forms.ErrorProvider(this.components);
            this.epZ = new System.Windows.Forms.ErrorProvider(this.components);
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.epX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.epY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.epZ)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 6);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(17, 13);
            label1.TabIndex = 0;
            label1.Text = "X:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(98, 6);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(17, 13);
            label2.TabIndex = 1;
            label2.Text = "Y:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(193, 6);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(17, 13);
            label3.TabIndex = 2;
            label3.Text = "Z:";
            // 
            // txtX
            // 
            this.txtX.Location = new System.Drawing.Point(39, 3);
            this.txtX.Name = "txtX";
            this.txtX.Size = new System.Drawing.Size(40, 20);
            this.txtX.TabIndex = 3;
            // 
            // txtY
            // 
            this.txtY.Location = new System.Drawing.Point(134, 3);
            this.txtY.Name = "txtY";
            this.txtY.Size = new System.Drawing.Size(40, 20);
            this.txtY.TabIndex = 4;
            // 
            // txtZ
            // 
            this.txtZ.Location = new System.Drawing.Point(229, 3);
            this.txtZ.Name = "txtZ";
            this.txtZ.Size = new System.Drawing.Size(40, 20);
            this.txtZ.TabIndex = 5;
            // 
            // epX
            // 
            this.epX.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epX.ContainerControl = this;
            // 
            // epY
            // 
            this.epY.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epY.ContainerControl = this;
            // 
            // epZ
            // 
            this.epZ.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.epZ.ContainerControl = this;
            // 
            // Vector3Input
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtZ);
            this.Controls.Add(this.txtY);
            this.Controls.Add(this.txtX);
            this.Controls.Add(label3);
            this.Controls.Add(label2);
            this.Controls.Add(label1);
            this.Name = "Vector3Input";
            this.Size = new System.Drawing.Size(272, 26);
            ((System.ComponentModel.ISupportInitialize)(this.epX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.epY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.epZ)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtY;
        private System.Windows.Forms.TextBox txtZ;
        private System.Windows.Forms.ErrorProvider epX;
        private System.Windows.Forms.ErrorProvider epY;
        private System.Windows.Forms.ErrorProvider epZ;
        public System.Windows.Forms.TextBox txtX;
    }
}
