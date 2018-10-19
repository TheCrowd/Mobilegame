using Assets.SGF.Network.FSPLite;
using SGF.Extension;
using SGF.Logger;
using System;
using System.Collections.Generic;



namespace SGF.Network.FSPLite.Client
{
    public class FSPManager
    {
        public string LOG_TAG = "FSPManager";

        private FSPParam mParam;
        private FSPClient mClient;
        private FSPFrameController mFrameCtrl;
        private DictionaryExt<int, FSPFrame> mFrameBuffer;
        private bool mIsRunning = false;

        private FSPGameState mGameState = FSPGameState.None;
        public FSPGameState GameState { get { return mGameState; } }

        private int mCurrentFrameIndex = 0;
        private int mClientLockedFrame = 0;
        private Action<int, FSPFrame> mFrameListener;

        //local info
        private uint mMinePlayerId = 0;
        private FSPFrame mNextLocalFrame;

        public event Action<int> onGameBegin;
        public event Action<int> onRoundBegin;
        public event Action<int> onControlStart;
        public event Action<int> onRoundEnd;
        public event Action<int> onGameEnd;
        public event Action<uint> onGameExit;


        public void Start(FSPParam param, uint playerId)
        {
            mParam = param;
            mMinePlayerId = playerId;
            LOG_TAG = "FSPManager[" + playerId + "]";

            if (!mParam.useLocal)
            {
                mClientLockedFrame = mParam.clientFrameRateMultiple - 1;

                mClient = new FSPClient();
                mClient.SetSessionId((ushort)param.sid);
                mClient.SetFSPAuthInfo(param.authId);
                mClient.Connect(param.host, param.port);
                mClient.SetFSPListener(OnFSPListener);
                mClient.VerifyAuth();
            }
            else
            {
                mClientLockedFrame = param.maxFrameId;
            }

            mFrameCtrl = new FSPFrameController();
            mFrameCtrl.Start(param);

            mFrameBuffer = new DictionaryExt<int, FSPFrame>();

            mIsRunning = true;
            mGameState = FSPGameState.Create;
            mCurrentFrameIndex = 0;

        }

        public void Stop()
        {
            mGameState = FSPGameState.None;

            if (mClient != null)
            {
                mClient.Close();
                mClient = null;
            }

            if (mFrameCtrl != null)
            {
                mFrameCtrl.Close();
                mFrameCtrl = null;
            }

            mFrameListener = null;
            mFrameBuffer.Clear();
            mIsRunning = false;

            onGameBegin = null;
            onRoundBegin = null;
            onControlStart = null;
            onGameEnd = null;
            onRoundEnd = null;
        }





        /// <summary>
        /// 设置帧数据的监听
        /// </summary>
        /// <param name="listener"></param>
        public void SetFrameListener(Action<int, FSPFrame> listener)
        {
            mFrameListener = listener;
        }

        /// <summary>
        /// Listen on FSP frame data
        /// </summary>
        /// <param name="frame"></param>
        private void OnFSPListener(FSPFrame frame)
        {
            AddServerFrame(frame);
        }

        /// <summary>
        /// invoked by external 
        /// </summary>
        public void EnterFrame()
        {
            if (!mIsRunning)
            {
                return;
            }

            if (!mParam.useLocal)
            {
                mClient.EnterFrame();

                int speed = mFrameCtrl.GetFrameSpeed(mCurrentFrameIndex);
                while (speed > 0)
                {
                    if (mCurrentFrameIndex < mClientLockedFrame)
                    {
                        mCurrentFrameIndex++;
                        FSPFrame frame = mFrameBuffer[mCurrentFrameIndex];
                        ExecuteFrame(mCurrentFrameIndex, frame);

                    }

                    speed--;
                }

                //add pre effect here
            }
            else
            {
                if (mClientLockedFrame == 0 || mCurrentFrameIndex < mClientLockedFrame)
                {
                    mCurrentFrameIndex++;
                    FSPFrame frame = mFrameBuffer[mCurrentFrameIndex];
                    ExecuteFrame(mCurrentFrameIndex, frame);

                }
            }
        }


        /// <summary>
        /// execute a frame
        /// </summary>
        /// <param name="frameId"></param>
        /// <param name="frame"></param>
        private void ExecuteFrame(int frameId, FSPFrame frame)
        {

            //handle game state Vkey first
            if (frame != null && frame.vkeys != null)
            {
                for (int i = 0; i < frame.vkeys.Count; i++)
                {
                    FSPVKey cmd = frame.vkeys[i];

                    if (cmd.vkey == FSPVKeyBase.GAME_BEGIN)
                    {
                        Handle_GameBegin(cmd.args[0]);
                    }
                    else if (cmd.vkey == FSPVKeyBase.ROUND_BEGIN)
                    {
                        Handle_RoundBegin(cmd.args[0]);
                    }
                    else if (cmd.vkey == FSPVKeyBase.CONTROL_START)
                    {
                        Handle_ControlStart(cmd.args[0]);
                    }
                    else if (cmd.vkey == FSPVKeyBase.ROUND_END)
                    {
                        Handle_RoundEnd(cmd.args[0]);
                    }
                    else if (cmd.vkey == FSPVKeyBase.GAME_END)
                    {
                        Handle_GameEnd(cmd.args[0]);
                    }
                    else if (cmd.vkey == FSPVKeyBase.GAME_EXIT)
                    {
                        Handle_GameExit(cmd.playerId);
                    }
                }
            }

            //other Virtual key can be handled by game manager
            if (mFrameListener != null)
            {
                mFrameListener(frameId, frame);
            }
        }



