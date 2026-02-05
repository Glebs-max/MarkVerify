using System.IO;
using System.Xml.Serialization;

namespace LabelDesigner.Services
{
    public static class DesignerService
    {
        public static void SaveLabel(DesignerViewModel model, string path)
        {
            XmlSerializer serializer = new(typeof(DesignerViewModel));

            using FileStream fs = new(path, FileMode.Create);
            serializer.Serialize(fs, model);
        }
        public static DesignerViewModel? LoadLabel(string path)
        {
            XmlSerializer serializer = new(typeof(DesignerViewModel));

            using FileStream fs = new(path, FileMode.Open);
            return (DesignerViewModel?)serializer.Deserialize(fs);
        }
    }
}
