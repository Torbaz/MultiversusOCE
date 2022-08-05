using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasicBot.GraphQL;
using Discord;
using Discord.WebSocket;
using static BasicBot.MonarkTypes.Message;

namespace BasicBot.Handler
{
    public static class Multiversus
    {
        public static Dictionary<ulong, gamething> things = new();

        public static gamething GetThing(ulong thing)
        {
            if (things.ContainsKey(thing))
                return things[thing];
            return null;
        }

        public class gamething
        {
            public string BlockedMap = "";
            public ulong GuildId;
            public string SelectedMap = "";
            public SocketUser User1;
            public SocketUser User2;
            public List<string> MapPool;

            public gamething(SocketUser user1, SocketUser user2, IUserMessage message, ulong guild)
            {
                User1 = user1;
                User2 = user2;
                Message = message;
                GuildId = guild;
            }

            public IUserMessage Message { get; set; }


            public BasicBot.Settings.Guild gld => Guild.GetDiscordOrMake(GuildId);

            public Dictionary<string, List<string>> Maps => gld.Maps;

            public bool IsTurn(SocketUser user)
            {
                var turn = BlockedMap == "";

                return (turn && User1.Id == user.Id) || (!turn && User2.Id == user.Id);
            }

            public void AddMapBanned(SocketUser user, string mapBan)
            {
                if (user.Id == User1.Id)
                {
                    MapPool.Remove(mapBan);
                    BlockedMap = mapBan;
                }
            }

            public async Task<bool> SelectMap(SocketUser user, string map)
            {
                if (IsTurn(user))
                {
                    if (BlockedMap == "")
                    {
                        AddMapBanned(user, map);
                        await BuildSelectPhase().UpdateMessage(Message);
                    }
                    else
                    {
                        SelectedMap = map;
                        await BuildDonePhase().UpdateMessage(Message);
                    }

                    return true;
                }

                return false;
            }

            public MonarkMessage BuildSelectPhase()
            {
                var msg = new MonarkMessage();
                msg.Components = new ComponentBuilder()
                    .WithSelectMenus("bans", BuildBanSelectOptions(), "Pick a map to select")
                    .WithButton("Restart Map Selection", "restart", ButtonStyle.Danger).Build();
                msg.AddEmbed(new EmbedBuilder().WithTitle("Please select a map to play").AddField($"{User1.Username}",
                    $"Map Banned:\n{BlockedMap}", true).AddField($"{User2.Username} (Your Turn)",
                    "Map Selected", true));


                return msg;
            }

            public MonarkMessage BuildPoolPhase()
            {
                if (Maps.Count == 0) return "There are no map pools created";

                if (Maps.Count == 1) return BuildBanPhase(Maps.First().Value);

                var message = new MonarkMessage();
                message.AddEmbed(new EmbedBuilder().WithTitle("Please select a map pool"));
                message.Components = new ComponentBuilder().WithSelectMenus("maps", BuildSelectOptions())
                    .WithButton("Restart Map Selection", "restart", ButtonStyle.Danger).Build();

                return message;
            }

            public MonarkMessage BuildBanPhase(List<string> mapPool)
            {
                MapPool = mapPool;

                var msg = new MonarkMessage();
                if (MapPool != null && MapPool.Count < 2)
                {
                    msg.AddEmbed(new EmbedBuilder().WithTitle("Error")
                        .AddField("Error", "An error has occurred. The map pool is empty."));
                    return msg;
                }

                msg.Components = new ComponentBuilder()
                    .WithSelectMenus("bans", BuildBanSelectOptions(), "Pick a map to ban")
                    .WithButton("Restart Map Selection", "restart", ButtonStyle.Danger).Build();
                msg.AddEmbed(new EmbedBuilder().WithTitle("Please select a map to ban").AddField(
                    $"{User1.Username} (Your Turn)",
                    $"Map Banned:\n{BlockedMap}", true).AddField($"{User2.Username}",
                    "Map selected:", true));
                return msg;
            }

            public MonarkMessage BuildDonePhase()
            {
                var msg = new MonarkMessage();
                msg.Components = new ComponentBuilder()
                    .WithButton("New Game", "restart")
                    .WithButton("End set", "end", ButtonStyle.Danger).Build();
                msg.AddEmbed(new EmbedBuilder().WithDescription("Done").AddField($"{User1.Username}",
                    $"Maps Banned:\n{BlockedMap}", true).AddField($"{User2.Username}",
                    $"Maps Selected:\n{SelectedMap}", true));


                return msg;
            }

            public List<SelectMenuOptionBuilder> BuildBanSelectOptions()
            {
                var options = new List<SelectMenuOptionBuilder>();

                foreach (var a in MapPool)
                    options.Add(new SelectMenuOptionBuilder(a, a));

                return options;
            }

            public MonarkMessage BuildFirst()
            {
                try
                {
                    var message = new MonarkMessage();
                    message.AddEmbed(new EmbedBuilder().WithTitle("Please select the state of the game."));
                    message.Components =
                        new ComponentBuilder()
                            .WithButton("First game of the set", "coinflip")
                            .WithButton($"{User1.Username} won last game", "wonlast1", ButtonStyle.Success)
                            .WithButton($"{User2.Username} won last game", "wonlast2", ButtonStyle.Success)
                            .WithButton("End set", "end", ButtonStyle.Danger).Build();
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

                foreach (var a in Maps.Keys) options.Add(new SelectMenuOptionBuilder(a, a));

                return options;
            }
        }

        public static Dictionary<StartID, Dictionary<StartID, Set>> sets = new();
        public static List<StartID> runningEvents = new();

        public static async void UpdateSets()
        {
            foreach (var e in runningEvents)
            {
                var perPage = 20;

                var req = await StartGGHandler.Client.GetSetsAndLinkedAccounts.ExecuteAsync(e, 1, perPage);
                // Check still admin.
                if (req.Data.Event.Tournament.Admins.Where(x => x.Id == req.Data.CurrentUser.Id).Count() != 1)
                {
                    // No longer admin.
                    Console.Error.WriteLine("No longer admin of tournament.");
                    // Remove existing sets from memory.
                    if (sets.ContainsKey(e))
                    {
                        sets[e].Clear();
                    }

                    break;
                }

                // Get Sets
                List<IGetSetsAndLinkedAccounts_Event_Sets_Nodes> eventSets = new();
                eventSets.AddRange(req.Data.Event.Sets.Nodes);
                // Get all the sets.
                if (eventSets.Count == perPage)
                {
                    var i = 2;
                    while (true)
                    {
                        var setsReq = await StartGGHandler.Client.GetSetsAndLinkedAccounts.ExecuteAsync(e, i, perPage);
                        eventSets.AddRange(setsReq.Data.Event.Sets.Nodes);
                        if (setsReq.Data.Event.Sets.Nodes.Count != perPage)
                            break;
                        i++;
                    }
                }

                Console.WriteLine(eventSets.Count);

                // Remove sets from the existing list if they aren't going still (Remove discord too)
                // Add new sets to the list (Start discord)
                // Check if games have changed on existing sets (New map selection in discord.)
            }
        }

        public static Set GetSet(StartID eventId, StartID setId)
        {
            if (sets.ContainsKey(eventId))
            {
                if (sets[eventId].ContainsKey(setId))
                {
                    return sets[eventId][setId];
                }
            }

            return null;
        }

        public class Set
        {
            public StartID StartId;
            public int CurrentGame;
            public gamething Gamething;
            public SocketTextChannel Channel;
            public SocketUser[] Team1;
            public SocketUser[] Team2;
        }
    }
}