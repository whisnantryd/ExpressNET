using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debouncehouse.ExpressNET.Models
{
    public class StaticFileProvider : IMiddleware
    {

        public StaticFileProvider(DirectoryInfo directory)
        {
            if (!directory.Exists)
                directory.Create();
        }

        #region IMiddleware Members

        public bool Process(HttpRequestWrapper req, HttpResponseWrapper res)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
