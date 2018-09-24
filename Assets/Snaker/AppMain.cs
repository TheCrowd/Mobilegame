using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SGF.Logger;
using Snaker.Service.Core.ModuleTest;
using Snaker.Service.UIManager.example;

public class AppMain : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MyLogger.EnableLog = true;
        MyLogger.EnableLogLoop = true;
        SampleModule sm = new SampleModule();
        sm.Init();
        UIExample uiTest = new UIExample();
        uiTest.Init();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
