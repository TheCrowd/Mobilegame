
namespace SGF.Network
{
    public interface IConnection
    {
        void Connect(string ip, int port);
        void Close();
        void Dispose();
        int Send(byte[] buffer, int len);
        int Recv(byte[] buffer, int offset);

    }
}

