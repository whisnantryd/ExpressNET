using System;
using System.Collections.Generic;
using Debouncehouse.ExpressNET.Helpers;

namespace Debouncehouse.ExpressNET.Routes
{

    public class Route
    {

        #region static members

        static ushort sort_index = 0;

        //static ushort getSortIndex()
        //{
        //    return ++sort_index;
        //}

        #endregion

        public string Path { get; private set; }

        public RequestHandler Handler { get; private set; }

        public ushort SortIndex { get; private set; }

        public List<RouteParameter> Parameters { get; private set; }

        public Route(RequestHandler handler)
        {
            Handler = new RequestHandler((req, res) =>
            {
                if (Parameters != null && Parameters.Count > 0)
                    foreach (RouteParameter rp in Parameters)
                        req.Parameters[rp.Key.ToUpper()] = req.Parts[rp.Index];

                handler(req, res);
            });

            SortIndex = ++sort_index;
        }

        public Route(string path, RequestHandler handler)
            : this(handler)
        {
            if (path == null || path.Length < 2)
                throw new ArgumentNullException("path");

            if (handler == null)
                throw new ArgumentNullException("handler");

            Path = path;
            Parameters = parsePathParameters();
        }

        private List<RouteParameter> parsePathParameters()
        {
            var newparameters = new List<RouteParameter>();

            if (Path == null)
                return newparameters;

            var parts = Path.Split('/');

            for (ushort i = 0; i < parts.Length; i++)
            {
                var param = parts[i];

                if (param.StartsWith(":"))
                    newparameters.Add(new RouteParameter(param.TrimStart(':'), i));
            }

            return newparameters;
        }

    }
    
}
