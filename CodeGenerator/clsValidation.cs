using System.Windows.Forms;

namespace CodeGenerator
{
    public class clsValidation
    {
        public static void ValidateRequiredTextBox(TextBox txt, ErrorProvider errorProvider, System.ComponentModel.CancelEventArgs e)
        {
            if(txt.Text.Trim() == string.Empty)
            {
                txt.Focus();
                errorProvider.SetError(txt, "This field is required.");
            }
            else
                errorProvider.SetError(txt, string.Empty);
        }
    }
}