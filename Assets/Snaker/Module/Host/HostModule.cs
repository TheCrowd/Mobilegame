using SGF.Network.FSPLite;
using SGF.Network.FSPLite.Server;
using SGF.ProtoBuf;
using SGF.Unity;
using Snaker.GameCore.Data;
using Snaker.Service.Core;
using Snaker.Service.UIManager;

namespace Snaker.Module
{
    class HostModule : BusinessModule
    {
        private ModuleEvent onStartServer;
        private ModuleEvent onCloseServer;

        public override void Create(object args)
        {
            base.Create(args);
            onStartServer = Event("onStartServer");
            onCloseServer = Event("onCloseServer");
        }


        protected override void Show(object arg)
        {
            //pop up host window
            UIManager.Instance.OpenWindow(UIConst.UIHostWin);
        }

        /// <summary>
        /// start server
        /// </summary>
        public void StartServer()
        {
            FSPServer.Instance.Start(0);

            //customized game params
            //for example game map ID, random seed, game mode etc.
            GameParam gameParam = new GameParam();
            byte[] customGameParam = PBSerializer.NSerialize(gameParam);

            //send customized params to game room
            //those params would send to players when game starts
            FSPServer.Instance.Room.SetCustomGameParam(customGameParam);
            FSPServer.Instance.SetServerTimeout(0);

            string ipport = GetRoomIP() + ":" + GetRoomPort();
            onStartServer.Invoke(ipport);
        }

        /// <summary>
        /// close server
        /// </summary>
        public void CloseServer()
        {
            FSPServer.Instance.Close();
            onCloseServer.Invoke(null);
        }



        /// <summary>
        /// Room IP
        /// </summary>
        /// <returns></returns>
        public string GetRoomIP()
        {
            return FSPServer.Instance.RoomIP;
        }

        /// <summary>
        /// Room Port
        /// </summary>
        /// <returns></returns>
        public int GetRoomPort()
        {
            return FSPServer.Instance.RoomPort;
        }

        /// <summary>
        /// Frame sync params
        /// </summary>
        /// <returns></returns>
        public FSPParam GetFSPParam()
        {
            return FSPServer.Instance.GetParam();
        }

    }
}
