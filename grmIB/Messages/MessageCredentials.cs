using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace grmIB.Messages
{
    class MessageCredentials
    {
#pragma warning disable 0649
        public string username;
        public string password;
#pragma warning restore 0649
        public MessageCredentials() { }
        public MessageCredentials(string user, string password)
        {
            if (user != "")
            {
                this.username = user;
                this.password = password;
            }
        }
    }
}
