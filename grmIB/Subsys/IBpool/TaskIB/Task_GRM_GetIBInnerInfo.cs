using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using grmIB.Entitties;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class Task_GRM_GetIBInnerInfo : ITaskIB
    {
        public static readonly string RezultName = "GRM_GetIBInnerInfo";

        /*
        string url;
        string UserName;
        string UserPassword;
        */

        //string ConnectPlatformVersion;
        COMConnector Connector = new COMConnector();
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

            if (!Connector.SetPlatfomVersion(appData.GRMApp.PlatformVersion))
            {
                Context_Success = false;
                return Context_Success;
            }

            string url = appData.GRMApp.URL;
            if (string.IsNullOrEmpty(url))
            {
                Context_Success = false;
                return Context_Success;
            }

            Connector.SetConnectionString(url, appData.User, appData.Password);

            return Context_Success;
        }

        public void Execute()
        {
            if (!Context_Success)
            {
                return;
            }

            C1IBInnerInfo Info = Connector.GetIBInnerInfo();

            //?Второй трай

            GRMApplication temp = RestAdapter.GetApplicationByName("");
            if (temp != null)
            {
                //Logger.Information("Получены сведения о приложении {@name}", appName);
                Rezult_Success = true;
                //GRMApplication = temp;
            }
            else
            {
                //Logger.Error("Ошибка запроса информации о приложении.");
                Rezult_Success = false;
            }
        }

        void ITaskIB.GetRezult(AppData appData)
        {
            throw new NotImplementedException();
        }
    }
}
