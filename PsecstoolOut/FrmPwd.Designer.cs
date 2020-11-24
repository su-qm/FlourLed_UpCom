namespace PsecstoolOut
{
    partial class FrmPwd
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
            this.plForm = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btCancle = new System.Windows.Forms.Button();
            this.btOk = new System.Windows.Forms.Button();
            this.tePsw = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.plForm.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // plForm
            // 
            this.plForm.Controls.Add(this.panel1);
            this.plForm.Controls.Add(this.label1);
            this.plForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plForm.Location = new System.Drawing.Point(0, 0);
            this.plForm.Name = "plForm";
            this.plForm.Size = new System.Drawing.Size(360, 220);
            this.plForm.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.panel1.Controls.Add(this.btCancle);
            this.panel1.Controls.Add(this.btOk);
            this.panel1.Controls.Add(this.tePsw);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 51);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(360, 169);
            this.panel1.TabIndex = 31;
            // 
            // btCancle
            // 
            this.btCancle.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancle.Location = new System.Drawing.Point(208, 95);
            this.btCancle.Name = "btCancle";
            this.btCancle.Size = new System.Drawing.Size(92, 28);
            this.btCancle.TabIndex = 5;
            this.btCancle.Text = "取消";
            this.btCancle.UseVisualStyleBackColor = true;
            this.btCancle.Click += new System.EventHandler(this.btCancle_Click);
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btOk.Location = new System.Drawing.Point(58, 95);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(92, 28);
            this.btOk.TabIndex = 4;
            this.btOk.Text = "进入系统";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // tePsw
            // 
            this.tePsw.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tePsw.Location = new System.Drawing.Point(133, 35);
            this.tePsw.Name = "tePsw";
            this.tePsw.Size = new System.Drawing.Size(167, 23);
            this.tePsw.TabIndex = 2;
            this.tePsw.UseSystemPasswordChar = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label2.Location = new System.Drawing.Point(55, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 14);
            this.label2.TabIndex = 6;
            this.label2.Text = "密  码：";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(98)))), ((int)(((byte)(172)))), ((int)(((byte)(202)))));
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("华文行楷", 18F);
            this.label1.ForeColor = System.Drawing.Color.OrangeRed;
            this.label1.Image = global::PsecstoolOut.Properties.Resources.PsecstoolExLogo32;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(360, 51);
            this.label1.TabIndex = 30;
            this.label1.Text = "     请输入还原出厂设置密码";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FrmPwd
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancle;
            this.ClientSize = new System.Drawing.Size(360, 220);
            this.Controls.Add(this.plForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmPwd";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmPwd";
            this.plForm.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel plForm;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btCancle;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.TextBox tePsw;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;

    }
}