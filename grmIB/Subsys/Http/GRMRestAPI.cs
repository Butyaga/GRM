using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
//using Serilog;
using grmIB.Messages;
using Newtonsoft.Json;
using System.Reflection;

namespace grmIB.Subsys.Http
{
    static class GRMRestAPI
    {
        static readonly string sContextAPI = "https://service-api.1capp.com/partner-api/v2";

        static readonly string sDestPoint_Configurations = "/configurations";
        static readonly string sDestPoint_Customers = "/customers";
        static readonly string sDestPoint_Applications = "/applications";

        static readonly string sPostfix_ApplicationRestart = "/action/restart";
        static readonly string sPostfix_ApplicationExchanges = "/exchanges";
        static readonly string sPostfix_ApplicationFileUpload = "/exchanges/upload";
        static readonly string sPostfix_ApplicationInstall = "/exchanges/install";
        static readonly string sPostfix_ApplicationBackup = "/backups";
        static readonly string sPostfix_ApplicationResources = "/resources";

        private static string sToken;
        private static string sMethod;
        private static string ContentType;
        private static Dictionary<string,string> Params = new Dictionary<string, string>();
        private static byte[] SendData;

        /*static public string GetConfigurations(out int StatusCode)
        {
            StatusCode = 200;
            HttpWebRequest request = GetNewRequest(sContextAPI + sDestPoint_Configurations);
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException excpt)
            {
                response = excpt.Response as HttpWebResponse;
                StatusCode = (int)excpt.Status;
            }
            StreamReader stream = new StreamReader(response.GetResponseStream());
            return stream.ReadToEnd();
        }*/

