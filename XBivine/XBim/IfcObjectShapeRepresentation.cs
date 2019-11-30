using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common.Geometry;

namespace XBivine.XBim
{
    class IfcObjectShapeRepresentation
    {
        //public XbimMatrix3D Transformation;
        //public XbimRect3D BoundingBox;
        public List<XbimPackedNormal> Normals;
        public List<int> Indices;
        public IList<XbimFaceTriangulation> Faces;
        public List<List<double>> Vertices;
    }
}
