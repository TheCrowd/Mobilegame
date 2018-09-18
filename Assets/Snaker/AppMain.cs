using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OT.Foundation;
using Snaker.Service.Core.ModuleTest;

public class AppMain : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MyLogger.EnableLog = true;
        MyLogger.EnableLogLoop = true;
        SampleModule sm = new SampleModule();
        sm.Init();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
