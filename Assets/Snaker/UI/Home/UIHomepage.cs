using Snaker.Service.UIManager;
using Snaker.Module;
using Snaker.Service.UserManager;
using UnityEngine.UI;
using SGF.Unity;
using Snaker.Service.Core;
using Snaker.GameCore.Data;
using Reign;
using SGF.Logger;

namespace Snaker.UI.Home
{
    public class UIHomepage : UIPage
    {
        public Text txtUserInfo;
        private static bool created;//is ad created
        private static Ad ad;


        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            UpdateUserInfo();
            if (created)
            {
                if (ad != null) ad.Visible = true;
            }
            CreateAd();
        }

        public void CreateAd()
        {
            created = true;

            // Ads - NOTE: You can pass in multiple "AdDesc" objects if you want more then one ad.
            var desc = new AdDesc();

            // global settings
            desc.Testing = true;// NOTE: To test ads on iOS, you must enable them in iTunes Connect.
            desc.Visible = true;
            desc.UnityUI_AdMaxWidth = 0.3f;
            desc.UnityUI_AdMaxHeight = 0.15f;
            desc.UseClassicGUI = false;
            desc.GUIOverrideEnabled = false;
            desc.UnityUI_SortIndex = 1000;

            // Editor
            desc.Editor_AdAPI = AdAPIs.EditorTestAd;
            desc.Editor_AdGravity = AdGravity.BottomCenter;
            desc.Editor_AdScale = 2;

            desc.Editor_MillennialMediaAdvertising_APID = "";
            desc.Editor_MillennialMediaAdvertising_AdGravity = AdGravity.BottomCenter;
            //desc.Editor_MillennialMediaAdvertising_RefreshRate = 120,
            // WinRT settings (Windows 8.0 & 8.1)
            desc.WinRT_AdAPI = AdAPIs.MicrosoftAdvertising;
            desc.WinRT_MicrosoftAdvertising_ApplicationID = "";
            desc.WinRT_MicrosoftAdvertising_UnitID = "";
            desc.WinRT_MicrosoftAdvertising_AdGravity = AdGravity.BottomCenter;
            desc.WinRT_MicrosoftAdvertising_AdSize = WinRT_MicrosoftAdvertising_AdSize.Wide_728x90;

            // iOS settings
            desc.iOS_AdAPI = AdAPIs.AdMob;
            desc.iOS_iAd_AdGravity = AdGravity.BottomCenter;

            desc.iOS_AdMob_AdGravity = AdGravity.BottomCenter;
            desc.iOS_AdMob_UnitID = "";// NOTE: You can use legacy (PublisherID) too, You MUST have this even for Testing!
            desc.iOS_AdMob_AdSize = iOS_AdMob_AdSize.Banner_320x50;

            // Android settings
#if AMAZON
		desc.Android_AdAPI = AdAPIs.Amazon;// Choose between AdMob or Amazon
#else
            desc.Android_AdAPI = AdAPIs.AdMob;// Choose between AdMob or Amazon
#endif

            desc.Android_AdMob_UnitID = "";// NOTE: You MUST have this even for Testing!
            desc.Android_AdMob_AdGravity = AdGravity.BottomCenter;
            desc.Android_AdMob_AdSize = Android_AdMob_AdSize.Banner_320x50;

            desc.Android_AmazonAds_ApplicationKey = "";
            desc.Android_AmazonAds_AdSize = Android_AmazonAds_AdSize.Wide_320x50;
            desc.Android_AmazonAds_AdGravity = AdGravity.BottomCenter;
            //desc.Android_AmazonAds_RefreshRate = 120;

            // create ad
            ad = AdManager.CreateAd(desc, null);
            if (ad != null) ad.Visible = true;
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

        public void OnBtnCloseAd()
        {
            if (ad != null)
            {
                ad.Visible = false;
            }
        }

        public void OnBtnOpenAd()
        {
            if (ad != null)
            {
                ad.Visible = true;
            }
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
            OpenModule(ModuleConst.PVPModule, (int)GameMode.UnlimitedPVP);
        }


    }
}


