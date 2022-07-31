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
using String = System.String;
using static BasicBot.Handler.Multiversus;

namespace BasicBot.Commands
{
    public class ModalCommand : InteractionModuleBase<SocketInteractionContext<SocketModal>>
    {

    }
}
