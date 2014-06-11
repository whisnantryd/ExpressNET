using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Debouncehouse.ExpressNET
{
    public static class Extensions
    {

        /// <summary>
        /// Converts the string to standardized route path format ('api/v1' -> '/API/V1')
        /// </summary>
        public static string ToRoute(this string obj)
        {
            obj = "/" + obj.Trim(new char[] { '/' }).ToUpper();

            return obj;
        }

    }
}
