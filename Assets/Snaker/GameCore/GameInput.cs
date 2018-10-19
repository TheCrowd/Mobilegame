using System;
using SGF.Unity;
using Snaker.GameCore.Data;
using UnityEngine;
using System.Collections.Generic;
using SGF.Logger;
using SGF.Extension;

namespace Snaker.GameCore
{
    public class GameInput : MonoBehaviour
    {
        /// <summary>
        /// to record the state of keys
        /// </summary>
        private static DictionaryExt<KeyCode, bool> m_MapKeyState = new DictionaryExt<KeyCode, bool>();

        /// <summary>
        /// Virtual Key
        /// </summary>
        public static Action<int, float> OnVkey;

        private static GameInput m_Instance = null;

        //-------------------------------------------------------------------
        /// <summary>
        /// compoment of EasyTouch
        /// </summary>
        private EasyJoystick m_Joystick;

        /// <summary>
        /// button to accelerate and use skills 
        /// </summary>
        private EasyButton m_Button;


        /// <summary>
        /// create GameInput instance in current scene
        /// </summary>
        public static void Create()
        {
            if (m_Instance != null)
            {
                throw new Exception("GameInput Already created！");
                return;
            }

            //instantiate EasyJoyStick prefab
            //since it is easier to configure EasyJoyStick using prefab
            GameObject prefab = Resources.Load<GameObject>("GameInput");
            GameObject go = GameObject.Instantiate(prefab);
            m_Instance = GameObjectUtils.EnsureComponent<GameInput>(go);
        }

        /// <summary>
        /// release current GameInput object
        /// </summary>
        public static void Release()
        {
            m_MapKeyState.Clear();
            if (m_Instance != null)
            {
                GameObject.Destroy(m_Instance.gameObject);
                m_Instance = null;
            }
            OnVkey = null;
        }


        void Start()
        {
            m_Joystick = this.GetComponentInChildren<EasyJoystick>();
            m_Button = this.GetComponentInChildren<EasyButton>();

            if (m_Joystick == null || m_Button == null)
            {
                this.LogError("Start() m_Joystick == null || m_Button == null!");
            }
        }


        void OnEnable()
        {
            EasyJoystick.On_JoystickMove += On_JoystickMove;
            EasyJoystick.On_JoystickMoveEnd += On_JoystickMoveEnd;
            EasyButton.On_ButtonUp += On_ButtonUp;
            EasyButton.On_ButtonDown += On_ButtonDown;
        }

        void OnDisable()
        {
            EasyJoystick.On_JoystickMove -= On_JoystickMove;
            EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
            EasyButton.On_ButtonDown -= On_ButtonDown;
            EasyButton.On_ButtonUp -= On_ButtonUp;
        }

        void OnDestroy()
        {
            EasyJoystick.On_JoystickMove -= On_JoystickMove;
            EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
            EasyButton.On_ButtonDown -= On_ButtonDown;
            EasyButton.On_ButtonUp -= On_ButtonUp;
        }


        //-------------------------------------------------------------------


        void On_JoystickMove(MovingJoystick move)
        {
            if (move.joystick == m_Joystick)
            {
                HandleVKey(GameVKeys.MoveX, move.joystickValue.x);
                HandleVKey(GameVKeys.MoveY, move.joystickValue.y);
            }
        }

        void On_JoystickMoveEnd(MovingJoystick move)
        {
            if (move.joystick == m_Joystick)
            {
                HandleVKey(GameVKeys.MoveX, 0);
                HandleVKey(GameVKeys.MoveY, 0);
            }
        }


        void On_ButtonDown(string buttonName)
        {
            if (m_Button.name == buttonName)
            {
                HandleVKey(GameVKeys.SpeedUp, 2);
            }
        }

        void On_ButtonUp(string buttonName)
        {
            if (m_Button.name == buttonName)
            {
                HandleVKey(GameVKeys.SpeedUp, 1);
            }
        }

        //-------------------------------------------------------------------

        /// <summary>
        /// using callback function to enable listeners to handle key events
        /// </summary>
        /// <param name="vkey">Vkey.</param>
        /// <param name="arg">Argument.</param>
        private void HandleVKey(int vkey, float arg)
        {
            if (OnVkey != null)
            {
                OnVkey(vkey, arg);
            }
        }


        //-------------------------------------------------------------------
        //keyboard control
        #region keyboard control

        void Update()
        {
            HandleKey(KeyCode.A, GameVKeys.MoveX, -1, GameVKeys.MoveX, 0);
            HandleKey(KeyCode.D, GameVKeys.MoveX, 1, GameVKeys.MoveX, 0);
            HandleKey(KeyCode.W, GameVKeys.MoveY, 1, GameVKeys.MoveY, 0);
            HandleKey(KeyCode.S, GameVKeys.MoveY, -1, GameVKeys.MoveY, 0);
            HandleKey(KeyCode.Space, GameVKeys.SpeedUp, 2, GameVKeys.SpeedUp, 1);
        }

        /// <summary>
        /// iterate on keys
        /// convert physical keys to virtual key
        /// </summary>
        /// <param name="key">KeyCode for physical key</param>
        /// <param name="press_vkey">when key pressed, the corresponding virtual key code</param>
        /// <param name="press_arg">parameter for virtual key</param>
        /// <param name="release_vkey">virtual key code when the physical key is released</param>
        /// <param name="relase_arg">parameters for virtual key when the physical key is released</param>
        private void HandleKey(KeyCode key, int press_vkey, float press_arg, int release_vkey, float relase_arg)
        {
            if (Input.GetKey(key))
            {
                if (!m_MapKeyState[key])
                {
                    m_MapKeyState[key] = true;
                    HandleVKey(press_vkey, press_arg);//convert to virtual key
                }
            }
            else
            {
                if (m_MapKeyState[key])
                {
                    m_MapKeyState[key] = false;
                    HandleVKey(release_vkey, relase_arg);//convert to vritual key
                }
            }
        }

        #endregion
        //-------------------------------------------------------------------
    }
}
