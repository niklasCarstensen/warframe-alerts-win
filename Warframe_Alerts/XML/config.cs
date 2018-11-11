using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Warframe_Alerts
{
    public static class config
    {
        static string configPath = Global.CurrentExecutablePath + "\\config.xml";
        public static configData Data = new configData();
        static Loader L = new Loader();
        
        public static bool Exists()
        {
            return File.Exists(configPath);
        }
        public static void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(configData));
            using (TextWriter writer = new StreamWriter(configPath))
                serializer.Serialize(writer, Data);
        }
        public static void Load()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(configData));
            using (TextReader reader = new StreamReader(configPath))
                Data = (configData)deserializer.Deserialize(reader);
        }
    }
    public class Loader
    {
        public Loader()
        {
            if (config.Exists())
                config.Load();
            else
                config.Data = new configData();
        }
    }
}
