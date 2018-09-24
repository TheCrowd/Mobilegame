using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SGF.Logger;
using UnityEngine;

namespace Snaker.Service.UIManager
{
    public class UIWidget : UIPanel
    {
        /// <summary>
        /// args for open the widget
        /// </summary>
        protected object widgetOpenArg;

        /// <summary>
        /// used to open a widget
        /// </summary>
        /// <param name="arg"></param>
        public sealed override void Open(object arg = null)
        {
            this.Log("widget:Open() arg:{0}", arg);
            widgetOpenArg = arg;
            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
            }

            OnOpen(arg);
        }

        /// <summary>
        /// usded to close a widget
        /// </summary>
        public sealed override void Close(object arg = null)
        {
            this.Log("widget:Close() arg:{0}", arg);
            if (this.gameObject.activeSelf)
            {

                this.gameObject.SetActive(false);
            }

            OnClose(arg);
        }



    }
}
