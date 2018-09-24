using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SGF.Logger;

namespace Snaker.Service.UIManager
{
    public abstract class UIPanel : MonoBehaviour
    {
        public virtual void Open(object arg = null)
        {
            this.Log("Open() arg:{0}", arg);
        }

        public virtual void Close(object arg = null)
        {
            this.Log("Close() arg:{0}", arg);
        }

        /// <summary>
        /// if the UI has been opened
        /// </summary>
        public bool IsOpen { get { return this.gameObject.activeSelf; } }


        /// <summary>
        /// when the UI is closed, this function would be called
        /// when overwrite the function, it needs to support repetitive calls
        /// </summary>
        protected virtual void OnClose(object arg = null)
        {
            this.Log("UI:OnClose()");
        }

        /// <summary>
        /// when UI is opened 
        /// </summary>
        /// <param name="arg"></param>
        protected virtual void OnOpen(object arg = null)
        {
            this.Log("UI:OnOpen() ");
        }
    }
}
