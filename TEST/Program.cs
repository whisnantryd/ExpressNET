using System;
using System.Text;
using Debouncehouse.ExpressNET;

namespace TEST
{
    class Program
    {
        static int counter = 0;

        static void Main(string[] args)
        {
            var app = new Express();

            var baseRoute = app.Route("clock");

            baseRoute.GET("/tod", (req, res) =>
            {
                res.StatusCode = 200;
                res.Send(System.DateTime.Now.ToString("hh\\:mm\\:ss"));
            });

            app.Listen("http://*:56000/");

            while (true) { }
        }

    }
}
