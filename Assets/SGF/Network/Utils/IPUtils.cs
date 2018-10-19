using SGF.Logger;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SGF.Network.Utils
{
    public class IPUtils
    {
        public static bool IOS_IPv6_First = true;
        //===========================================================

        public static readonly IPAddress IP_Broadcast = new IPAddress(0xffffffffL);
        public static IPEndPoint IPEP_Any = new IPEndPoint(IPAddress.Any, 0);
        public static IPEndPoint IPEP_IPv6Any = new IPEndPoint(IPAddress.IPv6Any, 0);

        #region functions:IsBroadcast,IsFatalException,GetIPEndPointAny
        public static bool IsBroadcast(IPAddress ip)
        {
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return false;
            }

            return ip.Equals(IP_Broadcast);
        }

        public static bool IsFatalException(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }
            return (((exception is OutOfMemoryException) ||
                (exception is StackOverflowException)) ||
                (exception is ThreadAbortException));
        }

        public static IPEndPoint GetIPEndPointAny(AddressFamily family, int port)
        {
            if (family == AddressFamily.InterNetwork)
            {
                if (port == 0)
                {
                    return IPEP_Any;
                }

                return new IPEndPoint(IPAddress.Any, port);
            }
            else if (family == AddressFamily.InterNetworkV6)
            {
                if (port == 0)
                {
                    return IPEP_IPv6Any;
                }

                return new IPEndPoint(IPAddress.IPv6Any, port);
            }
            return null;
        }
        #endregion

        #region function:get remote clients' address
        public static IPEndPoint GetHostEndPoint(string host, int port)
        {
            IPAddress address = null;
            if (IPAddress.TryParse(host, out address))
            {
#if TestIPv6
                MyLogger.Log("UdpSocket.GetHostEndPoint() TestIPv6: if it is a IP Already,convert to IPv6");
                if (!address.IsIPv6LinkLocal && !address.IsIPv6SiteLocal)
                {
                    string tmp = "64:ff9b::" + address.ToString();
                    IPAddress addr_v6 = null;
                    if (IPAddress.TryParse(tmp, out addr_v6))
                    {
                        return new IPEndPoint(addr_v6, port);
                    }
                    else
                    {
                        Debuger.LogError("UdpSocket.GetHostEndPoint() TestIPv6，封装IPv6失败:" + tmp);
                    }
                }

#endif
                return new IPEndPoint(address, port);
            }
            else
            {
                //Not an IP, regard as a domain name
                IPAddress[] ips = Dns.GetHostAddresses(host);

                if (MyLogger.EnableLog)
                {
                    MyLogger.Log("UdpSocket.GetHostEndPoint() Dns GetHostAddresses:");
                    for (int i = 0; i < ips.Length; i++)
                    {
                        MyLogger.Log("[" + i + "] " + ips[i] + ", " + ips[i].AddressFamily);
                    }
                }


                List<IPAddress> listIPv4 = new List<IPAddress>();
                List<IPAddress> listIPv6 = new List<IPAddress>();

                for (int i = 0; i < ips.Length; i++)
                {
                    if (ips[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        listIPv4.Add(ips[i]);
                    }
                    else
                    {
                        listIPv6.Add(ips[i]);
                    }
                }


#if UNITY_ANDROID

                if (listIPv4.Count > 0)
                {
                    return new IPEndPoint(listIPv4[0], port);
                }

                if (listIPv6.Count > 0)
                {
                    return new IPEndPoint(listIPv6[0], port);
                }

#else
                // a switch , later we can use Lua to set 
                if (IOS_IPv6_First)
                {
                    //IPv6 First
                    if (listIPv6.Count > 0)
                    {
                        return new IPEndPoint(listIPv6[0], port);
                    }

                    if (listIPv4.Count > 0)
                    {
                        return new IPEndPoint(listIPv4[0], port);
                    }
                }
                else
                {
                    //IPv4 First
                    if (listIPv4.Count > 0)
                    {
                        return new IPEndPoint(listIPv4[0], port);
                    }

                    if (listIPv6.Count > 0)
                    {
                        return new IPEndPoint(listIPv6[0], port);
                    }
                }

#endif

            }

            return null;
        }
        #endregion


        private static string m_selfIpAddress = "";
        public static void CheckSelfIPAddress()
        {
            m_selfIpAddress = UnityEngine.Network.player.ipAddress;
        }



        public static string SelfIP
        {
            get { return m_selfIpAddress; }
        }


    }
}
