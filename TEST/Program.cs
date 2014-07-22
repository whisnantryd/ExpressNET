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

            baseRoute[HTTP.GET]("member/:id/name", (req, res) =>
            {
                // parameter keys are converted to upper case
                var id = req.Parameters["ID"];

                res.Response.StatusCode = 200;
                res.SendThenClose("Receieved request for member with id " + id + ", name = nathan");
            });

            baseRoute.Use(new StaticFileProvider(@"C:\"));

            app.Listen("http://*/membership/");

            while (true) { }
        }

    }
}