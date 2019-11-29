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
            Srv = new RestServer();
            Srv.LogToConsole().Start();
        }

        public RestServer Srv { get => _srv; set => _srv = value; }

        public void StopServer()
        {
            this._srv.Stop();
        }
    }
}
