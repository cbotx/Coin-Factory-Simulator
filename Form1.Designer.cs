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
            Label_Money.Margin = new Padding(4, 0, 4, 0);
            Label_Money.Name = "Label_Money";
            Label_Money.Size = new Size(246, 33);
            Label_Money.TabIndex = 0;
            Label_Money.Text = "00:00    Money: 0";
            Label_Money.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(2880, 1620);
            Controls.Add(Label_Money);
            Margin = new Padding(58, 27, 58, 27);
            Name = "Form1";
            Load += Form1_Load;
            KeyDown += On_KeyDown;
            MouseMove += Form1_MouseMove;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label Label_Money;
    }
}
