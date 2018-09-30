using SGF.Extension;
using SGF.Logger;
using System.Collections.Generic;
using UnityEngine;

namespace Snaker.GameCore.Entity.Factory
{


    //==========================================================

    public static class ViewFactory
    {
        public static bool EnableLog = false;
        private const string LOG_TAG = "ViewFactory";
        private static bool isInit = false;
        private static Transform viewRoot;
        private static Recycler objRecycler;

        /// <summary>
        /// instantiated objects
        /// </summary>
        private static DictionaryExt<EntityObject, ViewObject> mapObjectList;


        public static void Init(Transform viewRoot)
        {
            if (isInit)
            {
                return;
            }
            isInit = true;

            viewRoot = viewRoot;

            mapObjectList = new DictionaryExt<EntityObject, ViewObject>();
            objRecycler = new Recycler();

        }

        /// <summary>
        /// release all objects inlcuding idle ones
        /// </summary>
        public static void Release()
        {
            isInit = false;

            foreach (var pair in mapObjectList)
            {
                pair.Value.ReleaseInFactory();
                pair.Value.Dispose();
            }
            mapObjectList.Clear();

            objRecycler.Release();

            viewRoot = null;
        }


        public static void CreateView(string resPath, string resDefaultPath, EntityObject entity, Transform parent = null)
        {
            ViewObject obj = null;
            string recycleType = resPath;
            bool useRecycler = true;

            obj = objRecycler.Pop(recycleType) as ViewObject;
            if (obj == null)
            {
                useRecycler = false;
                obj = InstanceViewFromPrefab(recycleType, resDefaultPath);
            }

            if (obj != null)
            {
                if (!obj.gameObject.activeSelf)
                {
                    obj.gameObject.SetActive(true);
                }

                if (parent != null)
                {
                    obj.transform.SetParent(parent, false);
                }
                else
                {
                    obj.transform.SetParent(viewRoot, false);
                }

                obj.CreateInFactory(entity, recycleType);

                if (EnableLog && MyLogger.EnableLog)
                {
                    MyLogger.Log(LOG_TAG, "CreateView() {0}:{1} -> {2}:{3}, UseRecycler:{4}",
                        entity.GetType().Name, entity.GetHashCode(),
                        obj.GetRecycleType(), obj.GetInstanceID(),
                        useRecycler);
                }

                if (mapObjectList.ContainsKey(entity))
                {
                    MyLogger.LogError(LOG_TAG, "CreateView()"," Mapping already exist！");
                }
                mapObjectList[entity] = obj;
            }
        }


        public static void ReleaseView(EntityObject entity)
        {
            if (entity != null)
            {

                ViewObject obj = mapObjectList[entity];
                if (obj != null)
                {
                    if (EnableLog && MyLogger.EnableLog)
                    {
                        MyLogger.Log(LOG_TAG, "ReleaseView() {0}:{1} -> {2}:{3}",
                            entity.GetType().Name, entity.GetHashCode(),
                            obj.GetRecycleType(), obj.GetInstanceID());
                    }

                    mapObjectList.Remove(entity);
                    obj.ReleaseInFactory();
                    obj.gameObject.SetActive(false);

                    //push to recycler
                    objRecycler.Push(obj);



                }
            }
        }



        private static ViewObject InstanceViewFromPrefab(string prefabName, string defaultPrefabName)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabName);
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>(defaultPrefabName);
            }
            GameObject go = GameObject.Instantiate(prefab);
            ViewObject instance = go.GetComponent<ViewObject>();

            if (instance == null)
            {
                MyLogger.LogError(LOG_TAG, "InstanceViewFromPrefab()","prefab = " + prefabName);
            }

            return instance;
        }






    }
}
