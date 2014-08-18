using System;
using System.Text;
using Debouncehouse.ExpressNET;
using Debouncehouse.ExpressNET.Enums;
using Debouncehouse.ExpressNET.Models;

namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new Express();

            var baseRoute = app.Route("");

            var members = new System.Collections.Generic.Dictionary<int, string>();

            members.Add(92, "Billsky");

            baseRoute[HTTP.GET]("member", (req, res) =>
            {
                res.Response.StatusCode = 200;
                res.SendThenClose("Hey");
            });

            baseRoute[HTTP.GET]("member/:id/name", (req, res) =>
            {
                // parameter keys are converted to upper case
                var id = int.Parse(req.Parameters["ID"]);

                if (members.ContainsKey(id))
                {
                    var name = members[id].ToString();

                    res.Response.StatusCode = 200;
                    res.SendThenClose("Receieved request for member with id " + id + ", name = " + name);
                }
                else
                {
                    res.Response.StatusCode = 301;
                    res.SendThenClose("Unable to find information on member #" + id);
                }
            });

            baseRoute.Use(new StaticFileProvider(@"C:\"));
            
            app.Listen("http://*/membership/");

            while (true) { }
        }

    }
}