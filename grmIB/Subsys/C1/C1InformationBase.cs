using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using grmIB.Entitties;
using grmIB.Subsys;
//using Serilog;

namespace RestAPI.C1
{
    class C1InformationBase
    {
        //private readonly static ILogger Logger = Log.Logger.ForContext<C1InformationBase>();
        string IBName;
        GRMApplication GRMApp;
        List<GRMApplicationFileInfo> GRMFiles;
        List<GRMApplicationBackup> GRMBackups;
        bool SuccessRequest;

        /// Constractors
        C1InformationBase() { }
        C1InformationBase(string str)
        {
            IBName = str;
        }

        /// Public Methods
        public string GetIBConfigName()
        {
            return GRMApp.ConfigurationName;
        }
        public string GetIBConfigVersion()
        {
            return GRMApp.Version;
        }
        public void UpdateInfoApp()
        {
            GRMApplication temp = RestAdapter.GetApplicationByName(IBName);
            if (temp == null)
            {
                SuccessRequest = false;
                //Logger.Error("Не найдена ИБ с именем {Name} или ошибка подключения к серису 1С:ГРМ", IBName);
            }
            else
            {
                GRMApp = temp;
                SuccessRequest = true;
            }
        }
        public void UpdateInfoExchange()
        {
            if (GRMApp == null)
            {
                UpdateInfoApp();
            }
            if (SuccessRequest)
            {
                List<GRMApplicationFileInfo> temp = RestAdapter.GetApplicationFileList(GRMApp.ID);
                if (temp == null)
                {
                    SuccessRequest = false;
                    //Logger.Error("Ошибка получения файлв обмена для ИБ с именем {Name} или ошибка подключения к серису 1С:ГРМ", IBName);
                }
                else
                {
                    GRMFiles = temp;
                    SuccessRequest = true;
                }
            }
        }
        public void UpdateInfoBackup()
        {
            if (GRMApp == null)
            {
                UpdateInfoApp();
            }
            if (SuccessRequest)
            {
                List<GRMApplicationBackup> temp = RestAdapter.GetApplicationBackupList(GRMApp.ID);
                if (temp == null)
                {
                    SuccessRequest = false;
                    //Logger.Error("Ошибка получения списка резервных копий для ИБ с именем {Name} или ошибка подключения к серису 1С:ГРМ", IBName);
                }
                else
                {
                    GRMBackups = temp;
                    SuccessRequest = true;
                }
            }
        }
    }
}
