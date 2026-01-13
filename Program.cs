using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverter = Newtonsoft.Json.JsonConverter;

ZapTokens.Load();
ZapPerms.Load();

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

public class PavPerm
{
    public bool CanZap = false;
    public bool CanBuzz = false;
    public bool CanBeep = false;
}

public static class ZapPerms
{
    public static Dictionary<ulong /*victem*/, Dictionary<ulong /*attacker*/, PavPerm>> Perms = new();

    public static void Save()
    {
        File.WriteAllText("UserPerms.txt", JObject.FromObject(Perms).ToString(Formatting.Indented));
    }

    public static void Load()
    {
        if (File.Exists("UserPerms.txt"))
        {
            Perms.Clear();
            var jobj = JObject.Parse(File.ReadAllText("UserPerms.txt"));
            var dict = jobj.ToObject<Dictionary<ulong /*victem*/, Dictionary<ulong /*attacker*/, PavPerm>>>();
            if (dict == null) return;
            Perms = dict;
        }

        Console.WriteLine($"Loaded {Perms.Count} User permissions");
    }

    public static PavPerm GetOrCreatePermFor(ulong attacker, ulong victem)
    {
        if (!Perms.ContainsKey(victem))
        {
            Perms.Add(victem, new Dictionary<ulong, PavPerm>());
        }

        if (!Perms[victem].ContainsKey(attacker))
        {
            Perms[victem][attacker] = new();
        }

        return Perms[victem][attacker];
    }

    public static void AddPerm(ulong attacker, ulong victem, PavStimulesKind what)
    {
        var perm = GetOrCreatePermFor(attacker, victem);

        switch (what)
        {
            case PavStimulesKind.Zap:
                perm.CanZap = true;
                break;
            case PavStimulesKind.Buzz:
                perm.CanBuzz = true;
                break;
            case PavStimulesKind.Beep:
                perm.CanBeep = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(what), what, null);
        }

        Save();
    }

    public static void RemovePerm(ulong attacker, ulong victem, PavStimulesKind what)
    {
        var perm = GetOrCreatePermFor(attacker, victem);

        switch (what)
        {
            case PavStimulesKind.Zap:
                perm.CanZap = false;
                break;
            case PavStimulesKind.Buzz:
                perm.CanBuzz = false;
                break;
            case PavStimulesKind.Beep:
                perm.CanBeep = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(what), what, null);
        }

        Save();
    }

    public static bool IsAllowedTo(ulong attacker, ulong victem, PavStimulesKind what)
    {
        var perms = GetOrCreatePermFor(attacker, victem);

        return what switch
        {
            PavStimulesKind.Zap => perms.CanZap,
            PavStimulesKind.Buzz => perms.CanBuzz,
            PavStimulesKind.Beep => perms.CanBeep,
            _ => throw new ArgumentOutOfRangeException(nameof(what), what, null)
        };
    }
}


