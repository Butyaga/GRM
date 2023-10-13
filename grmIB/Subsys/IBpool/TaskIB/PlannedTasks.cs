using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class PlannedTasks
    {
        AppData appData = new AppData();
        List<ITaskIB> TasksIB = new List<ITaskIB>();

        public PlannedTasks() { }
        public PlannedTasks(string name, bool typical, string user, string password)
        {
            Init(name, typical, user, password);
        }
        public PlannedTasks(string name)
        {
            Init(name);
        }

        void Init(string appName)
        {
            bool typical = true;
            string user = "SrvUsr";
            string  password = "StrongPass21";
            Init(appName, typical, user, password);
        }
        void Init(string appName, bool typical, string user, string password)
        {
            appData.Name = appName;
            appData.Typical = typical;
            appData.User = user;
            appData.Password = password;
        }

        public void AddTask(ITaskIB tsk)
        {
            TasksIB.Add(tsk);
        }
        public void Execute()
        {
            foreach (ITaskIB taskIB in TasksIB)
            {
                bool predicate = taskIB.Predicate(appData);
                if (!predicate)
                {
                    continue;
                }

                bool context = taskIB.SetContext(appData);
                if (!context)
                {
                    continue;
                }

                taskIB.Execute();
                taskIB.GetRezult(appData);
            }
        }

        static private string xml_InformationBase = "InformationBase";
        static private string xml_Name = "Name";
        static private string xml_Alias = "Alias";
        static private string xml_Typical = "Typical";
        static private string xml_User = "User";
        static private string xml_Password = "Password";
        static private string xml_LocalBackupPath = "LocalBackupPath";
        static private string xml_Tasks = "Tasks";
        static private string xml_Item = "Item";

        public void SaveToXDoc(XElement xElm)
        {
            XElement xIB = new XElement(xml_InformationBase);
            xIB.Add(new XAttribute(xml_Name, appData.Name));
            xIB.Add(new XElement(xml_Alias, appData.Alias));
            xIB.Add(new XElement(xml_Typical, appData.Typical.ToString()));
            xIB.Add(new XElement(xml_User, appData.User));
            xIB.Add(new XElement(xml_Password, appData.Password));
            xIB.Add(new XElement(xml_LocalBackupPath, appData.LocalBackupPath));

            XElement xTasks = new XElement(xml_Tasks);
            foreach (var task in TasksIB)
            {
                xTasks.Add(new XElement(xml_Item, task.GetType().Name));
            }
            xIB.Add(xTasks);

            xElm.Add(xIB);
        }

        public bool LoadFromXML(XElement xElm)
        {
            appData.Name = xElm.Attribute(xml_Name)?.Value;
            if (string.IsNullOrEmpty(appData.Name))
            {
                return false;
            }

            appData.Alias = xElm.Element(xml_Alias)?.Value;

            appData.Typical = ParseStringToBool(xElm.Element(xml_Typical)?.Value);

            appData.User = xElm.Element(xml_User)?.Value;
            if (string.IsNullOrEmpty(appData.User))
            {
                return false;
            }

            appData.Password = xElm.Element(xml_Password)?.Value;
            if (string.IsNullOrEmpty(appData.Password))
            {
                return false;
            }

            appData.LocalBackupPath = xElm.Element(xml_LocalBackupPath)?.Value;

            string prefix_Namespace = GetType().Namespace + ".";
            XElement xTasks = xElm.Element(xml_Tasks);
            foreach (var xTask in xTasks.Elements(xml_Item))
            {
                Type type = Type.GetType(prefix_Namespace + xTask.Value);
                if (type is null)
                {
                    return false;
                }
                AddTask((ITaskIB)Activator.CreateInstance(type));
            }

            return true;
        }

        private bool ParseStringToBool(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return true;
            }

            if (bool.TryParse(str, out bool rezult))
            {
                return rezult;
            }

            return true;
        }
    }
}
