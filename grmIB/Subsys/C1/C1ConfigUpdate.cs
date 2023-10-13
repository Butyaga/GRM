using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RestAPI.C1
{
    class C1ConfigUpdate
    {
        static readonly string FileInfoName = "\\UpdInfo.txt";

        public string UpdatePath = "";
        public C1Version Version = new C1Version();
        public List<C1Version> FromVersion = new List<C1Version>();
        public DateTime UpdateDate = new DateTime();
        
        public C1ConfigUpdate() { }
        public C1ConfigUpdate(string path)
        {
            if (Directory.Exists(path))
            {
                UpdatePath = path;
                GetUpdateInfo();
            }
        }
        private void GetUpdateInfo()
        {
            string FileInfoPath = UpdatePath + FileInfoName;
            if (File.Exists(FileInfoPath))
            {
                string Info = File.ReadAllText(FileInfoPath);
                string[] strings = Info.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in strings) AddParam(str);
            }
        }
        private void AddParam(string str)
        {
            int pos = str.IndexOf('=');
            if (pos == -1) return;
            string paramName = str.Substring(0, pos).Trim();
            string value = str.Substring(pos + 1).Trim();
            switch (paramName)
            {
                case "Version":
                    Version.SetVersion(value);
                    break;
                case "FromVersions":
                    string[] vers = value.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (var ver in vers)
                    {
                        FromVersion.Add(new C1Version(ver));
                        C1Version Last = FromVersion.Last();
                        if (!(Last.SuccessConstruct)) FromVersion.Remove(Last);
                    }
                    break;
                case "UpdateDate":
                    DateTime.TryParse(value, out UpdateDate);
                    break;
            }

        }
        public static List<C1ConfigUpdate> GetListUpdateConfig(string path)
        {
            List<C1ConfigUpdate> listUpdateConfig = new List<C1ConfigUpdate>();
            if (path != "")
            {
                string[] colDir = Directory.GetDirectories(path);
                foreach (var dir in colDir)
                {
                    C1ConfigUpdate updateConf = new C1ConfigUpdate(dir);
                    listUpdateConfig.Add(updateConf);
                }
            }
            return listUpdateConfig;
        }
    }
}
