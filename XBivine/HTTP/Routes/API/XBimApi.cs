﻿using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using XBivine.Database;
using XBivine.Model;
using XBivine.XBim;

namespace XBivine.HTTP.Routes.API
{
    [RestResource]
    class XBimApi
    {
        // we store our sessions here. It's a simple integer increment for now. Use hash / auth later
        static Dictionary<int, XBimParser> sessions = new Dictionary<int, XBimParser>();
        static int sessionCounter = 0;

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/api/xbim/load")]
        public IHttpContext LoadProject(IHttpContext ctx)
        {
            if (ctx.Request.QueryString["projectid"] == null)
            {
                HttpResponseExtensions.SendResponse(ctx.Response, HttpStatusCode.Ok, "{status: \"error\", reason: \"not found\"}");
                return ctx;
            }

            MProject p = SqliteDb.GetProject(Int32.Parse(ctx.Request.QueryString["projectid"]));
            XBimParser parsival = new XBimParser(p.FileName);
            if (parsival.HasLoaded())
            {
                sessions.Add(sessionCounter, parsival);
                HttpResponseExtensions.SendResponse(ctx.Response, HttpStatusCode.Ok, "{status: \"success\", session: "+sessionCounter.ToString()+"}");
                sessionCounter++;
            }
            
            return ctx;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/api/xbim/unload")]
        public IHttpContext UnloadProject(IHttpContext ctx)
        {
            if (ctx.Request.QueryString["sessionid"] == null)
            {
                HttpResponseExtensions.SendResponse(ctx.Response, HttpStatusCode.Ok, "{status: \"error\", reason: \"not found\"}");
                return ctx;
            }

            int sessionid = Int32.Parse(ctx.Request.QueryString["sessionid"]);
            if (sessions.ContainsKey(sessionid))
            {
                XBimParser parsomator;
                sessions.TryGetValue(sessionid, out parsomator);
                parsomator.Unload();
                sessions.Remove(sessionid);
                HttpResponseExtensions.SendResponse(ctx.Response, HttpStatusCode.Ok, "{status: \"success\"}");
            } else
            {
                HttpResponseExtensions.SendResponse(ctx.Response, HttpStatusCode.Ok, "{status: \"error\", reason: \"session not found\"}");
            }

            return ctx;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/api/xbim/sessions")]
        public IHttpContext GetSessions(IHttpContext ctx)
        {
            Dictionary<int, string> filehandles = new Dictionary<int, string>();
            foreach(int key in sessions.Keys)
            {
                XBimParser parseme;
                sessions.TryGetValue(key, out parseme);
                filehandles.Add(key, parseme.GetFilename());
            }

            HttpResponseExtensions.SendResponse(ctx.Response, HttpStatusCode.Ok, JsonConvert.SerializeObject(filehandles, Formatting.Indented));
            return ctx;
        }
    }
}