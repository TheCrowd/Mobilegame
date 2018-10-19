using Snaker.Module;
using Snaker.Module.PVP;
using UnityEngine.UI;
using SGF.UI.Component;
using SGF.Network.FSPLite.Server.Data;
using System.Collections.Generic;
using Snaker.Service.UIManager;
using Snaker.Service.Core;
using SGF.Unity;
using SGF.Logger;
using SGF.Random;

namespace Snaker.UI.PVP
{
    public class UIPVPRoomPage : UIPage
    {
        public Button btnJoinRoom;
        public Button btnRoomReady;
        public UIList ctlRoomPlayerList;

        private PVPRoom m_room;

        private PVPRoom GetRoom()
        {
            PVPModule module = ModuleManager.Instance.GetModule(ModuleConst.PVPModule) as PVPModule;

            return module.GetRoom();
        }


        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            m_room = GetRoom();

            m_room.OnUpdateRoomInfo += OnRoomUpdate;
            ctlRoomPlayerList.SetData(m_room.players);

            ModuleManager.Instance.Event(ModuleConst.HostModule, "onStartServer").AddListener(OnStartServer);
            ModuleManager.Instance.Event(ModuleConst.HostModule, "onCloseServer").AddListener(OnCloseServer);
        }

        protected override void OnClose(object arg = null)
        {
            ModuleManager.Instance.Event(ModuleConst.HostModule, "onStartServer").RemoveListener(OnStartServer);
            ModuleManager.Instance.Event(ModuleConst.HostModule, "onCloseServer").RemoveListener(OnCloseServer);

            if (m_room != null)
            {
                m_room.OnUpdateRoomInfo -= OnRoomUpdate;
                m_room = null;
            }

            base.OnClose(arg);
        }

        private void OnStartServer(object arg)
        {
            string ipport = arg as string;
            string[] tmps = ipport.Split(':');
            string ip = tmps[0];
            int port = int.Parse(tmps[1]);
            m_room.JoinRoom(ip, port);
        }

        private void OnCloseServer(object arg)
        {
            m_room.ExitRoom();
        }


        public void OnBtnJoinRoom()
        {
            if (UIUtils.GetButtonText(btnJoinRoom) == "Join a room")
            {
                UIWindow wnd = UIManager.Instance.OpenWindow(UIConst.UIRoomFindWin);
                wnd.onClose += (arg) =>
                {
                    string str = arg as string;
                    if (string.IsNullOrEmpty(str))
                    {
                        return;
                    }

                    string[] tmps = str.Split(':');
                    if (tmps.Length != 2)
                    {
                        this.LogWarning("OnBtnJoinRoom() RoomIPPort format error！");
                        return;
                    }

                    string ip = tmps[0];
                    int port = int.Parse(tmps[1]);
                    m_room.JoinRoom(ip, port);

                };


            }
            else
            {
                m_room.ExitRoom();
            }

        }



        public void OnBtnRoomReady()
        {
            if (UIUtils.GetButtonText(btnRoomReady) == "Get ready")
            {
                m_room.RoomReady();
            }
            else
            {
                m_room.CancelReady();
            }
        }

        public void OnBtnShowHost()
        {
            ModuleManager.Instance.ShowModule(ModuleConst.HostModule);
        }

        public void OnBtnGoBack()
        {
            PVPModule module = ModuleManager.Instance.GetModule(ModuleConst.PVPModule) as PVPModule;

            if (m_room.IsInRoom)
            {
                m_room.ExitRoom();
            }

            module.CloseRoom();

        }



        /// <summary>
        /// Raises the button test event.
        /// </summary>
        public void OnBtnTest()
        {
            List<FSPPlayerData> list = new List<FSPPlayerData>();
            for (int i = 0; i < 20; ++i)
            {
                FSPPlayerData data = new FSPPlayerData();
                data.name = "name";
                data.id = (uint)i + 1;
                data.userId = (uint)RandomGen.Default.Range(100000, 999999);
                data.sid = data.id;
                data.isReady = RandomGen.Default.Rnd() > 0.5f;
                list.Add(data);
            }

            ctlRoomPlayerList.SetData(list);
        }


        private void OnRoomUpdate(FSPRoomData data)
        {
            ctlRoomPlayerList.SetData(data.players);
        }

        void Update()
        {
            m_room = GetRoom();
            if (m_room != null)
            {
                UIUtils.SetButtonText(btnJoinRoom, m_room.IsInRoom ? "Exit the room" : "Join a room");
                UIUtils.SetButtonText(btnRoomReady, m_room.IsReady ? "Cancel ready" : "Get ready");

                UIUtils.SetActive(btnJoinRoom, !m_room.IsReady);
                UIUtils.SetActive(btnRoomReady, m_room.IsInRoom);
            }
        }
    }
}