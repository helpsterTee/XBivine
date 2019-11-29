using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XBivine.Model;

namespace XBivine.HTTP.Routes.API
{
    [RestResource]
    class Projects
    {
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/api/projects")]
        public IHttpContext GetProjects(IHttpContext ctx)
        {
            MProject p1 = new MProject();
            p1.ProjectID = 0;
            p1.ProjectVersion = 1;
            p1.Author = "helpsterTee";
            p1.ProjectName = "Dummy Project";
            p1.Created = new DateTime();
            p1.LastChanged = new DateTime();

            Dictionary<string, MProject[]> projects = new Dictionary<string, MProject[]>
            {
                { "projects", new MProject[]{ p1 } }
            };
            HttpResponseExtensions.SendResponse(ctx.Response, HttpStatusCode.Ok, JsonConvert.SerializeObject(projects, Formatting.Indented));
            return ctx;
        }

    }
}
