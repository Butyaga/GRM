using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using Serilog;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace grmIB.Subsys.Http
{
    class HttpFileUpload
    {
        //private readonly static ILogger Logger = Log.Logger.ForContext<HttpFileUpload>();
        private readonly static string sContentType = "application/octet-stream";
        private readonly static int BLOCK_SIZE = 1024;
        private readonly static int BUFFER_SIZE = 4 * 1024 * BLOCK_SIZE;
        // MinInetSpeed - Для расчета таймаута отправки файла (Мбит/с)
        private readonly static int MinInetSpeed = 1;
        private readonly static int _MinInetSpeed = MinInetSpeed * 1024 * 1024 / 8;
        private readonly static int PeriodForMidleSpeed = 30;
        private readonly string sFilePath;
        private readonly string sUri;
        private readonly Queue<long> PeroidTransfered = new Queue<long>(PeriodForMidleSpeed);
        private uint NetTransferSpeed;
        private uint iReadConter;
        private long lByteTranfered;
        private bool isComplited = false;
        private bool LowNetSpeed = false;
        private bool Abort = false;
        private readonly static ManualResetEvent RequestComplited = new ManualResetEvent(false);
        public int ResposeCode { get; private set; }
        public string ResposeMessage { get; private set; }
        public HttpFileUpload(string path, string uri)
        {
            //Logger.Information($"Инициация отправки файла {path} по ссылке {uri}");
            if (File.Exists(path))
            {
                sFilePath = path;
            }
            else
            {
                //Logger.Error("Ошибка отправки файла: файл не существует {path}", path);
                throw new Exception($"Попытка отправить несуществующий файл: {path}");
            }
            sUri = uri;
        }
        public void Init()
        {
            long iFileSize;
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;

            //Logger.Information("Url отправки файла: {url}", sUri);
            Uri uriDest = new Uri(sUri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriDest);

            request.Method = WebRequestMethods.Http.Put;
            request.AllowWriteStreamBuffering = false;
            request.SendChunked = true;
            request.ContentType = sContentType;
            using (FileStream fileStream = new FileStream(sFilePath, FileMode.Open, FileAccess.Read))
            {
                iFileSize = fileStream.Length;
                request.ContentLength = iFileSize;
                //Logger.Information("Размер отправляемого файла: {size}", iFileSize);
            }

            int iTimeOut = (int)(iFileSize / _MinInetSpeed * 1000);

            /*
            Logger.Information("Инициализация отправки файла: размер - {size}; путь - {path}", iFileSize, sFilePath);
            Logger.Information("Запрос отправки файла: url - {size}; метод - {Method}; буфферинг - {Buffering}; " +
                "сегментирование - {SendChunked}; тип контента - {ContentType}; таймаут - {TimeOut}", sUri,
                request.Method, request.AllowWriteStreamBuffering, request.SendChunked, request.ContentType, iTimeOut);
            Logger.Information("Критическая минимальная скорость передачи - {speed} Мбит/с", MinInetSpeed);
            */

            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
            //Logger.Debug("Запрос запущен, ожидание завершения");

            RequestComplited.WaitOne(iTimeOut);
            if (isComplited)
            {
                //Logger.Information("Вызов завершен: код ответа - {endResponse}, сообщение - {Message}", ResposeCode, ResposeMessage);
            }
            else
            {
                if (LowNetSpeed)
                {
                    //Logger.Error("Задание отправки отменено по причине низкой скорости передачи");
                    throw new Exception("Низкая скорости передачи");
                }
                else
                {
                    //Logger.Warning("Превышено время максимального ожидания");
                    throw new Exception("Превышено время максимального ожидания");
                }
            }
        }
        public void AbortOperation()
        {
            Abort = true;
            RequestComplited.Set();
        }
        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            int iRead, offset, offsetNext;
            byte[] bDubleBuffer = new byte[BUFFER_SIZE * 2];
            Timer NetSpeedTimer = null;
            Stream webstream = null;
            HttpWebRequest request = null;
            Task<int> tBufferWriter = null;

            try
            {
                request = (HttpWebRequest)asynchronousResult.AsyncState;
                webstream = request.EndGetRequestStream(asynchronousResult);
                //Logger.Debug("Стрим рекверста получен");

                offset = 0;
                offsetNext = BUFFER_SIZE;
                iRead = ReadFileToBuffer(bDubleBuffer, offset);
                NetSpeedTimer = new Timer(new TimerCallback(CallBackTimer), null, 0, 1000);
                while (iRead > 0)
                {
                    tBufferWriter = new TaskFactory().StartNew(() => { return ReadFileToBuffer(bDubleBuffer, offsetNext); });
                    WriteWebStream(bDubleBuffer, offset, iRead, webstream);
                    ExchangeIntValue(ref offset, ref offsetNext);
                    if (Abort)
                    {
                        //Logger.Debug("Возведен флаг отмены. Останов задания.", iRead);
                        return;
                    }
                    tBufferWriter.Wait();
                    iRead = tBufferWriter.Result;
                }
            }
            finally
            {
                if (NetSpeedTimer != null) NetSpeedTimer.Dispose();
                if (webstream != null) webstream.Dispose();
                if (tBufferWriter != null) tBufferWriter.Dispose();
            }

            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }
        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            HttpWebResponse response = null;
            StreamReader streamRead = null;
            Stream streamResponse = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
                response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                ResposeCode = (int)response.StatusCode;

                streamResponse = response.GetResponseStream();
                streamRead = new StreamReader(streamResponse);
                ResposeMessage = streamRead.ReadToEnd();
            }
            finally
            {
                if (streamRead != null) streamRead.Close();
                if (streamResponse != null) streamResponse.Close();
                if (response != null) response.Close();
            }

            isComplited = true;
            //Logger.Debug("Объекты освобождены. Включаю семафор.", ResposeMessage);
            RequestComplited.Set();
        }
        private void WriteWebStream(byte[] DubleBuffer, int offset, int count, Stream webStream)
        {
            int iOperOffset = offset;
            int iOperCount = BLOCK_SIZE;
            int iOperRemains = count;
            do
            {
                if (iOperCount > iOperRemains)
                {
                    iOperCount = iOperRemains;
                }
                webStream.Write(DubleBuffer, iOperOffset, iOperCount);
                lByteTranfered += iOperCount;
                iOperOffset += iOperCount;
                iOperRemains -= iOperCount;
            } while (iOperRemains > 0);
        }
        private int ReadFileToBuffer(byte[] DubleBuffer, int offset)
        {
            int iReaded;
            using (FileStream fileStream = new FileStream(sFilePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(iReadConter * BUFFER_SIZE, SeekOrigin.Begin);
                iReaded = fileStream.Read(DubleBuffer, offset, BUFFER_SIZE);
            }
            iReadConter++;
            return iReaded;
        }
        private void ExchangeIntValue(ref int item1, ref int item2)
        {
            int temp = item1;
            item1 = item2;
            item2 = temp;
        }
        private void CallBackTimer(object qwe)
        {
            PeroidTransfered.Enqueue(lByteTranfered);
            NetTransferSpeed = (uint)((PeroidTransfered.Peek() - PeroidTransfered.Last()) / PeroidTransfered.Count);
            if (PeroidTransfered.Count == PeriodForMidleSpeed && NetTransferSpeed < _MinInetSpeed)
            {
                LowNetSpeed = true;
                AbortOperation();
            }
        }
    }
}
