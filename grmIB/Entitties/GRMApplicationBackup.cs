using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using grmIB.Messages;
using Newtonsoft.Json;

namespace grmIB.Entitties
{
    class GRMApplicationBackup
    {
        public string ID { get; }
        public DateTime Date { get; }
        public float Size { get; }
        public string BackupType { get; }
        public string Link { get; }
        public string Status { get; }

        public GRMApplicationBackup() { }
        public GRMApplicationBackup(MessageBackup message)
        {
            ID = message.id;
            Date = message.date;
            Size = message.size;
            BackupType = message.backupType;
            Link = message.link;
            Status = message.status;
        }
        public static GRMApplicationBackup CreateBackupFromJSON(string str)
        {
            var jsonSS = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            MessageBackup message = JsonConvert.DeserializeObject<MessageBackup>(str, jsonSS);

            return new GRMApplicationBackup(message);
        }
        public static List<GRMApplicationBackup> CreateListBackupFromJSON(string str)
        {
            List<GRMApplicationBackup> ListBackup = new List<GRMApplicationBackup>();

            var jsonSS = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            List<MessageBackup> messages = JsonConvert.DeserializeObject<List<MessageBackup>>(str, jsonSS);

            if (messages != null)
            {
                foreach (var message in messages)
                {
                    ListBackup.Add(new GRMApplicationBackup(message));
                }
            }
            return ListBackup;
        }

    }
}
