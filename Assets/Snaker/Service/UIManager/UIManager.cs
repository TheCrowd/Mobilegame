using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Snaker.Service.Core;
using SGF.Logger;


namespace Snaker.Service.UIManager
{
    public class UIManager : ServiceModule<UIManager>
    {
        public const string LOG_TAG = "UIManager";

        public static string MainScene = "Main";
        public static string MainPage = "UIMainPage";


        class UIPageTrack
        {
            public string name;
            public string scene;
        }


        private Stack<UIPageTrack> pageTrackStack;
        private UIPageTrack currentPage;
        private Action<string> sceneLoaded;
        private List<UIPanel> listLoadedPanel;

        public UIManager()
        {
            pageTrackStack = new Stack<UIPageTrack>();
            listLoadedPanel = new List<UIPanel>();
        }

        /// <summary>
        /// Init UI resource
        /// </summary>
        /// <param name="uiResRoot">Root folder of UI resources, default is"ui/"</param>
        public void Init(string uiResRoot)
        {
            CheckSingleton();

            UIRes.UIResRoot = uiResRoot;

            //listen on UnityScene's loading event
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (sceneLoaded != null)
                {
                    sceneLoaded(scene.name);
                }
            };
        }


        /// <summary>
        /// load UI, if the UI is already in UIRoot, use the one in UIRoot instead
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        private T Load<T>(string name) where T : UIPanel
        {
            T ui = UIRoot.Find<T>(name);

            if (ui == null)
            {
                GameObject original = UIRes.LoadPrefab(name);
                if (original != null)
                {
                    GameObject go = GameObject.Instantiate(original);
                    ui = go.GetComponent<T>();
                    if (ui != null)
                    {
                        go.name = name;
                        UIRoot.AddChild(ui);
                    }
                    else
                    {
                        this.LogError("Load() Prefab did not add the component: " + name);
                    }
                }
                else
                {
                    this.LogError("Load() Res Not Found: " + name);
                }
            }

            if (ui != null)
            {
                if (listLoadedPanel.IndexOf(ui) < 0)
                {
                    listLoadedPanel.Add(ui);
                }
            }

            return ui;
        }

        private T Open<T>(string name, object arg = null) where T : UIPanel
        {
            T ui = Load<T>(name);
            if (ui != null)
            {
                ui.Open(arg);
            }
            else
            {
                this.LogError("Open() Failed! Name:{0}", name);
            }
            return ui;
        }

        private void CloseAllLoadedPanels()
        {
            for (int i = 0; i < listLoadedPanel.Count; i++)
            {
                if (listLoadedPanel[i].IsOpen)
                {
                    listLoadedPanel[i].Close();
                }
            }
        }

        //=======================================================================

        /// <summary>
        /// go to main page
        /// clear page stack 
        /// </summary>
        public void EnterMainPage()
        {
            pageTrackStack.Clear();
            OpenPageWorker(MainScene, MainPage, null);
        }


        //=======================================================================
        #region UIPage management
        /// <summary>
        /// open a page
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="page"></param>
        /// <param name="arg"></param>
        public void OpenPage(string scene, string page, object arg = null)
        {
            this.Log(LOG_TAG, "OpenPage() scene:{0}, page:{1}, arg:{2} ", scene, page, arg);

            if (currentPage != null)
            {
                pageTrackStack.Push(currentPage);
            }

            OpenPageWorker(scene, page, arg);
        }

        public void OpenPage(string page, object arg = null)
        {
            this.OpenPage(MainScene, page, arg);
        }

        /// <summary>
        /// back to last page
        /// </summary>
        public void GoBackPage()
        {
            this.Log(LOG_TAG, "GoBackPage()");
            if (pageTrackStack.Count > 0)
            {
                var track = pageTrackStack.Pop();
                OpenPageWorker(track.scene, track.name, null);
            }
            else if (pageTrackStack.Count == 0)
            {
                EnterMainPage();
            }
        }

        private void OpenPageWorker(string scene, string page, object arg)
        {
            this.Log(LOG_TAG, "OpenPageWorker() scene:{0}, page:{1}, arg:{2} ", scene, page, arg);

            string oldScene = SceneManager.GetActiveScene().name;

            currentPage = new UIPageTrack();
            currentPage.scene = scene;
            currentPage.name = page;

            //close all UIs of the current page
            CloseAllLoadedPanels();


            if (oldScene == scene)
            {
                Open<UIPage>(page, arg);
            }
            else
            {
                sceneLoaded = (sceneName) =>
                {
                    if (sceneName == scene)
                    {
                        sceneLoaded = null;
                        Open<UIPage>(page, arg);
                    }
                };

                SceneManager.LoadScene(scene);
            }
        }



        #endregion

        //=======================================================================

        #region UIWindow management

        public UIWindow OpenWindow(string name, object arg = null)
        {
            UIWindow ui = Open<UIWindow>(name, arg);
            return ui;
        }


        #endregion

        //=======================================================================

        #region UIWidget management

        public UIWidget OpenWidget(string name, object arg = null)
        {
            UIWidget ui = Open<UIWidget>(name, arg);
            return ui;
        }



        #endregion
    }
}

