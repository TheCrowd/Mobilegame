using SGF.Extension;
using System;
using System.Collections.Generic;

namespace Snaker.GameCore.Entity.Factory
{
    public interface IRecyclableObject
    {
        string GetRecycleType();
        void Dispose();
    }

    public class Recycler
    {
        /// <summary>
        /// idle object list
        /// </summary>
        private static DictionaryExt<string, Stack<IRecyclableObject>> idleObjectList;

        public Recycler()
        {
            idleObjectList = new DictionaryExt<string, Stack<IRecyclableObject>>();
        }

        public void Release()
        {
            foreach (var pair in idleObjectList)
            {
                foreach (var obj in pair.Value)
                {
                    obj.Dispose();
                }
                pair.Value.Clear();
            }

        }

        public void Push(IRecyclableObject obj)
        {
            string type = obj.GetRecycleType();
            Stack<IRecyclableObject> stackIdleObject = idleObjectList[type];
            if (stackIdleObject == null)
            {
                stackIdleObject = new Stack<IRecyclableObject>();
                idleObjectList.Add(type, stackIdleObject);
            }
            stackIdleObject.Push(obj);
        }

        public IRecyclableObject Pop(string type)
        {
            Stack<IRecyclableObject> stackIdleObject = idleObjectList[type];
            if (stackIdleObject != null && stackIdleObject.Count > 0)
            {
                return stackIdleObject.Pop();
            }
            return null;
        }
    }
}

