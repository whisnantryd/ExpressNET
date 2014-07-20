using System.Collections.Generic;

namespace Debouncehouse.ExpressNET.Helpers
{
    public class DynamicDictionary<T1, T2>
    {

        public Dictionary<T1, T2> dict;

        public DynamicDictionary()
        {
            dict = new Dictionary<T1, T2>();
        }

        public T2 this[T1 key]
        {
            get
            {
                if (dict.ContainsKey(key))
                    return (T2)dict[key];
                else
                    return default(T2);
            }
            set
            {
                if (dict.ContainsKey(key))
                    dict[key] = value;
                else
                    dict.Add(key, value);
            }
        }

    }
}
