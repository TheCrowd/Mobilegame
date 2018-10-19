using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using SGF.Network.KCP;
using SGF.Logger;
using SGF.ProtoBuf;

namespace SGF.Network.RPCLite
{
    public class RPCService
    {
        private string LOG_TAG = "RPCService";
        public delegate void CustomRPC(object[] args, IPEndPoint targetAddress);

        //KCPSocket
        private KCPSocket mSocket;
        private bool mIsRunning = false;




        private Dictionary<string, RPCMethodHelper> m_MapRPCBind;
        //=================================================================================
        #region Creator and releaser

        public RPCService(int port = 0)
        {
            m_MapRPCBind = new Dictionary<string, RPCMethodHelper>();

            //create a socket
            mSocket = new KCPSocket(port, 1);
            mSocket.AddReceiveListener(OnReceive);
            mSocket.EnableBroadcast = true;
            mIsRunning = true;

            port = mSocket.SelfPort;
            LOG_TAG = LOG_TAG + "[" + port + "]";
            MyLogger.Log(LOG_TAG, "RPCSocket() port:{0}", port.ToString());
        }


        public virtual void Dispose()
        {
            MyLogger.Log(LOG_TAG, "Dispose()");

            mIsRunning = false;

            if (mSocket != null)
            {
                mSocket.Dispose();
                mSocket = null;
            }

            m_MapRPCBind.Clear();

        }

        #endregion
        //=================================================================================
        public IPEndPoint SelfEndPoint { get { return mSocket.SelfEndPoint; } }
        public int SelfPort { get { return mSocket.SelfPort; } }
        public string SelfIP { get { return mSocket.SelfIP; } }

        //=================================================================================


        //=================================================================================
        #region Main thread 

        public void RPCTick()
        {
            if (mIsRunning)
            {
                mSocket.Update();
            }
        }
        #endregion


        //=================================================================================
        //Receive
        #region message receive functions: ACK, SYN, Broadcast
        private void OnReceive(byte[] buffer, int size, IPEndPoint remotePoint)
        {
            try
            {
                var msg = PBSerializer.NDeserialize<RPCMessage>(buffer);
                HandleRPCMessage(msg, remotePoint);
            }
            catch (Exception e)
            {
                MyLogger.LogError(LOG_TAG, "OnReceive()","->HandleMessage->Error:" + e.Message + "\n" + e.StackTrace);
            }
        }


