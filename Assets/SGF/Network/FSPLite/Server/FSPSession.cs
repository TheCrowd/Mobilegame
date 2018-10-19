using System;
using System.Net;
using SGF.Network.KCP;
using SGF.ProtoBuf;

namespace SGF.Network.FSPLite.Server
{
    public class FSPSession
    {
        //========================================================================
        //---------------------------------------------------------
        private string LOG_TAG = "FSPSession";


        //========================================================================
        //Session index var, can use both Sid and Ip as index
        public uint m_Sid;
        public uint Id { get { return m_Sid; } }

        private Action<FSPDataC2S> mRecvListener;
        private KCPSocket mSocket;
        private byte[] mSendBuffer = new byte[40960];
        private bool isEndPointChanged = false;
        private IPEndPoint mEndPoint;
        public IPEndPoint EndPoint
        {
            get { return mEndPoint; }
            set
            {
                if (mEndPoint == null || !mEndPoint.Equals(value))
                {
                    isEndPointChanged = true;
                }
                else
                {
                    isEndPointChanged = false;
                }

                mEndPoint = value;
            }
        }

        public bool IsEndPointChanged { get { return isEndPointChanged; } }

        //========================================================================

        public FSPSession(uint sid, KCPSocket socket)
        {
            m_Sid = sid;
            mSocket = socket;
            LOG_TAG = "FSPSession<" + m_Sid.ToString("d4") + ">";
        }

        public virtual void Close()
        {
            if (mSocket != null)
            {
                mSocket.CloseKcp(EndPoint);
                mSocket = null;
            }
        }

        //-------------------------------------------------------------------


        public void SetReceiveListener(Action<FSPDataC2S> listener)
        {
            mRecvListener = listener;
        }


        public bool Send(FSPFrame frame)
        {
            if (mSocket != null)
            {
                FSPDataS2C data = new FSPDataS2C();
                data.frames.Add(frame);
                int len = PBSerializer.NSerialize(data, mSendBuffer);
                return mSocket.SendTo(mSendBuffer, len, EndPoint);
            }

            return false;
        }


        public void Receive(FSPDataC2S data)
        {
            if (mRecvListener != null)
            {
                mRecvListener(data);
            }
        }

    }
}