        #region handle GameState 

        public void SendGameBegin()
        {
            SendFSP(FSPVKeyBase.GAME_BEGIN, 0);
        }

        private void Handle_GameBegin(int arg)
        {
            mGameState = FSPGameState.GameBegin;
            if (onGameBegin != null)
            {
                onGameBegin(arg);
            }
        }

        public void SendRoundBegin()
        {
            SendFSP(FSPVKeyBase.ROUND_BEGIN, 0);
        }

        private void Handle_RoundBegin(int arg)
        {
            mGameState = FSPGameState.RoundBegin;
            mCurrentFrameIndex = 0;

            if (!mParam.useLocal)
            {
                mClientLockedFrame = mParam.clientFrameRateMultiple - 1;
            }
            else
            {
                mClientLockedFrame = mParam.maxFrameId;
            }

            mFrameBuffer.Clear();

            if (onRoundBegin != null)
            {
                onRoundBegin(arg);
            }
        }

        public void SendControlStart()
        {
            SendFSP(FSPVKeyBase.CONTROL_START, 0);
        }
        private void Handle_ControlStart(int arg)
        {
            mGameState = FSPGameState.ControlStart;
            if (onControlStart != null)
            {
                onControlStart(arg);
            }
        }

        public void SendRoundEnd()
        {
            SendFSP(FSPVKeyBase.ROUND_END, 0);
        }
        private void Handle_RoundEnd(int arg)
        {
            mGameState = FSPGameState.RoundEnd;
            if (onRoundEnd != null)
            {
                onRoundEnd(arg);
            }
        }

        public void SendGameEnd()
        {
            SendFSP(FSPVKeyBase.GAME_END, 0);
        }
        private void Handle_GameEnd(int arg)
        {
            mGameState = FSPGameState.GameEnd;
            if (onGameEnd != null)
            {
                onGameEnd(arg);
            }
        }


        public void SendGameExit()
        {
            SendFSP(FSPVKeyBase.GAME_EXIT, 0);
        }

        private void Handle_GameExit(uint playerId)
        {
            if (onGameExit != null)
            {
                onGameExit(playerId);
            }
        }


        #endregion




        private void AddServerFrame(FSPFrame frame)
        {


            if (frame.frameId <= 0)
            {
                ExecuteFrame(frame.frameId, frame);
                return;
            }

            frame.frameId = frame.frameId * mParam.clientFrameRateMultiple;
            mClientLockedFrame = frame.frameId + mParam.clientFrameRateMultiple - 1;

            mFrameBuffer.Add(frame.frameId, frame);
            mFrameCtrl.AddFrameId(frame.frameId);
        }



        /// <summary>
        /// send FSP VKey to external
        /// </summary>
        /// <param name="vkey"></param>
        /// <param name="arg"></param>
        public void SendFSP(int vkey, int arg = 0)
        {
            if (!mIsRunning)
            {
                return;
            }

            if (!mParam.useLocal)
            {
                mClient.SendFSP(vkey, arg, mCurrentFrameIndex);
            }
            else
            {
                SendFSPLocal(vkey, arg);
            }
        }



        /// <summary>
        /// to be compatible with local, like PVE mode can also use Frame Synchronization Protocol 
        /// </summary>
        /// <param name="vkey"></param>
        /// <param name="arg"></param>
        private void SendFSPLocal(int vkey, int arg = 0)
        {
            MyLogger.Log(LOG_TAG, "SendFSPLocal() vkey={0}, arg={1}", vkey.ToString(), arg);
            int nextFrameId = mCurrentFrameIndex + 1;
            if (mNextLocalFrame == null || mNextLocalFrame.frameId != nextFrameId)
            {
                mNextLocalFrame = new FSPFrame();
                mNextLocalFrame.frameId = nextFrameId;
                mNextLocalFrame.vkeys = new List<FSPVKey>();

                mFrameBuffer.Add(mNextLocalFrame.frameId, mNextLocalFrame);
            }

            FSPVKey cmd = new FSPVKey();
            cmd.vkey = vkey;
            cmd.args = new int[] { arg };
            cmd.playerId = mMinePlayerId;

            mNextLocalFrame.vkeys.Add(cmd);


        }


        //======================================================================

        public string ToDebugString()
        {
            string str = "";
            if (mFrameCtrl != null)
            {
                str += ("NewestFrameId:" + mFrameCtrl.NewestFrameId) + "; ";
                str += ("PlayedFrameId:" + mCurrentFrameIndex) + "; ";
                str += ("IsInBuffing:" + mFrameCtrl.IsInBuffing) + "; ";
                str += ("IsInSpeedUp:" + mFrameCtrl.IsInSpeedUp) + "; ";
                str += ("FrameBufferSize:" + mFrameCtrl.FrameBufferSize) + "; ";
            }

            if (mClient != null)
            {
                str += mClient.ToDebugString();
            }

            return str;
        }
    }
}
