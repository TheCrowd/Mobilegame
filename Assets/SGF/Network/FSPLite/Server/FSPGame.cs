using Assets.SGF.Network.FSPLite;
using SGF.Logger;
using System;
using System.Collections.Generic;




namespace SGF.Network.FSPLite.Server
{
    public class FSPGame
    {

        //---------------------------------------------------------
        public string LOG_TAG = "FSPGame";

        //---------------------------------------------------------

        private FSPParam mFSPParam;

        /// <summary>
        /// Max player number in a game because int has only 31 bits to be flags 
        /// </summary>
        private const int MaxPlayerNum = 31;

        //game states
        private FSPGameState mState;
        private int mStateParam1;
        private int mStateParam2;

        public FSPGameState State { get { return mState; } }
        public int StateParam1 { get { return mStateParam1; } }
        public int StateParam2 { get { return mStateParam2; } }


        //---------------------------------------------------------
        //Player VKey flag
        private int mGameBeginFlag = 0;
        private int mRoundBeginFlag = 0;
        private int mControlStartFlag = 0;
        private int mRoundEndFlag = 0;
        private int mGameEndFlag = 0;


        //Round id
        private int mCurRoundId = 0;
        public int CurrentRoundId { get { return mCurRoundId; } }
        //---------------------------------------------------------
        // frame id
        private int mCurFrameId = 0;
        public int CurrentFrameId { get { return mCurFrameId; } }

        //current locked frame
        private FSPFrame mLockedFrame = new FSPFrame();

        //player list
        private List<FSPPlayer> mPlayerList = new List<FSPPlayer>();
        //player to be deleted
        private List<FSPPlayer> mPlayersExitOnNextFrameList = new List<FSPPlayer>();

        //one or more player exit the game
        public Action<uint> onGameExit;

        //game over
        public Action<int> onGameEnd;

        //=========================================================
        //dealy GC
        public static bool UseDelayGC = false;
        private List<object> mListObjectsForDelayGC = new List<object>();

        //---------------------------------------------------------
        public void Create(FSPParam param)
        {
            MyLogger.Log(LOG_TAG, "Create()");
            mFSPParam = param;
            mCurRoundId = 0;

            ClearRound();
            SetGameState(FSPGameState.Create);

        }


        public void Dispose()
        {
            SetGameState(FSPGameState.None);
            for (int i = 0; i < mPlayerList.Count; i++)
            {
                FSPPlayer player = mPlayerList[i];
                FSPServer.Instance.DelSession(player.Sid);
                player.Dispose();
            }
            mPlayerList.Clear();
            mListObjectsForDelayGC.Clear();
            GC.Collect();
            onGameExit = null;
            onGameEnd = null;

            MyLogger.Log(LOG_TAG, "Dispose()");
        }

        //---------------------------------------------------------
        public bool AddPlayer(uint playerId, uint sid)
        {
            MyLogger.Log(LOG_TAG, "AddPlayer() playerId:{0}, sid:{1}", playerId.ToString(), sid);

            if (mState != FSPGameState.Create)
            {
                MyLogger.LogError(LOG_TAG, "AddPlayer() cannot create player in current state! State = {0}", mState.ToString());
                return false;
            }

            FSPPlayer player = null;
            for (int i = 0; i < mPlayerList.Count; i++)
            {
                player = mPlayerList[i];
                if (player.Id == playerId)
                {
                    MyLogger.LogWarning(LOG_TAG, "AddPlayer()"," PlayerId used！replace with new id! PlayerId = " + playerId);
                    mPlayerList.RemoveAt(i);
                    FSPServer.Instance.DelSession(player.Sid);
                    player.Dispose();
                    break;
                }
            }

            if (mPlayerList.Count >= MaxPlayerNum)
            {
                MyLogger.LogError(LOG_TAG, "AddPlayer() maximum player reached! MaxPlayerNum = {0}", MaxPlayerNum.ToString());
                return false;
            }

            FSPSession session = FSPServer.Instance.AddSession(sid);
            player = new FSPPlayer(playerId, mFSPParam.serverTimeout, session, OnPlayerReceive);
            mPlayerList.Add(player);

            return true;
        }


        private FSPPlayer GetPlayer(uint playerId)
        {
            FSPPlayer player = null;
            for (int i = 0; i < mPlayerList.Count; i++)
            {
                player = mPlayerList[i];
                if (player.Id == playerId)
                {
                    return player;
                }
            }
            return null;
        }

        internal int GetPlayerCount()
        {
            return mPlayerList.Count;
        }

        public List<FSPPlayer> GetPlayerList()
        {
            return mPlayerList;
        }

        //---------------------------------------------------------
        //receive cmd from playter
        private void OnPlayerReceive(FSPPlayer player, FSPVKey cmd)
        {
            //delay GC
            if (UseDelayGC)
            {
                mListObjectsForDelayGC.Add(cmd);
            }

            HandleClientCmd(player, cmd);
        }


