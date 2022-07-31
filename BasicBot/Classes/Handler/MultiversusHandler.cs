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
using static BasicBot.MonarkTypes.Message;

namespace BasicBot.Handler
{
    public static class Multiversus
    {
        public static Dictionary<ulong, gamething> things = new Dictionary<ulong, gamething>();

        public static gamething GetThing(ulong thing) 
        {
            if (things.ContainsKey(thing))
                return things[thing];
            return null;
        }
        

        public class gamething
        {
            public SocketUser User1;
            public SocketUser User2;
            public ulong GuildId;
            public List<string> MapPool = new List<string>();
            public string BlockedMap = "";
            public string SelectedMap = "";
            public SocketInteraction Interaction { get; set; }


            public BasicBot.Settings.Guild gld
            {
                get
                {
                    return Guild.GetDiscordOrMake(GuildId);
                }
            }

            public Dictionary<string, List<string>> Maps
            {
                get
                {
                    return gld.Maps;
                }
            }

            public bool IsTurn(SocketUser user)
            {
                var turn = BlockedMap == "";

                if (turn && User1.Id == user.Id)
                {
                    return true;
                }
                if (!turn && User2.Id == user.Id)
                {
                    return true;
                }

                return false;
            }

            public void AddMapBanned(SocketUser user, string mapBan)
            {
                if (user.Id == User1.Id)
                {
                    MapPool.Remove(mapBan);
                    BlockedMap = mapBan;
                }
            }

            public gamething(SocketUser user1, SocketUser user2, SocketInteraction interaction, ulong guild)
            {
                User1 = user1;
                User2 = user2;
                Interaction = interaction;
                GuildId = guild;
            }

            public async Task<bool> AddMapBan(SocketUser user, string map)
            {
                if (IsTurn(user))
                {
                    if (user.Id == User1.Id)
                    {
                        AddMapBanned(user, map);
                        await BuildSelectPhase().UpdateMessage(Interaction);
                    }
                    else
                    {
                        SelectedMap = map;
                        await BuildDonePhase().UpdateMessage(Interaction);
                    }

                    
                    

                    return true;
                }
                else
                    return false;
            }

            public MonarkMessage BuildSelectPhase()
            {

                if (MapPool.Count() <= 1)
                {
                    return BuildDonePhase();
                }

                var msg = new MonarkMessage();
                msg.Components = new ComponentBuilder().WithSelectMenus("bans", BuildBanSelectOptions(), "Pick a map to select").Build();
                msg.AddEmbed(new EmbedBuilder().
                    WithTitle("Please select a map to play").
                    AddField($"{User1.Username}",
                    $"Map Banned:\n{BlockedMap}", true).

                    AddField($"{User2.Username} (Your Turn)",
                    $"Map Selected", true));



                return msg;
            }


            public MonarkMessage BuildBanPhase(List<string> mapPool = null)
            {
                if (mapPool != null)
                {
                    MapPool = mapPool.Select(x => x).ToList();
                }

                if (MapPool.Count() <= 1)
                {
                    return BuildDonePhase();
                }

                var msg = new MonarkMessage();
                msg.Components = new ComponentBuilder().WithSelectMenus("bans", BuildBanSelectOptions(), "Pick a map to ban").Build();
                msg.AddEmbed(new EmbedBuilder().
                    WithTitle("Please select a map to ban").
                    AddField($"{User1.Username} (Your Turn)",
                    $"Map Banned:\n{BlockedMap}", true).

                    AddField($"{User2.Username}",
                    $"Map selected:", true));



                return msg;
            }

            public MonarkMessage BuildDonePhase()
            {
                var msg = new MonarkMessage();
                msg.Components = new ComponentBuilder().WithSelectMenus("bans", BuildBanSelectOptions(), "Pick a map to ban", disabled:true).Build();
                msg.AddEmbed(new EmbedBuilder().
                    WithDescription($"Done").
                    AddField($"{User1.Username}",
                    $"Maps Banned:\n{BlockedMap}", true).

                    AddField($"{User2.Username}",
                    $"Maps Selected:\n{SelectedMap}", true));



                return msg;
            }

            public List<SelectMenuOptionBuilder> BuildBanSelectOptions()
            {
                var options = new List<SelectMenuOptionBuilder>();

                foreach (var a in MapPool)
                {
                    options.Add(new SelectMenuOptionBuilder(a, a));
                }

                return options;
            }

            public MonarkMessage BuildFirst()
            {
                try
                {
                    if (Maps.Count == 0)
                    {
                        return "There are no map pools created";
                    }

                    if (Maps.Count == 1)
                    {
                        return BuildBanPhase(Maps.First().Value);
                    }

                    MonarkMessage message = new MonarkMessage();
                    message.AddEmbed(new EmbedBuilder().WithTitle("Please select a map pool"));
                    message.Components = new ComponentBuilder().WithSelectMenus("maps", BuildSelectOptions()).Build();
                    return message;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

            public List<SelectMenuOptionBuilder> BuildSelectOptions()
            {
                var options = new List<SelectMenuOptionBuilder>();

                foreach (var a in Maps.Keys)
                {
                    options.Add(new SelectMenuOptionBuilder(a, a));
                }

                return options;
            }
        }
    }
}
