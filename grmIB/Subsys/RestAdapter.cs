using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Serilog;
using grmIB.Entitties;
using grmIB.Subsys.Http;
using grmIB.Messages;
using Newtonsoft.Json;
using System.Net;
using System.Threading;

namespace grmIB.Subsys
{
    static class RestAdapter
    {
        private static AutoResetEvent QueueRESTRequest = new AutoResetEvent(true);
        private static AutoResetEvent QueueFileUpload = new AutoResetEvent(true);
        private static AutoResetEvent QueueFileDownload = new AutoResetEvent(true);

        public static List<GRMApplication> GetApplications()
        {
            QueueRESTRequest.WaitOne();
            string sMessage = GRMRestAPI.GetApplications();
            QueueRESTRequest.Set();
            List<GRMApplication> result = GRMApplication.CreateListFromJSON(sMessage);
            return result;
        }
        public static GRMApplication GetApplicationByID(string ID)
        {
            QueueRESTRequest.WaitOne();
            string sMessage = GRMRestAPI.GetApplicationByID(ID);
            QueueRESTRequest.Set();
            GRMApplication result = GRMApplication.CreateApplicationFromJSON(sMessage);
            return result;
        }
        public static GRMApplication GetApplicationByName(string Name)
        {
            List<GRMApplication> listApps = GetApplications();
            GRMApplication app = GRMApplication.FindByName(listApps, Name);
            return app;
        }
        public static List<GRMApplicationFileInfo> GetApplicationFileList(string ID)
        {
            QueueRESTRequest.WaitOne();
            string sMessage = GRMRestAPI.GetApplicationFileList(ID);
            QueueRESTRequest.Set();
            List<GRMApplicationFileInfo> result = GRMApplicationFileInfo.CreateListFileInfoFromJSON(sMessage);
            return result;
        }
        public static List<GRMApplicationBackup> GetApplicationBackupList(string ID)
        {
            QueueRESTRequest.WaitOne();
            string sMessage = GRMRestAPI.GetApplicationBackupList(ID);
            QueueRESTRequest.Set();
            List<GRMApplicationBackup> result = GRMApplicationBackup.CreateListBackupFromJSON(sMessage);
            return result;
        }
        public static GRMApplicationBackup GetApplicationBackupByID(string appID, string backupID)
        {
            QueueRESTRequest.WaitOne();
            string sMessage = GRMRestAPI.GetApplicationBackupByID(appID, backupID);
            QueueRESTRequest.Set();
            GRMApplicationBackup result = GRMApplicationBackup.CreateBackupFromJSON(sMessage);
            return result;
        }
        public static GRMApplicationBackup BackupApplication(string ID)
        {
            QueueRESTRequest.WaitOne();
            string sMessage = GRMRestAPI.BackupApplication(ID);
            QueueRESTRequest.Set();
            GRMApplicationBackup result = GRMApplicationBackup.CreateBackupFromJSON(sMessage);

            GRMApplication app = GetApplicationByID(ID);
            app.WaitForStatusRunning();

            try
            {
                result = GetApplicationBackupByID(app.ID, result.ID);
            }
            catch (WebException excpt)
            {
                if (excpt.Response is HttpWebResponse excResp)
                {
                    if (excResp.StatusCode == HttpStatusCode.NotFound) result = null;
                    else throw;
                }
                else throw;
            }
            return result;
        }
        public static bool ApplicationFileUpload(string applicationID, string filepath, string remotefilename)
        {
            QueueFileUpload.WaitOne();
            bool result = GRMRestAPI.ApplicationFileUpload(applicationID, filepath, remotefilename);
            QueueFileUpload.Set();
            return result;
        }
        public static bool ApplicationFileInstall(string applicationID, string fileneme, string user, string password)
        {
            //Log.Information("Истановка для приложения {app} из файла {file} под уч. данными {usr}/{pass}", applicationID, fileneme, user, password);
            MessageCredentials Credentials = new MessageCredentials(user, password);
            GRMApplication app = GetApplicationByID(applicationID);
            //Log.Debug("App {@app}", app);

            QueueRESTRequest.WaitOne();
            GRMRestAPI.ApplicationInstallFile(applicationID, fileneme, Credentials);
            QueueRESTRequest.Set();

            bool result = app.WaitForStatusRunning();
            return result;
        }
        public static bool ApplicationFileDelete(string appID, string filename)
        {
            QueueRESTRequest.WaitOne();
            bool result = GRMRestAPI.ApplicationFileDelete(appID, filename);
            QueueRESTRequest.Set();
            return result;
        }
        public static bool ApplicationBackupDownload(string appID, string backupID, string path)
        {
            GRMApplicationBackup backup = GetApplicationBackupByID(appID, backupID);

            QueueFileDownload.WaitOne();
            HttpFileDownload fileDL = new HttpFileDownload(path, backup.Link, backup.Size);
            try
            {
                fileDL.Init();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            QueueFileDownload.Set();

            bool result = false;
            if (fileDL.ResposeCode == 200)
            {
                result = true;
                //Log.Debug("Успешное скачивание файла");
            }
            //else Log.Error("Ошибка при скачивании файла");

            return result;
        }
        public static bool ApplicationBackupDelete(string appID, string backupID)
        {
            QueueRESTRequest.WaitOne();
            bool result = GRMRestAPI.ApplicationBackupDelete(appID, backupID);
            QueueRESTRequest.Set();
            return result;
        }
        public static long GetApplicationSpace(string appID)
        {
            QueueRESTRequest.WaitOne();
            string sMessage = GRMRestAPI.GetApplicationResources(appID);
            QueueRESTRequest.Set();
            List<GRMApplicationResource> listResource = GRMApplicationResource.CreateListFromJSON(sMessage);
            long result = GRMApplicationResource.GetApplicationSpace(listResource);
            return result;
        }
    }
}