        //---------------------------------------------------------

        /// <summary>
        /// handle cmd from client
        /// handle system VKey
        /// collect operation Vkey
        /// </summary>
        /// <param name="player"></param>
        /// <param name="cmd"></param>
        protected virtual void HandleClientCmd(FSPPlayer player, FSPVKey cmd)
        {
            uint playerId = player.Id;

            //check auth
            if (!player.HasAuth)
            {
                MyLogger.Log(LOG_TAG, "HandleClientCmd() hasAuth = false! Wait AUTH!");
                if (cmd.vkey == FSPVKeyBase.AUTH)
                {
                    MyLogger.Log(LOG_TAG, "HandleClientCmd() AUTH, playerId={0}", playerId.ToString());
                    player.SetAuth(cmd.args[0]);
                }
                return;
            }


            switch (cmd.vkey)
            {
                case FSPVKeyBase.GAME_BEGIN:
                    {
                        MyLogger.Log(LOG_TAG, "HandleClientCmd() GAME_BEGIN, playerId = {0}, cmd = {1}", playerId.ToString(), cmd);
                        SetFlag(playerId, ref mGameBeginFlag, "mGameBeginFlag");
                        break;
                    }
                case FSPVKeyBase.ROUND_BEGIN:
                    {
                        MyLogger.Log(LOG_TAG, "HandleClientCmd() ROUND_BEGIN, playerId = {0}, cmd = {1}", playerId.ToString(), cmd);
                        SetFlag(playerId, ref mRoundBeginFlag, "mRoundBeginFlag");
                        break;
                    }
                case FSPVKeyBase.CONTROL_START:
                    {
                        MyLogger.Log(LOG_TAG, "HandleClientCmd() CONTROL_START, playerId = {0}, cmd = {1}", playerId.ToString(), cmd);
                        SetFlag(playerId, ref mControlStartFlag, "mControlStartFlag");
                        break;
                    }
                case FSPVKeyBase.ROUND_END:
                    {
                        MyLogger.Log(LOG_TAG, "HandleClientCmd() ROUND_END, playerId = {0}, cmd = {1}", playerId.ToString(), cmd);
                        SetFlag(playerId, ref mRoundEndFlag, "mRoundEndFlag");
                        break;
                    }
                case FSPVKeyBase.GAME_END:
                    {
                        MyLogger.Log(LOG_TAG, "HandleClientCmd() GAME_END, playerId = {0}, cmd = {1}", playerId.ToString(), cmd);
                        SetFlag(playerId, ref mGameEndFlag, "mGameEndFlag");
                        break;
                    }
                case FSPVKeyBase.GAME_EXIT:
                    {
                        MyLogger.Log(LOG_TAG, "HandleClientCmd() GAME_EXIT, playerId = {0}, cmd = {1}", playerId.ToString(), cmd);
                        HandleGameExit(playerId, cmd);
                        break;
                    }
                default:
                    {
                        MyLogger.Log(LOG_TAG, "HandleClientCmd() playerId = {0}, cmd = {1}", playerId.ToString(), cmd);
                        AddCmdToCurrentFrame(playerId, cmd);
                        break;
                    }
            }


        }



        protected void AddCmdToCurrentFrame(uint playerId, FSPVKey cmd)
        {
            cmd.playerId = playerId;
            mLockedFrame.vkeys.Add(cmd);
        }

        protected void AddCmdToCurrentFrame(int vkey, int arg = 0)
        {
            FSPVKey cmd = new FSPVKey();
            cmd.vkey = vkey;
            cmd.args = new int[] { arg };
            cmd.playerId = 0;
            AddCmdToCurrentFrame(0, cmd);
        }

        private void HandleGameExit(uint playerId, FSPVKey cmd)
        {
            AddCmdToCurrentFrame(playerId, cmd);
            FSPPlayer player = GetPlayer(playerId);

            if (player != null)
            {
                player.WaitForExit = true;

                if (onGameExit != null)
                {
                    onGameExit(player.Id);
                }
            }
        }




