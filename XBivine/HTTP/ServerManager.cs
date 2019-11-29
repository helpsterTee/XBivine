using Grapevine.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBivine.HTTP
{

    class ServerManager
    {
        RestServer _srv;

        public ServerManager()
        {
            ServerSettings settings = new ServerSettings();
            settings.PublicFolder = new PublicFolder("./HTTP/Static");
            Srv = new RestServer(settings);

            Srv.LogToConsole().Start();
        }

        public RestServer Srv { get => _srv; set => _srv = value; }

        public void StopServer()
        {
            this._srv.Stop();
        }
    }
}
