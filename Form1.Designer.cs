namespace CoinFactorySim
{
    partial class Form1 {
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
        private void InitializeComponent() {
            Label_Money = new Label();
            SuspendLayout();
            // 
            // Label_Money
            // 
            Label_Money.AutoSize = true;
            Label_Money.Font = new Font("Arial", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Label_Money.Location = new Point(0, 0);
            Label_Money.Name = "Label_Money";
            Label_Money.Size = new Size(140, 33);
            Label_Money.TabIndex = 0;
            Label_Money.Text = "Money: 0";
            // 
            // Form1
            // 
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1920, 1080);
            Controls.Add(Label_Money);
            Margin = new Padding(39, 18, 39, 18);
            Name = "Form1";
            Load += Form1_Load;
            KeyDown += On_KeyDown;
            AutoScaleDimensions = new SizeF(96.0F, 96.0F);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label Label_Money;
    }
}
