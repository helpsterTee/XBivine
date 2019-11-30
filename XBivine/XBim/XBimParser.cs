using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using XBivine.Model;

namespace XBivine.XBim
{
    class XBimParser
    {
        IfcStore _model;
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
            _model.Close();
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
    }
}
