using SGF.Time;
using SGF.Unity;
using Snaker.Module;
using Snaker.Module.PVE;
using Snaker.Service.Core;
using Snaker.Service.UIManager;
using Snaker.Service.UserManager;
using UnityEngine.UI;

namespace Snaker.PVE.UI
{
    public class UIPVEGamePage : UIPage
    {
        public Text txtUserInfo;
        public Button btnReady;
        public Text txtTimeInfo;

        private PVEGame m_game;

        /// <summary>
        /// when open PVE UI
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);

            //listen on game status
            PVEModule module = ModuleManager.Instance.GetModule(ModuleConst.PVEModule) as PVEModule;
            m_game = module.GetCurrentGame();
            m_game.onMainPlayerDie += OnMainPlayerDie;
            m_game.onGameEnd += OnGameEnd;

            txtUserInfo.text = UserManager.Instance.MainUserData.name;
            txtTimeInfo.text = "";

        }

        protected override void OnClose(object arg)
        {
            m_game = null;
            base.OnClose(arg);
        }


        /// <summary>
        /// after user pressed ready, a new player in the game would be created 
        /// </summary>
		public void OnBtnReady()
        {
            UIUtils.SetActive(btnReady, false);
            m_game.CreatePlayer();


        }

        /// <summary>
        /// Pause the game
        /// </summary>
		public void OnBtnPauseGame()
        {
            m_game.Pause();

            MsgBoxAPI.ShowMsgBox("Pause", "Exit this game？", "OK|Resume Game", (arg) =>
            {
                if ((int)arg == 0)
                {
                    m_game.Terminate();
                }
                else
                {
                    m_game.Resume();
                }
            });

        }

        /// <summary>
        /// when the main player is died
        /// </summary>
		private void OnMainPlayerDie()
        {
            m_game.Pause();

            MsgBoxAPI.ShowMsgBox("You died！！！", "reborn to play again?", "exit|continue", (arg) =>
            {
                if ((int)arg == 0)
                {
                    m_game.Terminate();
                }
                else
                {
                    m_game.Resume();
                    m_game.RebornPlayer();
                }
            });

        }


        /// <summary>
        /// end the game
        /// </summary>
		private void OnGameEnd()
        {
            m_game = null;

            MsgBoxAPI.ShowMsgBox("Game Over", "show game score...", "OK", (arg) =>
            {
                UIManager.Instance.GoBackPage();
            });

        }


        /// <summary>
        /// show game time
        /// </summary>
		void Update()
        {
            if (m_game != null)
            {
                int time;
                if (m_game.IsTimelimited)
                {
                    time = m_game.GetRemainTime();//second

                }
                else
                {
                    time = m_game.GetElapsedTime();
                }

                txtTimeInfo.text = TimeUtils.GetTimeString("%hh:%mm:%ss", time);
            }
        }

    }
}

