#region

using System;
using System.Threading.Tasks;
using BasicBot.Handler;
using BasicBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;
using static BasicBot.Handler.Settings;

#endregion

namespace BasicBot;

internal class Program
{
    private static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    public static DiscordSocketClient discordClient;
    public SocketSelfUser selfUser => discordClient != null ? discordClient.CurrentUser : null;

    public async Task MainAsync()
    {
        Console.WriteLine("Hi");

        var currentUserRequest = await StartGGHandler.Client.GetCurrentUser.ExecuteAsync();

        var result =
            await StartGGHandler.Client.GetTournamentAndAdmins.ExecuteAsync(
                "tournament/multiversus-oceania-plasmatic-cup");

        result.EnsureNoErrors();

        if (result.Data == null)
            Console.WriteLine("Error, null data.");
        else if (result.Data.Tournament == null)
            Console.WriteLine("Null tournament");
        else if (result.Data.Tournament.Admins == null)
            Console.WriteLine("Null admins");
        else
        {
            var isAdmin = false;
            foreach (var admin in result.Data.Tournament.Admins)
            {
                if (admin.Id == currentUserRequest.Data.CurrentUser.Id)
                {
                    isAdmin = true;
                    break;
                }
            }

            if (isAdmin)
            {
                var events =
                    await StartGGHandler.Client.GetTournamentEvents.ExecuteAsync(
                        "tournament/multiversus-oceania-plasmatic-cup");

                foreach (var e in events.Data.Tournament.Events)
                {
                    var setsResult =
                        await StartGGHandler.Client.GetSetsAndLinkedAccounts.ExecuteAsync(e.Id.ToString(), 1, 10);
                    Console.WriteLine(setsResult.Data.Event.Sets.Nodes.Count);
                }
            }
            else
            {
                Console.WriteLine("Not admin");
            }
        }

        discordClient = new DiscordSocketClient(new DiscordSocketConfig()
            { LogLevel = LogSeverity.Verbose, GatewayIntents = GatewayIntents.All, AlwaysDownloadUsers = true });


        var token = GetSettings().BotToken;

        await discordClient.LoginAsync(TokenType.Bot, token);
        await discordClient.StartAsync();

        var services = ConfigureServices();

        await services.GetRequiredService<CommandHandlerService>().InitializeAsync();
        services.GetRequiredService<MessageHandlerService>().StartTimers();
        //discordClient.SetGameAsync("pp", null, ActivityType.Listening);


        await Task.Delay(-1);
    }

    private IServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton(discordClient)
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlerService>()
            .AddSingleton<MessageHandlerService>()
            .BuildServiceProvider();
    }
}