using Snaker.Service;
using Snaker.Service.UIManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace Snaker.UI.Home
{
    class UIChangeSkinWin:UIWindow
    {
        public Button[] buttons;
        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            int activeButton = SkinManager.Instance.SkinType;
            buttons[activeButton].Select();
        }

        public void OnBtnSkinClick(int skinType)
        {
            SkinManager.Instance.UpdateSkinType(skinType);
        }

    }
}
