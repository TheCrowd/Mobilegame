using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using SGF.Network.KCP;
using SGF.Network.RPCLite;
using SGF.Common;
using SGF.Logger;
using SGF.ProtoBuf;

namespace SGF.Network.FSPLite.Server
{

    public class FSPServer : Singleton<FSPServer>
    {
        //===========================================================
        //---------------------------------------------------------
        //log tags
        private string LOG_TAG_SEND = "FSPServer_Send";
        private string LOG_TAG_RECV = "FSPServer_Recv";
        private string LOG_TAG_MAIN = "FSPServer_Main";

        //===========================================================
        private FSPParam mParam = new FSPParam();
        /// <summary>
        /// frame internval...
        /// </summary>
        private long FRAME_TICK_INTERVAL = 666666;
        private bool mUseExternFrameTick = false;
        //===========================================================
        //data

        //thread module
        private Thread mThreadMain;
        private bool isRunning = false;
        public bool IsRunning { get { return isRunning; } }

        //Game socket 
        private KCPSocket mGameSocket;
        //Room RPC 
        private RPCService mRoomRPC;


        //===========================================================
        //logic
        private long mLogicLastTicks = 0;
        private long mRealTicksAtStart = 0;
        //===========================================================
        //---------------------------------------------------------
        //Session 
        //given that there won't be much session, using list is enough
        private List<FSPSession> m_ListSession = new List<FSPSession>();
        //=========================================================

        //room
        private FSPRoom mRoom;
        public FSPRoom Room { get { return mRoom; } }

        //game core
        private FSPGame mGame;
        public FSPGame Game { get { return mGame; } }

        //------------------------------------------------------------

        #region params settings

        public void SetFrameInterval(int serverFrameInterval, int clientFrameRateMultiple)//MS
        {
            FRAME_TICK_INTERVAL = serverFrameInterval * 333333 * 30 / 1000;
            FRAME_TICK_INTERVAL = serverFrameInterval * 10000;
            mParam.serverFrameInterval = serverFrameInterval;
            mParam.clientFrameRateMultiple = clientFrameRateMultiple;
        }

        public void SetServerTimeout(int serverTimeout)
        {
            mParam.serverTimeout = serverTimeout;
        }

        public int GetFrameInterval()//MS
        {
            return (int)(FRAME_TICK_INTERVAL / 10000);
        }

        public bool UseExternFrameTick
        {
            get { return mUseExternFrameTick; }
            set { mUseExternFrameTick = value; }
        }

        public FSPParam GetParam()
        {
            mParam.host = GameIP;
            mParam.port = GamePort;
            return mParam.Clone();
        }

        public int RealtimeSinceStartupMS
        {
            get
            {
                long dt = DateTime.Now.Ticks - mRealTicksAtStart;
                return (int)(dt / 10000);
            }
        }

        #endregion

        //------------------------------------------------------------
        #region Communication params

        public string GameIP
        {
            get { return mGameSocket != null ? mGameSocket.SelfIP : ""; }
        }

        public int GamePort
        {
            get { return mGameSocket != null ? mGameSocket.SelfPort : 0; }
        }


        public string RoomIP
        {
            get { return mRoomRPC != null ? mRoomRPC.SelfIP : ""; }
        }

        public int RoomPort
        {
            get { return mRoomRPC != null ? mRoomRPC.SelfPort : 0; }
        }

        #endregion


        //---------------------------------------------------------
        #region Session functions
        private FSPSession GetSession(uint sid)
        {
            FSPSession s = null;
            lock (m_ListSession)
            {
                for (int i = 0; i < m_ListSession.Count; i++)
                {
                    if (m_ListSession[i].Id == sid)
                    {
                        return m_ListSession[i];
                    }
                }
            }
            return null;
        }

        internal FSPSession AddSession(uint sid)
        {
            FSPSession s = GetSession(sid);
            if (s != null)
            {
                MyLogger.LogWarning(LOG_TAG_MAIN, "AddSession()"," SID used = " + sid);
                return s;
            }
            MyLogger.Log(LOG_TAG_MAIN, "AddSession() SID = " + sid);

            s = new FSPSession(sid, mGameSocket);

            lock (m_ListSession)
            {
                m_ListSession.Add(s);
            }
            return s;
        }

        internal void DelSession(uint sid)
        {
            MyLogger.Log(LOG_TAG_MAIN, "DelSession() sid = {0}", sid.ToString());

            lock (m_ListSession)
            {
                for (int i = 0; i < m_ListSession.Count; i++)
                {
                    if (m_ListSession[i].Id == sid)
                    {
                        m_ListSession[i].Close();
                        m_ListSession.RemoveAt(i);
                        return;
                    }
                }
            }
        }



