using System;
using System.Collections.Generic;
using SGF;
using SGF.Network.FSPLite;
using SGF.Network.FSPLite.Client;
using SGF.Unity;
using Snaker.Module.PVP.Data;
using UnityEngine;
using Snaker.GameCore.Data;
using Snaker.GameCore;
using SGF.Logger;
using Snaker.Service.UserManager;

namespace Snaker.Module.PVP
{

    /// <summary>
    ///  Game Logic
    /// </summary>
	public class PVPGame
    {
        public string LOG_TAG = "PVPGame";

        private FSPManager mgrFSP;
        private List<PlayerData> playerDataList;
        private uint mainPlayerId;

        public event Action onMainPlayerDie;
        public event Action onGameEnd;
        private GameContext m_context;

        //--------------------------------------------------

        /// <summary>
        /// Start Game
        /// </summary>
        /// <param name="param"></param>
        public void Start(PVPStartParam param)
        {
            MyLogger.Log(LOG_TAG, "StartGame() param:{0}", param.ToString());


            UserBean mainUserData = UserManager.Instance.MainUserData;
            playerDataList = param.players;
            for (int i = 0; i < playerDataList.Count; i++)
            {
                if (playerDataList[i].userId == mainUserData.id)
                {
                    mainPlayerId = playerDataList[i].id;
                    GameCamera.FocusPlayerId = mainPlayerId;
                }

                //register player data, this can provide player data for FSP
                //coz FSP is too easy to have player data
                GameManager.Instance.RegPlayerData(playerDataList[i]);
            }

            //start game
            GameManager.Instance.CreateGame(param.gameParam);
            GameManager.Instance.onPlayerDie += OnPlayerDie;//player died
            m_context = GameManager.Instance.Context;

            //start FSP 
            mgrFSP = new FSPManager();
            mgrFSP.Start(param.fspParam, mainPlayerId);
            mgrFSP.SetFrameListener(OnEnterFrame);
            mgrFSP.onGameBegin += OnGameBegin;//game start
            mgrFSP.onGameExit += OnGameExit;//player exit
            mgrFSP.onRoundEnd += OnRoundEnd;//player exit
            mgrFSP.onGameEnd += OnGameEnd;//game over


            //initial game input
            GameInput.Create();
            GameInput.OnVkey += OnVKey;

            //listen on EnterFrame
            MonoHelper.AddFixedUpdateListener(FixedUpdate);

            GameBegin();
        }


        /// <summary>
        /// stop the game 
        /// </summary>
		public void Stop()
        {
            MyLogger.Log(LOG_TAG, "StopGame()");

            GameManager.Instance.ReleaseGame();

            MonoHelper.RemoveFixedUpdateListener(FixedUpdate);
            GameInput.Release();

            if (mgrFSP != null)
            {
                mgrFSP.Stop();
                mgrFSP = null;
            }

            onMainPlayerDie = null;
            onGameEnd = null;
            m_context = null;
        }


        /// <summary>
        /// exit the game
        /// </summary>
		public void GameExit()
        {
            MyLogger.Log(LOG_TAG, "GameExit()");
            //because there might be other players in the game
            //so only the player self should exit the game
            mgrFSP.SendGameExit();
        }


        public void GameBegin()
        {
            MyLogger.Log(LOG_TAG, "GameBegin()");
            mgrFSP.SendGameBegin();
        }

        private void OnGameBegin(int arg)
        {
            MyLogger.Log(LOG_TAG, "OnGameBegin()");
            RoundBegin();
        }

        /// <summary>
        /// Round begin
        /// </summary>
		public void RoundBegin()
        {
            MyLogger.Log(LOG_TAG, "RoundBegin()");
            mgrFSP.SendRoundBegin();
        }

        /// <summary>
        /// round over
        /// </summary>
		public void RoundEnd()
        {
            MyLogger.Log(LOG_TAG, "RoundEnd()");
            mgrFSP.SendRoundEnd();
        }

        private void OnRoundEnd(int arg)
        {
            MyLogger.Log(LOG_TAG, "OnRoundEnd()");
            GameEnd();
        }

        public void GameEnd()
        {
            MyLogger.Log(LOG_TAG, "GameEnd()");
            mgrFSP.SendGameEnd();
        }


        /// <summary>
        /// Create a player
        /// </summary>
		public void CreatePlayer()
        {
            MyLogger.Log(LOG_TAG, "CreatePlayer()");
            mgrFSP.SendFSP(GameVKeys.CreatePlayer);
        }


        /// <summary>
        /// Reborn a player
        /// </summary>
		public void RebornPlayer()
        {
            MyLogger.Log(LOG_TAG, "RebornPlayer()");

            mgrFSP.SendFSP(GameVKeys.CreatePlayer);
        }


        //--------------------------------------------------
        /// <summary>
        /// input from GameInput
        /// </summary>
        /// <param name="vkey"></param>
        /// <param name="arg"></param>
        private void OnVKey(int vkey, float arg)
        {
            mgrFSP.SendFSP(vkey, (int)(arg * 10000));
        }

        /// <summary>
        /// game update
        /// </summary>
		private void FixedUpdate()
        {
            mgrFSP.EnterFrame();
        }

        private void OnEnterFrame(int frameId, FSPFrame frame)
        {
            GameManager.Instance.EnterFrame(frameId);

            if (frame != null && frame.vkeys != null)
            {
                for (int i = 0; i < frame.vkeys.Count; i++)
                {
                    FSPVKey cmd = frame.vkeys[i];
                    GameManager.Instance.InputVKey(cmd.vkey, ((float)cmd.args[0]) / 10000.0f, cmd.playerId);
                }
            }

            CheckTimeEnd();
        }

        /// <summary>
        /// check remaining time
        /// </summary>
		private void CheckTimeEnd()
        {
            if (IsTimelimited)
            {
                if (GetRemainTime() <= 0)
                {
                    RoundEnd();
                }
            }
        }


        /// <summary>
        /// is timelimited game mode
        /// </summary>
		public bool IsTimelimited
        {
            get
            {
                return m_context.param.mode == GameMode.TimeLimitedPVP;
            }
        }

        /// <summary>
        /// get remaining time for timelimited mode
        /// </summary>
        /// <returns></returns>
		public int GetRemainTime()
        {
            if (m_context.param.mode == GameMode.TimeLimitedPVP)
            {
                return (int)(m_context.param.limitedTime - m_context.currentFrameIndex * 0.033333333);
            }
            return 0;
        }

        /// <summary>
        /// how many time have passed
        /// </summary>
        /// <returns></returns>
		public int GetElapsedTime()
        {
            return (int)(m_context.currentFrameIndex * 0.033333333f);
        }


        //--------------------------------------------------

        /// <summary>
        /// player died
        /// </summary>
        /// <param name="playerId"></param>
        private void OnPlayerDie(uint playerId)
        {
            //when self died
            if (mainPlayerId == playerId)
            {
                if (onMainPlayerDie != null)
                {
                    onMainPlayerDie();
                }
                else
                {
                    this.LogError("OnPlayerDie() onMainPlayerDie == null!");
                }
            }
        }


        /// <summary>
        /// when some player exit
        /// </summary>
        /// <param name="playerId"></param>
        private void OnGameExit(uint playerId)
        {
            //when the exited player is yourself
            if (mainPlayerId == playerId)
            {
                if (onGameEnd != null)
                {
                    onGameEnd();
                }
            }


        }

        private void OnGameEnd(int arg)
        {
            if (onGameEnd != null)
            {
                onGameEnd();
            }
        }
    }
}
