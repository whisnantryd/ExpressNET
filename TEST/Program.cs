using System;
using System.Text;
using Debouncehouse.ExpressNET;
using Debouncehouse.ExpressNET.Models;

namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new Express();

            var baseRoute = app.Route("");

            baseRoute.GET("all", (req, res) =>
            {
                var s = req.Request.QueryString["test"];

                Console.WriteLine(s);

                res.Response.StatusCode = 200;
                res.Send(System.DateTime.Now.ToString("hh\\:mm\\:ss") + " - " + s);
            });

            var dt = new System.IO.DirectoryInfo(@"C:\");

            baseRoute.Use(new StaticFileProvider(dt));

            app.Listen("http://*/tracktemp");

            while (true) { }
        }

        static void app_Admin(HttpRequestWrapper req, HttpResponseWrapper res)
        {
            res.Send("<h1>Admin</h1>");
        }

        static void app_Client(HttpRequestWrapper req, HttpResponseWrapper res)
        {
            res.Send("<h1>Client</h1>");
        }

    }
}