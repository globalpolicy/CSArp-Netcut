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
            _controller.PopulateInterfaces();
        }

        private void cutoffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.DisconnectSelectedClients();
        }

        private void reconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.ReconnectClients();
        }

        
    }
}
