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
using XBivine.Database;
using XBivine.Model;

namespace XBivine.HTTP.Routes.API
{
    [RestResource]
    class Projects
    {
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/api/projects")]
        public IHttpContext GetProjects(IHttpContext ctx)
        {
            Dictionary<int, MProject> projs = SqliteDb.GetProjects();
            HttpResponseExtensions.SendResponse(ctx.Response, HttpStatusCode.Ok, JsonConvert.SerializeObject(projs, Formatting.Indented));
            return ctx;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/api/debug/insertTestProject")]
        public IHttpContext InsertTestProject(IHttpContext ctx)
        {
            MProject test = new MProject();
            test.ProjectVersion = 1337;
            test.Author = "TestAuthor";
            test.ProjectName = "A Testproject";
            test.Created = DateTime.Now;
            test.LastChanged = DateTime.Now;

            long id = SqliteDb.InsertProject(test);
            test.ProjectID = (int)id;

            HttpResponseExtensions.SendResponse(ctx.Response, HttpStatusCode.Ok, JsonConvert.SerializeObject(test, Formatting.Indented));
            return ctx;
        }

    }
}
