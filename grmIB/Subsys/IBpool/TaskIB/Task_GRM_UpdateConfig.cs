using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Serilog;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class Task_GRM_UpdateConfig: ITaskIB
    {
        //private readonly static ILogger Logger = Log.Logger.ForContext<Task_GRM_UpdateConfig>();

        public static readonly string RezultName = "GRM_UpdateConfig";

        bool Context_Success = false;
        bool Rezult_Success = false;

        string applicationID = "";
        string fileneme = "";
        string user = "";
        string password = "";

        public bool Predicate(AppData appData)
        {
            //Logger.Information("Проверка условий для запуска задачи обновления конфигурации.");
            bool criterion = appData.TasksResult.TryGetValue(Task_GRM_UploadConfigUpdate.RezultName, out bool check);
            //Logger.Debug("Существование результата {rezult} - {criterion}", Task_Local_SearchConfigUpdate.RezultName, criterion);
            criterion = criterion && check;
            //Logger.Information("Результат проверки условий - {criterion}", criterion);
            return criterion;
        }
        public bool SetContext(AppData appData)
        {
            //Logger.Information("Задание контекста для задачи обновления конфигурации приложения ГРМ.");
            applicationID = appData.GRMApp.ID;
            //Logger.Information("Идентификатор приложения ГРМ для проверки {id}.", applicationID);
            if (applicationID == "")
            {
                //Logger.Error("Не задан идентификатор приложения в сервисе ГРМ.");
                Context_Success = false;
                return Context_Success;
            }
            
            fileneme = appData.GRMUpdateFileName;
            if (fileneme == "")
            {
                //Logger.Error("Не задано имя файла обновления конфигурации приложения в сервисе ГРМ.");
                Context_Success = false;
                return Context_Success;
            }

            user = appData.User;
            if (user == "")
            {
                //Logger.Error("Не задано имя пользователя информационной базы в сервисе ГРМ.");
                Context_Success = false;
                return Context_Success;
            }

            password = appData.Password;
            if (password == "")
            {
                //Logger.Error("Не задан пароль пользователя информационной базы в сервисе ГРМ.");
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

            //Logger.Information("Запуск задачи обновления конфигурации приложения ГРМ {id}", applicationID);
            Rezult_Success = RestAdapter.ApplicationFileInstall(applicationID, fileneme, user, password);

            if (Rezult_Success)
            {
                //Logger.Information("Конфигурация приложения ГРМ успешно обновлена");
            }
            else
            {
                //Logger.Error("Ошибка при обновлении конфигурации приложения ГРМ");
            }
        }
        public void GetRezult(AppData appData)
        {
            bool rezult = Context_Success && Rezult_Success;
            appData.TasksResult.Add(RezultName, rezult);
        }
    }
}
