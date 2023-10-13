using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using grmIB.Subsys;

namespace grmIB.Entitties
{
    public class C1IBInnerInfo
    {
		//public string AppVersion { get; private set; }
		public string ConfVersion { get; private set; }
		public string ConfName { get; private set; }
		//public string BinDir { get; private set; }

		public void PopulateValues(SOAP_UpdHlp soap)
		{
			ConfVersion = soap.Request_GetConfigurationVersion();
			ConfName = soap.Request_GetConfigurationName();
		}

		public void PopulateValues(dynamic COMConnection)
        {
			//AppVersion = COMConnection.NewObject("СистемнаяИнформация").AppVersion;
			ConfVersion = COMConnection.Metadata.Version;
			ConfName = COMConnection.Metadata.Name;
			//BinDir = COMConnection.BinDir;
		}
	}
}
