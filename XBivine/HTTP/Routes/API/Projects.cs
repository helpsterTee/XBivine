using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using HttpMultipartParser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XBivine.Database;
using XBivine.Model;
using XBivine.XBim;

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

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/api/projects/add")]
        public IHttpContext PostProject(IHttpContext ctx)
        {
            if (ctx.Request.HasEntityBody)
            {
                int chunksize = 1024;
                using (Stream input = ((HttpRequest)ctx.Request).Advanced.InputStream)
                {
                    var parser = MultipartFormDataParser.Parse(input);

                    foreach (FilePart file in parser.Files)
                    {
                        using (BinaryReader reader = new BinaryReader(file.Data, ctx.Request.ContentEncoding))
                        {
                            string filename = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + ".ifc";
                            string storagefile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", filename);
                            using (BinaryWriter output = new BinaryWriter(File.Open(storagefile, FileMode.Create)))
                            {
                                byte[] chunk = reader.ReadBytes(chunksize);
                                while (chunk.Length > 0)
                                {
                                    output.Write(chunk);
                                    chunk = reader.ReadBytes(chunksize);
                                }
                            }

                            //ifc file available here
                            XBimParser xbparse = new XBimParser(storagefile);
                            MProject mp = xbparse.GetProject();
                            SqliteDb.InsertProject(mp);
                            HttpResponseExtensions.SendResponse(ctx.Response, HttpStatusCode.Ok, "{\"status\": \"success\"}");
                        }
                    }

                }
            }
            return ctx;
        }


        //debug
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
