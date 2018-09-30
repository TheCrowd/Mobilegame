using SGF.Extension;
using SGF.Random;
using Snaker.GameCore.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Snaker.GameCore
{
    /// <summary>
    /// game context
    /// information about a single game
    /// </summary>
    public class GameContext
    {
        /// <summary>
        /// game parameters
        /// </summary>
        public GameParam param = null;

        /// <summary>
        /// random number generator
        /// </summary>
        public RandomGen random = new RandomGen();

        /// <summary>
        /// current frame number
        /// </summary>
        public int currentFrameIndex = 0;


        /// <summary>
        /// the size of game map
        /// </summary>
        public Vector3 mapSize = new Vector3();


        //======================================================================
        private DictionaryExt<int, Color> m_mapColor = new DictionaryExt<int, Color>();
        public Color GetUniqueColor(int colorId)
        {
            if (m_mapColor.ContainsKey(colorId))
            {
                return m_mapColor[colorId];
            }

            Color c = new Color(random.Rnd(), random.Rnd(), random.Rnd());
            m_mapColor.Add(colorId, c);
            return c;
        }

        //======================================================================
        public Vector3 EntityToViewPoint(Vector3 pos)
        {
            pos = pos - mapSize / 2;
            pos.z = 0;

            return pos;
        }
    }
}

