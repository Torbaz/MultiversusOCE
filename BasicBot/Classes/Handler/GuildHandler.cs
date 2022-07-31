using Discord.WebSocket;
using Discord;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BasicBot.Handler.String;
using static BasicBot.Services.MessageHandlerService;
using BasicBot.Settings;

namespace BasicBot.Handler
{
    public static class Guild
    {
        private static Dictionary<ulong, BasicBot.Settings.Guild> Guilds = null;

        private static readonly string GuildsFile = CombineCurrentDirectory("Guilds.json");

        #region user settings
        private static Dictionary<ulong, BasicBot.Settings.Guild> LoadGuilds()
        {
            if (File.Exists(GuildsFile) && Guilds == null)
            {
                var jsonText = File.ReadAllText(GuildsFile);
                if (!string.IsNullOrWhiteSpace(jsonText))
                {
                    try
                    {
                        Guilds = JsonConvert.DeserializeObject<Dictionary<ulong, BasicBot.Settings.Guild>>(jsonText);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return null;
                    }
                    SaveGuilds();
                }
            }
            if (Guilds == null)//backup check if file is empty
            {
                Guilds = new Dictionary<ulong, BasicBot.Settings.Guild>();
                SaveGuilds();
            }
            return Guilds;
        }

        public static bool SaveGuilds()
        {
            if (Guilds != null)
            {
                var jsonText = JsonConvert.SerializeObject(Guilds, Formatting.Indented);
                if (!string.IsNullOrWhiteSpace(jsonText))
                {
                    File.WriteAllText(GuildsFile, jsonText);
                    return true;
                }
            }
            return false;
        }

        public static BasicBot.Settings.Guild GetDiscordOrMake(SocketGuild gld) =>
            GetDiscordOrMake(gld.Id);
        public static BasicBot.Settings.Guild GetDiscordOrMake(IGuild gld) =>
            GetDiscordOrMake(gld.Id);
        public static BasicBot.Settings.Guild GetDiscordOrMake(ulong guildID)
        {
            if (Guilds == null)
                LoadGuilds();
            if (!Guilds.ContainsKey(guildID))
            {
                var disc = MakeDiscord(guildID);
                disc.guildId = guildID;
                return disc;
            }
            return Guilds[guildID];
        }

        public static BasicBot.Settings.Guild GetDiscord(SocketGuild gld) =>
            GetDiscord(gld.Id);
        public static BasicBot.Settings.Guild GetDiscord(IGuild gld) =>
            GetDiscord(gld.Id);
        public static BasicBot.Settings.Guild GetDiscord(ulong guildID)
        {
            if (Guilds == null)
                LoadGuilds();
            if (!Guilds.ContainsKey(guildID))
            {
                return null;
            }
            return Guilds[guildID];
        }

        public static BasicBot.Settings.Guild MakeDiscord(ulong guildId)
        {
            if (Guilds == null)
                LoadGuilds();

            if (!Guilds.ContainsKey(guildId))
            {
                return Guilds[guildId] = new BasicBot.Settings.Guild();
            }
            SaveGuilds();

            return Guilds[guildId];
        }


        #endregion user settings

        public static IEnumerable<BasicBot.Settings.Guild> GetAllGuilds()
        {
            if (Guilds == null)
            {
                LoadGuilds();
            }

            return Guilds.Select(x => x.Value);
        }      

        public static BasicBot.Settings.Guild.StaffRoles GetStaffRoles(ulong guildId)
        {
            var discord = GetDiscord(guildId);

            if (discord == null)
            {
                return new BasicBot.Settings.Guild.StaffRoles();
            }

            return discord.StaffRole;
        }
    }
}
