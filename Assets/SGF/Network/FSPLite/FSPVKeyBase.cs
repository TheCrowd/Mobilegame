
namespace SGF.Network.FSPLite
{
    class FSPVKeyBase
    {

        /// <summary>
        /// PVP game starts
        /// </summary>
        public const int GAME_BEGIN = 100;

        /// <summary>
        /// round starts
        /// </summary>
        public const int ROUND_BEGIN = 101;

        /// <summary>
        /// start loading
        /// </summary>
        public const int LOAD_START = 102;
        /// <summary>
        /// loading progress
        /// </summary>
        public const int LOAD_PROGRESS = 103;

        /// <summary>
        /// players can start control snake
        /// </summary>
        public const int CONTROL_START = 104;

        /// <summary>
        /// exit during a game
        /// </summary>
        public const int GAME_EXIT = 105;

        /// <summary>
        /// round ends
        /// </summary>
        public const int ROUND_END = 106;

        /// <summary>
        /// PVP ends
        /// </summary>
        public const int GAME_END = 107;

        /// <summary>
        /// UDP authentication ID
        /// </summary>
        public const int AUTH = 108;

        /// <summary>
        /// PING repsonse 
        /// </summary>
        public const int PING = 109;
    }
}
