using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpROM.Core.Util
{
    public class PriorityQueue<T, T2>
    {
        private SortedDictionary<T, T2> Data = new SortedDictionary<T, T2>();
        public bool Empty()
        {
            return Data.Count == 0 ? true : false;
        }
        public KeyValuePair<T,T2> Peek()
        {
            return Data.First();
        }
        public KeyValuePair<T, T2> Dequeue()
        {
            KeyValuePair<T, T2> nextItem = Data.First();
            Data.Remove(nextItem.Key);
            return nextItem;
        }
        public bool ContainsKey(T key)
        {
            return Data.ContainsKey(key);
        }
        public T2 Get(T key)
        {
            return Data[key];
        }
        public bool Contains(T key)
        {
            return Data.ContainsKey(key);
        }
        public void Enqueue(T key, T2 value)
        {
            Data.Add(key, value);
        }
    }
}
