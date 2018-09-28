using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Snaker.Service.Core;
using Snaker.Service.UIManager;
using Snaker.Service.UserManager;
using SGF.Unity;


namespace Snaker.Module
{
    /// <summary>
    /// Login related issues
    /// one important feature is auto-reconncet when disconnects(to be implemented)
    /// </summary>
    public class LoginModule : BusinessModule
    {
        protected override void Show(object arg)
        {
            UIManager.Instance.OpenPage(UIConst.UILoginPage);
        }


        /// <summary>
        /// Login
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        public void Login(uint id, string name, string pwd)
        {
            //simply the logic coz we dont implement the server side
            //pretend that we have received success response from server
            UserBean ub = new UserBean();
            ub.id = id;
            ub.name = name;
            ub.defaultSnakeId = 1;

            //operations after getting successful response from server
            OnLoginSuccess(ub);

        }

        private void OnLoginSuccess(UserBean ub)
        {
            UserManager.Instance.UpdateMainUserData(ub);

            AppConfig.Value.mainUserData = UserManager.Instance.MainUserData;
            AppConfig.Save();

            //broadcast login event
            GlobalEvent.onLogin.Invoke(true);

            //enter the main page of app
            UIManager.Instance.EnterMainPage();
        }


    }
}

