using SGF.Network.Utils;
using Snaker.Service.Core;

namespace SGF.Network
{
    public class NetworkManager : ServiceModule<NetworkManager>
    {
        public void Init()
        {
            IPUtils.CheckSelfIPAddress();
        }
    }
}
