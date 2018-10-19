using SGF.Logger;
using SGF.Network.Utils;
using System;
using System.Net;
using System.Net.Sockets;



namespace SGF.Network
{
    public class UDPSocket
    {


        //===========================================================
        public static string LOG_TAG = "UDPSocket";

        private bool isActive;
        private Socket mSystemSocket;
        private AddressFamily mAddrFamily;
        private bool isBroadcast;
        private bool mEnableBlockOnRecv = false;//determine whether can block when receive


        //===========================================================
        #region Creator and releaser
        public UDPSocket(AddressFamily family, bool enableBlockOnRecv)
        {
            mEnableBlockOnRecv = enableBlockOnRecv;
            mAddrFamily = family;
            mSystemSocket = new Socket(mAddrFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        public AddressFamily AddressFamily { get { return mAddrFamily; } }

        public void Dispose()
        {
            Close();
        }


        public void Close()
        {
            if (mSystemSocket != null)
            {
                try
                {
                    mSystemSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    MyLogger.LogWarning(LOG_TAG, "Close() ",e.Message + e.StackTrace);
                }

                mSystemSocket.Close();
                mSystemSocket = null;
            }

            isActive = false;
            GC.SuppressFinalize(this);
        }



        public string SelfIP
        {
            get
            {
                return IPUtils.SelfIP;

            }
        }

        public int SelfPort
        {
            get
            {
                IPEndPoint ipep = mSystemSocket.LocalEndPoint as IPEndPoint;
                return ipep.Port;
            }
        }

        public Socket SystemSocket { get { return mSystemSocket; } }

        #endregion

        //------------------------------------------------------------
        #region Functions to bind port
        public int Bind(int port = 0)
        {
            MyLogger.Log(LOG_TAG, "Bind() port = " + port);
            if (mSystemSocket == null)
            {
                return 0;
            }

            //if the bound port is 0, will assign a random port
            IPEndPoint ipep = IPUtils.GetIPEndPointAny(mAddrFamily, port);
            mSystemSocket.Bind(ipep);
            isActive = true;
            return SelfPort;
        }


        #endregion


        //------------------------------------------------------------
        #region ReceiveFrom,SendTo Function
        public int ReceiveFrom(byte[] buffer, int maxsize, ref IPEndPoint remoteEP)
        {
            int cnt = 0;

            EndPoint ip = null;

            if (!mEnableBlockOnRecv)
            {
                if (mSystemSocket.Available <= 0)
                {
                    return 0;
                }
            }


            if (mAddrFamily == AddressFamily.InterNetwork)
            {
                //In IPv4 ,the same as android
                ip = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
                cnt = mSystemSocket.ReceiveFrom(buffer, maxsize, SocketFlags.None, ref ip);

                if (cnt > 0 && remoteEP != null && !remoteEP.Equals(ip))
                {
                    MyLogger.LogWarning(LOG_TAG, "ReceiveFrom()"," receive msg from stranger IP:Port(" + ip + ")!");
                    return 0;
                }
            }
            else
            {
                //In IPv6
                ip = remoteEP;
                cnt = mSystemSocket.ReceiveFrom(buffer, maxsize, SocketFlags.None, ref ip);
            }

            remoteEP = ip as IPEndPoint;



            return cnt;
        }




        public int SendTo(byte[] buffer, int size, IPEndPoint remoteEP)
        {
            int cnt = 0;



            //local socket is not available, use system socket instead
            if (cnt == 0)
            {
                cnt = mSystemSocket.SendTo(buffer, 0, size, SocketFlags.None, remoteEP);
            }

            return cnt;
        }
        #endregion
    }
}

