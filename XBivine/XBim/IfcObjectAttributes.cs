using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBivine.XBim
{
    class IfcObjectAttributes
    {
        public string GlobalId;
        public string Name;
        public string IfcType;
        public string Namespace;
        public bool HasGeometry = false;
        public string Description;
    }
}
