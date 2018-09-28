using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SGF.Logger;
using Snaker.Service.Core.ModuleTest;
using Snaker.Service.UIManager.example;
using Snaker.Service.UIManager;
using Snaker;
using Snaker.Service.Core;
using Snaker.Module;
using Snaker.Service.UserManager;
using SGF.Unity;

public class AppMain : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MyLogger.EnableLog = true;
        MyLogger.EnableLogLoop = true;
        //SampleModule sm = new SampleModule();
        //sm.Init();
        //UIExample uiTest = new UIExample();
        //uiTest.Init();
        this.Log(MyLogger.LogFileDir);

        AppConfig.Init();

        InitServices();
        InitModules();

        ModuleManager.Instance.ShowModule(ModuleConst.LoginModule);
    }

    private void InitServices()
    {
        ModuleManager.Instance.Init("Snaker.Module");

        //NetworkManager.Instance.Init();

        UIManager.Instance.Init("ui/");
        UIManager.MainPage = UIConst.UIHomePage;
        UIManager.MainScene = "Main";

        UserManager.Instance.Init();

        //GameManager.Instance.Init();
    }


    private void InitModules()
    {
        ModuleManager.Instance.CreateModule(ModuleConst.LoginModule);
        ModuleManager.Instance.CreateModule(ModuleConst.HomeModule);
        //ModuleManager.Instance.CreateModule(ModuleConst.PVEModule);
        //ModuleManager.Instance.CreateModule(ModuleConst.PVPModule);
        //ModuleManager.Instance.CreateModule(ModuleConst.HostModule);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
