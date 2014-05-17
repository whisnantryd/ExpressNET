using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debouncehouse.ExpressNET
{

    public interface IMiddleware
    {
        bool Process(System.Net.HttpListenerRequest req, System.Net.HttpListenerResponse res);
    }

}
