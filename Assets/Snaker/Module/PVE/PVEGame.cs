using SGF.Logger;
using SGF.Unity;
using Snaker.GameCore;
using Snaker.GameCore.Data;
using Snaker.GameCore.Player;
using Snaker.Service;
using Snaker.Service.UserManager;
using System;
using UnityEngine;

namespace Snaker.Module.PVE
{
    public class PVEGame
    {
        private uint m_mainPlayerId = 1;
        private int m_frameIndex = 0;
        private bool m_pause = false;
        public event Action onMainPlayerDie;
        public event Action onGameEnd;
        private GameContext m_context;


        /// <summary>
        /// start the game
        /// </summary>
        /// <param name="param"></param>
		public void Start(GameParam param)
        {
            GameManager.Instance.CreateGame(param);
            GameManager.Instance.onPlayerDie += OnPlayerDie;
            m_context = GameManager.Instance.Context;

            PlayerData pd = new PlayerData();
            pd.id = m_mainPlayerId;
            pd.userId = UserManager.Instance.MainUserData.id;
            pd.snakeData.id = SkinManager.Instance.SkinType;
            pd.snakeData.length = 50;
            pd.ai = 0;
            GameManager.Instance.RegPlayerData(pd);

            //initial Game Input
            GameInput.Create();
            GameInput.OnVkey += OnVKey;


            //listen on EnterFrame
            MonoHelper.AddFixedUpdateListener(FixedUpdate);

            GameCamera.FocusPlayerId = m_mainPlayerId;
        }


        /// <summary>
        /// stop the game
        /// </summary>
		public void Stop()
        {
            MonoHelper.RemoveFixedUpdateListener(FixedUpdate);

            GameInput.Release();

            GameManager.Instance.ReleaseGame();

            onGameEnd = null;
            onMainPlayerDie = null;
            m_context = null;
        }

        /// <summary>
        /// pause
        /// </summary>
		public void Pause()
        {
            m_pause = true;
        }


        /// <summary>
        /// resume game
        /// </summary>
		public void Resume()
        {
            m_pause = false;
        }


        /// <summary>
        /// terminate
        /// </summary>
		public void Terminate()
        {
            Pause();

            if (onGameEnd != null)
            {
                onGameEnd();
            }
        }

        /// <summary>
        /// create a player
        /// </summary>
		public void CreatePlayer()
        {
            GameManager.Instance.InputVKey(GameVKeys.CreatePlayer, 0, m_mainPlayerId);

        }

        /// <summary>
        /// refresh a player
        /// </summary>
		public void RebornPlayer()
        {
            CreatePlayer();
        }


        //--------------------------------------------------
        /// <summary>
        /// input from GameInput
        /// </summary>
        /// <param name="vkey"></param>
        /// <param name="arg"></param>
        private void OnVKey(int vkey, float arg)
        {
            GameManager.Instance.InputVKey(vkey, arg, m_mainPlayerId);
        }

        /// <summary>
        /// game loop
        /// </summary>
		private void FixedUpdate()
        {
            if (m_pause)
            {
                return;
            }


            m_frameIndex++;

            GameManager.Instance.EnterFrame(m_frameIndex);

            CheckTimeEnd();

            CheckCameraZoom();
        }

        public void CheckCameraZoom()
        {
            SnakePlayer mainPlayer=GameManager.Instance.GetPlayer(1);
            if (mainPlayer != null)
            {
                float vs=mainPlayer.Data.snakeData.length / 250f;
                if (vs < 1) vs = 1;
                //zoom out camera if the snake becomes large
                if (Camera.main.orthographicSize < 720)
                    Camera.main.orthographicSize = vs * 240;
            }
        }

        /// <summary>
        /// if game reaches the time limit(limited mode)
        /// </summary>
		private void CheckTimeEnd()
        {
            if (IsTimelimited)
            {
                if (GetRemainTime() <= 0)
                {
                    Terminate();
                }
            }
        }

        /// <summary>
        /// if game mode is time limited
        /// </summary>
		public bool IsTimelimited
        {
            get
            {
                return m_context.param.mode == GameMode.TimeLimitedPVE;
            }
        }

        /// <summary>
        /// remaining time
        /// </summary>
        /// <returns></returns>
		public int GetRemainTime()
        {
            if (m_context.param.mode == GameMode.TimeLimitedPVE)
            {
                return (int)(m_context.param.limitedTime - m_context.currentFrameIndex * 0.033333333);
            }
            return 0;
        }


        /// <summary>
        /// Elapsed game time
        /// </summary>
        /// <returns></returns>
		public int GetElapsedTime()
        {
            return (int)(m_context.currentFrameIndex * 0.033333333f);
        }


        //--------------------------------------------------
        /// <summary>
        /// when a player dies
        /// </summary>
        /// <param name="playerId"></param>
        private void OnPlayerDie(uint playerId)
        {
            if (m_mainPlayerId == playerId)
            {
                Pause();

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


    }
}
