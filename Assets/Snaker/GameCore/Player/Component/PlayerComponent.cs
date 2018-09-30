using System;
using Snaker.GameCore.Entity.Factory;

namespace Snaker.GameCore.Player.Component
{
    public abstract class PlayerComponent
    {
        public PlayerComponent(SnakePlayer player)
        {
        }

        public abstract void Release();

        public abstract void EnterFrame(int frameIndex);
    }
}
