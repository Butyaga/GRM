using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Serilog;
using grmIB.Entitties;
using System.IO;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class Task_GRM_DownloadBackups : ITaskIB
    {
        //private readonly static ILogger Logger = Log.Logger.ForContext<Task_GRM_UploadConfigUpdate>();

        bool Context_Success = false;
        bool Rezult_Success = false;

        string applicationID = "";
        string applicationName = "";
        string localPath = "";
        List<GRMApplicationBackup> listGRMFiles;

        public static readonly string RezultName = "GRM_DownloadBackups";

        public bool Predicate(AppData appData)
        {
            //Logger.Information("Проверка условий для запуска задачи скачивания резервных копий.");
            bool criterion = appData.TasksResult.TryGetValue(Task_GRM_CheckApplication.RezultName, out bool check);
            //Logger.Debug("Существование результата {rezult} - {criterion}", Task_GRM_CheckApplication.RezultName, criterion);
            criterion = criterion && check;
            //Logger.Information("Результат проверки условий - {criterion}", criterion);
            return criterion;
        }
        public bool SetContext(AppData appData)
        {
            applicationID = appData.GRMApp.ID;
            if (applicationID == "")
            {
                //Logger.Error("Идентификатор приложения не найден");
                Context_Success = false;
                return Context_Success;
            }
            if (string.IsNullOrEmpty(appData.Alias))
            {
                applicationName = appData.Name;
            }
            else
            {
                applicationName = appData.Alias;
            }

            listGRMFiles = RestAdapter.GetApplicationBackupList(applicationID);
            DateTime dateTime24HAgo = DateTime.Now.AddHours(-24);
            listGRMFiles.RemoveAll(file => file.Date <  dateTime24HAgo);
            if (listGRMFiles.Count == 0)
            {
                //Logger.Error("За последние 24 часа нет резервных копий");
                Context_Success = false;
                return Context_Success;
            }

            localPath = appData.LocalBackupPath;
            if (Directory.Exists(localPath))
            {
                Context_Success = true;
            }
            else
            {
                if (Directory.CreateDirectory(localPath).Exists)
                {
                    Context_Success = true;
                }
                else
                {
                    //Logger.Error("Не удалось создать каталог {path}", localPath);
                    Context_Success = false;
                }
            }
            
            return Context_Success;
        }
        public void Execute()
        {
            //Logger.Information("Инициация скачивания резервных копий за последний 24 часа для приложения {app}", applicationID);
            //Logger.Information("Каталог для скачивания файлов {path}", localPath);
            Rezult_Success = true;
            foreach (GRMApplicationBackup file in listGRMFiles)
            {
                string fullPath = localPath + "\\" + applicationName + file.Date.ToString("_yyyyMMdd_HHmm") + ".zip";
                //Logger.Information("Начало скачивания файла {fullPath}", fullPath);
                bool rezult = RestAdapter.ApplicationBackupDownload(applicationID, file.ID, fullPath);
                if (rezult)
                {
                    //Logger.Information("Файл успешно скачан {fullPath}", fullPath);
                }
                else
                {
                    //Logger.Error("Ошибка скачивания Файла {fullPath}", fullPath);
                }
                Rezult_Success = Rezult_Success && rezult;
            }

            if (Rezult_Success)
            {
                //Logger.Information("Успешное завершение задачи скачивания резервных копий");
            }
            else
            {
                //Logger.Information("Ошибка при скачивании резервных копий");
            }
        }
        public void GetRezult(AppData appData)
        {
            bool rezult = Context_Success && Rezult_Success;
            appData.TasksResult.Add(RezultName, rezult);
        }
    }
}
