using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.SGF.Network.FSPLite
{
    public enum FSPGameState
    {
        /// <summary>
        /// 0 initial state
        /// </summary>
        None=0,

        /// <summary>
        /// This state suggests that the game is under creation
        /// Only in this state can other players join the game
        /// When all palyers have send out VKey.GameBegin, enter the next state
        /// </summary>
        Create,
        /// <summary>
        /// this state suggests that game starts
        /// under this state, when all players has sent VKey.RoundBegin, or check if there are palyers disconnected
        /// when a palyer is disconnected, delete the player from FSPGame
        /// check how many palyers extst, if there are only 1 player, enter GameEnd state, 
        /// otherwise game state will not be influenced
        /// </summary>
        GameBegin,

        /// <summary>
        /// this state suggests that round begins
        /// (in this state, clients can load neccessary resources)
        /// in this state, when all players have sent Vkey.ControlStart, or check if there are players disconnected
        /// when all players have sent VKey.ControlStart, enter the next state
        /// when a player is disconnected, delete the player from FSPGame
        /// if there is only 1 player remaining, enter GameEnd state
        /// otherwise game state will not be influenced
        /// </summary>
        RoundBegin,

        /// <summary>
        /// this state suggests that players can start to control snakers
        /// (given that different player has different loading speed, we need to a state to unify the process)
        /// in this state, receive players' operation Vkey or Vkey.RoundEnd,Vkey.GameExit
        /// after all players has sent Vkey.RoundEnd, enter next state
        /// when a player is DCed, or Vkey.GameExit is sent, delete the player from FSPGame
        /// if there are only 1 player, enter GameEnd state, otherwise game state would not change.
        /// </summary>
        ControlStart,

        /// <summary>
        ///  (most game has only one round, but some has many rounds, determined by client logic)
        ///  in the state, wait for Vkey.GameEnd, or Veky.RoundBegin(if the game has many rounds)
        ///  when all players has sent VKey.GameEnd, or Vkey.RoundBegin, enter next state
        ///  When a player is DCed, delete the player from FSPGame
        ///  if there is only one player, enter GameEnd state, otherwise game state will not be influenced.
        /// </summary>
        RoundEnd,

        /// <summary>
        /// in this state, no longer receive Vkey, send Vkey.GameEnd to all players, wait for FSPServer to be closed
        /// </summary>
        GameEnd
    }
}
