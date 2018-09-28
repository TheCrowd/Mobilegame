using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Snaker.Service.Core;
using Snaker.Service.UIManager;
using Snaker.Service.UserManager;
using UnityEngine.UI;
using Snaker.Module;
namespace Snaker.UI.Login
{
    public class UILoginPage : UIPage
    {

        public InputField inputId;
        public InputField inputName;


        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            UserBean ub = AppConfig.Value.mainUserData;
            inputName.text = ub.name;
            inputId.text = ub.id.ToString();

        }



        public void OnBtnLogin()
        {
            uint userId = 0;
            uint.TryParse(inputId.text, out userId);
            string userName = inputName.text.Trim();
            if (userId == 0)
            {
                userId = (uint)Random.Range(10000, 99999);
            }

            var module = ModuleManager.Instance.GetModule(ModuleConst.LoginModule) as LoginModule;
            if (module != null)
            {
                module.Login(userId, userName, "");
            }
        }
    }
}