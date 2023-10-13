using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using grmIB.Subsys.IBpool;

namespace grmIB
{
    class Program
    {
        //static internal GeneralOptions AppOptions = new GeneralOptions();

        static void Main(string[] args)
        {
            InformationBasePool IBpool = new InformationBasePool();

            bool IBPoolNotLoaded = true;
            if (args.Length > 0)
            {
                if (Load(args[0], ref IBpool))
                {
                    IBPoolNotLoaded = false;
                }
            }

            if (IBPoolNotLoaded)
            {
                GeneralOptions.Init();
                IBpool.Init(1);
                Save(IBpool);
                return;
            }

            Subsys.Http.GRMRestAPI.SetAuthToken(GeneralOptions.AuthToken);
            
            IBpool.Execute();
        }

        static string xml_ProgrammOptions = "ProgrammOptions";

        static void Save(InformationBasePool pool)
        {
            XDocument xDoc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));
            XElement xRoot = new XElement(xml_ProgrammOptions);

            GeneralOptions.SaveToXML(xRoot);
            pool.SaveToXML(xRoot);
            xDoc.Add(xRoot);

            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string pathFile;
            int index = 0;
            do
            {
                index++;
                pathFile = Path.Combine(appDir, $"IBTasks_{index:d3}.xml");
            } while (File.Exists(pathFile));

            xDoc.Save(pathFile);
        }
        static bool Load(string fileName, ref InformationBasePool pool)
        {
            InformationBasePool newPool = new InformationBasePool();

            string filePath = fileName;
            if (string.IsNullOrEmpty(Path.GetDirectoryName(filePath)))
            {
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            }

            if (!File.Exists(filePath))
            {
                return false;
            }

            XDocument xDoc = XDocument.Load(filePath);
            XElement xRoot = xDoc.Root;

            if (!newPool.LoadFromXML(xRoot))
            {
                return false;
            }
            if (!GeneralOptions.LoadFromXML(xRoot))
            {
                return false;
            }

            pool = newPool;
            return true;
        }
    }
}
