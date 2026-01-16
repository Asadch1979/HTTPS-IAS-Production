using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AIS
    {

    public class LocalIPAddress
        {
        public string GetLocalIpAddress()
            {

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                    return ip.ToString();
                    }
                }
            return "";
            }

        public string GetMACAddress()
            {
            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();

            return macAddr.ToString();
            }
        public string GetFirstMACCardAddress()
            {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String sMacAddress = string.Empty;
            foreach (NetworkInterface adapter in nics)
                {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                    {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                    }
                }
            return sMacAddress;
            }
        }
    }
