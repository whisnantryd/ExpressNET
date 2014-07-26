ExpressNET
==========

###Simple http request router for .NET

####Example usage -

    var app = new Express();
    
    var baseRoute = app.Route("hello");
    
    baseRoute[HTTP.GET]("world", (req, res) =>
    {
        res.Response.StatusCode = 200;
        
        res.Send("Hello world");
    });
    
    app.Listen("http://*/myserver");

######Access this route from your browser -

    http://localhost/myserver/hello/world
    
    
####Example with path parameter -

    var app = new Express();
    
    var baseRoute = app.Route("hello");
    
    baseRoute[HTTP.GET]("user/:name", (req, res) =>
    {
        res.Response.StatusCode = 200;
        
        // parameters are always keyed in upper case
        var name = req.Parameters["NAME"];
        
        res.Send("Hello, " + name);
    });
    
    app.Listen("http://*/myserver");

######Access this route from your browser -

    http://localhost/myserver/hello/user/William
