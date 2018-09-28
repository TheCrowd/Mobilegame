using ProtoBuf;
using Snaker.Service.UserManager;
using UnityEngine;
using SGF.Logger;
using SGF.IO;
using SGF.ProtoBuf;

namespace Snaker
{
    /// <summary>
    /// configurations of the App, like background music, sound effect
    /// </summary>
    [ProtoContract]
    public class AppConfig
    {
        /// <summary>
        /// data of main user
        /// </summary>
        [ProtoMember(1)]
        public UserBean mainUserData = new UserBean();
        [ProtoMember(2)]
        public bool enableBgMusic = true;
        [ProtoMember(3)]
        public bool enableSoundEffect = true;


        //============================================================================
        private static AppConfig m_Value = new AppConfig();
        public static AppConfig Value { get { return m_Value; } }

#if UNITY_EDITOR
        public readonly static string Path = Application.persistentDataPath + "/AppConfig_Editor.data";
#else
        public readonly static string Path = Application.persistentDataPath + "/AppConfig.data";
#endif

        public static void Init()
        {
            MyLogger.Log("AppConfig", "Init() Path = " + Path);

            byte[] data = FileUtils.ReadFile(Path);
            if (data != null && data.Length > 0)
            {
                AppConfig cfg = (AppConfig)PBSerializer.NDeserialize(data, typeof(AppConfig));
                if (cfg != null)
                {
                    m_Value = cfg;
                }
            }
        }

        public static void Save()
        {
            MyLogger.Log("AppConfig", "Save() Value = " + m_Value);

            if (m_Value != null)
            {
                byte[] data = PBSerializer.NSerialize(m_Value);
                FileUtils.SaveFile(Path, data);
            }
        }

    }


}

