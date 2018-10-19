using SGF.Logger;
using System;
using System.Collections.Generic;
using System.Net;

namespace SGF.Network.Utils
{
    public class DNSUtils
    {
        public const string TAG = "DNSUtils";

        public static string[] GetHostByName(string hostNameOrAddress)
        {
            IPAddress ipaddr = null;
            if (IPAddress.TryParse(hostNameOrAddress, out ipaddr))
            {
                return new string[1] { ipaddr.ToString() };
            }



            IPAddress[] ipAddresses = null;

            try
            {
                ipAddresses = Dns.GetHostAddresses(hostNameOrAddress);
            }
            catch (Exception)
            {

            }

            if (ipAddresses != null && ipAddresses.Length > 0)
            {
                string[] ipstrs = new string[ipAddresses.Length];
                for (int i = 0; i < ipAddresses.Length; i++)
                {
                    ipstrs[i] = ipAddresses[i].ToString();
                }
                return ipstrs;
            }

            return new string[0];
        }


        public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
        {
            string[] ipstrs = GetHostByName(hostNameOrAddress);
            if (ipstrs == null || ipstrs.Length == 0)
            {
                return new IPAddress[0];
            }
            List<IPAddress> listIPAddrs = new List<IPAddress>();
            for (int i = 0; i < ipstrs.Length; i++)
            {
                IPAddress ipAddress = null;
                if (IPAddress.TryParse(ipstrs[i], out ipAddress))
                {
                    listIPAddrs.Add(ipAddress);
                }
            }
            return listIPAddrs.ToArray();
        }



        public static string[] GetUrlWithIP(string url)
        {
            MyLogger.Log(TAG, "GetUrlWithIP() old url=" + url);

            string head, hostname, port, path;
            URLUtils.SplitUrl(url, out head, out hostname, out port, out path);


            string[] ipstrs = GetHostByName(hostname);
            if (ipstrs == null || ipstrs.Length == 0)
            {
                MyLogger.Log(TAG, "GetUrlWithIP() domain cannot be parse! new url=" + url);
                return new string[1] { url };
            }

            MyLogger.Log(TAG, "GetUrlWithIP() Can Parse Total " + ipstrs.Length + " url!");
            string[] urls = new string[ipstrs.Length];
            for (int i = 0; i < ipstrs.Length; i++)
            {
                if (string.IsNullOrEmpty(port))
                {
                    urls[i] = head + ipstrs[i] + path;
                }
                else
                {
                    urls[i] = head + ipstrs[i] + ":" + port + path;
                }

                MyLogger.Log(TAG, "GetUrlWithIP() new url[" + (i + 1) + "]=" + urls[i]);
            }

            return urls;
        }




    }
}
