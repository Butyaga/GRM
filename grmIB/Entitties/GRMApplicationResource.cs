using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using grmIB.Messages;
using Newtonsoft.Json;

namespace grmIB.Entitties
{
    class GRMApplicationResource
    {
        public string Type { get; }
        public long Value { get; }
        public GRMApplicationResource() { }
        public GRMApplicationResource(MessageAplicationResource message)
        {
            Type = message.type;
            Value = message.value;
        }
        public static List<GRMApplicationResource> CreateListFromJSON(string str)
        {
            List<GRMApplicationResource> resources = new List<GRMApplicationResource>();

            var jsonSS = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            List<MessageAplicationResource> messages = JsonConvert.DeserializeObject<List<MessageAplicationResource>>(str, jsonSS);

            if (messages != null)
            {
                foreach (var message in messages)
                {
                    resources.Add(new GRMApplicationResource(message));
                }
            }
            return resources;
        }
        public static long GetApplicationSpace(List<GRMApplicationResource> resources)
        {
            foreach (GRMApplicationResource resource in resources)
            {
                if (resource.Type == "disk_space")
                {
                    return resource.Value;
                }
            }
            return 0;
        }
    }
}
