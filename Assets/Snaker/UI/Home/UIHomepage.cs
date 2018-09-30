using Snaker.Service.UIManager;
using Snaker.Module;
using Snaker.Service.UserManager;
using UnityEngine.UI;
using SGF.Unity;
using Snaker.Service.Core;
using Snaker.GameCore.Data;

namespace Snaker.UI.Home
{
    public class UIHomepage : UIPage
    {
        public Text txtUserInfo;

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            UpdateUserInfo();
        }


        private void UpdateUserInfo()
        {
            UserBean ub = UserManager.Instance.MainUserData;
            txtUserInfo.text = ub.name + ":Lv." + ub.level;
        }


        public void OnBtnUserInfo()
        {
            MsgBoxAPI.ShowMsgBox("re-login", "want to relogin?", "ok|cancel", btnNum =>
            {
                if ((int)btnNum == 0)
                {
                    HomeModule module = ModuleManager.Instance.GetModule(ModuleConst.HomeModule) as HomeModule;
                    module.TryReLogin();
                }
            });
        }


        private void OpenModule(string name, object arg = null)
        {
            var module = ModuleManager.Instance.GetModule("HomeModule") as HomeModule;
            if (module != null)
            {
                module.OpenModule(name, arg);
            }
        }


        public void OnBtnSetting()
        {
            OpenModule(ModuleConst.SettingModule);
        }

        public void OnBtnDailyCheckIn()
        {
            OpenModule(ModuleConst.DailyCheckInModule);
        }

        public void OnBtnActivity()
        {
            OpenModule(ModuleConst.ActivityModule);
        }

        public void OnBtnBuyCoin()
        {
            OpenModule(ModuleConst.ShopModule, "BuyCoin");
        }

        public void OnBtnFreeCoin()
        {
            OpenModule(ModuleConst.ShareModule);
        }


        public void OnBtnUnlimitedPVE()
        {
            OpenModule(ModuleConst.PVEModule, (int)GameMode.UnlimitedPVE);
        }

        public void OnBtnTimelimitPVE()
        {
            OpenModule(ModuleConst.PVEModule, (int)GameMode.TimeLimitedPVE);
        }

        public void OnBtnPVP()
        {
            //OpenModule(ModuleConst.PVPModule, (int)GameMode.EndlessPVP);
        }


    }
}


