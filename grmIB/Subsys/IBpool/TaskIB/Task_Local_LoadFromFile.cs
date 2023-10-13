using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Serilog;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class Task_Local_LoadFromFile:ITaskIB
    {
        //private readonly static ILogger Logger = Log.Logger.ForContext<Task_Local_LoadFromFile>();

        bool Context_Success = false;
        bool Rezult_Success = false;

        AppData Data;
        public static readonly string RezultName = "Local_LoadFromFile";

        public bool Predicate(AppData appData)
        {
            return true;
        }
        public bool SetContext(AppData appData)
        {
            Context_Success = true;
            return Context_Success;
        }
        public void Execute()
        {
            //Logger.Information("Инициализация загрузки состояния задач из файла");
            Data = AppData.LoadFromFile("");
            if (Data == null)
            {
                Rezult_Success = false;
                //Logger.Information("Ошибка при загрузке из файла");
            }
            else
            {
                Rezult_Success = true;
                //Logger.Information("Успешная загрузка из файла");
            }
        }
        public void GetRezult(AppData appData)
        {
            bool rezult = Context_Success && Rezult_Success;
            appData.TasksResult.Add(RezultName, rezult);
        }
    }
}
