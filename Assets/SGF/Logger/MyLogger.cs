using System;
using System.Diagnostics;
using System.IO;

namespace SGF.Logger
{
    public class MyLogger
    {
        public static bool EnableLog;
        public static bool EnableLogLoop;
        public static bool EnableTime = true;
        public static bool EnableSave = false;
        public static bool EnableStack = false;
        public static string LogFileDir = "";
        public static string LogFileName = "";
        public static string Prefix = "> ";
        public static StreamWriter LogFileWriter = null;
        public static bool UseUnityEngine = true;

        public static void Init()
        {
            if (UseUnityEngine)
            {
                LogFileDir = UnityEngine.Application.persistentDataPath + "/DebugerLog/";
            }
            else
            {
                string path = System.AppDomain.CurrentDomain.BaseDirectory;
                LogFileDir = path + "/DebugerLog/";
            }
        }

        private static void Internal_Log(string msg, object context = null)
        {
            if (UseUnityEngine)
            {
                UnityEngine.Debug.Log(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        private static void Internal_LogWarning(string msg, object context = null)
        {
            if (UseUnityEngine)
            {
                UnityEngine.Debug.LogWarning(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        private static void Internal_LogError(string msg, object context = null)
        {
            if (UseUnityEngine)
            {
                UnityEngine.Debug.LogError(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }


        //----------------------------------------------------------------------
        [Conditional("ENABLE_LOG_LOOP")]
        public static void LogLoop(string tag, string methodName, string message = "")
        {
            if (!MyLogger.EnableLogLoop)
            {
                return;
            }

            message = GetLogText(tag, methodName, message);
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        [Conditional("ENABLE_LOG_LOOP")]
        public static void LogLoop(string tag, string methodName, string format, params object[] args)
        {
            if (!MyLogger.EnableLogLoop)
            {
                return;
            }

            string message = GetLogText(tag, methodName, string.Format(format, args));
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        [Conditional("ENABLE_LOG_LOOP"), Conditional("ENABLE_LOG")]
        public static void Log(string tag, string methodName, string message = "")
        {
            if (!MyLogger.EnableLog)
            {
                return;
            }
            message = GetLogText(tag, methodName, message);
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        [Conditional("ENABLE_LOG_LOOP"), Conditional("ENABLE_LOG")]
        public static void Log(string tag, string methodName, string format, params object[] args)
        {
            if (!MyLogger.EnableLog)
            {
                return;
            }

            string message = GetLogText(tag, methodName, string.Format(format, args));
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        public static void LogError(string tag, string methodName, string message)
        {
            message = GetLogText(tag, methodName, message);
            Internal_LogError(Prefix + message);
            LogToFile("[E]" + message, true);
        }

        public static void LogError(string tag, string methodName, string format, params object[] args)
        {
            string message = GetLogText(tag, methodName, string.Format(format, args));
            Internal_LogError(Prefix + message);
            LogToFile("[E]" + message, true);
        }


        public static void LogWarning(string tag, string methodName, string message)
        {
            message = GetLogText(tag, methodName, message);
            Internal_LogWarning(Prefix + message);
            LogToFile("[W]" + message);
        }

        public static void LogWarning(string tag, string methodName, string format, params object[] args)
        {
            string message = GetLogText(tag, methodName, string.Format(format, args));
            Internal_LogWarning(Prefix + message);
            LogToFile("[W]" + message);
        }



        //----------------------------------------------------------------------
        private static string GetLogText(string tag, string methodName, string message)
        {
            string str = "";
            if (MyLogger.EnableTime)
            {
                DateTime now = DateTime.Now;
                str = now.ToString("HH:mm:ss.fff") + " ";
            }

            str = str + tag + "::" + methodName + "() " + message;
            return str;
        }



        //----------------------------------------------------------------------
        internal static string CheckLogFileDir()
        {
            if (string.IsNullOrEmpty(LogFileDir))
            {
               //this code can not be executed in threads
                try
                {
                    if (UseUnityEngine)
                    {
                        LogFileDir = UnityEngine.Application.persistentDataPath + "/DebugerLog/";
                    }
                    else
                    {
                        string path = System.AppDomain.CurrentDomain.BaseDirectory;
                        LogFileDir = path + "/DebugerLog/";
                    }
                }
                catch (Exception e)
                {
                    Internal_LogError("MyLogger::CheckLogFileDir() " + e.Message + e.StackTrace);
                    return "";
                }
            }

            try
            {
                if (!Directory.Exists(LogFileDir))
                {
                    Directory.CreateDirectory(LogFileDir);
                }
            }
            catch (Exception e)
            {
                Internal_LogError("MyLogger::CheckLogFileDir() " + e.Message + e.StackTrace);
                return "";
            }

            return LogFileDir;
        }

        internal static string GenLogFileName()
        {
            DateTime now = DateTime.Now;
            string filename = now.GetDateTimeFormats('s')[0].ToString();//2005-11-05T14:06:25
            filename = filename.Replace("-", "_");
            filename = filename.Replace(":", "_");
            filename = filename.Replace(" ", "");
            filename += ".log";

            return filename;
        }

        private static void LogToFile(string message, bool EnableStack = false)
        {
            if (!EnableSave)
            {
                return;
            }

            if (LogFileWriter == null)
            {
                LogFileName = GenLogFileName();
                LogFileDir = CheckLogFileDir();
                if (string.IsNullOrEmpty(LogFileDir))
                {
                    return;
                }

                string fullpath = LogFileDir + LogFileName;
                try
                {
                    LogFileWriter = File.AppendText(fullpath);
                    LogFileWriter.AutoFlush = true;
                }
                catch (Exception e)
                {
                    LogFileWriter = null;
                    Internal_LogError("MyLogger::LogToFile() " + e.Message + e.StackTrace);
                    return;
                }
            }

            if (LogFileWriter != null)
            {
                try
                {
                    LogFileWriter.WriteLine(message);
                    if ((EnableStack || MyLogger.EnableStack) && UseUnityEngine)
                    {
                        LogFileWriter.WriteLine(UnityEngine.StackTraceUtility.ExtractStackTrace());
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
    }
}

