using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Debouncehouse.ExpressNET.Helpers;
using Debouncehouse.ExpressNET.Models;
using Debouncehouse.ExpressNET.Routes;

namespace Debouncehouse.ExpressNET
{

    public class Express
    {

        private string abspath;

        private HttpListener server;

        private List<Router> routers;

        private Dictionary<int, RequestHandler> statushandlers;

        /// <summary>
        /// Initailizer
        /// </summary>
        public Express()
        {
            routers = new List<Router>();
        }

        private void getContextCallback(IAsyncResult ar)
        {
            // the user object is our server
            var srv = (HttpListener)ar.AsyncState;

            // if the server was not set or did not initialize properly, exit
            if (srv == null)
                return;

            // get the context for this http request
            var ctx = srv.EndGetContext(ar);

            // instruct the server to return to a listening state
            srv.BeginGetContext(getContextCallback, srv);

            notifyHandlers(ctx);
        }

        private void notifyHandlers(HttpListenerContext ctx)
        {
            // get the request and response objects associated with this context
            var req = new HttpRequestWrapper(ctx.Request, abspath);
            var res = new HttpResponseWrapper(ctx.Response);

            try
            {
                var matchRouters = routers.Where(r => req.RequestPath.StartsWith(r.BasePath)).ToList();

                var routelist = new List<Route>();

                // go through each router and add all matching route handlers to the route list
                foreach (Router rtr in matchRouters)
                    routelist.AddRange(rtr.FindRoutes(req));

                // invoke each route handler single-threaded, sequentially in the order they were originally added
                foreach (Route r in routelist.OrderBy(rt => rt.SortIndex).ToList())
                    if (res.IsHandled || res.IsClosed)
                        break;
                    else
                        r.Handler(req, res);

                // no route handlers for this request? respond with 404
                if (!res.IsHandled && !res.IsClosed)
                {
                    // call the user specified handler for status 404 if set
                    if (statushandlers != null && statushandlers.ContainsKey(404))
                        statushandlers[404](req, res);
                    else
                    {
                        // respond with a default 404
                        res.Response.StatusCode = 404;
                        res.Send("<html><head><title>Ooops</title></head><body><h1>404</h1><p>Invalid request</p></body></html>");
                    }
                }
            }
            catch (Exception ex)
            {
                // on error respond with 500 - internal server error
                // call the user specified handler for status 500 if set
                if (statushandlers != null && statushandlers.ContainsKey(500))
                    statushandlers[500](req, res);
                else
                {
                    // respond with a default 500
                    res.Response.StatusCode = 500;
                    res.Send("<html><head><title>Ooops</title></head><body><h1>500</h1><p>Internal server error</p></body></html>");
                }
            }

            if(!res.IsClosed)
                res.Response.Close();
        }

        /// <summary>
        /// Initializes a new route group for the specified base route or returns an existing route that matches the specified base route
        /// </summary>
        public Router Route(string path = "")
        {
            // convert the path to standard path format, all uppercase
            // ('somerequest/somepath/:parameter' -> '/REQUEST/PATH/:PARAMETER')
            // note that any parameter key will be converted to uppercase and these keys are case sensitive
            path = path.ToRoute();

            // retrieve an existing router matching the specified path
            var repo = routers.Find(r => r.BasePath == path);

            // if there is no existing router matching the specified path, initailize a new one
            if (repo == null)
                routers.Add(repo = new Router(path));

            return repo;
        }

        /// <summary>
        /// Sets a handler that will be called when a request status matches the specified status (404, 500 etc.)
        /// </summary>
        /// <param name="statuscode">Any standard http status code</param>
        public Express Status(int statuscode, RequestHandler handler)
        {
            // initialize the dictionary if null
            if (statushandlers == null)
                statushandlers = new Dictionary<int, RequestHandler>();

            // if a status handler has already been set for this key, update it
            if (statushandlers.ContainsKey(statuscode))
            {
                statushandlers[statuscode] = handler;
            }
            else
            {
                // no handler exists for this status, add it
                statushandlers.Add(statuscode, handler);
            }

            return this;
        }
        
        /// <summary>
        /// Instructs the object to begin listening for http requests matching the specified base url (http://www.mywebsite.com/api/v2)
        /// </summary>
        /// <param name="baseurl">The base url that should prepend all requests handled by this object,
        /// wildcards are permitted according to standard URI spec
        /// </param>
        public Express Listen(string baseurl)
        {
            if (server == null)
            {
                // get the absolute path from the url ( http://*:8080/MyWebApi/V1 -> /MyWebApi/V1 )
                var apath = baseurl.Replace("http://", "");
                abspath = apath.Remove(0, apath.IndexOf('/'));

                server = new HttpListener();

                server.Prefixes.Add(baseurl.TrimEnd('/') + "/");
                server.Start();
                server.BeginGetContext(getContextCallback, server);
            }

            return this;
        }

        /// <summary>
        /// Stops recieving new requests and closes the listener
        /// </summary>
        public Express Close()
        {
            if (server == null)
                return this;

            if(server.IsListening)
                server.Stop();

            server = null;

            return this;
        }

    }

}