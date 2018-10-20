using ProtoBuf;

namespace Snaker.GameCore.Data
{
    /// <summary>
    /// player's data in a game
    /// </summary>
    [ProtoContract]
    public class PlayerData
    {
        /// <summary>
        /// id in a game, only valid in a specific game
        /// </summary>
        [ProtoMember(1)]
        public uint id;

        /// <summary>
        /// user id of the player, global unique
        /// </summary>
        [ProtoMember(2)]
        public uint userId;

        /// <summary>
        /// players' name
        /// </summary>
        [ProtoMember(3)]
        public string name;

        /// <summary>
        /// snaker data for the player
        /// </summary>
        [ProtoMember(4)]
        public SnakeData snakeData = new SnakeData();

        /// <summary>
        /// team id for the player, if it is single mode, it is equal to player id
        /// </summary>
        [ProtoMember(5)]
        public int teamId = 0;

        /// <summary>
        /// score for the player
        /// </summary>
        [ProtoMember(6)]
        public int score = 0;

        /// <summary>
        /// id for AI players or disconnected players, if it is equal to 0, it suggests that it is not a ai player.
        /// </summary>
        [ProtoMember(7)]
        public int ai = 0;

        public int Score { get { return score; } }


    }
}

