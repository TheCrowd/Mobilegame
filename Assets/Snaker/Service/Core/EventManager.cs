using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Snaker.Service.Core
{
    public class ModuleEvent : UnityEvent<object>
    {

    }

    public class ModuleEvent<T> : UnityEvent<T>
    {

    }

    //EventManager manages event queue, the queue is stored in a Dictionary
    public class EventManager
    {
        private Dictionary<string, ModuleEvent> eventMaps;


        //get the ModuleEvent of a specific type
        public ModuleEvent GetEvent(string type)
        {
            if (eventMaps == null) {
                eventMaps = new Dictionary<string, ModuleEvent>();
            }
            if (!eventMaps.ContainsKey(type)) {
                eventMaps.Add(type, new ModuleEvent());
            }
            return eventMaps[type];
        }


        //clear all ModuleEvent
        public void Clear() {
            if (eventMaps != null) {
                foreach (var item in eventMaps) {
                    item.Value.RemoveAllListeners();
                }
                eventMaps.Clear();
            }
        }
    }
    
    
}
