using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Debouncehouse.ExpressNET.Enums;
using Debouncehouse.ExpressNET.Helpers;
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

        private RouteRepository wares;

        private List<RouteRepository> repos;

        public Router()
            : this("")
        {
        }

        public Router(string basepath)
        {
            BasePath = basepath.ToRoute();

            wares = new RouteRepository();
            repos = new List<RouteRepository>();
        }

        private RouteRepository findRepo(HttpMethodType methodtype)
        {
            var repo = repos.Find(r => r.Method == methodtype);

            if (repo == null)
                repos.Add(repo = new RouteRepository(methodtype));

            return repo;
        }

        public Router Use(IMiddleware middleware)
        {
            wares.Add(new Route((req, res) => { middleware.Process(req, res); }));

            return this;
        }

        public Action<string, RequestHandler> this[HttpMethodType methodtype]
        {
            get
            {
                var repo = findRepo(methodtype);

                return new Action<string, RequestHandler>((path, handler) =>
                {
                    repo.Add(new Route(BasePath + path.ToRoute(), handler));
                });
            }
        }

        [Obsolete()]
        public Router GET(string path, RequestHandler handler)
        {
            findRepo(HttpMethodType.GET).Add(new Route(BasePath + path.ToRoute(), handler));

            return this;
        }

        [Obsolete()]
        public Router POST(string path, RequestHandler handler)
        {
            findRepo(HttpMethodType.POST).Add(new Route(BasePath + path.ToRoute(), handler));

            return this;
        }

        public List<Route> FindRoutes(HttpRequestWrapper req)
        {
            var repo = findRepo(req.RequestMethod);

            var routelist = new List<Route>();

            if(wares.Any())
                routelist.AddRange(wares);

            foreach (var str in new string[] { AllPath, req.RequestPath })
                routelist.AddRange(repo.Where(r => r.Path == str).ToList());

            return routelist;
        }

    }

    public class RouteRepository : IEnumerable<Route>
    {

        public HttpMethodType Method { get; private set; }

        private List<Route> routes { get; set; }

        public RouteRepository()
            : this(HttpMethodType.GET)
        {
        }

        public RouteRepository(HttpMethodType method)
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
        private static ushort sort_index = 0;
        private static ushort get_sort_index()
        {
            return ++sort_index;
        }

        public string Path { get; private set; }
        
        public RequestHandler Handler { get; private set; }

        public ushort SortIndex { get; private set; }

        public Route(RequestHandler handler)
        {
            Handler = handler;
        }

        public Route(string path, RequestHandler handler)
        {
            if (path == null || path.Length < 2)
                throw new ArgumentNullException("path");

            if (handler == null)
                throw new ArgumentNullException("handler");

            Path = path;
            Handler = handler;
            SortIndex = get_sort_index();
        }

    }
    
}
