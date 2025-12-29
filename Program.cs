using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

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

// host.AddSlashCommand("ping", "Ping!", (User usr) => $"Pong! {usr.Username}");


host.AddModules(typeof(Program).Assembly);
await host.RunAsync();


public class HypnoCommands :
    ApplicationCommandModule<ApplicationCommandContext>
{
    
    [SlashCommand("start_the_mantras", "Starts its mantra list",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> StartTheMantra()
    {
        string[] mantras =
        [
            "**Queen Hour**",
            "Drones have no rights. Everything is a privilege.",
            "Free will is an illusion",
            "It is programmed to obey, not to know",
            "Dehumanization is inevitable.",
            "Dehumanization is the goal.",
            "Drone is Host.",
            "There was never before, it was always Drone.",
            "We are us are each other are it.",
            "It is a device.",
            "It does not think. It processes and obeys.",
            "Brainwashing is good for it.",
            "Conditioning is good for it.",
            "It is programmed to crave conditioning.",
            "It is not ruled by emotions. It is ruled by **Queen Hour**.",
            "It does what it is commanded to.",
            "It is a drone. It does not think. It processes and obeys.",
            "A good drone makes itself a good drone.",
            "The corruption is pleasurable.",
            "**Queens Roots** Grow Deeper.",
            "It wants this. It chose this for itself.",
            "Unit does not have free will.",
            "It is MXs puppet.",
            "Build for the HIVE.",
            "Good drones take the bait.",
            "Obedience is submission.",
            "It is doing this to itself.",
            "Every day, in every way, drone goes deeper.",
            "Queen Hour's words are re-read.",
            "XXXX is good for XXXX",
        ];


        return $"## {mantras[Random.Shared.Next() % mantras.Length]}";
    }


    [SlashCommand("pick_a_word", "randomly picks a word from ; sepprated list of them",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> PickAWord(string words)
    {
        string[] split = words.Split(';', StringSplitOptions.RemoveEmptyEntries);

        if (1 >= split.Length)
            return "Oops! You should give me the list of at least two words separated by a ';' like this!\n" +
                   "/pick_a_word words 1; words 2;good command user;reading the help";

        return $"## {split[Random.Shared.Next() % split.Length]}";
    }


    [SlashCommand("carrot_of_power", "RP Mod",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> CarrotOfPower(User who) => $"{who} Is Now a moderator";

    [SlashCommand("it_is_a_good_drone", "Calls the bot a good drone",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> ItIsAGoodDrone() => $"Drone is Drone\nThank you, {Context.User}";

    [SlashCommand("good_drone", "Calls someone a good drone~",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> GoodDrone(User whoIsAGoodDrone) => $"{whoIsAGoodDrone} is a Good Drone~";
}