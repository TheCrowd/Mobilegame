using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SGF.Unity;

namespace Snaker.Service.UIManager
{
    /// <summary>
    /// UI uitility functions
    /// </summary>
    public static class UIUtils
    {
        /// <summary>
        /// set the visibility of a UI
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="value"></param>
        public static void SetActive(UIBehaviour ui, bool value)
        {
            if (ui != null && ui.gameObject != null)
            {
                GameObjectUtils.SetActiveRecursively(ui.gameObject, value);
            }
        }

        public static void SetButtonText(Button btn, string text)
        {
            Text objText = btn.transform.GetComponentInChildren<Text>();
            if (objText != null)
            {
                objText.text = text;
            }
        }

        public static string GetButtonText(Button btn)
        {
            Text objText = btn.transform.GetComponentInChildren<Text>();
            if (objText != null)
            {
                return objText.text;
            }
            return "";
        }

        public static void SetChildText(UIBehaviour ui, string text)
        {
            Text objText = ui.transform.GetComponentInChildren<Text>();
            if (objText != null)
            {
                objText.text = text;
            }
        }
    }
}
