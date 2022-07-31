using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicBot.Settings
{
    public class Guild
    {
        [JsonProperty]
        public ulong guildId;

        [JsonProperty]
        public StaffRoles StaffRole = new();

        [JsonIgnore]
        public int? ModerationCount = null;

        [JsonProperty]
        public Dictionary<string, List<string>> Maps = new Dictionary<string, List<string>>();

        public class StaffRoles
        {
            [JsonProperty]
            public List<ulong> Admin = new List<ulong>();
            [JsonProperty]
            public List<ulong> Management = new List<ulong>();
            [JsonProperty]
            public List<ulong> Support = new List<ulong>();
        }
    }

}
