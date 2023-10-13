using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using grmIB.Messages;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
//using Serilog;
using grmIB.Subsys.Http;

namespace grmIB.Entitties
{
    class GRMApplication
    {
        public string ID { get; }
        public string ConfigurationId { get; }
        public string ConfigurationName { get; }
        string ConfigurationVersionId { get; }
        string InitialVersion { get; }
        public string Version { get; }
        string PlatformVersionId { get; }

        /// <summary>
        /// Версия платформы
        /// </summary>
        public string PlatformVersion { get; }
        public string Name { get; }
        public string Status { get; }

        /// <summary>
        /// URL для доступа к приложению
        /// </summary>
        public string URL { get; }
        int LicenseCount { get; }
        DateTime ScheduledDeleteDate { get; }
        bool Deleted { get; }
        string CustomerId { get; }

        public GRMApplication() { }
        public GRMApplication(MessageAplication message)
        {
            ID = message.id;
            ConfigurationId = message.configurationId;
            ConfigurationName = message.configurationName;
            ConfigurationVersionId = message.configurationVersionId;
            InitialVersion = message.initialVersion;
            Version = message.version;
            PlatformVersionId = message.platformVersionId;
            PlatformVersion = message.platformVersion;
            Name = message.name;
            Status = message.status;
            URL = message.url;
            LicenseCount = message.licenseCount;
            ScheduledDeleteDate = message.scheduledDeleteDate;
            Deleted = message.deleted; 
            CustomerId = message.customerId;
        }
        public static GRMApplication FindByName(List<GRMApplication> applications, string name)
        {
            foreach (GRMApplication application in applications)
            {
                if (application.Name == name)
                {
                    return application;
                }
            }
            return null;
        }
        public static List<GRMApplication> CreateListFromJSON(string str)
        {
            List<GRMApplication> applications = new List<GRMApplication>();

            var jsonSS = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            List<MessageAplication> messages = JsonConvert.DeserializeObject<List<MessageAplication>>(str, jsonSS);

            if (messages != null)
            {
                foreach (var message in messages)
                {
                    applications.Add(new GRMApplication(message));
                }
            }
            return applications;
        }
        public static GRMApplication CreateApplicationFromJSON(string str)
        {
            var jsonSS = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            MessageAplication message = JsonConvert.DeserializeObject<MessageAplication>(str, jsonSS);

            return new GRMApplication(message);
        }
        public GRMApplication RestartApplication()
        {
            //Log.Information("Restart application: {Name}", Name);
            string sResult = GRMRestAPI.RestartApplication(ID);
            return CreateApplicationFromJSON(sResult);
        }
        public bool WaitForStatusRunning()
        {
            bool bSuccess = false;
            string sStatusRunning = "running";
            int iMaxMinutesCount = 40; // Максимальное количество циклов ожидания
            int iCountPause = 60000; // пауза в милисекундах
            GRMApplication oResult;
            //Log.Debug("Ожидание запуска приложения {Name}. Максимальное время ожидания {WTime} минут", Name, iMaxMinutesCount*iCountPause/60000);

            for (int i = 0; i < iMaxMinutesCount; i++)
            {
                Thread.Sleep(iCountPause);
                oResult = grmIB.Subsys.RestAdapter.GetApplicationByID(ID);
                
                if (oResult.Status == sStatusRunning)
                {
                    bSuccess = true;
                    break;
                }
            }
            return bSuccess;
        }
    }
}
