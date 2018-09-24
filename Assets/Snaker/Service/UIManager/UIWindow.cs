using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SGF.Logger;
using UnityEngine;
using UnityEngine.UI;

namespace Snaker.Service.UIManager
{
    public class UIWindow : UIPanel
    {
        //=======================================================================

        public delegate void CloseEvent(object arg = null);

        //=======================================================================
        /// <summary>
        /// close buttn(most windows have a close button）
        /// </summary>
        [SerializeField]
        private Button btnClose;

        /// <summary>
        /// close window event
        /// </summary>
        public event CloseEvent onClose;

        /// <summary>
        /// args for opening the window
        /// </summary>
        protected object winOpenArg;

        /// <summary>
        /// if the Window has been opened
        /// </summary>
        private bool isOpenedOnce;

        /// <summary>
        /// invoked when the winodw is enabled
        /// </summary>
        protected void OnEnable()
        {
            this.Log("window:OnEnable()");
            if (btnClose != null)
            {
                btnClose.onClick.AddListener(OnBtnClose);
            }
        }

        /// <summary>
        /// invoked when the window is disabled
        /// </summary>
        protected void OnDisable()
        {
            this.Log("window:OnDisable()");

            if (btnClose != null)
            {
                btnClose.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// invoked when close button is clicked
        /// note not all windows has a close button
        /// </summary>
        private void OnBtnClose()
        {
            this.Log("window:OnBtnClose()");
            Close(0);
        }


        /// <summary>
        /// 调用它打开UIWindow
        /// </summary>
        /// <param name="arg"></param>
        public sealed override void Open(object arg = null)
        {
            this.Log("window:Open() arg:{0}", arg);
            winOpenArg = arg;
            isOpenedOnce = false;
            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
            }

            OnOpen(arg);
            isOpenedOnce = true;
        }

        /// <summary>
        /// 调用它以关闭UIWindow
        /// </summary>
        public sealed override void Close(object arg = null)
        {
            this.Log("window:Close()");
            if (this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(false);
            }

            OnClose(arg);
            if (onClose != null)
            {
                onClose(arg);
                onClose = null;
            }
        }



    }
}