        //---------------------------------------------------------
        /// <summary>
        /// enter frame
        /// </summary>
        public void EnterFrame()
        {
            //delete exited players
            for (int i = 0; i < mPlayersExitOnNextFrameList.Count; i++)
            {
                FSPPlayer player = mPlayersExitOnNextFrameList[i];
                FSPServer.Instance.DelSession(player.Sid);
                player.Dispose();
            }
            mPlayersExitOnNextFrameList.Clear();

            //hand game state
            HandleGameState();

            //check if game state changes
            if (mState == FSPGameState.None)
            {
                return;
            }

            if (mLockedFrame.frameId != 0 || !mLockedFrame.IsEmpty())
            {
                //send frame to players
                for (int i = 0; i < mPlayerList.Count; i++)
                {
                    FSPPlayer player = mPlayerList[i];
                    player.SendToClient(mLockedFrame);
                    if (player.WaitForExit)
                    {
                        mPlayersExitOnNextFrameList.Add(player);
                        mPlayerList.RemoveAt(i);
                        --i;
                    }
                }
            }

            //clear frame 0 in each iteration
            if (mLockedFrame.frameId == 0)
            {
                mLockedFrame = new FSPFrame();
                //delay GC
                if (UseDelayGC)
                {
                    mListObjectsForDelayGC.Add(mLockedFrame);
                }
            }


            //add up frame id 
            if (mState == FSPGameState.RoundBegin || mState == FSPGameState.ControlStart)
            {
                mCurFrameId++;
                mLockedFrame = new FSPFrame();
                mLockedFrame.frameId = mCurFrameId;
                //防止GC
                if (UseDelayGC)
                {
                    mListObjectsForDelayGC.Add(mLockedFrame);
                }
            }
        }


        /// <summary>
        /// check if there are player exit abnormally
        /// </summary>
        private bool CheckGameAbnormalEnd()
        {
            //check current player,if less than 2 
            if (mPlayerList.Count < 2)
            {
                //enter GameEnd state
                SetGameState(FSPGameState.GameEnd, (int)FSPGameEndState.AllOtherExit);
                AddCmdToCurrentFrame(FSPVKeyBase.GAME_END, (int)FSPGameEndState.AllOtherExit);
                return true;
            }

            // check player connection status
            for (int i = 0; i < mPlayerList.Count; i++)
            {
                FSPPlayer player = mPlayerList[i];
                if (player.IsLose())
                {
                    mPlayerList.RemoveAt(i);
                    FSPServer.Instance.DelSession(player.Sid);
                    player.Dispose();
                    --i;
                }
            }

            //check again, if player num is less than 2
            if (mPlayerList.Count < 2)
            {
                //enter GameEnd state directly
                SetGameState(FSPGameState.GameEnd, (int)FSPGameEndState.AllOtherLost);
                AddCmdToCurrentFrame(FSPVKeyBase.GAME_END, (int)FSPGameEndState.AllOtherLost);
                return true;
            }

            return false;
        }


        //set game state machine
        protected void SetGameState(FSPGameState state, int param1 = 0, int param2 = 0)
        {
            MyLogger.Log(LOG_TAG, "SetGameState() >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            MyLogger.Log(LOG_TAG, "SetGameState() {0} -> {1}, param1 = {2}, param2 = {3}", mState.ToString(), state, param1, param2);
            MyLogger.Log(LOG_TAG, "SetGameState() <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");

            mState = state;
            mStateParam1 = param1;
            mStateParam2 = param2;
        }


        //---------------------------------------------------------
        //Tick handle game state
        private void HandleGameState()
        {
            switch (mState)
            {
                case FSPGameState.None:
                    {
                        //进入这个状态的游戏，马上将会被回收
                        //这里是否要考虑session中的所有消息都发完了？
                        break;
                    }
                case FSPGameState.Create: //游戏刚创建，未有任何玩家加入, 这个阶段等待玩家加入
                    {
                        OnState_Create();
                        break;
                    }
                case FSPGameState.GameBegin: //游戏开始，等待RoundBegin
                    {
                        OnState_GameBegin();
                        break;
                    }
                case FSPGameState.RoundBegin: //回合已经开始，开始加载资源等，等待ControlStart
                    {
                        OnState_RoundBegin();
                        break;
                    }
                case FSPGameState.ControlStart: //在这个阶段可操作，这时候接受游戏中的各种行为包，并等待RoundEnd
                    {
                        OnState_ControlStart();
                        break;
                    }
                case FSPGameState.RoundEnd: //回合已经结束，判断是否进行下一轮，即等待RoundBegin，或者GameEnd
                    {
                        OnState_RoundEnd();
                        break;
                    }
                case FSPGameState.GameEnd://游戏结束
                    {
                        OnState_GameEnd();
                        break;
                    }
                default:
                    break;
            }
        }


        //---------------------------------------------------------
        #region state handle functions


        /// <summary>
        /// 游戏创建状态
        /// 只有在该状态下，才允许加入玩家
        /// 当所有玩家都发VKey.GameBegin后，进入下一个状态
        /// </summary>
        protected virtual int OnState_Create()
        {
            //如果有任何一方已经鉴权完毕，则游戏进入GameBegin状态准备加载
            if (IsFlagFull(mGameBeginFlag))
            {
                ResetRoundFlag();
                SetGameState(FSPGameState.GameBegin);
                AddCmdToCurrentFrame(FSPVKeyBase.GAME_BEGIN);
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// 游戏开始状态
        /// 在该状态下，等待所有玩家发VKey.RoundBegin，或者 判断玩家是否掉线
        /// 当所有人都发送VKey.RoundBegin，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_GameBegin()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }

            if (IsFlagFull(mRoundBeginFlag))
            {
                SetGameState(FSPGameState.RoundBegin);
                IncRoundId();
                AddCmdToCurrentFrame(FSPVKeyBase.ROUND_BEGIN, mCurRoundId);

                return 0;
            }

            return 0;
        }

        /// <summary>
        /// 回合开始状态
        /// （这个时候客户端可能在加载资源）
        /// 在该状态下，等待所有玩家发VKey.ControlStart， 或者 判断玩家是否掉线
        /// 当所有人都发送VKey.ControlStart，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_RoundBegin()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }

            if (IsFlagFull(mControlStartFlag))
            {
                SetGameState(FSPGameState.ControlStart);
                AddCmdToCurrentFrame(FSPVKeyBase.CONTROL_START);
                return 0;
            }

            return 0;
        }


        /// <summary>
        /// 可以开始操作状态
        /// （因为每个回合可能都会有加载过程，不同的玩家加载速度可能不同，需要用一个状态统一一下）
        /// 在该状态下，接收玩家的业务VKey， 或者 VKey.RoundEnd，或者VKey.GameExit
        /// 当所有人都发送VKey.RoundEnd，进入下一个状态
        /// 当有玩家掉线，或者发送VKey.GameExit，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_ControlStart()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }

            if (IsFlagFull(mRoundEndFlag))
            {
                SetGameState(FSPGameState.RoundEnd);
                ClearRound();
                AddCmdToCurrentFrame(FSPVKeyBase.ROUND_END, mCurRoundId);
                return 0;
            }

            return 0;
        }


