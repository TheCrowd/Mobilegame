using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using SGF.Logger;

namespace SGF.Unity
{

    public delegate void DelayFunction(object[] args);

    public class DelayInvoker : MonoSingleton<DelayInvoker>
    {
        private List<DelayHelper> helperList;
        private List<DelayHelper> unscaledHelperList;
        private static WaitForEndOfFrame msWaitForEndOfFrame = new WaitForEndOfFrame();


        class DelayHelper
        {
            public object group;
            public float delay;
            public DelayFunction func;
            public object[] args;

            public void Invoke()
            {
                if (func != null)
                {
                    try
                    {
                        func(args);
                    }
                    catch (Exception e)
                    {
                        MyLogger.LogError("DelayInvoker", "Invoke() Error:{0}\n{1}", e.Message, e.StackTrace);
                    }

                }
            }
        }

        public static void DelayInvoke(object group, float delay, DelayFunction func, params object[] args)
        {
            DelayInvoker.Instance.DelayInvokeWorker(group, delay, func, args);
        }


        public static void DelayInvoke(float delay, DelayFunction func, params object[] args)
        {
            DelayInvoker.Instance.DelayInvokeWorker(null, delay, func, args);
        }

        public static void UnscaledDelayInvoke(float delay, DelayFunction func, params object[] args)
        {
            DelayInvoker.Instance.UnscaledDelayInvokeWorker(null, delay, func, args);
        }

        public static void CancelInvoke(object group)
        {
            DelayInvoker.Instance.CancelInvokeWorker(group);
        }

        //====================================================================

        private void DelayInvokeWorker(object group, float delay, DelayFunction func, params object[] args)
        {
            if (helperList == null)
            {
                helperList = new List<DelayHelper>();
            }

            DelayHelper helper = new DelayHelper();
            helper.group = group;
            helper.delay = delay;
            helper.func += func;
            helper.args = args;

            helperList.Add(helper);
        }

        private void UnscaledDelayInvokeWorker(object group, float delay, DelayFunction func, params object[] args)
        {
            if (unscaledHelperList == null)
            {
                unscaledHelperList = new List<DelayHelper>();
            }

            DelayHelper helper = new DelayHelper();
            helper.group = group;
            helper.delay = delay;
            helper.func += func;
            helper.args = args;

            unscaledHelperList.Add(helper);
        }



        private void CancelInvokeWorker(object group)
        {
            if (null != helperList)
            {
                if (group == null)
                {

                    for (int i = 0; i < helperList.Count; i++)
                    {
                        helperList[i] = null;
                    }
                    helperList.Clear();

                    return;
                }

                for (int i = 0; i < helperList.Count(); ++i)
                {

                    DelayHelper helper = helperList[i];

                    if (helper.group == group)
                    {
                        helperList.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        //====================================================================

        void Update()
        {
            if (null != helperList)
            {
                for (int i = 0; i < helperList.Count(); ++i)
                {
                    DelayHelper helper = helperList[i];
                    helper.delay -= UnityEngine.Time.deltaTime;
                    if (helper.delay <= 0)
                    {
                        helperList.RemoveAt(i);
                        i--;

                        helper.Invoke();
                    }
                }
            }

            if (null != unscaledHelperList)
            {
                for (int i = 0; i < unscaledHelperList.Count(); ++i)
                {
                    DelayHelper helper = unscaledHelperList[i];
                    helper.delay -= UnityEngine.Time.unscaledDeltaTime;
                    if (helper.delay <= 0)
                    {
                        unscaledHelperList.RemoveAt(i);
                        i--;

                        helper.Invoke();
                    }
                }

            }
        }

        void OnDisable()
        {
            //            Debug.Log("DelayInvoker Release!!!");
            CancelInvoke(null);
            this.StopAllCoroutines();
        }

        //====================================================================

        public static void DelayInvokerOnEndOfFrame(DelayFunction func, params object[] args)
        {
            Instance.StartCoroutine(DelayInvokerOnEndOfFrameWorker(func, args));
        }

        private static IEnumerator DelayInvokerOnEndOfFrameWorker(DelayFunction func, params object[] args)
        {
            yield return msWaitForEndOfFrame;

            //Profiler.BeginSample("DelayInvoker_DelayInvokerOnEndOfFrame");

            try
            {
                func(args);
            }
            catch (Exception e)
            {
                MyLogger.LogError("DelayInvoker", "DelayInvokerOnEndOfFrame() Error:{0}\n{1}", e.Message, e.StackTrace);
            }

            //Profiler.EndSample();
        }


        public static void FixedTimeInvoke(int hours, int minitue)
        {

        }
    }



}
