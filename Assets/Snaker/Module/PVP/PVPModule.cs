using Snaker.GameCore.Data;
using SGF.Network.FSPLite;
using Snaker.Module.PVP;
using Snaker.Module.PVP.Data;
using Snaker.Service.Core;
using Snaker.Service.UIManager;
using SGF.Unity;
using Snaker.Service.UserManager;

namespace Snaker.Module
{
    public class PVPModule : BusinessModule
    {
        private PVPGame m_game;
        private PVPRoom m_room;


        /// <summary>
        /// show PVP main view
        /// </summary>
        /// <param name="arg"></param>
	    protected override void Show(object arg)
        {
            base.Show(arg);

            //open a room
            OpenRoom();
        }


        /// <summary>
        /// open a room
        /// </summary>
	    private void OpenRoom()
        {
            //create PVP room
            m_room = new PVPRoom();
            m_room.Create();

            //notify game starts
            m_room.onNotifyGameStart += (param) =>
            {
                StartGame(param);
            };

            //show room UI
            UIManager.Instance.OpenPage(UIConst.UIPVPRoomPage);
        }

        /// <summary>
        /// close the room
        /// </summary>
	    public void CloseRoom()
        {
            if (m_room != null)
            {
                m_room.Release();
                m_room = null;
            }

            //back to last UI
            UIManager.Instance.GoBackPage();
        }

        public PVPRoom GetRoom()
        {
            return m_room;
        }




        //----------------------------------------------------------------------
        /// <summary>
        /// start the game
        /// </summary>
        /// <param name="param"></param>
        private void StartGame(PVPStartParam param)
        {
            //create PVP game
            m_game = new PVPGame();
            m_game.Start(param);

            //when game ends
            m_game.onGameEnd += () =>
            {
                StopGame();
            };

            //show PVP game UI
            UIManager.Instance.OpenPage(UIConst.UIPVPGamePage);
        }

        /// <summary>
        /// stop the game
        /// </summary>
        private void StopGame()
        {
            if (m_game != null)
            {
                m_game.Stop();
                m_game = null;
            }

            if (m_room != null)
            {
                m_room.CancelReady();
            }

            ModuleManager.Instance.SendMessage(ModuleConst.HostModule, "ReStart");
        }

        public PVPGame GetGame()
        {
            return m_game;
        }


        /// <summary>
        /// for local test
        /// </summary>
        public void StartLocalTest()
        {
            //game params
            GameParam gameParam = new GameParam();
            gameParam.mapData.id = 0;
            gameParam.mode = GameMode.UnlimitedPVP;

            //FSP params
            FSPParam fspParam = new FSPParam();
            fspParam.useLocal = true;
            fspParam.sid = 1;


            //player params
            PlayerData playerData = new PlayerData();
            playerData.userId = UserManager.Instance.MainUserData.id;
            playerData.id = 1;

            PVPStartParam param = new PVPStartParam();
            param.fspParam = fspParam;
            param.gameParam = gameParam;
            param.players.Add(playerData);

            StartGame(param);
        }


    }
}