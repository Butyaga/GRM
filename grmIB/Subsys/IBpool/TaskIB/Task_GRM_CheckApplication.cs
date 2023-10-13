using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using grmIB.Entitties;
//using Serilog;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class Task_GRM_CheckApplication: ITaskIB
    {
        //private readonly static ILogger Logger = Log.Logger.ForContext<Task_GRM_CheckApplication>();

        public static readonly string RezultName = "GRM_CheckApplication";

        string appName;
        bool Context_Success = false;
        bool Rezult_Success = false;

        GRMApplication GRMApplication;

        public bool Predicate(AppData appData)
        {
            return true;
        }
        public bool SetContext(AppData appData)
        {
            //Logger.Information("Задание контекста для задачи проверки существования приложения ГРМ.");
            appName = appData.Name;
            //Logger.Information("Имя приложения ГРМ для проверки {neme}.", appName);
            if (appName == "")
            {
                //Logger.Error("Не задано имя приложения для проверки в сервисе ГРМ.");
                Context_Success = false;
            }
            else
            {
                Context_Success = true;
            }
            return Context_Success;
        }
        public void Execute()
        {
            if (!Context_Success)
            {
                return;
            }
            //Logger.Information("Запуск задачи проверки существования приложения ГРМ {name}", appName);
            GRMApplication temp = RestAdapter.GetApplicationByName(appName);
            if (temp != null)
            {
                //Logger.Information("Получены сведения о приложении {@name}", appName);
                Rezult_Success = true;
                GRMApplication = temp;
            }
            else
            {
                //Logger.Error("Ошибка запроса информации о приложении.");
                Rezult_Success = false;
            }
        }
        public void GetRezult(AppData appData)
        {
            bool rezult = Context_Success && Rezult_Success;
            appData.TasksResult.Add(RezultName, rezult);
            if (rezult)
            {
                appData.GRMApp = GRMApplication;
            }
        }
    }
}
