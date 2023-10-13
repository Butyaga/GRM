using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using grmIB.Subsys.V83.ActContext.UnmanagedDll;

namespace grmIB
{
    static internal class GeneralOptions
    {
        static internal string AuthToken { get; private set; }
        static internal string PathTypicalConfig { get; private set; }
        static internal string GRMRemouteFileName { get; private set; }
        static internal bool GRMRemouteFileAddVersion { get; private set; }
        static internal bool GRMRemouteFileAddDate { get; private set; }
        static internal bool GRMRemouteFileAddTime { get; private set; }

        static List<string> SearchPath_comcntr = new List<string>();
        static SortedDictionary<string,string> Discovered_comcntr = new SortedDictionary<string, string>();

        private static readonly string xml_GeneralOptions = "GeneralOptions";
        private static readonly string xml_GO_AuthToken = "AuthToken";
        private static readonly string xml_GO_PathTypicalConfig = "PathTypicalConfig";
        private static readonly string xml_GO_GRMRemouteFileName = "GRMRemouteFileName";
        private static readonly string xml_GO_GRMRemouteFileAddVersion = "GRMRemouteFileAddVersion";
        private static readonly string xml_GO_GRMRemouteFileAddDate = "GRMRemouteFileAddDate";
        private static readonly string xml_GO_GRMRemouteFileAddTime = "GRMRemouteFileAddTime";

        static internal void SaveToXML(XElement xRoot)
        {
            XElement GeneralOpt = new XElement(xml_GeneralOptions);
            GeneralOpt.Add(new XElement(xml_GO_AuthToken, AuthToken));
            GeneralOpt.Add(new XElement(xml_GO_PathTypicalConfig, PathTypicalConfig));
            GeneralOpt.Add(new XElement(xml_GO_GRMRemouteFileName, GRMRemouteFileName));
            GeneralOpt.Add(new XElement(xml_GO_GRMRemouteFileAddVersion, GRMRemouteFileAddVersion.ToString()));
            GeneralOpt.Add(new XElement(xml_GO_GRMRemouteFileAddDate, GRMRemouteFileAddDate.ToString()));
            GeneralOpt.Add(new XElement(xml_GO_GRMRemouteFileAddTime, GRMRemouteFileAddTime.ToString()));

            xRoot.Add(GeneralOpt);
        }

        static internal bool LoadFromXML(XElement xRoot)
        {
            XElement xGeneralOptions = xRoot.Element(xml_GeneralOptions);
            if (xGeneralOptions is null)
            {
                return false;
            }
            AuthToken = xGeneralOptions.Element(xml_GO_AuthToken)?.Value;
            PathTypicalConfig = xGeneralOptions.Element(xml_GO_PathTypicalConfig)?.Value;
            GRMRemouteFileName = xGeneralOptions.Element(xml_GO_GRMRemouteFileName)?.Value;
            GRMRemouteFileAddVersion = ParseStringToBool(xGeneralOptions.Element(xml_GO_GRMRemouteFileAddVersion)?.Value);
            GRMRemouteFileAddDate = ParseStringToBool(xGeneralOptions.Element(xml_GO_GRMRemouteFileAddDate)?.Value);
            GRMRemouteFileAddTime = ParseStringToBool(xGeneralOptions.Element(xml_GO_GRMRemouteFileAddTime)?.Value);

            return true;
        }

        static private bool ParseStringToBool(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (bool.TryParse(str, out bool rezult))
            {
                return rezult;
            }

            return false;
        }

        static internal void Init()
        {
            if (string.IsNullOrEmpty(AuthToken))
            {
                AuthToken = "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiI1NTczMSIsImlhdCI6MTY4MTEyNzc3NH0.EC_pDkLokTFKYAzKSMYGiPI_btI7p1SsHE50DjJB-6c";
                GRMRemouteFileAddVersion = false;
                GRMRemouteFileAddDate = false;
                GRMRemouteFileAddTime = false;
            }
            if (string.IsNullOrEmpty(PathTypicalConfig))
            {
                PathTypicalConfig = @"C:\Users\Будкин Андрей\AppData\Roaming\1C\1cv8\tmplts\1c";
            }
            if (string.IsNullOrEmpty(GRMRemouteFileName))
            {
                GRMRemouteFileName = "";
            }
            if (SearchPath_comcntr.Count == 0)
            {
                SearchPath_comcntr.Add(@"C:\Program Files\1cv8");
                DiscoverDll();
            }
        }

        public static string GetBinPath(string Version, bool ExactMatch=true)
        {
            if (!Discovered_comcntr.TryGetValue(Version, out string rezult))
            {
                rezult = "";
            }

            if (!ExactMatch && rezult == "")
            {
                rezult = Discovered_comcntr.LastOrDefault().Value;
            }
            return rezult ?? "";
        }

        private static void DiscoverDll()
        {
            Discovered_comcntr.Clear();
            string dllFileName = "comcntr.dll";

            foreach (var path in SearchPath_comcntr)
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    var FileCollection = dir.EnumerateFiles(dllFileName, SearchOption.AllDirectories);
                    foreach (var file in FileCollection)
                    {
                        if (NativeDllMachineType.DllIs64Bit(file.FullName))
                        {
                            var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(file.FullName);
                            string version = info.FileVersion;
                            Discovered_comcntr.Add(version, file.DirectoryName);
                        }
                    }
                }
            }
        }
    }
}
