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

        public static string ToRoute(this string obj)
        {
            obj = "/" + obj.Trim(new char[] { '/' }).ToUpper();

            return obj;
        }

        public static HttpListenerResponse Send(this HttpListenerResponse res, string msg)
        {
            var buff = Encoding.Default.GetBytes(msg);

            res.OutputStream.Write(buff, 0, buff.Length);

            return res;
        }

    }
}