        /// <summary>
        /// 回合结束状态
        /// （大部分游戏只有1个回合，也有些游戏有多个回合，由客户端逻辑决定）
        /// 在该状态下，等待玩家发送VKey.GameEnd，或者 VKey.RoundBegin（如果游戏不只1个回合的话）
        /// 当所有人都发送VKey.GameEnd，或者 VKey.RoundBegin时，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_RoundEnd()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }


            //这是正常GameEnd
            if (IsFlagFull(mGameEndFlag))
            {
                SetGameState(FSPGameState.GameEnd, (int)FSPGameEndState.Normal);
                AddCmdToCurrentFrame(FSPVKeyBase.GAME_END, (int)FSPGameEndState.Normal);
                return 0;
            }


            if (IsFlagFull(mRoundBeginFlag))
            {
                SetGameState(FSPGameState.RoundBegin);
                IncRoundId();
                AddCmdToCurrentFrame(FSPVKeyBase.ROUND_BEGIN, mCurRoundId);
                return 0;
            }


            return 0;
        }


        protected virtual int OnState_GameEnd()
        {
            //到这里就等业务层去读取数据了 
            if (onGameEnd != null)
            {
                onGameEnd(mStateParam1);
                onGameEnd = null;
            }
            return 0;
        }

        public bool IsGameEnd()
        {
            return mState == FSPGameState.GameEnd;
        }

        #endregion


        //--------------------------------------------------------------------
        //Round处理函数
        private int ClearRound()
        {
            mLockedFrame = new FSPFrame();
            mCurFrameId = 0;

            ResetRoundFlag();

            for (int i = 0; i < mPlayerList.Count; i++)
            {
                if (mPlayerList[i] != null)
                {
                    mPlayerList[i].ClearRound();
                }
            }

            return 0;
        }

        private void ResetRoundFlag()
        {
            mRoundBeginFlag = 0;
            mControlStartFlag = 0;
            mRoundEndFlag = 0;
            mGameEndFlag = 0;
        }

        private void IncRoundId()
        {
            ++mCurRoundId;
        }




        //--------------------------------------------------------------------
        #region Player 状态标志工具函数

        private void SetFlag(uint playerId, ref int flag, string flagname)
        {
            flag |= (0x01 << ((int)playerId - 1));
            MyLogger.Log(LOG_TAG, "SetFlag() player = {0}, flag = {1}", playerId.ToString(), flagname);
        }

        private void ClsFlag(int playerId, ref int flag, string flagname)
        {
            flag &= (~(0x01 << (playerId - 1)));
        }

        public bool IsAnyFlagSet(int flag)
        {
            return flag != 0;
        }

        public bool IsFlagFull(int flag)
        {
            if (mPlayerList.Count > 1)
            {
                for (int i = 0; i < mPlayerList.Count; i++)
                {
                    FSPPlayer player = mPlayerList[i];
                    int playerId = (int)player.Id;
                    if ((flag & (0x01 << (playerId - 1))) == 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;

        }


        #endregion


    }
}

