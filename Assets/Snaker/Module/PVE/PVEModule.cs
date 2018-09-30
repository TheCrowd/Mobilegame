
using SGF.Unity;
using Snaker.GameCore.Data;
using Snaker.Module.PVE;
using Snaker.Service.Core;
using Snaker.Service.UIManager;

namespace Snaker.Module
{
    public class PVEModule : BusinessModule
    {
        private PVEGame m_game;

        //show the main UI of the module
        protected override void Show(object arg)
        {
            base.Show(arg);

            int model = (int)arg;

            //TODO show the UI for stage selection

            //start the game directly
            StartGame(model);
        }

        /// <summary>
        /// start the game
        /// </summary>
        /// <param name="mode"></param>
        private void StartGame(int mode)
        {
            GameParam param = new GameParam();
            param.mode = (GameMode)mode;
            param.limitedTime = 10;

            m_game = new PVEGame();
            m_game.Start(param);
            m_game.onGameEnd += () =>
            {
                StopGame();
            };

            //open the UI for PVE game
            UIManager.Instance.OpenPage(UIConst.UIPVEGamePage);
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
        }

        public PVEGame GetCurrentGame()
        {
            return m_game;
        }

    }
}