        private void HandleRPCMessage(RPCMessage msg, IPEndPoint target)
        {
            MethodInfo mi = this.GetType().GetMethod(msg.name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (mi != null)
            {
                MyLogger.Log(LOG_TAG, "HandleRPCMessage() DefaultRPC:{0}, Target:{1}", msg.name, target);
                try
                {
                    var args = msg.args.ToList();
                    args.Add(target);

                    mi.Invoke(this, BindingFlags.NonPublic, null, args.ToArray(), null);
                }
                catch (Exception e)
                {
                    MyLogger.LogError(LOG_TAG, "HandleRPCMessage()","DefaultRPC<" + msg.name + ">response error:" + e.Message + "\n" + e.StackTrace + "\n");
                }
            }
            else
            {
                OnBindingRPCInvoke(msg, target);
            }
        }


        private void OnBindingRPCInvoke(RPCMessage msg, IPEndPoint target)
        {
            if (m_MapRPCBind.ContainsKey(msg.name))
            {
                MyLogger.Log(LOG_TAG, "OnBindingRPCInvoke() RPC:{0}, Target:{1}", msg.name, target);

                RPCMethodHelper rpc = m_MapRPCBind[msg.name];


                try
                {
                    rpc.Invoke(msg.args, target);
                }
                catch (Exception e)
                {
                    MyLogger.LogError(LOG_TAG, "OnBindingRPCInvoke()"," RPC<" + msg.name + ">response error:" + e.Message + "\n" + e.StackTrace + "\n");
                }

            }
            else
            {
                MyLogger.LogError(LOG_TAG, "OnBindingRPCInvoke() Unkown RPC request:{0}", msg.name);
            }
        }


        #endregion

        //=================================================================================
        //send
        #region message send functions
        //send SYN message

        private void SendMessage(IPEndPoint target, RPCMessage msg)
        {
            byte[] buffer = PBSerializer.NSerialize(msg);
            mSocket.SendTo(buffer, buffer.Length, target);
        }

        private void SendMessage(List<IPEndPoint> listTargets, RPCMessage msg)
        {
            byte[] buffer = PBSerializer.NSerialize(msg);

            for (int i = 0; i < listTargets.Count; i++)
            {
                IPEndPoint target = listTargets[i];
                if (target != null)
                {
                    mSocket.SendTo(buffer, buffer.Length, target);
                }
            }
        }

        private void SendBroadcast(int beginPort, int endPort, RPCMessage msg)
        {
            byte[] buffer = PBSerializer.NSerialize(msg);

            for (int i = beginPort; i < endPort; i++)
            {
                mSocket.SendTo(buffer, buffer.Length, new IPEndPoint(IPAddress.Broadcast, i));
            }
        }

        #endregion


        //=================================================================================
        //RPC invoke 
        public void RPC(IPEndPoint target, string name, params object[] args)
        {
            MyLogger.Log(LOG_TAG, "RPC() 1 to 1 invoke, name:{0}, target:{1}", name, target);

            RPCMessage msg = new RPCMessage();
            msg.name = name;
            msg.args = args;
            SendMessage(target, msg);

        }

        public void RPC(List<IPEndPoint> listTargets, string name, params object[] args)
        {
            MyLogger.Log(LOG_TAG, "RPC() 1 to many invoke, Begin, msg:{0}", name);

            RPCMessage msg = new RPCMessage();
            msg.name = name;
            msg.args = args;
            SendMessage(listTargets, msg);

            MyLogger.Log(LOG_TAG, "RPC() 1 to many invoke, End!");
        }

        public void RPC(int beginPort, int endPort, string name, params object[] args)
        {
            MyLogger.Log(LOG_TAG, "RPC() broadcast invoke, PortRange:{0}-{1}, Begin, msg:{2}", beginPort.ToString(), endPort, name);

            RPCMessage msg = new RPCMessage();
            msg.name = name;
            msg.args = args;
            SendBroadcast(beginPort, endPort, msg);
        }

        public void RPC(RPCService target, string name, params object[] args)
        {
            RPCMessage msg = new RPCMessage();
            msg.name = name;
            msg.args = args;
            target.HandleRPCMessage(msg, null);
        }

        //==========================================================================

        public void Bind(string name, RPCMethod rpc)
        {
            RPCMethodHelper helper = new RPCMethodHelper();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0>(string name, RPCMethod<T0> rpc)
        {
            RPCMethodHelper<T0> helper = new RPCMethodHelper<T0>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1>(string name, RPCMethod<T0, T1> rpc)
        {
            RPCMethodHelper<T0, T1> helper = new RPCMethodHelper<T0, T1>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2>(string name, RPCMethod<T0, T1, T2> rpc)
        {
            RPCMethodHelper<T0, T1, T2> helper = new RPCMethodHelper<T0, T1, T2>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3>(string name, RPCMethod<T0, T1, T2, T3> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3> helper = new RPCMethodHelper<T0, T1, T2, T3>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4>(string name, RPCMethod<T0, T1, T2, T3, T4> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4> helper = new RPCMethodHelper<T0, T1, T2, T3, T4>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4, T5>(string name, RPCMethod<T0, T1, T2, T3, T4, T5> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4, T5> helper = new RPCMethodHelper<T0, T1, T2, T3, T4, T5>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6> helper = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6, T7>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7> helper = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8> helper = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> helper = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }


    }
}
