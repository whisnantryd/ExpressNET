using System;
using System.Text;
using Debouncehouse.ExpressNET;

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
                res.Response.Send(System.DateTime.Now.ToString("hh\\:mm\\:ss") + " - " + s);
            });

            app.Status(404, (req, res) =>
            {
                res.Response.StatusCode = 404;
                res.Response.Send("<html><head><title>Ooops</title></head><body><h1>404</h1><p>Stupid head</p></body></html>");
            });

            app.Listen("http://*/tracktemp");

            while (true) { }
        }

    }
}
