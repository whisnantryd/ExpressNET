using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Debouncehouse.ExpressNET
{

    public class Router
    {
        const string METHOD_GET = "GET";
        const string METHOD_POST = "POST";

        public string BasePath { get; private set; }

        public string AllPath
        {
            get
            {
                return BasePath + "/*";
            }
        }

        private List<RouteRepository> Repos { get; set; }

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
            Repos = new List<RouteRepository>();
        }

        private RouteRepository findRepo(string method)
        {
            var repo = Repos.Find(r => r.Method == method);

            if (repo == null)
                Repos.Add(repo = new RouteRepository(method));

            return repo;
        }

        public Router GET(string path, Action<HttpListenerRequest, HttpListenerResponse> handler)
        {
            findRepo(METHOD_GET).Add(new Route(BasePath + path.ToRoute(), handler));

            return this;
        }

        public Router POST(string path, Action<HttpListenerRequest, HttpListenerResponse> handler)
        {
            findRepo(METHOD_POST).Add(new Route(BasePath + path.ToRoute(), handler));

            return this;
        }

        public List<Route> FindRoutes(HttpListenerRequest req)
        {
            var method = req.HttpMethod.ToUpper();
            var path = req.RawUrl.Split('?')[0].ToRoute();
            var repo = findRepo(method);

            var routelist = new List<Route>();

            foreach (var str in new string[] { AllPath, path })
                routelist.AddRange(repo.Where(r => r.Path == str));

            path = null;
            method = null;

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
        
        public Action<HttpListenerRequest, HttpListenerResponse> Handler { get; private set; }

        public ushort SortIndex { get; private set; }

        public Route(string path, Action<HttpListenerRequest, HttpListenerResponse> handler, ushort sortindex = 0)
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
