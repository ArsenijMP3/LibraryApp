namespace LibraryApp.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label labelUser;
        private System.Windows.Forms.Button buttonLogout;
        private System.Windows.Forms.Button buttonViewBooks;
        private System.Windows.Forms.Button buttonViewIssues;
        private System.Windows.Forms.Panel panelContent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.buttonLogout = new System.Windows.Forms.Button();
            this.labelUser = new System.Windows.Forms.Label();
            this.buttonViewBooks = new System.Windows.Forms.Button();
            this.buttonViewIssues = new System.Windows.Forms.Button();
            this.panelContent = new System.Windows.Forms.Panel();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.panelTop.Controls.Add(this.buttonLogout);
            this.panelTop.Controls.Add(this.labelUser);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1000, 55);
            this.panelTop.TabIndex = 0;
            // 
            // buttonLogout
            // 
            this.buttonLogout.BackColor = System.Drawing.Color.FromArgb(192, 57, 43);
            this.buttonLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonLogout.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.buttonLogout.ForeColor = System.Drawing.Color.White;
            this.buttonLogout.Location = new System.Drawing.Point(900, 12);
            this.buttonLogout.Name = "buttonLogout";
            this.buttonLogout.Size = new System.Drawing.Size(85, 32);
            this.buttonLogout.TabIndex = 1;
            this.buttonLogout.Text = "🚪 Выйти";
            this.buttonLogout.UseVisualStyleBackColor = false;
            // 
            // labelUser
            // 
            this.labelUser.AutoSize = true;
            this.labelUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelUser.ForeColor = System.Drawing.Color.White;
            this.labelUser.Location = new System.Drawing.Point(15, 17);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(60, 20);
            this.labelUser.TabIndex = 0;
            this.labelUser.Text = "👤 Гость";
            // 
            // buttonViewBooks
            // 
            this.buttonViewBooks.BackColor = System.Drawing.Color.FromArgb(46, 204, 113);
            this.buttonViewBooks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonViewBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonViewBooks.ForeColor = System.Drawing.Color.White;
            this.buttonViewBooks.Location = new System.Drawing.Point(30, 80);
            this.buttonViewBooks.Name = "buttonViewBooks";
            this.buttonViewBooks.Size = new System.Drawing.Size(220, 50);
            this.buttonViewBooks.TabIndex = 1;
            this.buttonViewBooks.Text = "📚 Каталог книг";
            this.buttonViewBooks.UseVisualStyleBackColor = false;
            // 
            // buttonViewIssues
            // 
            this.buttonViewIssues.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
            this.buttonViewIssues.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonViewIssues.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonViewIssues.ForeColor = System.Drawing.Color.White;
            this.buttonViewIssues.Location = new System.Drawing.Point(270, 80);
            this.buttonViewIssues.Name = "buttonViewIssues";
            this.buttonViewIssues.Size = new System.Drawing.Size(220, 50);
            this.buttonViewIssues.TabIndex = 2;
            this.buttonViewIssues.Text = "📋 Выдачи книг";
            this.buttonViewIssues.UseVisualStyleBackColor = false;
            this.buttonViewIssues.Visible = false;
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.BackColor = System.Drawing.Color.White;
            this.panelContent.Location = new System.Drawing.Point(20, 145);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(960, 440);
            this.panelContent.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.buttonViewIssues);
            this.Controls.Add(this.buttonViewBooks);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Библиотечная система - Главная";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}