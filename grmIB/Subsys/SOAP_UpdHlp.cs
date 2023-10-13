using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace grmIB.Subsys
{
    public class SOAP_UpdHlp
    {
        private string urlIB;
        private string userIB;
        private string passwordIB;

        static private string DefaultUser = "WebRequest";
        static private string DefaultPassword = "VeryStrongPassword76";

        static private string urlPostfix = "ws/UpdHlp.1cws";
        static private string stringResponse = "Response";

        static private string ns_ns = "http://schemas.xmlsoap.org/soap/envelope/";
        static private string ns_tns = "http://www.allegro.ru/UpdHlp";
        //static private string ns_xsd = "http://www.w3.org/2001/XMLSchema";
        //static private string ns_xsi = "http://www.w3.org/2001/XMLSchema-instance";

        static private string _statusActive = "СостояниеФоновогоЗадания.Активно";
        static private string _statusCompleted = "СостояниеФоновогоЗадания.Завершено";
        static private string _statusFailed = "СостояниеФоновогоЗадания.ЗавершеноАварийно";
        static private string _statusCanceled = "СостояниеФоновогоЗадания.Отменено";
        static private string _statusNotStarted = "НеЗапускалось";

        public SOAP_UpdHlp() { }
        public SOAP_UpdHlp(string url)
        {
            SetURLService(url);
        }

        public void SetURLService(string url)
        {
            urlIB = url;
            if (url.StartsWith("http",StringComparison.OrdinalIgnoreCase))
            {
                urlIB = url;
            }
            else
            {
                urlIB = new UriBuilder(Uri.UriSchemeHttps, url).ToString();
            }

            if (!urlIB.EndsWith(urlPostfix, true, null))
            {
                if (!urlIB.EndsWith("/"))
                {
                    urlIB += "/";
                }
                urlIB += urlPostfix;
            }
        }

        public void SetAuth(string usr, string pass)
        {
            userIB = usr;
            passwordIB = pass;
        }
        
        public bool Request_Ping()
        {
            string MethodName = "Ping";

            HttpWebRequest request = CreateRequest();
            XDocument soap = CreateSOAP(MethodName);
            using (Stream stream = request.GetRequestStream())
            {
                soap.Save(stream);
            }

            string responseResult = GetResponseMessage(request);

            string msgSOAP = GetValueFromSOAP(responseResult, MethodName + stringResponse);
            if (msgSOAP == "Ok")
            {
                return true;
            }
            return false;
        }

        public string Request_GetConfigurationVersion()
        {
            string MethodName = "GetConfigurationVersion";

            HttpWebRequest request = CreateRequest();
            XDocument soap = CreateSOAP(MethodName);
            using (Stream stream = request.GetRequestStream())
            {
                soap.Save(stream);
            }

            string responseResult = GetResponseMessage(request);

            string msgSOAP = GetValueFromSOAP(responseResult, MethodName + stringResponse);
            return msgSOAP;
        }

        public string Request_GetConfigurationName()
        {
            string MethodName = "GetConfigurationName";

            HttpWebRequest request = CreateRequest();
            XDocument soap = CreateSOAP(MethodName);
            using (Stream stream = request.GetRequestStream())
            {
                soap.Save(stream);
            }

            string responseResult = GetResponseMessage(request);

            string msgSOAP = GetValueFromSOAP(responseResult, MethodName + stringResponse);
            return msgSOAP;
        }

        public string Request_GetUpdateProcessingResult()
        {
            string MethodName = "GetUpdateProcessingResult";

            HttpWebRequest request = CreateRequest();
            XDocument soap = CreateSOAP(MethodName);
            using (Stream stream = request.GetRequestStream())
            {
                soap.Save(stream);
            }

            string responseResult = GetResponseMessage(request);

            string msgSOAP = GetValueFromSOAP(responseResult, MethodName + stringResponse);
            return msgSOAP;
        }

        public string Request_StartUpdateProcessing(out string RezultAddress)
        {
            string MethodName = "StartUpdateProcessing";

            HttpWebRequest request = CreateRequest();
            XDocument soap = CreateSOAP(MethodName);//, new Dictionary<string, string>() { { "RezultAddress", "" } });
            using (Stream stream = request.GetRequestStream())
            {
                soap.Save(stream);
            }

            string responseResult = GetResponseMessage(request);

            RezultAddress = GetValueFromSOAP(responseResult, "RezultAddress");
            string TaskID = GetValueFromSOAP(responseResult, "return");
            return TaskID;
        }

        public string Request_GetStatusUpdateProcessing(string taskID, string tempStorage, out string RezultValue)
        {
            string MethodName = "GetStatusUpdateProcessing";
            Dictionary<string, string> prmtrs = new Dictionary<string, string>();
            prmtrs.Add("TaskID", taskID);
            prmtrs.Add("RezultAddress", tempStorage);

            HttpWebRequest request = CreateRequest();
            XDocument soap = CreateSOAP(MethodName, prmtrs);
            using (Stream stream = request.GetRequestStream())
            {
                soap.Save(stream);
            }

            string responseResult = GetResponseMessage(request);

            RezultValue = GetValueFromSOAP(responseResult, "RezultValue");
            string TaskStatus = GetValueFromSOAP(responseResult, "return");
            return TaskStatus;
        }

        public bool Request_ConfirmLegality()
        {
            string MethodName = "ConfirmLegality";

            HttpWebRequest request = CreateRequest();
            XDocument soap = CreateSOAP(MethodName);
            using (Stream stream = request.GetRequestStream())
            {
                soap.Save(stream);
            }

            string responseResult = GetResponseMessage(request);

            string msgSOAP = GetValueFromSOAP(responseResult, MethodName + stringResponse);
            if (msgSOAP == "Ok")
            {
                return true;
            }

            return false;
        }

        public string Complex_UpdateProcessing()
        {
            int timeOut = 3 * 60 * 1000; // милисекунд
            int requestPause =  3 * 1000; // милисекунд

            string taskID = Request_StartUpdateProcessing(out string TempStorage);
            if (taskID == "Уже запущено")
            {
                return "Уже запущено задание обработки обновления.";
            }
            if (string.IsNullOrEmpty(taskID))
            {
                return "Произошла неизвестная ошибка при запуске фонового задания обработки обновления.";
            }
            Console.WriteLine($"Старт обработок обновления ИД: {taskID}");
            Console.WriteLine($"Адресс временного хранилища: {TempStorage}");

            int tm = 0;
            string answer = string.Empty;
            string rezultValue = string.Empty;
            while (tm < timeOut)
            {
                System.Threading.Thread.Sleep(requestPause);
                tm += requestPause;
                answer = Request_GetStatusUpdateProcessing(taskID, TempStorage, out rezultValue);
                Console.WriteLine($"Статус обработок: {answer} ;результат:{rezultValue}");
                if (answer != _statusActive)
                {
                    break;
                }
            }

            if (answer != _statusCompleted)
            {
                return "Произошла ошибка при запуске фонового задания обработки обновления.\nВозможно установлен \"Безопасный режим\" для расширения \"UpdHlp\"";
            }

            if (tm > timeOut)
            {
                return $"Превышение времени ожидания выполнения обработок обновления: {timeOut/1000} сек.";
            }

            string rezult = $"Результат выполнения фонового задания обновления: {rezultValue}";
            if (rezultValue != "НеТребуется")
            {
                rezult += "\n" + Request_GetUpdateProcessingResult();
            }
            
            return rezult;
        }

        private void GetAuth(out string usr, out string pass)
        {
            if (string.IsNullOrEmpty(userIB))
            {
                usr = DefaultUser;
            }
            else
            {
                usr = userIB;
            }

            if (string.IsNullOrEmpty(passwordIB))
            {
                pass = DefaultPassword;
            }
            else
            {
                pass = passwordIB;
            }
        }

        private HttpWebRequest CreateRequest()
        {
            Uri uriDest = new Uri(urlIB);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriDest);
            request.Accept = "text/xml";
            request.ContentType = "text/xml;charset=utf-8";
            request.Headers.Add("SOAPAction", urlIB);
            request.Method = WebRequestMethods.Http.Post;

            GetAuth(out string User, out string Password);
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{User}:{Password}"));
            request.Headers.Add("Authorization", "Basic " + encoded);

            return request;
        }

        private string GetResponseMessage(HttpWebRequest request)
        {
            string result = "";
            try
            {
                WebResponse response = request.GetResponse();
                StreamReader rd = new StreamReader(response.GetResponseStream());
                result = rd.ReadToEnd();
            }
            catch (WebException webEx)
            {
                Console.WriteLine("В ИБ не установлено расширение \"UpdHlp\" или не совместимая версия расширения");
                Console.WriteLine($"ErrorMessage: {webEx.Message}");
                //throw;
            }
            
            return result;
        }

        private XDocument CreateSOAP(string action, Dictionary<string,string> prmtrs = null)
        {
            XNamespace ns = ns_ns;
            XNamespace tns = ns_tns;

            XElement xAction = new XElement(tns + action);

            if (prmtrs != null)
            {
                foreach (var prmtr in prmtrs)
                {
                    xAction.Add(new XElement(tns + prmtr.Key, prmtr.Value));
                }
            }

            XDocument soapRequest = new XDocument(
                new XDeclaration("1.0", "UTF-8", "no"),
                new XElement(ns + "Envelope",
                        new XAttribute(XNamespace.Xmlns + "soap", ns),
                        new XAttribute(XNamespace.Xmlns + "tns", tns),
                    new XElement(ns + "Body", xAction)
                )
            );
            return soapRequest;
        }

        private string GetValueFromSOAP(string soap, string prm)
        {
            XNamespace tns = ns_tns;

            XDocument xdoc = XDocument.Parse(soap);
            string rezult = xdoc.Descendants(tns + prm).FirstOrDefault()?.Value;

            return rezult;
        }
    }
}
