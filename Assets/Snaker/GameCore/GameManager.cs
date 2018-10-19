
using System.Collections.Generic;
using SGF;
using Snaker.GameCore.Data;
using Snaker.GameCore.Entity.Food;
using Snaker.GameCore.Entity.Factory;
using Snaker.GameCore.Player;
using Snaker.GameCore.Maps;
using UnityEngine;
using Snaker.Service.Core;
using SGF.Logger;
using SGF.Extension;

namespace Snaker.GameCore
{
    public class GameManager : ServiceModule<GameManager>
    {
        private string LOG_TAG = "GameManager";

        private bool isRunning;


        /// <summary>
        /// context for the game
        /// </summary>
        private GameContext context;

        /// <summary>
        /// map
        /// </summary>
        private GameMap gameMap;

        //players and foods
        private List<EntityObject> foodList = new List<EntityObject>();
        private List<SnakePlayer> playerList = new List<SnakePlayer>();
        private DictionaryExt<uint, PlayerData> mapPlayerData = new DictionaryExt<uint, PlayerData>();

        public event PlayerDieEvent onPlayerDie;

        //======================================================================
        public bool IsRunning { get { return isRunning; } }
        public GameContext Context { get { return context; } }
        public GameMode GameMode { get { return context.param.mode; } }
        //======================================================================

        public void Init()
        {

        }

        //======================================================================


        public void CreateGame(GameParam param)
        {
            if (isRunning)
            {
                MyLogger.LogError(LOG_TAG, "Create()","Game Is Runing Already!");
                return;
            }

            this.Log(LOG_TAG, "Create() param:{0}", param);

            //create game context to store global game status
            context = new GameContext();
            context.param = param;
            context.random.Seed = param.randSeed;
            context.currentFrameIndex = 0;


            //create map
            gameMap = new GameMap();
            gameMap.Load(param.mapData);
            context.mapSize = gameMap.Size;


            //initial entity factory
            EntityFactory.Init();
            ViewFactory.Init(gameMap.View.transform);

            //initial game camera
            GameCamera.Create();

            isRunning = true;
        }

        public void ReleaseGame()
        {
            if (!isRunning)
            {
                return;
            }

            isRunning = false;

            GameCamera.Release();

            for (int i = 0; i < playerList.Count; ++i)
            {
                playerList[i].Release();
            }
            playerList.Clear();

            foodList.Clear();

            ViewFactory.Release();
            EntityFactory.Release();

            if (gameMap != null)
            {
                gameMap.Unload();
                gameMap = null;
            }

            onPlayerDie = null;
        }


        //======================================================================

        public void InputVKey(int vkey, float arg, uint playerId)
        {
            if (playerId == 0)
            {
                //handle other VKey, global Vkey like GameExit, CreatePlayer
                HandleOtherVKey(vkey, arg, playerId);
            }
            else
            {
                SnakePlayer player = GetPlayer(playerId);
                if (player != null)
                {
                    player.InputVKey(vkey, arg);
                }
                else
                {
                    //handle other Vkey
                    HandleOtherVKey(vkey, arg, playerId);

                }
            }
        }

        private void HandleOtherVKey(int vkey, float arg, uint playerId)
        {
            //handle global Vkey
            bool hasHandled = false;
            hasHandled = hasHandled || DoVKey_CreatePlayer(vkey, arg, playerId);
            hasHandled = hasHandled || DoVKey_ReleasePlayer(vkey, arg, playerId);
        }


        //===========================================================================
        private bool DoVKey_CreatePlayer(int vkey, float arg, uint playerId)
        {
            if (vkey == GameVKeys.CreatePlayer)
            {
                CreatePlayer(playerId);
                return true;
            }

            return false;
        }

        private bool DoVKey_ReleasePlayer(int vkey, float arg, uint playerId)
        {
            //if (vkey == FSPVKeyBase.GAME_EXIT)
            //{
            //    ReleasePlayer(playerId);
            //    return true;
            //}

            return false;
        }

