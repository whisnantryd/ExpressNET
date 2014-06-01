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

            var baseRoute = app.Route("clock");

            baseRoute.GET("tod", (req, res) =>
            {
                var s = req.QueryString["test"];

                Console.WriteLine(s);

                res.StatusCode = 200;
                res.Send(System.DateTime.Now.ToString("hh\\:mm\\:ss") + " - " + s);
            });

            app.Status(404, (req, res) =>
            {
                res.StatusCode = 404;
                res.Send("<html><head><title>Ooops</title></head><body><h1>404</h1><p>Stupid head</p></body></html>");
            });

            app.Listen("http://*:56000/");

            while (true) { }
        }

    }
}
