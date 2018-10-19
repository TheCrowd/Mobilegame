using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.SGF.Network.FSPLite
{
    // this file defines the state when a game ends
    public enum FSPGameEndState
    {
       Normal=0, // normal exit
       AllOtherExit=1, //all other players exit the game actively
       AllOtherLost=2, //all other palyers disconnect from the game(passively normally)
    }
}