        static public string GetApplications()
        {
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications;

            string sMessage = DoRequest(sUri);

            return sMessage;
        }
        public static string GetApplicationByID(string applicationID)
        {
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications + "/" + applicationID;

            string sMessage = DoRequest(sUri);

            return sMessage;
        }
        public static string RestartApplication(string applicationID)
        {
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications + "/" + applicationID + sPostfix_ApplicationRestart;
            sMethod = WebRequestMethods.Http.Post;

            string sMessage = DoRequest(sUri);

            return sMessage;
        }
        public static string BackupApplication(string applicationID)
        {
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications + "/" + applicationID + sPostfix_ApplicationBackup;
            sMethod = WebRequestMethods.Http.Post;

            string sMessage = DoRequest(sUri);

            return sMessage;
        }
        public static bool ApplicationBackupDelete(string applicationID, string backupID)
        {
            //Log.Information("Инициация Удаления резервной копии с ИД {file} для приложения {ID}", backupID, applicationID);
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications + "/" + applicationID + sPostfix_ApplicationBackup + "/" + backupID;
            //Log.Debug("Ссылка запроса: {url}", sUri);
            sMethod = "DELETE";
            bool result = true;

            try
            {
                DoRequest(sUri);
                //Log.Debug("Файл удален");
            }
            catch (WebException excpt)
            {
                if (excpt.Response is HttpWebResponse excResp)
                {
                    //Log.Error(excpt, "Ответ веб-сервера - ошибка: код={Code}, Сообщение={Message}", (int)excResp.StatusCode, StreamToString(excResp.GetResponseStream()));
                    result = false;
                }
                else
                {
                    //Log.Error(excpt, "Неизвестная ошибка: код={Code}, Сообщение={Message}", (int)excpt.Status, excpt.Message);
                    throw;
                }
            }

            return result;
        }
        public static bool ApplicationFileUpload(string applicationID, string filepath, string remotefilename)
        {
            //Log.Information("Инициация загрузки файла {file} с именем {remotefilename} для приложения {ID}", filepath, remotefilename, applicationID);
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications + "/" + applicationID + sPostfix_ApplicationFileUpload;
            //Log.Debug("Ссылка запроса: {url}", sUri);
            sMethod = WebRequestMethods.Http.Post;
            Params.Add("filename", remotefilename);

            string sMessage = DoRequest(sUri);
            //Log.Debug("Сервер вернул сообщение: {Message}", sMessage);

            var jsonSS = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            MessageExchangeUploadInfo UploadInfo = JsonConvert.DeserializeObject<MessageExchangeUploadInfo>(sMessage, jsonSS);

            //Log.Debug("Ссылка для отправки файла: {url}", UploadInfo.url);
            HttpFileUpload fileUpload = new HttpFileUpload(filepath, UploadInfo.url);
            fileUpload.Init();

            bool result = false;
            if (fileUpload.ResposeCode == 200)
            {
                result = true;
                //Log.Debug("Успешная отправка файла");
            }
            //else Log.Error("Ошибка при отправке файла");
            return result;
        }
        public static void ApplicationInstallFile(string applicationID, string fileneme, MessageCredentials Credentials)
        {
            //Log.Information("Инициация установки из файла {file} для приложения {ID}", fileneme, applicationID);
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications + "/" + applicationID + sPostfix_ApplicationInstall;
            //Log.Debug("Ссылка запроса: {url}", sUri);

            sMethod = WebRequestMethods.Http.Post;
            Params.Add("filename", fileneme);
            ContentType = "application/json";

            CredentialsToSendData(Credentials);

            string sMessage = DoRequest(sUri);
            //Log.Information("Ответное сообщение: {message}", sMessage);
        }
        public static bool ApplicationFileDelete(string applicationID, string filename)
        {
            //Log.Information("Инициация Удаления файла {file} для приложения {ID}", filename, applicationID);
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications + "/" + applicationID + sPostfix_ApplicationExchanges;
            //Log.Debug("Ссылка запроса: {url}", sUri);
            sMethod = "DELETE";
            Params.Add("filename", filename);
            bool result = true;

            try
            {
                DoRequest(sUri);
                //Log.Debug("Файл удален");
            }
            catch (WebException excpt)
            {
                if (excpt.Response is HttpWebResponse excResp)
                {
                    //Log.Error(excpt, "Ответ веб-сервера - ошибка: код={Code}, Сообщение={Message}", (int)excResp.StatusCode, StreamToString(excResp.GetResponseStream()));
                    result = false;
                }
                else
                {
                    //Log.Error(excpt, "Неизвестная ошибка: код={Code}, Сообщение={Message}", (int)excpt.Status, excpt.Message);
                    throw;
                }
            }

            return result;
        }
        public static string GetApplicationBackupList(string applicationID)
        {
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications + "/" + applicationID + sPostfix_ApplicationBackup;

            string sMessage = DoRequest(sUri);

            return sMessage;
        }
        public static string GetApplicationBackupByID(string applicationID, string backupID)
        {
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications + "/" + applicationID + sPostfix_ApplicationBackup + "/" + backupID;

            string sMessage = DoRequest(sUri);

            return sMessage;
        }
        public static string GetApplicationFileList(string applicationID)
        {
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications + "/" + applicationID + sPostfix_ApplicationExchanges;

            string sMessage = DoRequest(sUri);

            return sMessage;
        }
        public static string GetApplicationResources(string applicationID)
        {
            SetDefault();
            string sUri = sContextAPI + sDestPoint_Applications + "/" + applicationID + sPostfix_ApplicationResources;

            string sMessage = DoRequest(sUri);

            return sMessage;
        }

