using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Debouncehouse.ExpressNET.Enums;
using Debouncehouse.ExpressNET.Helpers;

namespace Debouncehouse.ExpressNET.Routes
{

    public class RouteRepository : IEnumerable<Route>
    {

        public HTTP Method { get; private set; }

        private List<Route> routes { get; set; }

        public RouteRepository()
            : this(HTTP.GET)
        {
        }

        public RouteRepository(HTTP method)
        {
            Method = method;
            routes = new List<Route>();
        }

        public void Add(Route newroute)
        {
            if (!routes.Contains(newroute))
                routes.Add(newroute);
        }

        public List<Route> Match(string requestpath)
        {
            var ret_routes = new List<Route>();

            ret_routes.AddRange(routes.Where(r => r.Path == requestpath).ToList());

            var paramroutes = routes.Where(r => r.Parameters.Any()).ToList();

            if (paramroutes.Any())
            {
                var parts = requestpath.Split('/');

                foreach (Route route in paramroutes)
                {
                    var rparts = route.Path.Split('/');

                    if (parts.PathMatch(rparts))
                        ret_routes.Add(route);
                }
            }

            return ret_routes;
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

}
