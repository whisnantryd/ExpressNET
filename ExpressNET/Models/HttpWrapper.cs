using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Debouncehouse.ExpressNET.Models
{

    public class HttpRequestWrapper
    {

        public HttpListenerRequest Request
        {
            get;
            private set;
        }

        public string RequestPath
        {
            get
            {
                return Request.RawUrl.Replace(basePath, "").Split('?')[0].ToRoute();
            }
        }

        public string RequestMethod
        {
            get
            {
                return Request.HttpMethod.ToUpper();
            }
        }

        private string basePath;

        public HttpRequestWrapper(HttpListenerRequest request, string basepath)
        {
            this.Request = request;
            this.basePath = basepath;
        }

    }

    public class HttpResponseWrapper
    {

        public HttpListenerResponse Response
        {
            get;
            private set;
        }

        public HttpResponseWrapper(HttpListenerResponse res)
        {
            Response = res;
        }

    }

}
