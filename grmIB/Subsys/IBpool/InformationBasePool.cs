using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using grmIB.Subsys.IBpool.TaskIB;

namespace grmIB.Subsys.IBpool
{
    class InformationBasePool
    {
        internal List<PlannedTasks> IBList = new List<PlannedTasks>();

        internal void Execute()
        {
            foreach (var IB in IBList)
            {
                IB.Execute();
            }
        }
        internal void Init(int progNumber)
        {
            if (progNumber == 1)
            {
                IBList.Clear();

                PlannedTasks IB = new PlannedTasks("AllegroUT");
                IB.AddTask(new Task_GRM_CheckApplication());
                IB.AddTask(new Task_GRM_GetIBInnerInfoByWS());
                IB.AddTask(new Task_Local_SearchConfigUpdate());
                IB.AddTask(new Task_GRM_UploadConfigUpdate());
                IB.AddTask(new Task_GRM_UpdateConfig());
                IB.AddTask(new Task_GRM_UpdateProcessing());
                IB.AddTask(new Task_GRM_ConfirmLegality());
                IBList.Add(IB);

                IB = new PlannedTasks("qwertty123");
                IB.AddTask(new Task_GRM_CheckApplication());
                IB.AddTask(new Task_GRM_GetIBInnerInfoByWS());
                IB.AddTask(new Task_Local_SearchConfigUpdate());
                IB.AddTask(new Task_GRM_UploadConfigUpdate());
                IB.AddTask(new Task_GRM_UpdateConfig());
                IB.AddTask(new Task_GRM_UpdateProcessing());
                IB.AddTask(new Task_GRM_ConfirmLegality());
                IBList.Add(IB);
            }
        }

        static string xml_InfoBases = "InfoBases";

        internal void SaveToXML(XElement xRoot)
        {
            XElement xPoolIB = new XElement(xml_InfoBases);

            foreach (var IB in IBList)
            {
                IB.SaveToXDoc(xPoolIB);
            }

            xRoot.Add(xPoolIB);
        }

        internal bool LoadFromXML(XElement xRoot)
        {
            XElement xInfoBases = xRoot.Element(xml_InfoBases);
            if (xInfoBases is null)
            {
                return false;
            }

            foreach (var xElem in xInfoBases.Elements())
            {
                PlannedTasks IB = new PlannedTasks();
                if (IB.LoadFromXML(xElem))
                {
                    IBList.Add(IB);
                }
            }

            if (IBList.Count == 0)
            {
                return false;
            }
            return true;
        }

    }
}
