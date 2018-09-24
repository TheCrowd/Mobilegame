using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SGF.Logger;
using UnityEngine;

namespace Snaker.Service.UIManager
{
    public class UIRoot : MonoBehaviour
    {
        public const string LOG_TAG = "UIRoot";

        /// <summary>
        /// search UI intance from UIRoot through class type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Find<T>() where T : MonoBehaviour
        {
            string name = typeof(T).Name;
            GameObject obj = Find(name);
            if (obj != null)
            {
                return obj.GetComponent<T>();
            }


            return null;
        }

        /// <summary>
        /// search UI instance from UIRoot through class type or  UI name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Find<T>(string name) where T : MonoBehaviour
        {
            GameObject obj = Find(name);
            if (obj != null)
            {
                return obj.GetComponent<T>();

            }

            return null;
        }

        /// <summary>
        /// search GameObject in UIRoot throught UI name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject Find(string name)
        {
            Transform obj = null;
            GameObject root = FindUIRoot();
            if (root != null)
            {
                obj = root.transform.Find(name);
            }

            if (obj != null)
            {
                return obj.gameObject;
            }

            //MyLogger.LogError(LOG_TAG, "Find() UI:{0} 不存在！", name);
            return null;
        }

        /// <summary>
        /// find UIRoot
        /// </summary>
        /// <returns></returns>
        public static GameObject FindUIRoot()
        {
            GameObject root = GameObject.Find("UIRoot");
            if (root != null && root.GetComponent<UIRoot>() != null)
            {
                return root;
            }
            MyLogger.Log(LOG_TAG,"FindUIRoot() UIRoot Is Not Exist!!!");
            return null;
        }

        /// <summary>
        /// When a UIpage,UIWindow or UIWidget is added to UIRoot
        /// </summary>
        /// <param name="child"></param>
        public static void AddChild(UIPanel child)
        {
            GameObject root = FindUIRoot();
            if (root == null || child == null)
            {
                return;
            }


            child.transform.SetParent(root.transform, false);
            return;
        }

    }
}
