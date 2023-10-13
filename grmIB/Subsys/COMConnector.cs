using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using grmIB.Subsys.V83;
using grmIB.Entitties;

namespace grmIB.Subsys
{
    class COMConnector
    {
        private string ConnectionString { get; set; }
        private string BinPath { get; set; }
        //private V83Isolated contextV83;

        private static string className = "V83.COMConnector";
        //private static string dllFileName = "comcntr.dll";

        public void SetConnectionString(string url, string userName = null, string userPassword = null)
        {
            StringBuilder strConn = new StringBuilder($"ws=\"{url}\"");
            //StringBuilder strConn = new StringBuilder($"ws=\"https://{url}\"");

            if (!string.IsNullOrEmpty(userName))
            {
				strConn.Append($";Usr=\"{userName}\"");
				if (!string.IsNullOrEmpty(userPassword))
				{
					strConn.Append($";Pwd=\"{userPassword}\"");
				}
			}
            ConnectionString = strConn.ToString();
		}

        public bool SetPlatfomVersion(string version)
        {
            BinPath = GeneralOptions.GetBinPath(version);
            if (string.IsNullOrEmpty(BinPath))
            {
                return false;
            }
            return true;
        }

        public C1IBInnerInfo GetIBInnerInfo()
        {
            C1IBInnerInfo Info = new C1IBInnerInfo();

            V83Isolated v83Helper = new V83Isolated(BinPath);
            v83Helper.Activate();

            Type COMType = Type.GetTypeFromProgID(className);
            dynamic V83COM = null;
            dynamic V83Connect = null;

            try
            {
                V83COM = Activator.CreateInstance(COMType);
                V83Connect = V83COM.Connect(ConnectionString);
                Info.PopulateValues(V83Connect);
            }
            catch
            {
                Info = null;
            }
            if (!(V83Connect == null))
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(V83Connect);
            }
            if (!(V83COM == null))
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(V83COM);
            }

            v83Helper.Deactivate();
            v83Helper.Dispose();

            return Info;
        }
    }
}
