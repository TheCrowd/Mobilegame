using System;
using System.Net;
using SGF.Network.KCP;
using SGF.Unity;
using SGF.Logger;
using SGF.ProtoBuf;
using SGF.Network.Utils;

namespace SGF.Network.FSPLite.Client
{
    public class FSPClient
    {
        //===========================================================
        public delegate void FSPTimeoutListener(FSPClient target, int val);

        //===========================================================
        //log tags
        public string LOG_TAG_SEND = "FSPClient_Send";
        public string LOG_TAG_RECV = "FSPClient_Recv";
        public string LOG_TAG_MAIN = "FSPClient_Main";
        public string LOG_TAG = "FSPClient";

        //===========================================================
        //data

        //thread var
        private bool isRunning = false;

        //base communication var
        private KCPSocket m_Socket;
        private string m_Host;
        private int m_Port;
        private IPEndPoint m_HostEndPoint = null;
        private ushort m_SessionId = 0;

        //===========================================================

        //===========================================================
        //receive var
        private Action<FSPFrame> m_RecvListener;
        private byte[] m_TempRecvBuf = new byte[10240];

        //sent var
        private bool m_EnableFSPSend = true;
        private int m_AuthId;
        private FSPDataC2S m_TempSendData = new FSPDataC2S();
        private byte[] m_TempSendBuf = new byte[128];

        private bool m_WaitForReconnect = false;
        private bool m_WaitForSendAuth = false;

        //===========================================================
        //===========================================================
        //------------------------------------------------------------
        #region Creator and Releaser
        public FSPClient()
        {

        }

        public void Close()
        {
            MyLogger.Log(LOG_TAG_MAIN, "Close()");
            Disconnect();
            m_RecvListener = null;
            m_WaitForReconnect = false;
            m_WaitForSendAuth = false;
        }


        #endregion


        //------------------------------------------------------------
        #region set session ID
        //IP of clients may change over time, need a fixed session ID to identify players
        public void SetSessionId(ushort sid)
        {
            LOG_TAG_MAIN = "FSPClient_Main<" + sid.ToString("d4") + ">";
            LOG_TAG_SEND = "FSPClient_Send<" + sid.ToString("d4") + ">";
            LOG_TAG_RECV = "FSPClient_Recv<" + sid.ToString("d4") + ">";
            LOG_TAG = LOG_TAG_MAIN;

            m_SessionId = sid;
            m_TempSendData = new FSPDataC2S();
            m_TempSendData.vkeys.Add(new FSPVKey());
            m_TempSendData.sid = sid;
        }



        #endregion

        //------------------------------------------------------------
        #region set FSP auth information

        public void SetFSPAuthInfo(int authId)
        {
            MyLogger.Log(LOG_TAG_MAIN, "SetFSPAuthInfo() " + authId);
            m_AuthId = authId;
        }

        public void SetFSPListener(Action<FSPFrame> listener)
        {
            m_RecvListener = listener;
        }

        #endregion

        //------------------------------------------------------------

        #region connecting functions

        public bool IsRunning { get { return isRunning; } }

        public void VerifyAuth()
        {
            m_WaitForSendAuth = false;
            SendFSP(FSPVKeyBase.AUTH, m_AuthId, 0);
        }

        public void Reconnect()
        {
            MyLogger.Log(LOG_TAG_MAIN, "Reconnect() start reconnecting");
            m_WaitForReconnect = false;

            Disconnect();
            Connect(m_Host, m_Port);
            VerifyAuth();
        }

        public bool Connect(string host, int port)
        {
            if (m_Socket != null)
            {
                MyLogger.LogError(LOG_TAG_MAIN, "Connect()"," cannot build connection, please shut down last connection！");
                return false;
            }

            MyLogger.Log(LOG_TAG_MAIN, "Connect() start initial connection， host = {0}, port = {1}", port.ToString(),(object)host);

            m_Host = host;
            m_Port = port;

            try
            {
                //get Host's IPEndPoint
                MyLogger.Log(LOG_TAG_MAIN, "Connect() get Host's IPEndPoint");
                m_HostEndPoint = IPUtils.GetHostEndPoint(m_Host, m_Port);
                if (m_HostEndPoint == null)
                {
                    MyLogger.LogError(LOG_TAG_MAIN, "Connect()" ,"cannot convert host name to IP！");
                    Close();
                    return false;
                }
                MyLogger.Log(LOG_TAG_MAIN, "Connect() HostEndPoint = {0}", m_HostEndPoint.ToString());

                isRunning = true;

                //create a Socket
                MyLogger.Log(LOG_TAG_MAIN, "Connect() create UdpSocket, AddressFamily = {0}", ""+m_HostEndPoint.AddressFamily);
                m_Socket = new KCPSocket(0, 1);
                //m_Socket.Connect(m_HostEndPoint);
                m_Socket.AddReceiveListener(m_HostEndPoint, OnReceive);

            }
            catch (Exception e)
            {
                MyLogger.LogError(LOG_TAG_MAIN, "Connect() " ,e.Message + e.StackTrace);
                Close();
                return false;
            }


            return true;
        }

        private void Disconnect()
        {
            MyLogger.Log(LOG_TAG_MAIN, "Disconnect()");



            isRunning = false;

            if (m_Socket != null)
            {
                m_Socket.Dispose();
                m_Socket = null;
            }


            m_HostEndPoint = null;
        }




        #endregion


        //------------------------------------------------------------

        #region Receive

        private void OnReceive(byte[] buffer, int size, IPEndPoint remotePoint)
        {
            FSPDataS2C data = PBSerializer.NDeserialize<FSPDataS2C>(buffer);

            if (m_RecvListener != null)
            {
                for (int i = 0; i < data.frames.Count; i++)
                {
                    m_RecvListener(data.frames[i]);
                }

            }
        }


        #endregion


        //------------------------------------------------------------

        #region Send

        public bool SendFSP(int vkey, int arg, int clientFrameId)
        {
            if (isRunning)
            {
                FSPVKey cmd = m_TempSendData.vkeys[0];
                cmd.vkey = vkey;
                cmd.args = new int[] { arg };
                cmd.clientFrameId = (uint)clientFrameId;

                int len = PBSerializer.NSerialize(m_TempSendData, m_TempSendBuf);

                return m_Socket.SendTo(m_TempSendBuf, len, m_HostEndPoint);
            }
            return false;
        }

        #endregion  


        //------------------------------------------------------------
        public void EnterFrame()
        {
            if (!isRunning)
            {
                return;
            }

            m_Socket.Update();


            if (m_WaitForReconnect)
            {
                if (NetCheck.IsAvailable())
                {
                    Reconnect();
                }
                else
                {
                    MyLogger.Log(LOG_TAG_MAIN, "EnterFrame() wait for reconnection，but network is not available！");
                }
            }

            if (m_WaitForSendAuth)
            {
                VerifyAuth();
            }
        }



        public string ToDebugString()
        {
            string str = "";
            return str;
        }
    }
}
