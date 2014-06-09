using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Debouncehouse.ExpressNET.Models;

namespace Debouncehouse.ExpressNET
{

    public class Router
    {
        const string METHOD_GET = "GET";
        const string METHOD_POST = "POST";

        private string basePath;
        public string BasePath
        {
            get
            {
                return basePath == "/" ? "" : basePath;
            }
            private set
            {
                basePath = value;
            }
        }

        public string AllPath
        {
            get
            {
                return BasePath + "/*";
            }
        }

        private List<RouteRepository> repos { get; set; }

        public Router()
        {
            init("");
        }

        public Router(string basepath)
        {
            init(basepath);
        }

        private void init(string basepath)
        {
            BasePath = basepath.ToRoute();
            repos = new List<RouteRepository>();
        }

        private RouteRepository findRepo(string method)
        {
            var repo = repos.Find(r => r.Method == method);

            if (repo == null)
                repos.Add(repo = new RouteRepository(method));

            return repo;
        }

        public Router GET(string path, RequestHandler handler)
        {
            findRepo(METHOD_GET).Add(new Route(BasePath + path.ToRoute(), handler));

            return this;
        }

        public Router POST(string path, RequestHandler handler)
        {
            findRepo(METHOD_POST).Add(new Route(BasePath + path.ToRoute(), handler));

            return this;
        }

        public List<Route> FindRoutes(HttpRequestWrapper req)
        {
            //var path = req.RawUrl.Split('?')[0].ToRoute();
            var repo = findRepo(req.RequestMethod);

            var routelist = new List<Route>();

            foreach (var str in new string[] { AllPath, req.RequestPath })
                routelist.AddRange(repo.Where(r => r.Path == str).ToList());

            return routelist;
        }

    }

    public class RouteRepository : IEnumerable<Route>
    {

        public string Method { get; private set; }

        private List<Route> routes { get; set; }

        public RouteRepository(string method)
        {
            Method = method;
            routes = new List<Route>();
        }

        public void Add(Route newroute)
        {
            if (!routes.Contains(newroute))
                routes.Add(newroute);
        }

        #region IEnumerable<Route> Members

        public IEnumerator<Route> GetEnumerator()
        {
            return routes.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return routes.GetEnumerator();
        }

        #endregion
    }

    public class Route
    {
        public string Path { get; private set; }
        
        public RequestHandler Handler { get; private set; }

        public ushort SortIndex { get; private set; }

        public Route(string path, RequestHandler handler, ushort sortindex = 0)
        {
            if (path == null || path.Length < 2)
                throw new ArgumentNullException("path");

            if (handler == null)
                throw new ArgumentNullException("handler");

            Path = path;
            Handler = handler;
            SortIndex = sortindex;
        }

    }
    
}
