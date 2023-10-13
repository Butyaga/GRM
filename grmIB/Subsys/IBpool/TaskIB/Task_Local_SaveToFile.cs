using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Serilog;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class Task_Local_SaveToFile : ITaskIB
    {
        //private readonly static ILogger Logger = Log.Logger.ForContext<Task_Local_SaveToFile>();

        bool Context_Success = false;
        bool Rezult_Success = false;

        AppData Data;
        public static readonly string RezultName = "Local_SaveToFile";

        public bool Predicate(AppData appData)
        {
            return true;
        }
        public bool SetContext(AppData appData)
        {
            Data = appData;
            Context_Success = true;
            return Context_Success;
        }
        public void Execute()
        {
            //Logger.Information("Инициализация сохранения состояния задач в файл");
            Rezult_Success = Data.SaveToFile();
            if (Rezult_Success)
            {
                //Logger.Information("Успешная сохранение в файл");
            }
            else
            {
                //Logger.Information("Ошибка при сохранении файла");
            }
        }
        public void GetRezult(AppData appData)
        {
            bool rezult = Context_Success && Rezult_Success;
            appData.TasksResult.Add(RezultName, rezult);
        }
    }
}
