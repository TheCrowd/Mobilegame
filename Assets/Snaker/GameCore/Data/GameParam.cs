
using ProtoBuf;

namespace Snaker.GameCore.Data
{
    /// <summary>
    /// parameters for launch game
    /// </summary>
    [ProtoContract]
    public class GameParam
    {
        /// <summary>
        /// GameId, id assigned to each game
        /// will use it in PVP mode
        /// </summary>
        [ProtoMember(1)]
        public int id = 0;

        /// <summary>
        /// determine which map this game will use
        /// </summary>
        [ProtoMember(2)]
        public MapData mapData = new MapData();


        /// <summary>
        /// random seed, enable different clients to generate the same random numbers
        /// </summary>
        [ProtoMember(3)]
        public int randSeed = 0;


        /// <summary>
        /// game mode, default is unlimited PVE
        /// </summary>
        [ProtoMember(4)]
        public GameMode mode = GameMode.UnlimitedPVE;


        /// <summary>
        /// time limit for limited mode,default is 150 seconds
        /// </summary>
        [ProtoMember(5)]
        public int limitedTime = 150;
    }
}

