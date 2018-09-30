using ProtoBuf;

namespace Snaker.GameCore.Data
{
    [ProtoContract]
    public class MapData
    {
        /// <summary>
        /// id of map, can be used to find the resources of the map
        /// </summary>
        [ProtoMember(1)]
        public int id = 0;
        /// <summary>
        /// name of the map, used to show in UI
        /// </summary>
		[ProtoMember(2)]
        public string name = "";

    }
}

