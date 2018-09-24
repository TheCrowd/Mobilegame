using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snaker.Service.UIManager
{
    public static class UIRes
    {
        public static string UIResRoot = "ui/";

        /// <summary>
        /// prefab to load UI
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject LoadPrefab(string name)
        {
            GameObject asset = (GameObject)Resources.Load(UIResRoot + name);
            return asset;
        }
    }
}
