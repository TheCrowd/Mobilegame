using SGF.Time;
using SGF.UI.Component;
using SGF.Unity;
using Snaker.GameCore;
using Snaker.GameCore.Data;
using Snaker.GameCore.Player;
using Snaker.Module;
using Snaker.Module.PVP;
using Snaker.Service.Core;
using Snaker.Service.UIManager;
using Snaker.Service.UserManager;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using SGF.Logger;

namespace Snaker.PVP.UI
{
    public class UIPVPGamePage : UIPage
    {
        public Text txtUserInfo;
        public Button btnReady;
        public Text txtTimeInfo;
        public Text txtScoreInfo;
        public UIList playerRankList;

        private PVPGame m_game;
        private string cur_score = "0";
        private uint m_mainPlayerId = 0;

        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);

            PVPModule module = ModuleManager.Instance.GetModule(ModuleConst.PVPModule) as PVPModule;
            m_game = module.GetGame();
            m_game.onMainPlayerDie += OnMainPlayerDie;
            m_game.onGameEnd += OnGameEnd;

            txtUserInfo.text = UserManager.Instance.MainUserData.name;
            txtTimeInfo.text = "";
            UpdatePlayerRank();

        }

        protected override void OnClose(object arg)
        {
            m_game = null;
            base.OnClose(arg);
        }



        public void OnBtnReady()
        {
            UIUtils.SetActive(btnReady, false);
            m_game.CreatePlayer();
        }



        private void OnMainPlayerDie()
        {
            MsgBoxAPI.ShowMsgBox("dead!!!", "continue the game？", "exit|continue", (arg) =>
            {
                if ((int)arg == 0)
                {
                    m_game.GameExit();
                }
                else
                {
                    m_game.RebornPlayer();
                }
            });

        }

        private void OnGameEnd()
        {
            m_game = null;

            MsgBoxAPI.ShowMsgBox("game over", "your score for this game is: " + cur_score, "OK", (arg) =>
            {
                UIManager.Instance.GoBackPage();
            });

        }



        void Update()
        {
            if (m_game != null)
            {
                int time = 0;
                if (m_game.IsTimelimited)
                {
                    time = m_game.GetRemainTime();//second

                }
                else
                {
                    time = m_game.GetElapsedTime();
                }

                txtTimeInfo.text = TimeUtils.GetTimeString("%hh:%mm:%ss", time);
                //when game ends, score is reset to 0, in this case do not update
                if (!GetPlayerScore().Equals("0"))
                {
                    txtScoreInfo.text = "score:" + GetPlayerScore();
                    cur_score = GetPlayerScore();
                }

                //update player list
                UpdatePlayerRank();
            }
        }

        public string GetPlayerScore()
        {
            string result = "0";
            List<SnakePlayer> playerList = GameManager.Instance.GetPlayerList();
            uint userID= UserManager.Instance.MainUserData.id;
            foreach (SnakePlayer tempPlayer in playerList)
            {
                if (tempPlayer.Data.userId == userID)
                {
                    m_mainPlayerId = tempPlayer.Id;
                }
            }
            SnakePlayer mainPlayer = GameManager.Instance.GetPlayer(m_mainPlayerId);
            if (mainPlayer != null)
            {
                result = mainPlayer.Data.Score.ToString();
            }
            return result;
        }

        private void UpdatePlayerRank()
        {
            List<SnakePlayer> playerList = GameManager.Instance.GetPlayerList();
            List<PlayerData> playerDataList = new List<PlayerData>();
            foreach (SnakePlayer tempPlayer in playerList)
            {
                PlayerData pd = new PlayerData();
                pd.id = tempPlayer.Data.id;
                pd.name = tempPlayer.Data.name;
                pd.score = tempPlayer.Data.score;
                pd.userId = tempPlayer.Data.userId;
                playerDataList.Add(pd);
            }

            List<PlayerData> SortedPDList = playerDataList.OrderByDescending(o => o.score).ToList();
            //update player list
            MyLogger.Log("player list:"+SortedPDList.Count.ToString());
            playerRankList.SetData(SortedPDList);
        }

    }
}

