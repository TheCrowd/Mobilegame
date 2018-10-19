using System;
using System.Collections.Generic;
using System.Net;
using SGF;
using SGF.Network;
using SGF.Network.FSPLite.Server;
using SGF.Network.FSPLite.Server.Data;
using SGF.Network.RPCLite;
using Snaker.GameCore.Data;
using Snaker.Module.PVP.Data;
using UnityEngine;
using SGF.Unity;
using Snaker.Service.UserManager;
using SGF.Network.Utils;
using SGF.ProtoBuf;
using SGF.Logger;
namespace Snaker.Module.PVP
{

    /// <summary>
    /// implement room logic
    /// using RPC to communicate with main room
    /// </summary>
    public class PVPRoom : RPCService
    {

        private IPEndPoint mRoomAddress;

        private uint mMinUserId;
        private string mMainUserName = "";

        private List<FSPPlayerData> playerInfoList = new List<FSPPlayerData>();

        private bool isInRoom = false;
        private bool isReady = false;
        private int pingValue = 0;

        public Action onJoin;
        public Action onExit;
        public Action<FSPRoomData> OnUpdateRoomInfo;
        public Action<PVPStartParam> onNotifyGameStart;
        public Action onNotifyGameResult;

        public PVPRoom() : base(0)
        {
        }

        public void Create()
        {
            mMinUserId = UserManager.Instance.MainUserData.id;
            mMainUserName = UserManager.Instance.MainUserData.name;
            MonoHelper.AddUpdateListener(OnUpdate);
        }

        public void Release()
        {
            MonoHelper.RemoveUpdateListener(OnUpdate);
            base.Dispose();
        }

        private void Reset()
        {
            isReady = false;
            isInRoom = false;
            playerInfoList.Clear();
        }

        private void OnUpdate()
        {
            this.RPCTick();
        }

        /// <summary>
        /// Join a room
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void JoinRoom(string ip, int port)
        {
            mRoomAddress = IPUtils.GetHostEndPoint(ip, port);

            PlayerData pd = new PlayerData();
            pd.userId = mMinUserId;
            pd.name = mMainUserName;
            byte[] customPlayerData = PBSerializer.NSerialize(pd);

            RPC(mRoomAddress, FSPRoom.RPC_JoinRoom, mMinUserId, mMainUserName, customPlayerData);
        }

        private void _RPC_OnJoinRoom(IPEndPoint target)
        {
            if (onJoin != null)
            {
                onJoin();
            }
        }


        /// <summary>
        /// exit a room
        /// </summary>
        public void ExitRoom()
        {
            RPC(mRoomAddress, FSPRoom.RPC_ExitRoom, mMinUserId);

            Reset();

            if (onExit != null)
            {
                onExit();
            }
        }



        /// <summary>
        /// get ready in a room
        /// </summary>
        public void RoomReady()
        {
            RPC(mRoomAddress, FSPRoom.RPC_RoomReady, mMinUserId);
        }


        /// <summary>
        /// cancel ready in a room
        /// </summary>
        public void CancelReady()
        {
            RPC(mRoomAddress, FSPRoom.RPC_CancelReady, mMinUserId);
        }



        /// <summary>
        /// PING calculate delay
        /// </summary>
        public void Ping()
        {
            int t = (int)(Time.realtimeSinceStartup * 1000);
            RPC(mRoomAddress, FSPRoom.RPC_Ping, t);
        }

        private void _RPC_OnPing(int pingArg, IPEndPoint target)
        {
            pingValue = (int)(Time.realtimeSinceStartup * 1000) - pingArg;
        }



        public bool IsReady { get { return isReady; } }
        public bool IsInRoom { get { return isInRoom; } }
        public int PingValue { get { return pingValue; } }
        public List<FSPPlayerData> players { get { return playerInfoList; } }


        /// <summary>
        /// update room information
        /// </summary>
        /// <param name="args"></param>
        /// <param name="targetAddress"></param>
        private void _RPC_UpdateRoomInfo(byte[] buff, IPEndPoint targetAddress)
        {
            var info = PBSerializer.NDeserialize<FSPRoomData>(buff);
            playerInfoList = info.players;

            isInRoom = false;
            isReady = false;

            for (int i = 0; i < playerInfoList.Count; ++i)
            {
                if (playerInfoList[i].userId == mMinUserId)
                {
                    isInRoom = true;
                    isReady = playerInfoList[i].isReady;
                }
            }

            this.Log("RPC_UpdateRoomInfo() Player Count: {0}", playerInfoList.Count);

            if (OnUpdateRoomInfo != null)
            {
                OnUpdateRoomInfo(info);
            }
        }


        /// <summary>
        /// notify game start
        /// </summary>
        /// <param name="args"></param>
        /// <param name="targetAddress"></param>
        private void _RPC_NotifyGameStart(byte[] buff, IPEndPoint targetAddress)
        {
            var info = PBSerializer.NDeserialize<FSPGameStartParam>(buff);

            PVPStartParam startParam = new PVPStartParam();

            for (int i = 0; i < info.players.Count; i++)
            {
                byte[] customPlayerData = info.players[i].customPlayerData;
                PlayerData pd = PBSerializer.NDeserialize<PlayerData>(customPlayerData);
                pd.id = info.players[i].id;
                pd.userId = info.players[i].userId;
                pd.name = info.players[i].name;
                pd.teamId = (int)info.players[i].id;
                startParam.players.Add(pd);
            }

            startParam.fspParam = info.fspParam;
            startParam.gameParam = PBSerializer.NDeserialize<GameParam>(info.customGameParam);

            this.Log("RPC_NotifyGameStart() param: \n{0}", startParam.ToString());

            if (onNotifyGameStart != null)
            {
                onNotifyGameStart(startParam);
            }
        }


        /// <summary>
        /// notify game ends
        /// </summary>
        /// <param name="args"></param>
        /// <param name="targetAddress"></param>
		private void _RPC_NotifyGameResult(IPEndPoint targetAddress, int reason)
        {
            this.Log("_RPC_NotifyGameResult() ");
            if (onNotifyGameResult != null)
            {
                onNotifyGameResult();
            }
        }
    }
}
