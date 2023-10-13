using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Serilog;
using System.IO;
using RestAPI.C1;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class Task_GRM_UploadConfigUpdate:ITaskIB
    {
        //private readonly static ILogger Logger = Log.Logger.ForContext<Task_GRM_UploadConfigUpdate>();

        bool Context_Success = false;
        bool Rezult_Success = false;

        string applicationID = "";
        string updateFilePath = "";
        string remotefilename = "";

        public static readonly string RezultName = "GRM_UploadConfigUpdate";
        string[] updateFileNames = { "\\1cv8.cfu", "\\1cv8.cf" };

        public bool Predicate(AppData appData)
        {
            //Logger.Information("Проверка условий для запуска задачи закачки обновления.");
            bool criterion = appData.TasksResult.TryGetValue(Task_Local_SearchConfigUpdate.RezultName, out bool check);
            //Logger.Information("Существование результата {rezult} - {criterion}", Task_Local_SearchConfigUpdate.RezultName, criterion);
            criterion = criterion && check;
            //Logger.Information("Результат проверки условий - {criterion}", criterion);
            return criterion;
        }
        public bool SetContext(AppData appData)
        {
            applicationID = appData.GRMApp.ID;
            if (applicationID == "")
            {
                Context_Success = false;
                return Context_Success;
            }
            else
            {
                Context_Success = true;
            }
            //Logger.Information("ID приложения для загрузки обновления: {id}", applicationID);

            string updateDirPath = appData.ConfigUpdate.UpdatePath;
            if (Directory.Exists(updateDirPath))
            {
                foreach (string file in updateFileNames)
                {
                    string fileName = updateDirPath + file;
                    if (File.Exists(fileName))
                    {
                        updateFilePath = fileName;
                        break;
                    }
                }
                if (updateFilePath != "")
                {
                    //Logger.Information("Найден непосредственный файл обновления: {path}", updateFilePath);
                    Context_Success = true;
                }
                else
                {
                    //Logger.Error("Не найден непосредственный файл обновления");
                    Context_Success = false;
                }
            }
            else
            {
                //Logger.Error("Каталог с обновлением не найден: {path}", updateDirPath);
                Context_Success = false;
            }

            remotefilename = GetFileName(updateFilePath, appData.ConfigUpdate);
            //Logger.Information("Имя загружаемого файла в обмене ГРМ: {name}", remotefilename);

            if (Context_Success)
            {
                //Logger.Information("Успешное задание контекста выполнения задачи");
            }
            else
            {
                //Logger.Information("Ошибка при задании контекста выполнения задачи");
            }
            return Context_Success;
        }
        public void Execute()
        {
            //Logger.Information("Инициация отправлки файла {path} для приложения {id} с именем {name}", updateFilePath, applicationID, remotefilename);
            Rezult_Success = RestAdapter.ApplicationFileUpload(applicationID, updateFilePath, remotefilename);
            if (Rezult_Success)
            {
                //Logger.Information("Успешная отправка файла");
            }
            else
            {
                //Logger.Information("Ошибка при отправке файла");
            }
        }
        public void GetRezult(AppData appData)
        {
            bool rezult = Context_Success && Rezult_Success;
            appData.TasksResult.Add(RezultName, rezult);
            if (rezult)
            {
                appData.GRMUpdateFileName = remotefilename;
            }
        }

        private string GetFileName(string path, C1ConfigUpdate configUpdate)
        {
            string fileExtension = Path.GetExtension(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (GeneralOptions.GRMRemouteFileName != "")
            {
                fileName = GeneralOptions.GRMRemouteFileName;
            }

            if (GeneralOptions.GRMRemouteFileAddVersion)
            {
                fileName += "_" + configUpdate.Version.ToString();
            }

            DateTime dt = DateTime.Now;
            if (GeneralOptions.GRMRemouteFileAddDate)
            {
                fileName += "_" + dt.ToString("yyyyMMdd");
            }

            if (GeneralOptions.GRMRemouteFileAddTime)
            {
                fileName += "_" + dt.ToString("HHmm");
            }

            return fileName + "." + fileExtension;
        }
    }
}
