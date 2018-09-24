using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snaker.Service.UIManager;
using Snaker.Service.Core;

namespace Snaker.Service.UIManager.example
{
    public class UIPage1:UIPage
    {
        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
        }

        public void OnBtnOpenPage2()
        {
            UIManager.Instance.OpenPage("UIPage2");
            
        }
    }
}
