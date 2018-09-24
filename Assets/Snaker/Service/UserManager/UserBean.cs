using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Snaker.Service.UserManager
{
    /// <summary>
    /// User Bean
    /// use ProtoBuf to serilize and deserilize
    /// </summary>
    [ProtoContract]
    public class UserBean
    {
        [ProtoMember(1)]
        public uint id;
        [ProtoMember(2)]
        public string name;
        [ProtoMember(3)]
        public int level;
        [ProtoMember(4)]
        public int defaultSnakeId;

    }
}