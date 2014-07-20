using System;
using System.Collections.Generic;
using System.Linq;
using Debouncehouse.ExpressNET.Enums;
using Debouncehouse.ExpressNET.Helpers;
using Debouncehouse.ExpressNET.Models;

namespace Debouncehouse.ExpressNET.Routes
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

        private RouteRepository findRepo(HTTP methodtype)
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

        public Action<string, RequestHandler> this[HTTP methodtype]
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
            findRepo(HTTP.GET).Add(new Route(BasePath + path.ToRoute(), handler));

            return this;
        }

        [Obsolete()]
        public Router POST(string path, RequestHandler handler)
        {
            findRepo(HTTP.POST).Add(new Route(BasePath + path.ToRoute(), handler));

            return this;
        }

        public List<Route> FindRoutes(HttpRequestWrapper req)
        {
            var repo = findRepo(req.RequestMethod);

            var routelist = new List<Route>();

            if (wares.Any())
                routelist.AddRange(wares);

            foreach (var str in new string[] { AllPath, req.RequestPath })
            {
                //routelist.AddRange(repo.Where(r => r.Path == str).ToList());
                routelist.AddRange(repo.Match(str));
                
            }

            return routelist;
        }

    }

}
