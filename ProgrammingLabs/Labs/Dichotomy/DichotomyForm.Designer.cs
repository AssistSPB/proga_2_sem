namespace ProgrammingLabs.Labs.Dichotomy
{
    partial class DichotomyForm
    {         
        private System.ComponentModel.IContainer components = null;
         
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
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();
               
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 710);
            this.Name = "DichotomyForm";
            this.Text = "Метод дихотомии — поиск корня f(x) = 0";
            this.ResumeLayout(false);
        }

    }
}