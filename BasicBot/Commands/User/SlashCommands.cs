using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static BasicBot.Services.MessageHandlerService;
using System.Linq;
using System.Xml;
using Discord.WebSocket;
using static BasicBot.Handler.User;
using Discord.Interactions;
using SummaryAttribute = Discord.Interactions.SummaryAttribute;
using static BasicBot.Commands.ModalCommand;
using BasicBot.Settings;
using BasicBot.Handler;
using static BasicBot.Commands.ModalCommand;
using ContextType = Discord.Interactions.ContextType;
using Guild = BasicBot.Handler.Guild;
using static BasicBot.Handler.Multiversus;
using static BasicBot.MonarkTypes.Message;

namespace BasicBot.Commands
{
    //[DontAutoRegister()]
    public class SlashCommand : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        [SlashCommand("add-map-pool", "Map pools to be added")]
        public async Task AddMapPool(string Name, List<string> Maps)//, )
        {
            var gld = Guild.GetDiscordOrMake(Context.Guild);
            gld.Maps[Name] = Maps;

            await Context.Interaction.RespondAsync("Done", ephemeral: true);
            Guild.SaveGuilds();
        }

        [SlashCommand("remove-map-pool", "Map pools to be removed")]
        public async Task RemoveMapPool(string Name)//, )
        {
            var gld = Guild.GetDiscordOrMake(Context.Guild);
            if (!gld.Maps.ContainsKey(Name))
            {
                await Context.Interaction.RespondAsync($"Failed to fine {Name}", ephemeral: true);
                return;
            }

            gld.Maps.Remove(Name);

            await Context.Interaction.RespondAsync("removed", ephemeral: true);
            Guild.SaveGuilds();
        }


        [SlashCommand("coinflip", "flip a coin betwen two users")]
        public async Task flip(SocketUser user)//, )
        {
            if (BasicBot.Handler.Random.RandomBool())
            {
                await Context.Interaction.RespondAsync($"I Choose {user.Mention}");
            }
            else
            {
                await Context.Interaction.RespondAsync($"I Choose {Context.User}");
            }

        }

        [SlashCommand("coinflip-game", "flip a coin betwen two users")]
        public async Task flipgame(SocketUser user)//, )
        {
            SocketUser user1, user2;

            if (BasicBot.Handler.Random.RandomBool())
            {
                user1 = user;
                user2 = Context.User;
            }
            else
            {
                user2 = user;
                user1 = Context.User;
            }

            MonarkMessage _msg = new MonarkMessage();
            _msg.AddEmbed(new EmbedBuilder().WithTitle("Building..."));
            await _msg.SendMessage(Context.Interaction, false);

            var msg = await Context.Interaction.GetOriginalResponseAsync();

            var gamething = new gamething(user1, user2, Context.Interaction, Context.Guild.Id);

            things[msg.Id] = gamething;

            await gamething.BuildFirst().UpdateMessage(msg);

        }


        [SlashCommand("game", "game")]
        public async Task game(SocketUser user)//, )
        {
            MonarkMessage _msg = new MonarkMessage();
            _msg.AddEmbed(new EmbedBuilder().WithTitle("Building..."));
            await _msg.SendMessage(Context.Interaction, false);

            var msg = await Context.Interaction.GetOriginalResponseAsync();

            var gamething = new gamething(Context.User, user, Context.Interaction, Context.Guild.Id);

            things[msg.Id] = gamething;

            await gamething.BuildFirst().UpdateMessage(msg);
        }
    }
}
