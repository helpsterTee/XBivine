using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common.Geometry;
using Xbim.Common.XbimExtensions;
using Xbim.Geometry.Engine.Interop;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;
using XBivine.Model;

namespace XBivine.XBim
{
    class XBimParser
    {
        IfcStore _model;
        XbimGeometryEngine _geom;
        Xbim3DModelContext _context; 
        string file;

        public XBimParser(string file)
        {
            try {
                _model = IfcStore.Open(file);
                this.file = file;
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public bool HasLoaded()
        {
            return _model != null;
        }

        public void Unload()
        {
            _geom = null;
            _model.Close();
        }

        public void LoadGeometry()
        {
            _geom = new XbimGeometryEngine();
            _context = new Xbim3DModelContext(_model);
            _context.CreateContext();
        }

        public string GetFilename()
        {
            return file;
        }

        public MProject GetProject()
        {
            MProject proj = new MProject();
            IIfcProject iProj = _model.Instances.OfType<IIfcProject>().First();

            proj.FileName = file;

            proj.Author = iProj.OwnerHistory.OwningUser.ThePerson.ToString();

            var createTime = DateTimeOffset.FromUnixTimeSeconds(iProj.OwnerHistory.CreationDate);
            proj.Created = createTime.DateTime;

            if (iProj.OwnerHistory.LastModifiedDate.HasValue == false)
            {
                proj.LastChanged = createTime.DateTime;
            } else
            {
                var time = DateTimeOffset.FromUnixTimeSeconds(iProj.OwnerHistory.LastModifiedDate.Value);
                proj.LastChanged = time.DateTime;
            }

            proj.ProjectName = iProj.Name;

            return proj;
        }

        // Methods below for use with the JSONDeserializer
        public Dictionary<string, IIfcActor> GetActors()
        {
            Dictionary<string, IIfcActor> actors = new Dictionary<string, IIfcActor>();
            foreach(IIfcActor act in _model.Instances.OfType<IIfcActor>())
            {
                actors.Add(act.GlobalId, act);
            }
            return actors;
        }

        public Dictionary<string, IfcObjectAttributes> GetObjectsOfClass(string classname)
        {
            Dictionary<string, IfcObjectAttributes> objs = new Dictionary<string, IfcObjectAttributes>();

            foreach (var obj in _model.Instances.Where(x => x.GetType().Name.Equals(classname)))
            {
                if (obj is IIfcObject)
                {
                    IIfcObject ifcObj = (IIfcObject)obj;
                    bool hasGeometry = false;
                    if (obj is IIfcProduct)
                    {
                        IIfcProduct prod = (IIfcProduct)obj;
                        if (prod.Representation != null)
                        {
                            hasGeometry = true;
                        }
                    }

                    objs.Add(ifcObj.GlobalId, new IfcObjectAttributes
                    {
                        Name = ifcObj.Name,
                        Description = ifcObj.Description,
                        GlobalId = ifcObj.GlobalId,
                        Namespace = obj.GetType().Namespace,
                        IfcType = obj.GetType().Name,
                        HasGeometry = hasGeometry
                    });
                }
            }
            return objs;
        }

        public Dictionary<int, IfcObjectShapeRepresentation> GetShapesOfProduct(string guid)
        {
            Dictionary<int, IfcObjectShapeRepresentation> shapes = new Dictionary<int, IfcObjectShapeRepresentation>();
            IIfcProduct prod = _model.Instances.OfType<IIfcProduct>().Where(x => x.GlobalId.Equals(guid)).First();
            IIfcProductRepresentation prodrep = prod.Representation;

            //https://github.com/xBimTeam/XbimGeometry/issues/131
            using (var modgeom = _model.GeometryStore.BeginRead())
            {
                var instances = modgeom.ShapeInstancesOfEntity(prod.EntityLabel);
                var geometries = instances.Select(i => modgeom.ShapeGeometryOfInstance(i) as IXbimShapeGeometryData);
                List<bool> geometryHasTransformation = new List<bool>();
                foreach (var instance in instances)
                {
                    if (instance.Transformation.Str() == "") geometryHasTransformation.Add(false);
                    else geometryHasTransformation.Add(true);
                }

                int geometrycount = 0;
                foreach (var geometry in geometries)
                {
                    List<List<double>> vertices = new List<List<double>>();
                    using (var ms = new MemoryStream(geometry.ShapeData))
                    using (var br = new BinaryReader(ms))
                    {
                        var mesh = br.ReadShapeTriangulation();
                        if (mesh == null) continue;

                        if (geometryHasTransformation.ElementAt(geometrycount))
                        {
                            var transform = instances.ElementAt(0).Transformation;

                            mesh = mesh.Transform(transform);
                        }

                        foreach (var Meshvertex in mesh.Vertices)
                        {
                            List<double> vertex = new List<double>
                                        {
                                            Math.Round(Meshvertex.X,4),
                                            Math.Round(Meshvertex.Y,4),
                                            Math.Round(Meshvertex.Z,4)
                                        };
                            vertices.Add(vertex);
                        }

                        var faces = mesh.Faces;
                        var indices = new List<int>();
                        var normals = new List<XbimPackedNormal>();

                        foreach (var face in faces)
                        {
                            foreach (var index in face.Indices)
                                indices.Add(index);
                            foreach (var normal in face.Normals)
                                normals.Add(normal);
                        }

                        IfcObjectShapeRepresentation objShape = new IfcObjectShapeRepresentation();
                        objShape.Faces = faces;
                        objShape.Indices = indices;
                        objShape.Normals = normals;
                        objShape.Vertices = vertices;

                        shapes.Add(geometrycount, objShape);
                    }
                    geometrycount++;
                }
            }
            
            return shapes;
        }

        public Dictionary<string, IfcElementRepresentation> GetShapes()
        {
            Dictionary<string, IfcElementRepresentation> elementReps = new Dictionary<string, IfcElementRepresentation>();
            IEnumerable<IIfcElement> prod = _model.Instances.OfType<IIfcElement>();

            foreach (IIfcElement e in prod)
            {
                IIfcProductRepresentation prodrep = e.Representation;

                IfcElementRepresentation eleRep = new IfcElementRepresentation();
                eleRep.attributes = new IfcObjectAttributes();
                eleRep.attributes.GlobalId = e.GlobalId;
                eleRep.attributes.Description = e.Description;
                eleRep.attributes.HasGeometry = true;
                eleRep.attributes.IfcType = e.GetType().Name;
                eleRep.attributes.Namespace = e.GetType().Namespace;
                eleRep.attributes.Name = e.Name;

                eleRep.shapes = new Dictionary<int, IfcObjectShapeRepresentation>();

                //https://github.com/xBimTeam/XbimGeometry/issues/131
                using (var modgeom = _model.GeometryStore.BeginRead())
                {
                    var instances = modgeom.ShapeInstances;
                    var geometries = instances.Select(i => modgeom.ShapeGeometryOfInstance(i) as IXbimShapeGeometryData);
                    List<bool> geometryHasTransformation = new List<bool>();
                    foreach (var instance in instances)
                    {
                        if (instance.Transformation.Str() == "") geometryHasTransformation.Add(false);
                        else geometryHasTransformation.Add(true);
                    }

                    int geometrycount = 0;
                    foreach (var geometry in geometries)
                    {
                        List<List<double>> vertices = new List<List<double>>();
                        using (var ms = new MemoryStream(geometry.ShapeData))
                        using (var br = new BinaryReader(ms))
                        {
                            var mesh = br.ReadShapeTriangulation();
                            if (mesh == null) continue;

                            if (geometryHasTransformation.ElementAt(geometrycount))
                            {
                                var transform = instances.ElementAt(0).Transformation;

                                mesh = mesh.Transform(transform);
                            }

                            foreach (var Meshvertex in mesh.Vertices)
                            {
                                List<double> vertex = new List<double>
                                        {
                                            Math.Round(Meshvertex.X,4),
                                            Math.Round(Meshvertex.Y,4),
                                            Math.Round(Meshvertex.Z,4)
                                        };
                                vertices.Add(vertex);
                            }

                            var faces = mesh.Faces;
                            var indices = new List<int>();
                            var normals = new List<XbimPackedNormal>();

                            foreach (var face in faces)
                            {
                                foreach (var index in face.Indices)
                                    indices.Add(index);
                                foreach (var normal in face.Normals)
                                    normals.Add(normal);
                            }

                            IfcObjectShapeRepresentation objShape = new IfcObjectShapeRepresentation();
                            objShape.Faces = faces;
                            objShape.Indices = indices;
                            objShape.Normals = normals;
                            objShape.Vertices = vertices;

                            eleRep.shapes.Add(geometrycount, objShape);
                        }
                        geometrycount++;
                    }
                }
                elementReps.Add(eleRep.attributes.GlobalId,eleRep);
            }

            return elementReps;
        }
    }
}
