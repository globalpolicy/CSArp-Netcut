using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using SharpPcap;
using PacketDotNet;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.IO;

/*
 Reference:
 http://stackoverflow.com/questions/14114971/sending-my-own-arp-packet-using-sharppcap-and-packet-net
 https://www.codeproject.com/Articles/12458/SharpPcap-A-Packet-Capture-Framework-for-NET
*/

namespace CSArp
{
    public static class GetClientList
    {
        private static ICaptureDevice capturedevice;

        /// <summary>
        /// Populates listview with machines connected to the LAN
        /// </summary>
        /// <param name="view"></param>
        /// <param name="interfacefriendlyname"></param>
        public static void GetAllClients(IView view, string interfacefriendlyname)
        {
            #region initialization
            view.MainForm.BeginInvoke(new Action(() => view.ToolStripStatusScan.Text = "Scanning..."));
            view.MainForm.BeginInvoke(new Action(() => view.ToolStripProgressBarScan.Value = 0));
            Dictionary<IPAddress, PhysicalAddress> clientlist = new Dictionary<IPAddress, PhysicalAddress>(); //this is exclusively for preventing redundant entries into listview
            if (capturedevice != null)
            {
                try
                {
                    capturedevice.StopCapture(); //stop previous capture
                    capturedevice.Close(); //close previous instances
                }
                catch (PcapException ex)
                {
                    Debug.Print("Exception at GetAllClients while trying to capturedevice.StopCapture() or capturedevice.Close()\n" + ex.Message);
                }
            }
            view.ListView1.Items.Clear();
            #endregion

            CaptureDeviceList capturedevicelist = CaptureDeviceList.Instance;
            capturedevicelist.Refresh(); //crucial for reflection of any network changes
            capturedevice = (from devicex in capturedevicelist where ((SharpPcap.WinPcap.WinPcapDevice)devicex).Interface.FriendlyName == interfacefriendlyname select devicex).ToList()[0];
            capturedevice.Open(DeviceMode.Promiscuous, 1000); //1000 milliseconds seems to be the accepted norm for timeout
            IPAddress myipaddress = ((SharpPcap.WinPcap.WinPcapDevice)capturedevice).Addresses[1].Addr.ipAddress; //possible critical point : Addresses[1] in hardcoding the index for obtaining ipv4 address

            #region Sending ARP requests to probe for all possible IP addresses on LAN
            int ipindex = 1;
            new Thread(() =>
             {
                 try
                 {
                     for (ipindex = 1; ipindex <= 255; ipindex++)
                     {
                         ARPPacket arprequestpacket = new ARPPacket(ARPOperation.Request, PhysicalAddress.Parse("00-00-00-00-00-00"), IPAddress.Parse(GetRootIp(myipaddress) + ipindex), capturedevice.MacAddress, myipaddress);
                         EthernetPacket ethernetpacket = new EthernetPacket(capturedevice.MacAddress, PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"), EthernetPacketType.Arp);
                         ethernetpacket.PayloadPacket = arprequestpacket;
                         capturedevice.SendPacket(ethernetpacket);
                     }
                 }
                 catch (Exception ex)
                 {
                     Debug.Print("Exception at GetClientList.GetAllClients() inside new Thread(()=>{}) while sending packets\n" + ex.Message);
                 }
             }).Start();
            #endregion

            #region Retrieving ARP packets floating around and finding out the senders' IP and MACs
            capturedevice.Filter = "arp";
            int clientindex = 0;
            RawCapture rawcapture = null;
            long scanduration = 5000;
            new Thread(() =>
            {
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while ((rawcapture = capturedevice.GetNextPacket()) != null && stopwatch.ElapsedMilliseconds <= scanduration)
                    {
                        Packet packet = Packet.ParsePacket(rawcapture.LinkLayerType, rawcapture.Data);
                        ARPPacket arppacket = (ARPPacket)packet.Extract(typeof(ARPPacket));
                        if (!clientlist.ContainsKey(arppacket.SenderProtocolAddress) && arppacket.SenderProtocolAddress.ToString() != "0.0.0.0" && areCompatibleIPs(arppacket.SenderProtocolAddress, myipaddress))
                        {
                            view.ListView1.BeginInvoke(new Action(() =>
                            {
                                view.ListView1.Items.Add(new ListViewItem(new string[] { (++clientindex).ToString(), arppacket.SenderProtocolAddress.ToString(), GetMACString(arppacket.SenderHardwareAddress), "On", ApplicationSettingsClass.GetSavedClientNameFromMAC(GetMACString(arppacket.SenderHardwareAddress)) }));
                            }));
                            //Debug.Print("{0} @ {1}", arppacket.SenderProtocolAddress, arppacket.SenderHardwareAddress);
                            clientlist.Add(arppacket.SenderProtocolAddress, arppacket.SenderHardwareAddress);
                        }
                        int percentageprogress = (int)((float)stopwatch.ElapsedMilliseconds / scanduration * 100);
                        view.MainForm.BeginInvoke(new Action(() => view.ToolStripStatusScan.Text = "Scanning " + percentageprogress + "%"));
                        view.MainForm.BeginInvoke(new Action(() => view.ToolStripProgressBarScan.Value = percentageprogress));
                        //Debug.Print(packet.ToString() + "\n");
                    }
                    stopwatch.Stop();
                    capturedevice.StopCapture();
                    view.MainForm.BeginInvoke(new Action(() => view.ToolStripStatusScan.Text = clientindex + " devices found"));
                    view.MainForm.BeginInvoke(new Action(() => view.ToolStripProgressBarScan.Value = 100));
                }
                catch (PcapException ex)
                {
                    Debug.Print("PcapException @ GetClientList.GetAllClients() @ new Thread(()=>{}) while retrieving packets\n" + ex.Message);
                    view.MainForm.BeginInvoke(new Action(() => view.ToolStripStatusScan.Text = "Refresh for scan"));
                    view.MainForm.BeginInvoke(new Action(() => view.ToolStripProgressBarScan.Value = 0));
                }
                catch(Exception ex)
                {
                    Debug.Print(ex.Message);
                }

            }).Start();
            #endregion
        }

        /// <summary>
        /// Converts a PhysicalAddress to colon delimited string like FF:FF:FF:FF:FF:FF
        /// </summary>
        /// <param name="physicaladdress"></param>
        /// <returns></returns>
        private static string GetMACString(PhysicalAddress physicaladdress)
        {
            try
            {
                string retval = "";
                for (int i = 0; i <= 5; i++)
                    retval += physicaladdress.GetAddressBytes()[i].ToString("X2") + ":";
                return retval.Substring(0, retval.Length - 1);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Converts say 192.168.1.4 to 192.168.1.
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <returns></returns>
        private static string GetRootIp(IPAddress ipaddress)
        {
            string ipaddressstring = ipaddress.ToString();
            return ipaddressstring.Substring(0, ipaddressstring.LastIndexOf(".") + 1);
        }

        /// <summary>
        /// Checks if both IPAddresses have the same root ip
        /// </summary>
        /// <param name="ip1"></param>
        /// <param name="ip2"></param>
        /// <returns></returns>
        private static bool areCompatibleIPs(IPAddress ip1, IPAddress ip2)
        {
            return (GetRootIp(ip1) == GetRootIp(ip2)) ? true : false;
        }
    }
}