        public static void SetAuthToken(string token)
        {
            sToken = token;
        }
        private static void SetDefault()
        {
            sMethod = WebRequestMethods.Http.Get;
            ContentType = null;
            Params.Clear();
            SendData = null;
        }
        /*static public string GetCustomers(out int StatusCode)
        {
            StatusCode = 200;
            HttpWebRequest request = GetNewRequest(sContextAPI + sDestPoint_Customers);
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException excpt)
            {
                response = excpt.Response as HttpWebResponse;
                StatusCode = (int)excpt.Status;
            }
            StreamReader stream = new StreamReader(response.GetResponseStream());
            return stream.ReadToEnd();
        }*/
        /*private static string StreamToString(Stream strm)
        {
            StreamReader reader = new StreamReader(strm);
            return reader.ReadToEnd();
        }*/
        /*private static HttpWebRequest GetNewRequest(string sUri)
        {
            Uri uriDest = new Uri(sUri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriDest);
            AddHeaderAuthorization(request);
            return request;
        }*/
        /*private static void AddHeaderAuthorization(HttpWebRequest request)
        {
            request.Headers.Add("Authorization", "Bearer " + sToken);
        }*/
        private static string DoRequest(string sUri)
        {
            if (Params.Count > 0)
            {
                var colParams = Params.Select(x => Uri.EscapeDataString(x.Key) + "=" + Uri.EscapeDataString(x.Value));
                string sParams = string.Join("&", colParams);

                sUri += "?" + sParams;
            }

            //Log.Debug("Попытка запроса {sUri} Метод - {Method} ", sUri, sMethod);
            HttpWebRequest request = CreateRequest(sUri);
            request.Method = sMethod;

            if (ContentType != null) request.ContentType = ContentType;

            if (SendData != null)
            {
                request.ContentLength = SendData.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(SendData, 0, SendData.Length);
                }
            }

            return GetResponse(request);
        }
        /*private static string UploadFile(string sUri, string filepath)
        {
            Log.Debug("Попытка загрузки на сервер файла {file}", filepath);
            Log.Debug("Ссылка для загрузки {url}", sUri);
            const int BUFFER_SIZE = 64 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];
            int iReadedByte;

            SetDefault();
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;

            Uri uriDest = new Uri(sUri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriDest);

            request.Method = WebRequestMethods.Http.Put;
            request.AllowWriteStreamBuffering = false; 
            request.SendChunked = true;
            request.ContentType = "application/octet-stream";

            if (!File.Exists(filepath))
            {
                throw new Exception($"Файл отсутствует: {filepath}");
            }
            
            FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            request.ContentLength = fileStream.Length;

            try
            {
                Stream stream = request.GetRequestStream();
                do
                {
                    iReadedByte = fileStream.Read(buffer, 0, BUFFER_SIZE);
                    stream.Write(buffer, 0, iReadedByte);
                } while (iReadedByte > 0);

                stream.Close();
            }
            catch (WebException excpt)
            {
                if (excpt.Response is HttpWebResponse excResp)
                {
                    Log.Error(excpt, "Ответ веб-сервера - ошибка: код={Code}, Сообщение={Message}", (int)excResp.StatusCode, StreamToString(excResp.GetResponseStream()));
                }
                else
                {
                    Log.Error(excpt, "Неизвестная ошибка: код={Code}, Сообщение={Message}", (int)excpt.Status, excpt.Message);
                }
                throw;
            }
            fileStream.Close();

            return GetResponse(request);
        }*/
        private static HttpWebRequest CreateRequest(string sUri)
        {
            Uri uriDest = new Uri(sUri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriDest);
            request.Headers.Add("Authorization", "Bearer " + sToken);
            return request;
        }
        private static string GetResponse(HttpWebRequest request)
        {
            string sMessage;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                sMessage = StreamToString(response.GetResponseStream());
            }
            catch (WebException excpt)
            {
                if (excpt.Response is HttpWebResponse excResp)
                {
                    //Log.Error(excpt, "Ответ веб-сервера - ошибка: код={Code}, Сообщение={Message}", (int)excResp.StatusCode, StreamToString(excResp.GetResponseStream()));
                }
                else
                {
                    //Log.Error(excpt, "Неизвестная ошибка: код={Code}, Сообщение={Message}", (int)excpt.Status, excpt.Message);
                }
                throw;
            }

            //Log.Debug("Сервер вернул сообщение: {Message}", sMessage);
            return sMessage;
        }
        private static string StreamToString(Stream strm)
        {
            StreamReader reader = new StreamReader(strm);
            return reader.ReadToEnd();
        }
        private static void CredentialsToSendData (MessageCredentials Credentials)
        {
            string str = JsonConvert.SerializeObject(Credentials);
            //string sData = Uri.EscapeDataString(str);
            SendData = Encoding.UTF8.GetBytes(str);
        }
    }
}
