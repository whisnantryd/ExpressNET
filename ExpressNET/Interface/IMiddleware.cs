using Debouncehouse.ExpressNET.Models;

namespace Debouncehouse.ExpressNET
{

    public interface IMiddleware
    {
        bool Process(HttpRequestWrapper req, HttpResponseWrapper res);
    }

}
