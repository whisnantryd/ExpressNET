using System;
using System.Collections.Generic;
using Debouncehouse.ExpressNET.Helpers;

namespace Debouncehouse.ExpressNET.Routes
{

    public class Route
    {

        #region static members

        private static ushort sort_index = 0;

        private static ushort getSortIndex()
        {
            return ++sort_index;
        }

        #endregion

        public string Path { get; private set; }

        public RequestHandler Handler { get; private set; }

        public ushort SortIndex { get; private set; }

        public List<RouteParameter> Parameters { get; private set; }

        public Route(RequestHandler handler)
        {
            Parameters = new List<RouteParameter>();

            Handler = new RequestHandler((req, res) =>
            {
                if (Parameters.Count > 0)
                    foreach (RouteParameter rp in Parameters)
                        req.Parameters[rp.Key.ToUpper()] = req.Parts[rp.Index];

                handler(req, res);
            });
        }

        public Route(string path, RequestHandler handler)
            : this(handler)
        {
            if (path == null || path.Length < 2)
                throw new ArgumentNullException("path");

            if (handler == null)
                throw new ArgumentNullException("handler");

            Path = path;
            //Handler = handler;
            SortIndex = getSortIndex();
            Parameters = parsePathParameters();
        }

        private List<RouteParameter> parsePathParameters()
        {
            var newparameters = new List<RouteParameter>();

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
