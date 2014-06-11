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

        public bool IsClosed { get; private set; }

        public HttpListenerResponse Response
        {
            get;
            private set;
        }

        public HttpResponseWrapper(HttpListenerResponse res)
        {
            Response = res;
        }

        public HttpResponseWrapper Send(string msg)
        {
            if (IsClosed)
                throw new InvalidOperationException("attempt to send on closed response");

            var buff = Encoding.Default.GetBytes(msg);

            if (Response.OutputStream.CanWrite)
            {
                Response.OutputStream.Write(buff, 0, buff.Length);
                return this;
            }

            throw new InvalidOperationException("write to output stream");
        }

        public HttpResponseWrapper SendThenClose(string msg)
        {
            if (IsClosed)
                throw new InvalidOperationException("attempt to send on closed response");

            Send(msg);

            Response.Close();

            IsClosed = true;

            return this;
        }

    }

}
