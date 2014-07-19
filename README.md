ExpressNET
==========

Simple http request router for .NET

Example usage -

'''
var app = new Express();

var baseRoute = app.Route("hello");

baseRoute[HttpMethodType.GET]("world", (req, res) =>
{
    res.Response.StatusCode = 200;
    
    res.Send("Hello world");
});

app.Listen("http://*/myserver");
'''
Access this route from your browser -
'''
http://localhost/hello/world
'''
