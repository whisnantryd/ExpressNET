using System.IO;

namespace Debouncehouse.ExpressNET.Models
{
    public class StaticFileProvider : IMiddleware
    {

        private DirectoryInfo baseDirectory;

        public StaticFileProvider(string directory)
            : this(new DirectoryInfo(directory))
        {
        }

        public StaticFileProvider(DirectoryInfo directory)
        {
            if (!directory.Exists) { directory.Create(); }

            baseDirectory = directory;
        }

        #region IMiddleware Members

        public bool Process(HttpRequestWrapper req, HttpResponseWrapper res)
        {
            var path = baseDirectory.FullName + req.RequestPath.Replace("/", @"\");

            if (File.Exists(path))
            {
                var fileinfo = new FileInfo(path);

                res.Response.StatusCode = 200;
                res.Response.ContentType = "text/" + fileinfo.Extension.TrimStart('.');
                res.SendThenClose(File.ReadAllText(path));

                return true;
            }
            else
                res.Response.StatusCode = 404;

            return false;
        }

        #endregion

    }
}
