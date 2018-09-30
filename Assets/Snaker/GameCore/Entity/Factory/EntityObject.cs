using System;
using UnityEngine;
namespace Snaker.GameCore.Entity.Factory
{
    public abstract class EntityObject : IRecyclableObject
    {
        //----------------------------------------------------------------------
        private bool m_isReleased = false;
        public bool IsReleased { get { return m_isReleased; } }


        internal void InstanceInFactory()
        {
            m_isReleased = false;
        }

        //----------------------------------------------------------------------
        internal void ReleaseInFactory()
        {
            if (!m_isReleased)
            {
                Release();
                m_isReleased = true;
            }
        }

        protected abstract void Release();


        //----------------------------------------------------------------------
        public virtual Vector3 Position()
        {
            return Vector3.zero;
        }


        //----------------------------------------------------------------------

        public string GetRecycleType()
        {
            return this.GetType().FullName;
        }

        public void Dispose()
        {
            //system GC would take care of this
            //we need to Do Nothing!
        }
        //----------------------------------------------------------------------
    }
}

