using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snaker.Service.Core
{
    /// <summary>
    /// this class handles message and event between modules
    /// </summary>
    public class ModuleManager:ServiceModule<ModuleManager>
    {
        class MessageObject
        {
            public string target;
            public string msg;
            public object[] args;
        }
        private Dictionary<string, BusinessModule> moduleMaps;
        private Dictionary<string, EventManager> modulePreListenEvents;
        private Dictionary<string, List<MessageObject>> cachedMessages;

        private string m_domain; //store the namespace of modules

        public ModuleManager() {
            moduleMaps = new Dictionary<string, BusinessModule>();
            cachedMessages = new Dictionary<string, List<MessageObject>>();
            modulePreListenEvents = new Dictionary<string, EventManager>();
        }

        public void Init(string domain = "Snaker.Module") {
            CheckSingleton();
            m_domain = domain;
        }

        public T CreateModule<T>(object args = null) where T : BusinessModule {
            return (T) CreateModule(typeof(T).Name, args);
        }

        public BusinessModule CreateModule(string name, object args = null) {
            if (moduleMaps.ContainsKey(name)) {
                return null;
            }
            BusinessModule module = null;
            Type type = Type.GetType(m_domain + "." + name);
            if (type != null)
            {
                module = Activator.CreateInstance(type) as BusinessModule;
            }
            else {
                module = new LuaModule(name);
            }
            moduleMaps.Add(name, module);

            //handle pre listened events
            if (modulePreListenEvents.ContainsKey(name)) {
                EventManager eventManager = modulePreListenEvents[name];
                modulePreListenEvents.Remove(name);
                module.SetEventManager(eventManager);

            }

            module.Create(args);

            //handle cached message
            if (cachedMessages.ContainsKey(name)) {
                List<MessageObject> msgList = cachedMessages[name];
                foreach(MessageObject msgObj in msgList){
                    module.HandleMsg(msgObj.msg, msgObj.args);
                }
                cachedMessages.Remove(name);
            }

            return module;

        }

        public void ReleaseModule(BusinessModule module) {
            if (module != null)
            {
                if (moduleMaps.ContainsKey(module.Name))
                {
                    moduleMaps.Remove(module.Name);
                    module.Release();
                }
                else {

                }
            }
        }

        public void ReleaseAll() {
            foreach (var preListenEvent in modulePreListenEvents) {
                preListenEvent.Value.Clear();
            }
            modulePreListenEvents.Clear();
            cachedMessages.Clear();

            foreach (var module in moduleMaps) {
                module.Value.Release();
            }
            moduleMaps.Clear();
        }

        public T GetModule<T>() where T : BusinessModule
        {
            return (T)GetModule(typeof(T).Name);
        }

        public BusinessModule GetModule(String name)
        {
            if (moduleMaps.ContainsKey(name))
            {
                return moduleMaps[name];
            }
            return null;
        }

        //=========    Handle Messages ============
        public void SendMessage(string target, string msg, params object[] args)
        {
            BusinessModule module = GetModule(target);
            if (module != null)
            {
                module.HandleMsg(msg, args);
            }
            else
            {
                List<MessageObject> msgList = GetCachedMsgList(target);
                MessageObject msgObj = new MessageObject();
                msgObj.target = target;
                msgObj.msg = msg;
                msgObj.args = args;
                msgList.Add(msgObj);
            }
        }

        private List<MessageObject> GetCachedMsgList(string target) {
            List<MessageObject> msgList = null;
            if (!cachedMessages.ContainsKey(target))
            {
                msgList = new List<MessageObject>();
                cachedMessages.Add(target, msgList);
            }
            else
            {
                msgList = cachedMessages[target];
            }
            return msgList;


        }


        //==========
        public ModuleEvent Event(string target, string type) {
            ModuleEvent moduleEvent = null;
            BusinessModule module = GetModule(target);
            if (module != null)
            {
                moduleEvent = module.Event(type);
            }
            else
            {
                EventManager eventManager = GetPreListenEvents(target);
                moduleEvent = eventManager.GetEvent(type);
            }
            return moduleEvent;
        }

        private EventManager GetPreListenEvents(string target)
        {
            EventManager eventManager = null;
            if (!modulePreListenEvents.ContainsKey(target))
            {
                eventManager = new EventManager();
                modulePreListenEvents.Add(target, eventManager);
            }
            else {
                eventManager = modulePreListenEvents[target];
            }
            return eventManager;
        }


    }
}
