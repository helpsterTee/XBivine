using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc2x3.Kernel;
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
            IfcProject iProj = _model.Instances.OfType<IfcProject>().First();

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
    }
}
