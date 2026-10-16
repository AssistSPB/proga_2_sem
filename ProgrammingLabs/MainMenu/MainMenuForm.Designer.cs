namespace ProgrammingLabs.MainMenu
{
    partial class MainMenuForm
    {
         
        private System.ComponentModel.IContainer? components = null;

         
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }


        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1060, 650);
            Name = "MainMenuForm";
            Text = "Programming Labs";
            ResumeLayout(false);
        }

    }
}
