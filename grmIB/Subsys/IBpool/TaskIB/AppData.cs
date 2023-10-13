using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using grmIB.Entitties;
using RestAPI.C1;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class AppData
    {
        public string Name;
        public string Alias;
        public bool Typical = true;
        public string User;
        public string Password;

        public string GRMUpdateFileName;
        public string LocalBackupPath;
        public string ProcessingRezult;
        public GRMApplication GRMApp;
        public C1IBInnerInfo IBInnerInfo;
        public C1ConfigUpdate ConfigUpdate;
        public Dictionary<string, bool> TasksResult = new Dictionary<string, bool>();

        private static readonly string fileName = "_AppData.xml";

        /// <summary>
        /// Writes the given object instance to an XML file.
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public bool SaveToFile()
        {
            bool rezult = true;
            bool append = false;
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(AppData));
                writer = new StreamWriter(Name + fileName, append);
                serializer.Serialize(writer, this);
            }
            catch
            {
                rezult = false;
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
            return rezult;
        }
        public static AppData LoadFromFile(string nameIB)
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(AppData));
                reader = new StreamReader(nameIB + fileName);
                return (AppData)serializer.Deserialize(reader);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
