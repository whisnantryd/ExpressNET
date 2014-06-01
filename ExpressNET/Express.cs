using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Debouncehouse.ExpressNET
{
    public class Express
    {

        private HttpListener server { get; set; }

        private List<Router> routers { get; set; }

        private Dictionary<int, RequestHandler> statushandlers { get; set; }

        public Express()
        {
            init();
        }

        private void init()
        {
            routers = new List<Router>();
        }

        private void getContextCallback(IAsyncResult ar)
        {
            var srv = (HttpListener)ar.AsyncState;

            if (srv == null)
                return;

            var ctx = srv.EndGetContext(ar);

            srv.BeginGetContext(getContextCallback, srv);

            notifyHandlers(ctx);
        }

        private void notifyHandlers(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var res = ctx.Response;

            var requestRoute = req.RawUrl.ToRoute();

            var matchRouters = routers.Where(r => requestRoute.StartsWith(r.BasePath)).ToList();

            var routelist = new List<Route>();

            foreach (Router rtr in matchRouters)
                routelist.AddRange(rtr.FindRoutes(req));

            foreach (Route r in routelist.OrderBy(rt => rt.SortIndex).ToList())
                r.Handler(req, res);

            if (!routelist.Any())
            {
                if (statushandlers.ContainsKey(404))
                    statushandlers[404](req, res);
                else
                {
                    res.StatusCode = 404;
                    res.Send("<html><head><title>Ooops</title></head><body><h1>404</h1><p>Invalid request</p></body></html>");
                }
            }

            res.Close();
        }

        public Router Route(string path = "")
        {
            path = path.ToRoute();

            var repo = routers.Find(r => r.BasePath == path);

            if (repo == null)
                routers.Add(repo = new Router(path));

            return repo;
        }

        public Express Status(int statuscode, RequestHandler handler)
        {
            if (statushandlers == null)
                statushandlers = new Dictionary<int, RequestHandler>();

            if (statushandlers.ContainsKey(statuscode))
                statushandlers[statuscode] = handler;
            else
                statushandlers.Add(statuscode, handler);

            return this;
        }

        public Express Listen(string baseurl)
        {
            if (server == null)
            {
                server = new HttpListener();

                server.Prefixes.Add(baseurl);
                server.Start();
                server.BeginGetContext(getContextCallback, server);
            }

            return this;
        }

    }
}