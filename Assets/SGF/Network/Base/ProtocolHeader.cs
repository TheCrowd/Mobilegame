namespace SGF.Network
{
    public class ProtocolHeader
    {
        public const int Length = 14;
        public uint pid = 0;
        public uint index = 0;
        public int dataSize = 0;
        public ushort checksum = 0;
        public static ProtocolHeader Deserialize(NetBuffer buffer)
        {
            ProtocolHeader head = new ProtocolHeader();
            head.pid = buffer.ReadUInt();
            head.index = buffer.ReadUInt();
            head.dataSize = buffer.ReadInt();
            head.checksum = buffer.ReadUShort();
            return head;
        }

        public NetBuffer Serialize(NetBuffer buffer)
        {
            buffer.WriteUInt(pid);
            buffer.WriteUInt(index);
            buffer.WriteInt(dataSize);
            buffer.WriteUShort(checksum);
            return buffer;
        }

    }
}
