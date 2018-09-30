using System;
using UnityEngine;


namespace Snaker.GameCore.Entity.Factory
{
    public abstract class ViewObject : MonoBehaviour, IRecyclableObject
    {
        //----------------------------------------------------------------------
        private string m_recycleType;


        //----------------------------------------------------------------------
        /// <summary>
        /// Create functions n factory.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="recycleType">some types are resource name while some are class name.</param>
        internal void CreateInFactory(EntityObject entity, string recycleType)
        {
            m_recycleType = recycleType;

            Create(entity);
        }


        protected abstract void Create(EntityObject entity);


        //----------------------------------------------------------------------
        internal void ReleaseInFactory()
        {
            Release();
        }

        protected abstract void Release();


        //----------------------------------------------------------------------

        public string GetRecycleType()
        {
            return m_recycleType;
        }

        public void Dispose()
        {
            try
            {
                GameObject.Destroy(this.gameObject);
            }
            catch (Exception e)
            {

            }
        }
    }

}
