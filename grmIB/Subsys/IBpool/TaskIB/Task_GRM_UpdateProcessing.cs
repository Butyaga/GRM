using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using grmIB.Entitties;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class Task_GRM_UpdateProcessing : ITaskIB
    {
        public static readonly string RezultName = "GRM_UpdateProcessing";

        SOAP_UpdHlp webService;
        string ProcessingRezult;

        bool Context_Success = false;
        bool Rezult_Success = false;

        public bool Predicate(AppData appData)
        {
            bool criterion = appData.TasksResult.TryGetValue(Task_GRM_UpdateConfig.RezultName, out bool check);
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

            Rezult_Success = true;
            try
            {
                ProcessingRezult = webService.Complex_UpdateProcessing();
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
                appData.ProcessingRezult = ProcessingRezult;
            }
        }
    }
}
