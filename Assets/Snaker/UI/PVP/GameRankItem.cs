using SGF.Network.FSPLite.Server.Data;
using SGF.UI.Component;
using Snaker.GameCore.Data;
using Snaker.GameCore.Player;
using Snaker.Service.UIManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace Assets.Snaker.UI.PVP
{
    class GameRankItem : UIListItem
    {
        public Text txtPlayerInfo;

        private PlayerData m_data;

        public override void UpdateItem(int index, object data)
        {
            m_data = data as PlayerData;
            if (m_data != null)
            {
                if (m_data.name != null)
                    txtPlayerInfo.text = "[" + m_data.id + "] " + m_data.name + ": " + m_data.Score.ToString();
                else
                    txtPlayerInfo.text = "[" + m_data.id + "] " + "AI: " + m_data.Score.ToString();
            }
        }

        void OnGUI()
        {
            if (m_data != null)
            {
                if(m_data.name!=null)
                    txtPlayerInfo.text = "[" + m_data.id + "] " +m_data.name+ ": " + m_data.Score.ToString();
                else
                    txtPlayerInfo.text = "[" + m_data.id + "] " +"AI: " + m_data.Score.ToString();
            }
        }

    }
}
