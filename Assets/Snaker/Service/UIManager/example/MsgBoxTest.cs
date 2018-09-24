using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SGF.Logger;
using UnityEngine;
using Snaker.UI.Common;
using Snaker.Service.UIManager;
using SGF.Unity;

namespace Snaker.Service.UIManager.example
{
    class MsgBoxTest : MonoBehaviour
    {
        void Start()
        {
            var arg = new UIMsgBox.UIMsgBoxArg();
            arg.title = "megbox Test";
            arg.content = "it works! lol";
            arg.btnText = "OK|Cancel|Close";
            //UIManager.Instance.OpenWindow("Common/UIMsgBox", arg);
            MsgBoxAPI.ShowMsgBox("I am a msg box", "MsgBox with title", "OK|cancle|close", OnMsgBoxClick);

        }
        private void OnMsgBoxClick(object arg)
        {
            this.LogWarning("ButtonIndex:{0}", arg);
        }
        private void Update()
        {

        }
    }
}
