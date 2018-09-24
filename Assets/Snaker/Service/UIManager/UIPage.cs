using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SGF.Logger;

namespace Snaker.Service.UIManager
{
    public class UIPage : UIPanel
    {
        /// <summary>
        /// back button， most page need a back button
        /// </summary>
        [SerializeField]
        private Button btnPageBack;

        /// <summary>
        /// arguments to oepn the UI
        /// </summary>
        protected object pageOpenArg;

        /// <summary>
        /// if the UI has been opened before
        /// </summary>
        private bool isOpenedOnce;

        /// <summary>
        /// invoked when the page is enabled
        /// </summary>
        protected void OnEnable()
        {
            this.Log("OnEnable()");
            if (btnPageBack != null)
            {
                btnPageBack.onClick.AddListener(OnBtnGoBack);
            }

#if UNITY_EDITOR
            if (isOpenedOnce)
            {
                //if the UI has been opened before，
                //leverage UnityEditor to trigger Open/Close issue
                //easier to debug
                OnOpen(pageOpenArg);
            }
#endif
        }

        /// <summary>
        /// invoked when the page is disabled
        /// </summary>
        protected void OnDisable()
        {
            this.Log("UIPage:OnDisable()");
#if UNITY_EDITOR
            if (isOpenedOnce)
            {
                //if the UI has been opened before，
                //leverage UnityEditor to trigger Open/Close issue
                //easier to debug
                OnClose();
            }
#endif
            if (btnPageBack != null)
            {
                btnPageBack.onClick.RemoveAllListeners();
            }
        }


        /// <summary>
        /// invoked when "back" button is clicked
        /// However it is worth noticing that not all page has a back button
        /// </summary>
        private void OnBtnGoBack()
        {
            this.Log("UIPage:OnBtnGoBack()");
            UIManager.Instance.GoBackPage();
        }

        /// <summary>
        /// to open a UI page
        /// </summary>
        /// <param name="arg"></param>
        public sealed override void Open(object arg = null)
        {
            this.Log("Open()");
            pageOpenArg = arg;
            isOpenedOnce = false;

            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
            }

            OnOpen(arg);
            isOpenedOnce = true;
        }

        public sealed override void Close(object arg = null)
        {
            this.Log("Close()");
            if (this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(false);
            }

            OnClose(arg);
        }


    }
}

