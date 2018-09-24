using System.Diagnostics;
using System.Reflection;

namespace SGF.Logger
{
    public static class LoggerExtension
    {

        [Conditional("ENABLE_LOG_LOOP")]
        public static void LogLoop(this object obj, string message = "")
        {
            if (!MyLogger.EnableLogLoop)
            {
                return;
            }

            MyLogger.LogLoop(GetLogTag(obj), GetLogCallerMethod(), (string)message);
        }

        [Conditional("ENABLE_LOG_LOOP")]
        public static void LogLoop(this object obj, string format, params object[] args)
        {
            if (!MyLogger.EnableLogLoop)
            {
                return;
            }

            MyLogger.LogLoop(GetLogTag(obj), GetLogCallerMethod(), string.Format(format, args));
        }


        //----------------------------------------------------------------------

        [Conditional("ENABLE_LOG_LOOP"), Conditional("ENABLE_LOG")]
        public static void Log(this object obj, string message = "")
        {
            if (!MyLogger.EnableLog)
            {
                return;
            }

            MyLogger.Log(GetLogTag(obj), GetLogCallerMethod(), (string)message);
        }

        [Conditional("ENABLE_LOG_LOOP"), Conditional("ENABLE_LOG")]
        public static void Log(this object obj, string format, params object[] args)
        {
            if (!MyLogger.EnableLog)
            {
                return;
            }

            MyLogger.Log(GetLogTag(obj), GetLogCallerMethod(), string.Format(format, args));
        }


        //----------------------------------------------------------------------


        public static void LogError(this object obj, string message)
        {
            MyLogger.LogError(GetLogTag(obj), GetLogCallerMethod(), (string)message);
        }

        public static void LogError(this object obj, string format, params object[] args)
        {
            MyLogger.LogError(GetLogTag(obj), GetLogCallerMethod(), string.Format(format, args));
        }



        //----------------------------------------------------------------------

        public static void LogWarning(this object obj, string message)
        {
            MyLogger.LogWarning(GetLogTag(obj), GetLogCallerMethod(), (string)message);
        }


        public static void LogWarning(this object obj, string format, params object[] args)
        {
            MyLogger.LogWarning(GetLogTag(obj), GetLogCallerMethod(), string.Format(format, args));
        }

        //----------------------------------------------------------------------



        //----------------------------------------------------------------------
        private static string GetLogTag(object obj)
        {
            FieldInfo fi = obj.GetType().GetField("LOG_TAG");
            if (fi != null)
            {
                return (string)fi.GetValue(obj);
            }

            return obj.GetType().Name;
        }

        private static Assembly ms_Assembly;
        private static string GetLogCallerMethod()
        {
            StackTrace st = new StackTrace(2, false);
            if (st != null)
            {
                if (null == ms_Assembly)
                {
                    ms_Assembly = typeof(MyLogger).Assembly;
                }

                int currStackFrameIndex = 0;
                while (currStackFrameIndex < st.FrameCount)
                {
                    StackFrame oneSf = st.GetFrame(currStackFrameIndex);
                    MethodBase oneMethod = oneSf.GetMethod();

                    if (oneMethod.Module.Assembly != ms_Assembly)
                    {
                        return oneMethod.Name;
                    }

                    currStackFrameIndex++;
                }

            }

            return "";
        }
    }
}
