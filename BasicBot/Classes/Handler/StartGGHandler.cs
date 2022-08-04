using System;
using System.Net.Http.Headers;
using BasicBot.GraphQL;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;

namespace BasicBot.Handler;

public class StartGGHandler
{
    public static IStartGGClient Client
    {
        get
        {
            if (client == null)
            {
                Initialize();
            }
            return client;
        }
    }

    private static IStartGGClient client;
    private static ServiceCollection serviceCollection;
    private static ServiceProvider serviceProvider;
    
    private static void Initialize()
    {
        serviceCollection = new ServiceCollection();

        serviceCollection.AddStartGGClient().ConfigureHttpClient((c) =>
        {
            c.BaseAddress = new Uri("https://api.start.gg/gql/alpha");
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.GetSettings().StartGGToken);
        });

        serviceProvider = serviceCollection.BuildServiceProvider();
        client = serviceProvider.GetRequiredService<IStartGGClient>();
    }
}