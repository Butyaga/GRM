using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Serilog;
using Newtonsoft.Json;
using grmIB.Messages;
using grmIB.Subsys.Http;

namespace grmIB.Entitties
{
    class GRMApplicationFileInfo
    {
        public string Name { get; }
        public string Url { get; }
        public long Size { get; }

        public GRMApplicationFileInfo() { }
        public GRMApplicationFileInfo(MessageExchangeFileInfo message)

        {
            Name = message.name;
            Url = message.url;
            Size = message.size;
        }
        public static GRMApplicationFileInfo CreateFileInfoFromJSON(string str)
        {
            var jsonSS = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            MessageExchangeFileInfo message = JsonConvert.DeserializeObject<MessageExchangeFileInfo>(str, jsonSS);

            return new GRMApplicationFileInfo(message);
        }
        public static List<GRMApplicationFileInfo> CreateListFileInfoFromJSON(string str)
        {
            List<GRMApplicationFileInfo> ListFileInfo = new List<GRMApplicationFileInfo>();

            var jsonSS = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            List<MessageExchangeFileInfo> messages = JsonConvert.DeserializeObject<List<MessageExchangeFileInfo>>(str, jsonSS);

            if (messages != null)
            {
                foreach (var message in messages)
                {
                    ListFileInfo.Add(new GRMApplicationFileInfo(message));
                }
            }
            return ListFileInfo;
        }
        public static GRMApplicationFileInfo FindByName(List<GRMApplicationFileInfo> listFileInfo, string name)
        {
            foreach (GRMApplicationFileInfo FileInfo in listFileInfo)
            {
                if (FileInfo.Name == name)
                {
                    return FileInfo;
                }
            }
            return null;
        }
    }
}
