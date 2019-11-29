using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XBivine.Database;
using XBivine.HTTP;

namespace XBivine
{
    class Main
    {
        ServerManager sm;
        SqliteDb db;

        public Main()
        {
            db = new SqliteDb();

            //needs to be called as last item to prevent blocking
            sm = new ServerManager();
        }
        
    }
}
