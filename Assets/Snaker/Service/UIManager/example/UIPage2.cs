using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snaker.Service.UIManager;
using Snaker.Service.Core;

namespace Snaker.Service.UIManager.example
{
    public class UIPage2 : UIPage
    {
        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
        }

        public void OnBtnOpenWin1()
        {
            UIManager.Instance.OpenWindow("UIWin1");
        }

        public void OnBtnOepnWidget1()
        {
            UIManager.Instance.OpenWidget("UIWidget1");

        }
    }
}
