using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using Serilog;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace grmIB.Subsys.Http
{
    class HttpFileDownload
    {
        //private readonly static ILogger Logger = Log.Logger.ForContext<HttpFileDownload>();
        private readonly static int BLOCK_SIZE = 1024;
        private readonly static int BUFFER_SIZE = 4 * 1024 * BLOCK_SIZE;
        // MinInetSpeed - Для расчета таймаута отправки файла (Мбит/с)
        private readonly static float MinInetSpeed = 3; // Мбит/с
        private readonly static float _MinInetSpeed = MinInetSpeed / 8; // Мб/с
        private readonly static int PeriodForMidleSpeed = 30;
        private readonly string sFilePath;
        private readonly string sUri;
        private readonly float lFileSize;
        private readonly Queue<long> PeroidTransfered = new Queue<long>(PeriodForMidleSpeed);
        private uint NetTransferSpeed;
        private long lByteTranfered;
        private bool isComplited = false;
        private bool LowNetSpeed = false;
        private bool Abort = false;
        private readonly static AutoResetEvent RequestComplited = new AutoResetEvent(false);
        private BlockingCollection<byte[]> FileWriteQueue = new BlockingCollection<byte[]>();
        public int ResposeCode { get; private set; }
        public string ResposeMessage { get; private set; }

        public HttpFileDownload(string path, string uri, float size)
        {
            //string path = "\"" + sPath + "\"";
            //Logger.Information($"Инициация получения файла {path} по ссылке {uri}");
            if (File.Exists(path))
            {
                //Logger.Error("Удаление уже существующего файла {path}", path);
                File.Delete(path);
            }
            //Logger.Error("Создание пустого файла {path}", path);
            File.Create(path).Close();
            sFilePath = path;
            sUri = uri;
            lFileSize = size;
        }
        public void Init()
        {
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;

            //Logger.Information("Url скачивания файла: {url}", sUri);
            Uri uriDest = new Uri(sUri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriDest);

            request.Method = WebRequestMethods.Http.Get;

            int iTimeOut = (int)(lFileSize / _MinInetSpeed);

            /*
            Logger.Information("Инициализация скачивания файла: размер - {size}; путь - {path}", lFileSize, sFilePath);
            Logger.Information("Запрос отправки файла: url - {size}; метод - {Method}; буфферинг - {Buffering}; " +
                "сегментирование - {SendChunked}; тип контента - {ContentType}; таймаут - {TimeOut}", sUri,
                request.Method, request.AllowWriteStreamBuffering, request.SendChunked, request.ContentType, iTimeOut);
            Logger.Information("Критическая минимальная скорость передачи - {speed} Мбит/с", MinInetSpeed);
            */

            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
            //Logger.Debug("Запрос запущен, ожидание завершения");

            RequestComplited.WaitOne(++iTimeOut * 1000);
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
        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            int iRead;
            //, offset, offsetNext;
            //byte[] bDubleBuffer = new byte[BUFFER_SIZE * 2];

            Timer NetSpeedTimer = null;
            Task BufferWriter = null;

            HttpWebResponse response = null;
            Stream streamResponse = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
                response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                streamResponse = response.GetResponseStream();
                //Logger.Debug("Стрим респонза получен");

                NetSpeedTimer = new Timer(new TimerCallback(CallBackTimer), null, 0, 1000);
                BufferWriter = new TaskFactory().StartNew(new Action(WriteBufferToFile), TaskCreationOptions.LongRunning);

                do
                {
                    byte[] NewBuffer = new byte[BUFFER_SIZE];
                    iRead = ReadResponseStream(NewBuffer, streamResponse);

                    if (Abort)
                    {
                        //Logger.Debug("Возведен флаг отмены. Останов задания.");
                        return;
                    }

                    if (iRead < BUFFER_SIZE) Array.Resize(ref NewBuffer, iRead);
                    FileWriteQueue.Add(NewBuffer);
                } while (iRead > 0);
                FileWriteQueue.CompleteAdding();
                ResposeCode = (int)response.StatusCode;
                BufferWriter.Wait();
            }
            finally
            {
                if (BufferWriter != null) BufferWriter.Dispose();
                FileWriteQueue.Dispose();
                if (NetSpeedTimer != null) NetSpeedTimer.Dispose();
                if (streamResponse != null) streamResponse.Close();
                if (response != null) response.Close();
            }

            isComplited = true;
            //Logger.Debug("Объекты освобождены. Включаю семафор.", ResposeMessage);
            RequestComplited.Set();
        }
        private int ReadResponseStream(byte[] Buffer, Stream stream)
        {
            int iReaded;
            int iSumReaded = 0;
            int iFreeSize = Buffer.Length;
            int iReadBlockSize = BLOCK_SIZE;
            
            do
            {
                if (iFreeSize < iReadBlockSize) iReadBlockSize = iFreeSize;
                iReaded = stream.Read (Buffer, iSumReaded, iReadBlockSize);
                iSumReaded += iReaded;
                iFreeSize -= iReaded;
                lByteTranfered += iReaded;
                //if (iReaded != BLOCK_SIZE) Logger.Debug("Подозрительно: iReaded = {iReaded}, iSumReaded = {iSumReaded}", iReaded, iSumReaded);
                if (Abort) break;
            } while (iReaded > 0 && iFreeSize > 0);
            return iSumReaded;
        }
        private void WriteBufferToFile()
        {
            using (FileStream fileStream = new FileStream(sFilePath, FileMode.Append, FileAccess.Write))
            {
                foreach (var buffer in FileWriteQueue.GetConsumingEnumerable())
                {
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
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
