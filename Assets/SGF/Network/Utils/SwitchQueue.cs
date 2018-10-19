using System.Collections;

namespace SGF.Network.Utils
{
    public class Utility
    {

        public static void Swap<QT>(ref QT t1, ref QT t2)
        {

            QT temp = t1;
            t1 = t2;
            t2 = temp;
        }
    }

    public class SwitchQueue<T> where T : class
    {

        private Queue consumeQueue;
        private Queue produceQueue;

        public SwitchQueue()
        {
            consumeQueue = new Queue(16);
            produceQueue = new Queue(16);
        }

        public SwitchQueue(int capcity)
        {
            consumeQueue = new Queue(capcity);
            produceQueue = new Queue(capcity);
        }

        // producer
        public void Push(T obj)
        {
            lock (produceQueue)
            {
                produceQueue.Enqueue(obj);
            }
        }

        // consumer.
        public T Pop()
        {

            return (T)consumeQueue.Dequeue();
        }

        public bool Empty()
        {
            return 0 == consumeQueue.Count;
        }

        public void Switch()
        {
            lock (produceQueue)
            {
                Utility.Swap(ref consumeQueue, ref produceQueue);
            }
        }

        public void Clear()
        {
            lock (produceQueue)
            {
                consumeQueue.Clear();
                produceQueue.Clear();
            }
        }
    }
}
