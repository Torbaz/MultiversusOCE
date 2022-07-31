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
using static BasicBot.Handler.Multiversus;
using static BasicBot.MonarkTypes.Message;

namespace BasicBot.Commands
{
    public class ButtonCommand : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        [ComponentInteraction("bans:*")]
        public async Task RoleSelection(string id, string[] selected)
        {
            await DeferAsync(true);
            if (GetThing(Context.Interaction.Message.Id) is gamething game)
            {
                MonarkMessage msg = "Bugged";

                if (await game.AddMapBan(Context.User, selected.First()))
                {
                    //msg = "Added Ban";
                }
                else
                {
                    msg = "Its not your turn";
                    await msg.SendMessage(Context.Interaction);
                }


            }

        }

        [ComponentInteraction("maps:*")]
        public async Task MapSelection(string id, string[] selected)
        {
            var gld = Handler.Guild.GetDiscordOrMake(Context.Guild);
            if (!gld.Maps.ContainsKey(selected.First()))
            {
                await Context.Interaction.RespondAsync($"Failed to fine", ephemeral: true);
                return;
            }

            await DeferAsync(true);

            if (GetThing(Context.Interaction.Message.Id) is gamething game)
            {
                if (game.User1.Id == Context.User.Id || game.User2.Id == Context.User.Id)
                {
                    await game.BuildBanPhase(gld.Maps[selected.First()]).UpdateMessage(game.Interaction);
                }

            }

        }
    }
}
