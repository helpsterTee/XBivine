using Grapevine.Interfaces.Server;
using Grapevine.Server.Attributes;
using Grapevine.Server;
using Grapevine.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace XBivine.HTTP.Routes.API
{
    [RestResource]
    class GetVersion
    {
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/api/version")]
        public IHttpContext ReturnVersion(IHttpContext ctx)
        {
            Dictionary<string, string> versionInfo = new Dictionary<string, string>
            {
                { "Version", "0.1" }
            };
            HttpResponseExtensions.SendResponse(ctx.Response, HttpStatusCode.Ok, JsonConvert.SerializeObject(versionInfo, Formatting.Indented));
            return ctx;
        }

    }
}
