using System;
using System.Net;
using System.Text;
using Debouncehouse.ExpressNET.Enums;
using Debouncehouse.ExpressNET.Helpers;

namespace Debouncehouse.ExpressNET.Models
{

    public class HttpRequestWrapper
    {

        public HttpListenerRequest Request { get; private set; }

        public HTTP RequestMethod { get; private set; }

        public string RequestPath
        {
            get
            {
                var ret = Request.RawUrl.Split('?')[0].ToRoute();

                if (basePath != "/")
                    ret = ret.Replace(basePath, "");

                return ret;
            }
        }

        public DynamicDictionary<string, string> Parameters = new DynamicDictionary<string, string>();

        public string[] Parts { get; private set; }

        private string basePath;

        public HttpRequestWrapper(HttpListenerRequest request, string basepath)
        {
            this.Request = request;
            this.basePath = basepath.ToRoute();
            this.Parts = RequestPath.Split('/');

            RequestMethod = (HTTP)Enum.Parse(typeof(HTTP), Request.HttpMethod.ToUpper());
        }

    }

    public class HttpResponseWrapper
    {

        public bool IsClosed { get; private set; }

        public bool IsHandled { get; private set; }

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

                IsHandled = true;

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

            IsHandled = true;

            IsClosed = true;

            return this;
        }

    }

}
