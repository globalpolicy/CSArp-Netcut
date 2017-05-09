using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSArp
{
    public interface IView
    {
        ListView ListView1 { get; }
        ToolStripStatusLabel ToolStripStatus { get; }
        ToolStripComboBox ToolStripComboBoxDeviceList { get; }
        Form MainForm { get; }
        NotifyIcon NotifyIcon1 { get; }
        ToolStripTextBox ToolStripTextBoxClientName { get; }
        ToolStripStatusLabel ToolStripStatusScan { get; }
        ToolStripProgressBar ToolStripProgressBarScan { get; }
        ToolStripMenuItem ShowLogToolStripMenuItem { get; }
        RichTextBox LogRichTextBox { get; }
        SaveFileDialog SaveFileDialogLog { get; }
    }
}
