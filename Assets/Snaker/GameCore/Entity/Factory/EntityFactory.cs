using System;
using System.Collections.Generic;
using SGF.Logger;

namespace Snaker.GameCore.Entity.Factory
{
    //==========================================================
    /// <summary>
    /// Entity Factory to create entities
    /// 
    /// </summary>
    public static class EntityFactory
    {
        public static bool EnableLog = false;
        private static string LOG_TAG = "EntityFactory";

        private static bool isInit = false;
        private static Recycler objRecycler;

        /// <summary>
        /// the list of instantiated objects
        /// </summary>
        private static List<EntityObject> objectList;

        public static void Init()
        {
            if (isInit)
            {
                MyLogger.Log(LOG_TAG, "don't initialize EntityFacotry twice!");
                return;
            }
            isInit = true;

            objectList = new List<EntityObject>();
            objRecycler = new Recycler();

        }

        /// <summary>
        /// release all objects including idle ones
        /// </summary>
        public static void Release()
        {
            isInit = false;

            for (int i = 0; i < objectList.Count; i++)
            {
                objectList[i].ReleaseInFactory();
                objectList[i].Dispose();
            }
            objectList.Clear();

            objRecycler.Release();

        }


        /// <summary>
        /// instantiate an entity
        /// </summary>
        /// <returns>The entity.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T InstanceEntity<T>() where T : EntityObject, new()
        {
            EntityObject obj = null;
            bool useRecycler = true;

            //search in recycler first
            Type type = typeof(T);
            obj = objRecycler.Pop(type.FullName) as EntityObject;
            if (obj == null)
            {
                useRecycler = false;
                obj = new T();
            }
            obj.InstanceInFactory();

            objectList.Add(obj);

            if (EnableLog && MyLogger.EnableLog)
            {
                MyLogger.Log(LOG_TAG, "InstanceEntity() {0}:{1}, UseRecycler:{2}", obj.GetType().Name, obj.GetHashCode(), useRecycler);
            }

            return (T)obj;
        }

        /// <summary>
        /// release an entity
        /// </summary>
        /// <param name="entity">EntityObject.</param>
        public static void ReleaseEntity(EntityObject obj)
        {
            if (obj != null)
            {
                if (EnableLog && MyLogger.EnableLog)
                {
                    MyLogger.Log(LOG_TAG, "ReleaseEntity() {0}:{1}", obj.GetType().Name, obj.GetHashCode());
                }

                obj.ReleaseInFactory();
                // we don't delete the object from list object directly
                // instead we delete objects all together
                // this is for a better efficiency.
            }
        }



        /// <summary>
        /// Clears the released objects.
        /// </summary>
        public static void ClearReleasedObjects()
        {
            for (int i = objectList.Count - 1; i >= 0; i--)
            {
                if (objectList[i].IsReleased)
                {
                    EntityObject obj = objectList[i];
                    objectList.RemoveAt(i);

                    //add to recycler
                    objRecycler.Push(obj);
                }
            }
        }
    }
}

