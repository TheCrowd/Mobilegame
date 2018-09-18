using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OT.Foundation;
using System.Reflection;

namespace Snaker.Service.Core
{
    //an abstract module regarding to business operations
    public abstract class BusinessModule:Module
    {
        private string moduleName = "";

        public string Name {
            get {
                if (string.IsNullOrEmpty(moduleName)) {
                    moduleName = this.GetType().Name;
                }
                return moduleName;
            }
        }
        public string moduleTitle;

        //======= constructor ===================
        public BusinessModule() {

        }

        internal BusinessModule(string name) {
            moduleName = name;
        }

        internal void SetEventManager(EventManager _eventManager) {
            this.eventManager = _eventManager;
        }

        public EventManager eventManager;

        public ModuleEvent Event(string type) {
            return GetEventManager().GetEvent(type);
        }

        protected EventManager GetEventManager() {
            if (eventManager == null) {
                eventManager = new EventManager();
            }
            return eventManager;
        }

        //======= Operations =================
        public virtual void Create(object args = null) {
            this.Log("Create() args={0}", args);
        }

        public override void Release()
        {
            if (eventManager != null) {
                eventManager.Clear();
                eventManager = null;
            }

            base.Release();
        }

        public virtual void Show()
        {
            this.Log("Show()");
        }

        //====== Message
        internal void HandleMsg(string msg, object[] args) {
            this.Log("HandleMessage() msg:{0}, args{1}", msg, args);

            MethodInfo mi = this.GetType().GetMethod(msg, System.Reflection.BindingFlags.NonPublic|BindingFlags.Instance);
            if (mi != null)
            {
                mi.Invoke(this, BindingFlags.NonPublic, null, args, null);
            }
            else {
                OnModuleMessage(msg, args);
            }
        }

        protected virtual void OnModuleMessage(string msg, object[] args) {
            this.Log("onModuleMessage() msg:{0}, args{1}", msg, args);
        }
    }   
}
