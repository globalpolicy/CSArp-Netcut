using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CSArp
{
    public static class DebugOutputClass
    {
        public static void Print(IView view, string output)
        {
            try
            {
                string datetimenow = DateTime.Now.ToString();
                view.LogRichTextBox.Invoke(new Action(() =>
                {
                    view.LogRichTextBox.Text += datetimenow + " : " + output + "\n";
                    view.LogRichTextBox.SelectionStart = view.LogRichTextBox.Text.Length;
                    view.LogRichTextBox.ScrollToCaret();
                }));
                
                Debug.Print(output);
            }
            catch (InvalidOperationException) { }

        }
    }
}
