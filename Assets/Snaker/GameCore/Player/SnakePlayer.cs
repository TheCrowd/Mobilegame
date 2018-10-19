using System.Collections.Generic;
using SGF;
using Snaker.GameCore.Data;
using Snaker.GameCore.Player.Component;
using Snaker.GameCore.Entity.Snake;
using Snaker.GameCore.Entity.Factory;
using UnityEngine;
using SGF.Logger;

namespace Snaker.GameCore.Player
{
    public class SnakePlayer
    {
        private string LOG_TAG = "SnakePlayer";
        //======================================================================
        private PlayerData m_data = new PlayerData();
        private SnakeHead m_head;
        private SnakeTail m_tail;
        private GameObject m_container;
        private GameContext m_context;

        //----------------------------------------------------------------------
        /// <summary>
        /// As a player, some function needs to be implemented in a combined way, therefore component is needed
        /// Note:not all entities have components
        /// </summary>
        private List<PlayerComponent> m_listCompoent = new List<PlayerComponent>();

        //======================================================================
        public uint Id { get { return m_data.id; } }
        public PlayerData Data { get { return m_data; } }
        public SnakeData SnakeData { get { return m_data.snakeData; } }
        public SnakeNode Head { get { return m_head; } }
        public SnakeNode Tail { get { return m_tail; } }
        public GameObject Container { get { return m_container; } }
        public int TeamId { get { return m_data.teamId; } }
        public float HitDistance { get { return m_data.snakeData.size * m_data.snakeData.viewScale; } }
        //======================================================================

        /// <summary>
        /// Create SnakePlayer with the specified data and pos.
        /// </summary>
        /// <returns>void</returns>
        /// <param name="data">Data.</param>
        /// <param name="pos">Position.</param>
        public void Create(PlayerData data, Vector3 pos)
        {
            LOG_TAG = LOG_TAG + "[" + data.id + "]";

            m_data = data;
            m_context = GameManager.Instance.Context;

            //create gameobject for player
            m_container = new GameObject("SnakePlayer" + data.id);

            //create snake head
            m_head = EntityFactory.InstanceEntity<SnakeHead>();
            m_head.Create(0, m_data, m_container.transform);

            //create snake tail
            m_tail = EntityFactory.InstanceEntity<SnakeTail>();
            m_tail.Create(0, m_data, m_container.transform);

            //connect head and tail
            m_head.SetNext(m_tail);
            m_tail.SetPrev(m_head);

            //add default number of nodes to snake
            int initCount = m_data.snakeData.length;
            m_data.snakeData.length = 0;
            AddNodes(initCount);

            //create aiSnake
            if (m_data.ai > 0)
            {
                var ai = new AISnake(this);
                m_listCompoent.Add(ai);
            }

            //move to position where the snake is born
            MoveTo(pos);
        }


        /// <summary>
        /// Release this instance.
        /// </summary>
        public void Release()
        {
            //Release Component!
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].Release();
            }
            m_listCompoent.Clear();

            //release the snake, head and tail included
            SnakeNode node = m_head;
            while (node != null)
            {
                SnakeNode next = node.Next;
                EntityFactory.ReleaseEntity(node);
                node = next;
            }
            m_head = null;
            m_tail = null;

            if (m_container != null)
            {
                GameObject.Destroy(m_container);
                m_container = null;
            }

