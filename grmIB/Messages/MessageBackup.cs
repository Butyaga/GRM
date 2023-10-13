using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace grmIB.Messages
{
    class MessageBackup
    {
#pragma warning disable 0649
        public string id;
        public DateTime date;
        public float size;
        public string backupType;
        public string link;
        public string status;
#pragma warning restore 0649
    }
}
