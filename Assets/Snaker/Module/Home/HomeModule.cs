

using SGF.Unity;
using Snaker.Service.Core;
using Snaker.Service.UIManager;

namespace Snaker.Module
{
    public class HomeModule : BusinessModule
    {
        public void TryReLogin()
        {
            //TODO verify if current user can re login
            UIManager.Instance.OpenPage(UIConst.UILoginPage);
        }

        public void OpenModule(string name, object arg)
        {
            switch (name)
            {
                case ModuleConst.PVEModule:
                case ModuleConst.PVPModule:
                    ModuleManager.Instance.ShowModule(name, arg);
                    break;
                default:
                    MsgBoxAPI.ShowMsgBox(name, "Module to be implemented...", "ok");
                    break;
            }
        }
    }
}