public class ZapCommands :
    ApplicationCommandModule<ApplicationCommandContext>
{
    public static string NoTokenError(User who) =>
        $"{who} needs to dm this bot there token from here https://pavlok.readme.io/reference/intro/getting-started";

    public static string NoPermsError(User victum, User inflicter, PavStimulesKind kind) =>
        $"{inflicter} hasnt been allowed to {kind} {victum}.\n{victum} must run /allow_{kind} {inflicter}";


    public static async Task<string> SendPavStimuls(PavStimulesKind kind, int power, string? why, string token)
    {
        if (why == null) why = "No Reason Provided";
        if (0 > power) power = 0;
        if (power > 100) power = 100;

        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.pavlok.com/api/v5/stimulus/send"),
            Headers =
            {
                { "accept", "application/json" }, { "Authorization", token },
            },
            Content = new StringContent(
                $@"{{""stimulus"":{{""stimulusType"":""{kind.ToApiString()}"",""stimulusValue"":{power},""reason"":""{why}""}}}}")
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            }
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var t = response.Content.ReadAsStringAsync();
            return await t;
        }
    }

    [SlashCommand("allow_all_pav", "allow user use all pav commands on you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> AllowAllPav(User attacker)
    {
        ZapPerms.AddPerm(attacker.Id, Context.User.Id, PavStimulesKind.Zap);
        ZapPerms.AddPerm(attacker.Id, Context.User.Id, PavStimulesKind.Beep);
        ZapPerms.AddPerm(attacker.Id, Context.User.Id, PavStimulesKind.Buzz);
        return $"*giggles* {attacker} get **full pav control**";
    }

    [SlashCommand("disallow_all_pav", "disallow user from all pav commands on you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> DisAllowAllPav(User attacker)
    {
        ZapPerms.RemovePerm(attacker.Id, Context.User.Id, PavStimulesKind.Zap);
        ZapPerms.RemovePerm(attacker.Id, Context.User.Id, PavStimulesKind.Beep);
        ZapPerms.RemovePerm(attacker.Id, Context.User.Id, PavStimulesKind.Buzz);
        return $"{attacker} may no longer user PAV Commands on you :(";
    }


    [SlashCommand("allow_zap", "allow user to zap you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> AllowZap(User attacker)
    {
        ZapPerms.AddPerm(attacker.Id, Context.User.Id, PavStimulesKind.Zap);
        return $"{attacker} may now Zap you *giggles*";
    }

    [SlashCommand("disallow_zap", "disallow user to zap you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> DisAllowZap(User attacker)
    {
        ZapPerms.RemovePerm(attacker.Id, Context.User.Id, PavStimulesKind.Zap);
        return $"{attacker} may no longer zap you";
    }

    [SlashCommand("allow_buzz", "allow user to buzz you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> AllowBuzz(User attacker)
    {
        ZapPerms.AddPerm(attacker.Id, Context.User.Id, PavStimulesKind.Buzz);
        return $"{attacker} may now Buzz you";
    }

    [SlashCommand("disallow_buzz", "disallow user to buzz you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> DisAllowBuzz(User attacker)
    {
        ZapPerms.RemovePerm(attacker.Id, Context.User.Id, PavStimulesKind.Buzz);
        return $"{attacker} may no longer Buzz you";
    }


    [SlashCommand("allow_beep", "allow user to beep",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> AllowBeep(User attacker)
    {
        ZapPerms.AddPerm(attacker.Id, Context.User.Id, PavStimulesKind.Beep);
        return $"{attacker} may now beep you";
    }

    [SlashCommand("disallow_beep", "disallow user to beep you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> DisAllowBeep(User attacker)
    {
        ZapPerms.RemovePerm(attacker.Id, Context.User.Id, PavStimulesKind.Beep);
        return $"{attacker} may no longer Beep";
    }


    [SlashCommand("add_pavlok_token", "add pavlok token", Contexts = [InteractionContextType.BotDMChannel])]
    public async Task<string> AddToken(string token)
    {
        ZapTokens.UpsertUserToken(Context.User.Id, token);
        return "Token Saved";
    }


    [SlashCommand("zap", "zap a user",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> Zap(User who, int power, string why)
    {
        if (!ZapPerms.IsAllowedTo(Context.User.Id, who.Id, PavStimulesKind.Zap))
        {
            return NoPermsError(who, Context.User, PavStimulesKind.Zap);
        }

        if (!ZapTokens.UserAuthTokens.TryGetValue(who.Id, out string token))
        {
            return NoTokenError(who);
        }

        await SendPavStimuls(PavStimulesKind.Zap, power, why, token);

        return $"Task Complete :zap:";
    }


    [SlashCommand("buzz", "Buzz a user",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> Buzz(User who, int power, string? why)
    {
        if (!ZapPerms.IsAllowedTo(Context.User.Id, who.Id, PavStimulesKind.Buzz))
        {
            return NoPermsError(who, Context.User, PavStimulesKind.Buzz);
        }

        if (!ZapTokens.UserAuthTokens.TryGetValue(who.Id, out string token))
        {
            return NoTokenError(who);
        }

        await SendPavStimuls(PavStimulesKind.Buzz, power, why, token);

        return $"Task Complete :vibration_mode:";
    }

    [SlashCommand("beep", "Buzz a user",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> Beep(User who, int power, string? why)
    {
        if (!ZapPerms.IsAllowedTo(Context.User.Id, who.Id, PavStimulesKind.Beep))
            return NoPermsError(who, Context.User, PavStimulesKind.Beep);

        if (!ZapTokens.UserAuthTokens.TryGetValue(who.Id, out string token)) return NoTokenError(who);

        await SendPavStimuls(PavStimulesKind.Beep, power, why, token);

        return $"Task Complete :loud_sound:";
    }
}