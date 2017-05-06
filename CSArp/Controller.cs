/*
 * CSArp
 * An arpspoofing program
 * Author : globalpolicy
 * Contact : yciloplabolg@gmail.com
 * Blog : c0dew0rth.blogspot.com
 * Github : globalpolicy
 * Time : May 6, 2017 @ 08:28AM
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpPcap;
using SharpPcap.WinPcap;
using SharpPcap.AirPcap;
using SharpPcap.LibPcap;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace CSArp
{
    public class Controller
    {
        #region fields
        private IView _view;
        #endregion

        #region constructor
        public Controller(IView view)
        {
            _view = view;
        }
        #endregion

        /// <summary>
        /// Populate the available network cards
        /// </summary>
        public void PopulateInterfaces()
        {
            CaptureDeviceList capturedevicelist = CaptureDeviceList.Instance;
            List<string> capturedevicelistofstring = new List<string>();
            capturedevicelist.ToList().ForEach((ICaptureDevice capturedevice) =>
            {
                if (capturedevice is WinPcapDevice)
                {
                    WinPcapDevice winpcapdevice = (WinPcapDevice)capturedevice;
                    capturedevicelistofstring.Add(winpcapdevice.Interface.FriendlyName);
                }
                else if (capturedevice is AirPcapDevice)
                {
                    AirPcapDevice airpcapdevice = (AirPcapDevice)capturedevice;
                    capturedevicelistofstring.Add(airpcapdevice.Interface.FriendlyName);
                }
            });
            _view.ToolStripComboBoxDeviceList.Items.AddRange(capturedevicelistofstring.ToArray());
        }

        /// <summary>
        /// Populate the LAN clients
        /// </summary>
        public void RefreshClients()
        {
            if (_view.ToolStripComboBoxDeviceList.Text != "")
            {
                GetClientList.GetAllClients_TEST(_view, _view.ToolStripComboBoxDeviceList.Text);
            }
            else
            {
                MessageBox.Show("Please select a network interface!", "Interface", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        /// <summary>
        /// Disconnects clients selected in the listview
        /// </summary>
        public void DisconnectSelectedClients()
        {
            if (_view.ListView1.SelectedItems.Count > 0)
            {
                Dictionary<IPAddress, PhysicalAddress> targetlist = new Dictionary<IPAddress, PhysicalAddress>();
                int parseindex = 0;
                foreach (ListViewItem listitem in _view.ListView1.SelectedItems)
                {
                    targetlist.Add(IPAddress.Parse(listitem.SubItems[1].Text), PhysicalAddress.Parse(listitem.SubItems[2].Text.Replace(":", "-")));
                    _view.ListView1.BeginInvoke(new Action(() =>
                    {
                        _view.ListView1.SelectedItems[parseindex++].SubItems[3].Text = "Off";
                    }));
                    Debug.Print(listitem.SubItems[1].Text + "@" + listitem.SubItems[2].Text);
                }
                DisconnectReconnect.Disconnect(targetlist, GetGatewayIP(_view.ToolStripComboBoxDeviceList.Text), GetGatewayMAC(_view.ToolStripComboBoxDeviceList.Text), _view.ToolStripComboBoxDeviceList.Text);

            }
        }

        /// <summary>
        /// Reconnects clients by stopping fake ARP requests
        /// </summary>
        public void ReconnectClients()
        {
            DisconnectReconnect.Reconnect();
            this.RefreshClients();
        }

        #region Trivial GUI elements control methods
        public void ShowAboutBox()
        {
            MessageBox.Show("Author : globalpolicy\nContact : yciloplabolg@gmail.com\nBlog : c0dew0rth.blogspot.com\nGithub : globalpolicy\nContributions are welcome!", "About CSArp", MessageBoxButtons.OK);
        }
        public void EndApplication()
        {
            Application.Exit();
        }
        #endregion

        #region Private helper functions
        /// <summary>
        /// Return the gateway IPAddress of the selected network interface's
        /// </summary>
        /// <param name="friendlyname">The friendly name of the selected network interface</param>
        /// <returns>Returns the gateway IPAddress of the selected network interface's</returns>
        private IPAddress GetGatewayIP(string friendlyname)
        {
            IPAddress retval = null;
            string interfacename = "";
            foreach (ICaptureDevice capturedevice in CaptureDeviceList.Instance)
            {
                if (capturedevice is WinPcapDevice)
                {
                    WinPcapDevice winpcapdevice = (WinPcapDevice)capturedevice;
                    if (winpcapdevice.Interface.FriendlyName == friendlyname)
                    {
                        interfacename = winpcapdevice.Interface.Name;
                    }
                }
                else if (capturedevice is AirPcapDevice)
                {
                    AirPcapDevice airpcapdevice = (AirPcapDevice)capturedevice;
                    if (airpcapdevice.Interface.FriendlyName == friendlyname)
                    {
                        interfacename = airpcapdevice.Interface.Name;
                    }
                }
            }
            if (interfacename != "")
            {
                foreach (var networkinterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkinterface.Name == friendlyname)
                    {
                        foreach (var gateway in networkinterface.GetIPProperties().GatewayAddresses)
                        {
                            if (gateway.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) //filter ipv4 gateway ip address
                                retval = gateway.Address;
                        }
                    }
                }
            }
            return retval;
        }
        private PhysicalAddress GetGatewayMAC(string friendlyname)
        {
            PhysicalAddress retval = null;
            string gatewayip = GetGatewayIP(friendlyname).ToString();
            foreach (ListViewItem listviewitem in _view.ListView1.Items)
            {
                if (listviewitem.SubItems[1].Text == gatewayip)
                {
                    retval = PhysicalAddress.Parse(listviewitem.SubItems[2].Text.Replace(":", "-"));
                }
            }
            return retval;
        }
        #endregion
    }
}
