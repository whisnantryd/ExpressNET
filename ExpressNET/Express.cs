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

            var req = ctx.Request;
            var res = ctx.Response;

            var matchingRouters = routers.Where(r => req.RawUrl.ToUpper().StartsWith(r.BasePath)).OrderBy(r => r.SortIndex).ToList();

            matchingRouters.ForEach(r => r.Process(req, res));

            if (!matchingRouters.Any() && !req.RawUrl.Contains("favicon"))
                res.StatusCode = 500;

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

        public void Listen(string baseurl)
        {
            if (server == null)
            {
                server = new HttpListener();

                server.Prefixes.Add(baseurl);
                server.Start();
                server.BeginGetContext(getContextCallback, server);
            }
        }

    }
}
