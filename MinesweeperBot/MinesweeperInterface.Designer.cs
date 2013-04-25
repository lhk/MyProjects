namespace MinesweeperBot
{
    partial class MinesweeperInterface
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
            this.SuspendLayout();
            // 
            // MinesweeperInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(867, 527);
            this.Name = "MinesweeperInterface";
            this.Text = "MinesweeperInterface";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MinesweeperInterface_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MinesweeperInterface_MouseDown);
            this.ResumeLayout(false);

        }

        #endregion


    }
}