            m_context = null;
        }

        public void SetBodyVisible(bool value)
        {
            m_data.snakeData.bodyVisible = value;
        }


        //----------------------------------------------------------------------

        public void MoveTo(Vector3 pos)
        {
            m_head.MoveTo(pos);
        }

        internal void AddNodes(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                //apart from head and tail, the number for nodes starts from 1
                m_data.snakeData.length++;
                SnakeNode newNode = EntityFactory.InstanceEntity<SnakeNode>();
                newNode.Create(m_data.snakeData.length, m_data, m_container.transform);

                m_tail.Prev.SetNext(newNode);
                newNode.SetNext(m_tail);
                m_tail.SetPrev(newNode);
            }

            //calculate the view scale of snake
            float vs = m_data.snakeData.length / 500f;
            vs = Mathf.Min(vs, 1);
            vs = Mathf.Max(vs, 0.5f);
            m_data.snakeData.viewScale = vs;

            this.Log(LOG_TAG, "AddNodes() NewCount:{0}, SnakeLength:{1}, ViewSize:{2}", cnt, m_data.snakeData.length, vs);
        }

        public bool TryEatFood(EntityObject entity)
        {
            // TODO:a formula to determine how many foods consumed can make up a node for snake
            if (HitTest(entity, HitDistance))
            {
                AddNodes(m_data.snakeData.size / m_data.snakeData.keyStep);
                return true;
            }
            return false;
        }

        public bool TryHitEnemies(List<SnakePlayer> listPlayers)
        {
            for (int j = 0; j < listPlayers.Count; j++)
            {
                SnakePlayer other = listPlayers[j];
                if (this != other)
                {
                    if (HitTest(other, HitDistance))
                    {
                        Blast();
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryHitBound(GameContext context)
        {
            Rect rect = new Rect(0, 0, context.mapSize.x, context.mapSize.y);
            if (!rect.Contains(m_head.Position()))
            {
                Blast();
                return true;
            }
            return false;
        }

        public bool HitTest(SnakePlayer player, float testDistance)
        {
            SnakeNode node = player.Head;
            while (node != null)
            {
                if (node.IsKeyNode())
                {
                    float distance = Vector3.Distance(m_head.Position(), node.Position());
                    if (distance < testDistance)
                    {
                        return true;
                    }
                }

                node = node.Next;
            }

            return false;
        }

        public bool HitTest(EntityObject entity, float testDistance)
        {
            float distance = Vector3.Distance(m_head.Position(), entity.Position());
            if (distance < testDistance)
            {
                return true;
            }
            return false;
        }

        private void Blast()
        {
            //TODO: some explosion effects can be implemented here
            SnakeNode node = m_head;
            while (node != null)
            {
                if (node.IsKeyNode())
                {
                    Vector3 pos = GetRandomPosition(node.Position(), 8);

                    node.Blast();

                    if (GameManager.Instance.Context.random.Rnd() > 0.5)
                    {
                        GameManager.Instance.AddFood(pos, m_data.teamId);
                    }
                }

                node = node.Next;
            }
        }

        private Vector3 GetRandomPosition(Vector3 center, int r)
        {
            int dx = m_context.random.Range(-r, r);
            int dy = m_context.random.Range(-r, r);
            center.x += dx;
            center.y += dy;
            return center;

        }

        //==================================================================

        public void InputVKey(int vkey, float arg)
        {
            bool hasHandled = false;
            hasHandled = hasHandled || DoVKey_Move(vkey, arg);
        }


        public void EnterFrame(int frameIndex)
        {
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].EnterFrame(frameIndex);
            }

            HandleMove();
        }


        #region Move
        private Vector3 m_MoveDirection = new Vector3();
        private Vector3 m_InputMoveDirection = new Vector3();
        private float m_MoveSpeed = 1;
        public Vector3 MoveDirection { get { return m_MoveDirection; } }

        private bool DoVKey_Move(int vkey, float args)
        {
            switch (vkey)
            {
                case GameVKeys.MoveX:
                    m_InputMoveDirection.x = args;
                    break;
                case GameVKeys.MoveY:
                    m_InputMoveDirection.y = args;
                    break;
                case GameVKeys.SpeedUp:
                    m_MoveSpeed = args;
                    break;
                default:
                    return false;
            }

            return true;
        }

        private void HandleMove()
        {
            for (int i = 0; i < m_MoveSpeed; i++)
            {
                if (m_InputMoveDirection.magnitude > 0)
                {
                    m_MoveDirection = m_InputMoveDirection;
                }

                if (m_MoveDirection.magnitude > 0)
                {
                    Vector3 pos = m_head.Position() + m_MoveDirection.normalized * 2;
                    MoveTo(pos);
                }
            }

        }
        #endregion
    }
}