        //===========================================================================

        public void EnterFrame(int frameIndex)
        {
            if (!isRunning)
            {
                return;
            }

            if (frameIndex < 0)
            {
                context.currentFrameIndex++;
            }
            else
            {
                context.currentFrameIndex = frameIndex;
            }

            EntityFactory.ClearReleasedObjects();

            for (int i = 0; i < playerList.Count; ++i)
            {
                playerList[i].EnterFrame(frameIndex);
            }


            List<uint> listDiePlayerId = new List<uint>();

            for (int i = 0; i < playerList.Count; i++)
            {
                SnakePlayer player = playerList[i];

                if (player.TryHitBound(context))
                {
                    listDiePlayerId.Add(player.Id);
                    ReleasePlayerAt(i);
                    i--;

                    continue;
                }

                if (player.TryHitEnemies(playerList))
                {
                    listDiePlayerId.Add(player.Id);
                    ReleasePlayerAt(i);
                    i--;
                    continue;
                }

                for (int j = 0; j < foodList.Count; j++)
                {
                    EntityObject food = foodList[j];
                    if (player.TryEatFood(food))
                    {
                        RemoveFoodAt(j);
                        j--;
                    }
                }
            }

            if (gameMap != null)
            {
                gameMap.EnterFrame(frameIndex);
            }

            if (onPlayerDie != null)
            {
                for (int i = 0; i < listDiePlayerId.Count; ++i)
                {
                    onPlayerDie(listDiePlayerId[i]);
                }
            }

        }

        //==================================================================

        public void RegPlayerData(PlayerData data)
        {
            mapPlayerData[data.id] = data;
            //mapPlayerData.Add(data.id, data);
        }

        //==================================================================

        internal void CreatePlayer(uint playerId)
        {
            PlayerData data = mapPlayerData[playerId];

            SnakePlayer player = new SnakePlayer();

            Vector3 mapSize = context.mapSize;
            Vector3 posMax = mapSize * 0.7f;
            Vector3 posMin = mapSize - posMax;
            Vector3 pos = new Vector3();
            pos.x = context.random.Range(posMin.x, posMax.x);
            pos.y = context.random.Range(posMin.y, posMax.y);
            pos.z = context.random.Range(posMin.z, posMax.z);
            pos.z = 0;

            player.Create(data, pos);
            playerList.Add(player);
        }


        private void ReleasePlayer(uint playerId)
        {
            int index = GetPlayerIndex(playerId);
            ReleasePlayerAt(index);
        }

        private void ReleasePlayerAt(int index)
        {
            if (index >= 0)
            {
                SnakePlayer player = playerList[index];
                playerList.RemoveAt(index);

                player.Release();
            }
        }

        internal SnakePlayer GetPlayer(uint playerId)
        {
            int index = GetPlayerIndex(playerId);
            if (index >= 0)
            {
                return playerList[index];
            }
            return null;
        }

        private int GetPlayerIndex(uint playerId)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].Id == playerId)
                {
                    return i;
                }
            }
            return -1;
        }

        internal List<SnakePlayer> GetPlayerList()
        {
            return playerList;
        }

        //==================================================================

        internal void AddFoodRandom()
        {
            Vector3 pos;
            pos.x = context.random.Range(0, context.mapSize.x);
            pos.y = context.random.Range(0, context.mapSize.y);
            pos.z = context.random.Range(0, context.mapSize.z);

            pos.z = 0;
            int color = context.random.Range(1, mapPlayerData.Count);
            AddFood(pos, color);
        }

        internal void AddFood(Vector3 pos, int color)
        {
            NormalFood food = EntityFactory.InstanceEntity<NormalFood>();
            food.Create(0, color, pos);

            foodList.Add(food);
        }

        private void RemoveFoodAt(int i)
        {
            EntityObject food = foodList[i];
            foodList.RemoveAt(i);
            EntityFactory.ReleaseEntity(food);
        }

        public List<EntityObject> GetFoodList()
        {
            return foodList;
        }



    }
}
