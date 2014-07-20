using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debouncehouse.ExpressNET.Routes
{
    public class RouteParameter
    {

        public string Key { get; private set; }

        public ushort Index { get; private set; }

        public RouteParameter(string key, ushort index)
        {
            Key = key;
            Index = index;
        }

        public string Value(string requestpath)
        {
            var parts = requestpath.Split('/');

            if (parts.Length > Index)
                return parts[Index];

            return null;
        }

    }
}
