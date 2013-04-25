using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CurveFever3DClient
{
    public static class ObjectCache
    {
        static Dictionary<string, object> cache = new Dictionary<string, object>();

        public static T Get<T>(string name, Func<T> createMethod)
        {
            if (cache.Keys.Contains(name)) return (T)cache[name];
            else
            {
                object o = createMethod();
                cache.Add(name, o);
                return (T)o;
            }
        }
    }
}
