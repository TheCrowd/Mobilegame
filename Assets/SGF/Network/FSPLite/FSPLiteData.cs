using System.Collections.Generic;
using ProtoBuf;
using SGF.ProtoBuf;

namespace SGF.Network.FSPLite
{
    //==========================================================
    #region FSP initial params

    [ProtoContract]
    public class FSPParam
    {
        [ProtoMember(1)]
        public string host;
        [ProtoMember(2)]
        public int port;
        [ProtoMember(3)]
        public uint sid;
        [ProtoMember(4)]
        public int serverFrameInterval = 66;
        [ProtoMember(5)]
        public int serverTimeout = 12000;//ms
        [ProtoMember(6)]
        public int clientFrameRateMultiple = 2;
        [ProtoMember(7)]
        public bool enableSpeedUp = true;
        [ProtoMember(8)]
        public int defaultSpeed = 1;
        [ProtoMember(9)]
        public int frameBufferSize = 0;
        [ProtoMember(10)]
        public bool enableAutoBuffer = true;
        [ProtoMember(11)]
        public int maxFrameId = 1800;
        [ProtoMember(12)]
        public bool useLocal = false;
        [ProtoMember(13)]
        public int authId = 0;


        public FSPParam Clone()
        {
            byte[] buffer = PBSerializer.NSerialize(this);
            return (FSPParam)PBSerializer.NDeserialize(buffer, typeof(FSPParam));
        }
    }
    #endregion

    //==========================================================

    //==========================================================
    #region data from client from server
    [ProtoContract]
    public class FSPDataC2S
    {
        [ProtoMember(1)]
        public ushort sid = 0;
        [ProtoMember(2)]
        public List<FSPVKey> vkeys = new List<FSPVKey>();
    }

    #endregion


    //==========================================================
    #region data from server to client
    [ProtoContract]
    public class FSPDataS2C
    {
        [ProtoMember(1)]
        public List<FSPFrame> frames = new List<FSPFrame>();
    }


    #endregion


    #region date shared by client and server

    /// <summary>
    /// to compatible with keyboard and joysticker, abstract operations in the formate [virtual key+args]
    /// </summary>
    [ProtoContract]
    public class FSPVKey
    {
        /// <summary>
        /// Virtual Key value
        /// </summary>
        [ProtoMember(1)]
        public int vkey;

        /// <summary>
        /// arguments
        /// </summary>
        [ProtoMember(2)]
        public int[] args;

        /// <summary>
        /// S2C  server send PlayerId
        /// C2S  clients upload ClientFrameId
        /// </summary>
        [ProtoMember(3)]
        public uint playerIdOrClientFrameId;

        public uint playerId
        {
            get { return playerIdOrClientFrameId; }
            set { playerIdOrClientFrameId = value; }
        }

        public uint clientFrameId
        {
            get { return playerIdOrClientFrameId; }
            set { playerIdOrClientFrameId = value; }
        }

        public override string ToString()
        {
            return "{vkey:" + vkey + ",arg:" + args[0] + ",playerIdOrClientFrameId:" + playerIdOrClientFrameId + "}";
        }


    }



    [ProtoContract]
    public class FSPFrame //send from server to client
    {
        [ProtoMember(1)]
        public int frameId;
        [ProtoMember(2)]
        public List<FSPVKey> vkeys = new List<FSPVKey>();

        public bool IsEquals(FSPFrame obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj.ToString() == this.ToString();
        }


        public bool IsEmpty()
        {
            if (vkeys == null || vkeys.Count == 0)
            {
                return true;
            }
            return false;
        }

        public bool ContainsVKey(int vkey)
        {
            if (!IsEmpty())
            {
                for (int i = 0; i < vkeys.Count; i++)
                {
                    if (vkeys[i].vkey == vkey)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            string tmp = "";

            if (vkeys != null && vkeys.Count > 0)
            {
                for (int i = 0; i < vkeys.Count - 1; i++)
                {
                    tmp += vkeys[i].ToString() + ",";
                }
                tmp += vkeys[vkeys.Count - 1].ToString();
            }

            return "{frameId:" + frameId + ", vkeys:[" + tmp + "]}";
        }
    }


    //==========================================================
    //shared data

    #endregion

}
