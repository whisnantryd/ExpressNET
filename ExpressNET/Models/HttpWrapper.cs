using System;
using System.IO;
using System.IO.Compression;
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

        public bool CanAcceptEncoding(string encoding)
        {
            for (int i = 0; i < Request.Headers.Count; i++)
            {
                var key = Request.Headers.Keys[i];
                var value = Request.Headers[key];

                if (key == "Accept-Encoding")
                {
                    return value.Contains("gzip");
                }
            }

            return false;
        }

    }

    public class HttpResponseWrapper
    {

        public bool IsClosed { get; private set; }

        public bool IsHandled { get; private set; }

        public HttpListenerResponse Response { get; private set; }

        public HttpResponseWrapper(HttpListenerResponse res)
        {
            Response = res;
        }

        public HttpResponseWrapper(HttpListenerResponse res, Func<string, bool> acceptsencoding)
        {
            Response = res;

            if (acceptsencoding == null)
            {
                acceptsEncoding = (encoding) =>
                {
                    return false;
                };
            }
            else
            {
                acceptsEncoding = acceptsencoding;
            }
        }

        public HttpResponseWrapper Send(string msg)
        {
            if (IsClosed)
                throw new InvalidOperationException("attempt to send on closed response");

            var buff = Encoding.Default.GetBytes(msg);

            if (Response.OutputStream.CanWrite)
            {
                if (acceptsEncoding("gzip"))
                {
                    Response.AppendHeader("Content-Encoding", "gzip");

                    using (GZipStream zipstream = new GZipStream(Response.OutputStream, CompressionMode.Compress))
                    {
                        zipstream.Write(buff, 0, buff.Length);
                    }
                }
                else
                {
                    Response.OutputStream.Write(buff, 0, buff.Length);
                }

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

            IsHandled = IsClosed = true;

            return this;
        }

        public HttpResponseWrapper Send(System.Drawing.Image image)
        {
            Response.AddHeader("Content-Type", "image/png");
            //Response.AddHeader("Content-Length", image.);

            if (acceptsEncoding("gzip"))
            {
                Response.AppendHeader("Content-Encoding", "gzip");

                using (GZipStream zipstream = new GZipStream(Response.OutputStream, CompressionMode.Compress))
                {
                    image.Save(zipstream, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            else
            {
                image.Save(Response.OutputStream, System.Drawing.Imaging.ImageFormat.Png);
            }
            
            Response.OutputStream.Flush();
            Response.OutputStream.Close();

            IsHandled = true;

            return this;
        }

        private Func<string, bool> acceptsEncoding;
        
    }

}
