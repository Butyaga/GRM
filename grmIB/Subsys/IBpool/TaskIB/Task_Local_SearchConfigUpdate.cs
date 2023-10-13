using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestAPI.C1;
//using Serilog;
using System.IO;

namespace grmIB.Subsys.IBpool.TaskIB
{
    class Task_Local_SearchConfigUpdate : ITaskIB
    {
        //private readonly static ILogger Logger = Log.Logger.ForContext<Task_Local_SearchConfigUpdate>();

        public static readonly string RezultName = "Local_SearchConfigUpdate";
        static readonly Dictionary<string, string> ConfigLocation = new Dictionary<string, string>()
        {
            { "1C:Бухгалтерия 8 КОРП", "\\AccountingCorp" },
            { "БухгалтерияПредприятияКОРП", "\\AccountingCorp" },
            { "1C:Бухгалтерия 8 ПРОФ", "\\Accounting" },
            { "БухгалтерияПредприятия", "\\Accounting" },
            { "БухгалтерияПредприятияБазовая", "\\AccountingBase" },
            { "1С:Зарплата и Управление Персоналом 8 КОРП", "\\hrmcorp" },
            { "ЗарплатаИУправлениеПерсоналомКОРП", "\\hrmcorp" },
            { "1С:Зарплата и Управление Персоналом 8", "\\hrm" },
            { "ЗарплатаИУправлениеПерсоналом", "\\hrm" },
            { "ЗарплатаИУправлениеПерсоналомБазовая", "\\hrmbase" },
            { "УправлениеТорговлей", "\\trade" },
            { "УправлениеТорговлейБазовая", "\\tradebase" }
        };

        bool Context_Success = false;
        bool Rezult_Success = false;
        C1ConfigUpdate Rezult_ConfigUpdate;

        string ConfigName;
        C1Version Version;
        string UpdatePath;

        public void Execute()
        {
            string UpdateSearchPath = GetUpdatesLocation(ConfigName);
            List<C1ConfigUpdate> listUpdCfg = C1ConfigUpdate.GetListUpdateConfig(UpdateSearchPath);
            listUpdCfg.RemoveAll(cfg => !cfg.FromVersion.Contains(Version));

            if (listUpdCfg.Count == 0)
            {
                Rezult_ConfigUpdate = null;
                Rezult_Success = false;
                return;
            }

            listUpdCfg.Sort( delegate(C1ConfigUpdate x, C1ConfigUpdate y)
            {
                return x.Version.CompareTo(y.Version);
            });

            Rezult_ConfigUpdate = listUpdCfg.Last();
            Rezult_Success = true;
        }

        public void GetRezult(AppData appData)
        {
            bool rezult = Context_Success && Rezult_Success;
            appData.TasksResult.Add(RezultName, rezult);
            if (rezult)
            {
                appData.ConfigUpdate = Rezult_ConfigUpdate;
            }
        }

        public bool Predicate(AppData appData)
        {
            //Logger.Information("Проверка условий для запуска задачи.");
            bool criterion = appData.TasksResult.TryGetValue(Task_GRM_CheckApplication.RezultName, out bool check);
            //Logger.Information("Существование результата {rezult} - {criterion}", Task_GRM_CheckApplication.RezultName, criterion);
            criterion = criterion && check;
            //Logger.Information("Результат проверки условий - {criterion}", criterion);
            return criterion;
        }

        public bool SetContext(AppData appData)
        {
            if (string.IsNullOrEmpty(appData.IBInnerInfo?.ConfName))
            {
                ConfigName = appData.GRMApp.ConfigurationName;
            }
            else
            {
                ConfigName = appData.IBInnerInfo.ConfName;
            }
            //Logger.Information("Имя конфигурации для поиска обновления: {neme}", ConfigName);

            if (string.IsNullOrEmpty(appData.IBInnerInfo?.ConfVersion))
            {
                Version = new C1Version(appData.GRMApp.Version);
            }
            else
            {
                Version = new C1Version(appData.IBInnerInfo.ConfVersion);
            }

            
            Context_Success = Version.SuccessConstruct;
            if (Context_Success)
            {
                //Logger.Information("Версии конфигурации для поиска - {version}", appData.GRMApp.Version);
            }
            else
            {
                //Logger.Error("Ошибка считывания версии конфигурации {version}", appData.GRMApp.Version);
                return Context_Success;
            }

            UpdatePath = GeneralOptions.PathTypicalConfig;
            Context_Success = Directory.Exists(UpdatePath);
            if (Context_Success)
            {
                //Logger.Information("Поиск обновлений конфигурации будет произведен в {path}", UpdatePath);
            }
            else
            {
                //Logger.Error("Каталог для поиска обновлений конфигурации не существует: {path}", UpdatePath);
                return Context_Success;
            }

            return Context_Success;
        }


        private string GetUpdatesLocation(string configName)
        {
            if (ConfigLocation.TryGetValue(configName, out string TargetCatalog))
            {
                string fullPath = UpdatePath + TargetCatalog;
                if (Directory.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            return "";
        }
    }
}
