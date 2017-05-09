using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSArp
{
    public partial class Form1 : Form, IView
    {
        private Controller _controller;
        public Form1()
        {
            InitializeComponent();
            _controller = new Controller(this);
        }

        

        #region IView members
        public ListView ListView1
        {
            get
            {
                return listView1;
            }
        }
        public ToolStripStatusLabel ToolStripStatus
        {
            get
            {
                return toolStripStatus;
            }
        }
        public ToolStripComboBox ToolStripComboBoxDeviceList
        {
            get
            {
                return toolStripComboBoxDevicelist;
            }
        }
        public Form MainForm
        {
            get
            {
                return this;
            }
        }
        public NotifyIcon NotifyIcon1
        {
            get
            {
                return notifyIcon1;
            }
        }
        public ToolStripTextBox ToolStripTextBoxClientName
        {
            get
            {
                return toolStripTextBoxClientName;
            }
        }
        public ToolStripStatusLabel ToolStripStatusScan
        {
            get
            {
                return toolStripStatusScan;
            }
        }
        public ToolStripProgressBar ToolStripProgressBarScan
        {
            get
            {
                return toolStripProgressBarScan;
            }
        }
        public ToolStripMenuItem ShowLogToolStripMenuItem
        {
            get
            {
                return showLogToolStripMenuItem;
            }
        }
        public RichTextBox LogRichTextBox
        {
            get
            {
                return richTextBoxLog;
            }
        }
        public SaveFileDialog SaveFileDialogLog
        {
            get
            {
                return saveFileDialog1;
            }
        }
        #endregion

        private void toolStripMenuItemRefreshClients_Click(object sender, EventArgs e)
        {
            _controller.RefreshClients();
        }

        private void aboutCSArpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.ShowAboutBox();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.EndApplication();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _controller.AttachOnExitEventHandler();
            _controller.PopulateInterfaces();
            _controller.SetSavedInterface();
            _controller.InitializeNotifyIcon();
        }

        private void cutoffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.DisconnectSelectedClients();
        }

        private void reconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.ReconnectClients();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            _controller.FormResized(sender, e);
        }

        private void toolStripTextBoxClientName_KeyUp(object sender, KeyEventArgs e)
        {
            _controller.ToolStripTextBoxClientNameKeyUp(sender, e);
        }

        private void toolStripMenuItemMinimize_Click(object sender, EventArgs e)
        {
            _controller.ToolStripMinimizeClicked();
        }

        private void toolStripMenuItemSaveSettings_Click(object sender, EventArgs e)
        {
            _controller.ToolStripSaveClicked();
        }

        private void showLogToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            _controller.ShowLogToolStripMenuItemChecked();
        }

        private void saveStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.SaveLogShowDialogBox();
        }

        private void clearStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.ClearLog();
        }
    }
}
