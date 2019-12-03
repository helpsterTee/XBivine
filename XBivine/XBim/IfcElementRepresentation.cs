using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBivine.XBim
{
    class IfcElementRepresentation
    {
        public IfcObjectAttributes attributes;
        public Dictionary<int, IfcObjectShapeRepresentation> shapes;
    }
}
