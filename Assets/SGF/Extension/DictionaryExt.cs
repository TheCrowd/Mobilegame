using System;
using System.Collections.Generic;

namespace SGF.Extension
{
    public class DictionaryExt<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public new TValue this[TKey indexKey]
        {
            set { base[indexKey] = value; }
            get
            {
                try
                {
                    return base[indexKey];
                }
                catch (Exception)
                {
                    return default(TValue);
                }
            }
        }
    }
}
