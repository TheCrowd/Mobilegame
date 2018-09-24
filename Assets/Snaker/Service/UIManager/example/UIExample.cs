using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Snaker.Service.UIManager.example
{ 
    public class UIExample
    {
        public void Init()
        {
            UIManager.Instance.Init("ui/example");
            UIManager.MainPage = "UIPage1";
            UIManager.Instance.EnterMainPage();
        }

    }
}