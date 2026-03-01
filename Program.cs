using System.IO;
using System.Threading.Tasks;
using HypnoBot;
using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

internal class Program
{
    public static async Task Main(string[] args)
    {
        PavCreds.Load();
        PiShockCreds.Load();
        StimPermsStorage.Load();
        TimeZoneDataStorage.Load();

        var token = File.ReadAllText("DISCORD_TOKEN.txt");

        var builder = Host.CreateApplicationBuilder(args);

        builder.Services
            .AddDiscordGateway(opt =>
            {
                opt.Token = token;
                opt.Intents = GatewayIntents.All;
            })
            .AddGatewayHandlers(typeof(Program).Assembly)
            .AddApplicationCommands();

        var host = builder.Build();

        host.AddModules(typeof(Program).Assembly);
        await host.RunAsync();
    }
}