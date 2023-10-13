using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using grmIB.Entitties;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class Task_GRM_GetIBInnerInfoByWS : ITaskIB
    {
        public static readonly string RezultName = "GRM_GetIBInnerInfo";

        SOAP_UpdHlp webService;
        C1IBInnerInfo InnerInfo;

        bool Context_Success = false;
        bool Rezult_Success = false;

        public bool Predicate(AppData appData)
        {
            bool criterion = appData.TasksResult.TryGetValue(Task_GRM_CheckApplication.RezultName, out bool check);
            criterion = criterion && check;
            return criterion;
        }

        public bool SetContext(AppData appData)
        {
            Context_Success = true;

            string url = appData.GRMApp.URL;
            if (string.IsNullOrEmpty(url))
            {
                Context_Success = false;
                return Context_Success;
            }

            webService = new SOAP_UpdHlp(url);
            webService.SetAuth(appData.User, appData.Password);

            return Context_Success;
        }

        public void Execute()
        {
            if (!Context_Success)
            {
                return;
            }

            InnerInfo = new C1IBInnerInfo();
            Rezult_Success = true;
            try
            {
                InnerInfo.PopulateValues(webService);
            }
            catch (Exception)
            {
                Rezult_Success = false;
            }
        }

        void ITaskIB.GetRezult(AppData appData)
        {
            bool rezult = Context_Success && Rezult_Success;
            appData.TasksResult.Add(RezultName, rezult);
            if (rezult)
            {
                appData.IBInnerInfo = InnerInfo;
            }
        }
    }
}