        private void DelAllSession()
        {
            MyLogger.Log(LOG_TAG_MAIN, "DelAllSession()");
            lock (m_ListSession)
            {
                for (int i = 0; i < m_ListSession.Count; i++)
                {
                    m_ListSession[i].Close();
                }
                m_ListSession.Clear();
            }

        }

        #endregion

        //------------------------------------------------------------

        #region start server
        public bool Start(int port)
        {
            if (isRunning)
            {
                MyLogger.LogWarning(LOG_TAG_MAIN, "Start()","cannot start duplicated Server！");
                return false;
            }
            MyLogger.Log(LOG_TAG_MAIN, "Start()  port = {0}", port.ToString());

            DelAllSession();

            try
            {
                mLogicLastTicks = DateTime.Now.Ticks;
                mRealTicksAtStart = mLogicLastTicks;

                //create Game Socket
                mGameSocket = new KCPSocket(0, 1);
                mGameSocket.AddReceiveListener(OnReceive);
                isRunning = true;

                //create game room
                mRoom = new FSPRoom();
                mRoom.Create();
                mRoomRPC = mRoom;

                //create  thread
                MyLogger.Log(LOG_TAG_MAIN, "Start()  create server thead");
                mThreadMain = new Thread(Thread_Main) { IsBackground = true };
                mThreadMain.Start();

            }
            catch (Exception e)
            {
                MyLogger.LogError(LOG_TAG_MAIN, "Start() " , e.Message);
                Close();
                return false;
            }

            //when user exit the game using stop button in UnityEditor, cannot release all resource in time
            //add listener here
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlayModeChanged;
            UnityEditor.EditorApplication.playmodeStateChanged += OnEditorPlayModeChanged;
#endif
            return true;
        }


#if UNITY_EDITOR
        private void OnEditorPlayModeChanged()
        {
            if (UnityEngine.Application.isPlaying == false)
            {
                UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlayModeChanged;
                Close();
            }
        }
#endif

        public void Close()
        {
            MyLogger.Log(LOG_TAG_MAIN, "Close()");

            isRunning = false;

            if (mGame != null)
            {
                mGame.Dispose();
                mGame = null;
            }

            if (mRoom != null)
            {
                mRoom.Dispose();
                mRoom = null;
                mRoomRPC = null;
            }

            if (mGameSocket != null)
            {
                mGameSocket.Dispose();
                mGameSocket = null;
            }

            if (mThreadMain != null)
            {
                mThreadMain.Interrupt();
                mThreadMain = null;
            }

            DelAllSession();
        }

        #endregion


        #region Game Logic

        public FSPGame StartGame()
        {
            if (mGame != null)
            {
                mGame.Dispose();
            }
            mGame = new FSPGame();
            mGame.Create(mParam);
            return mGame;
        }

        public void StopGame()
        {
            if (mGame != null)
            {
                mGame.Dispose();
                mGame = null;
            }
        }

        #endregion


        #region receive thread
        //------------------------------------------------------------

        private void OnReceive(byte[] buffer, int size, IPEndPoint remotePoint)
        {
            FSPDataC2S data = PBSerializer.NDeserialize<FSPDataC2S>(buffer);

            FSPSession session = GetSession(data.sid);
            if (session == null)
            {
                MyLogger.LogWarning(LOG_TAG_RECV, "DoReceive()","unknown SID = " + data.sid);
                //player does not exist,reply nothing
                return;
            }
            this.Log("DoReceive() Receive Buffer, SID={0}, IP={1}, Size={2}", session.Id, remotePoint, buffer.Length);

            session.EndPoint = remotePoint;
            session.Receive(data);
        }

        #endregion



        //------------------------------------------------------------
        #region Main thread
        private void Thread_Main()
        {
            MyLogger.Log(LOG_TAG_MAIN, "Thread_Main() Begin ......");

            while (isRunning)
            {
                try
                {
                    DoMainLoop();
                }
                catch (Exception e)
                {
                    MyLogger.LogError(LOG_TAG_MAIN, "Thread_Main() " , e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(10);
                }
            }

            MyLogger.Log(LOG_TAG_MAIN, "Thread_Main() End!");
        }


        //------------------------------------------------------------
        private void DoMainLoop()
        {
            long nowticks = DateTime.Now.Ticks;
            long interval = nowticks - mLogicLastTicks;

            if (interval > FRAME_TICK_INTERVAL)
            {
                mLogicLastTicks = nowticks - (nowticks % FRAME_TICK_INTERVAL);

                if (!mUseExternFrameTick)
                {
                    EnterFrame();
                }
            }
        }
        #endregion


        public void EnterFrame()
        {
            if (isRunning)
            {
                mGameSocket.Update();
                mRoomRPC.RPCTick();

                if (mGame != null)
                {
                    mGame.EnterFrame();
                }
            }
        }


    }


}