using System;
using System.Threading.Tasks;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace HypnoBot;

public class ZapCommands :
    ApplicationCommandModule<ApplicationCommandContext>
{
    public static string NoTokenError(User who) =>
        $"{who} needs to dm this bot there token from here https://pavlok.readme.io/reference/intro/getting-started or there pi shock data";

    public static string NoPermsError(User victum, User inflicter, StimKind kind) =>
        $"{inflicter} hasnt been allowed to {kind} {victum}.\n{victum} must run /allow_{kind} {inflicter}";


    [SlashCommand("allow_all_stims", "allow user use all stim commands on you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> AllowAllStims(User attacker)
    {
        StimPermsStorage.AddPerm(attacker.Id, Context.User.Id, StimKind.Zap);
        StimPermsStorage.AddPerm(attacker.Id, Context.User.Id, StimKind.Beep);
        StimPermsStorage.AddPerm(attacker.Id, Context.User.Id, StimKind.Buzz);
        return $"*giggles* {attacker} get **full pav control**";
    }

    [SlashCommand("disallow_all_stims", "disallow user from all stim commands on you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> DisAllowAllStims(User attacker)
    {
        StimPermsStorage.RemovePerm(attacker.Id, Context.User.Id, StimKind.Zap);
        StimPermsStorage.RemovePerm(attacker.Id, Context.User.Id, StimKind.Beep);
        StimPermsStorage.RemovePerm(attacker.Id, Context.User.Id, StimKind.Buzz);
        return $"{attacker} may no longer user PAV Commands on you :(";
    }


    [SlashCommand("allow_zap", "allow user to zap you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> AllowZap(User attacker)
    {
        StimPermsStorage.AddPerm(attacker.Id, Context.User.Id, StimKind.Zap);
        return $"{attacker} may now Zap you *giggles*";
    }

    [SlashCommand("disallow_zap", "disallow user to zap you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> DisAllowZap(User attacker)
    {
        StimPermsStorage.RemovePerm(attacker.Id, Context.User.Id, StimKind.Zap);
        return $"{attacker} may no longer zap you";
    }

    [SlashCommand("allow_buzz", "allow user to buzz you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> AllowBuzz(User attacker)
    {
        StimPermsStorage.AddPerm(attacker.Id, Context.User.Id, StimKind.Buzz);
        return $"{attacker} may now Buzz you";
    }

    [SlashCommand("disallow_buzz", "disallow user to buzz you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> DisAllowBuzz(User attacker)
    {
        StimPermsStorage.RemovePerm(attacker.Id, Context.User.Id, StimKind.Buzz);
        return $"{attacker} may no longer Buzz you";
    }


    [SlashCommand("allow_beep", "allow user to beep",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> AllowBeep(User attacker)
    {
        StimPermsStorage.AddPerm(attacker.Id, Context.User.Id, StimKind.Beep);
        return $"{attacker} may now beep you";
    }

    [SlashCommand("disallow_beep", "disallow user to beep you",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> DisAllowBeep(User attacker)
    {
        StimPermsStorage.RemovePerm(attacker.Id, Context.User.Id, StimKind.Beep);
        return $"{attacker} may no longer Beep";
    }


    [SlashCommand("add_pavlok_token", "add pavlok token", Contexts = [InteractionContextType.BotDMChannel])]
    public async Task<string> AddPavLocToken(string token)
    {
        PavCreds.UpsertUserToken(Context.User.Id, token);
        return "Token Saved";
    }


    [SlashCommand("add_pishock_token",
        "add pishock token", Contexts = [InteractionContextType.BotDMChannel])]
    public async Task<string> AddPishockToken(
        string username, string apikey, string shareCode
    )
    {
        PiShockCreds.UpsertUserToken(Context.User.Id, new(username, apikey, shareCode));
        return "Token Saved";
    }


    // handles choosing the API to zap with, and getting the api tokens for it
    public async Task<string> SendStim(StimKind kind, string? why, int power, User who)
    {
        bool hadOneAtleast = false;
        bool hadError = false;
        string errorMsg = "";

        if (PavCreds.UserAuthTokens.TryGetValue(who.Id, out string token))
        {
            hadOneAtleast = true;
            var res = await PavLocApi.SendPavStim(kind, power, why, token);
            if (!res.IsSuccessStatusCode)
            {
                hadError = true;
                errorMsg = res.ToString();
            }
        }

        if (PiShockCreds.Creeds.TryGetValue(who.Id, out var psc))
        {
            hadOneAtleast = true;
            var res = await PiShockApi.SendPiShockStim(kind, power, 1, psc);
            if (!res.IsSuccessStatusCode)
            {
                hadError = true;
                errorMsg = res.ToString();
            }
        }

        if (!hadOneAtleast) return NoTokenError(who);
        if (hadError) return $"Something went wrong.\n{errorMsg}";

        return kind switch
        {
            StimKind.Zap => "Task Complete :zap:",
            StimKind.Buzz => "Task Complete :vibration_mode:",
            StimKind.Beep => "Task Complete :loud_sound:",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }


    [SlashCommand("zap", "zap a user",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> Zap(User who, int power, string why)
    {
        if (!StimPermsStorage.IsAllowedTo(Context.User.Id, who.Id, StimKind.Zap))
        {
            return NoPermsError(who, Context.User, StimKind.Zap);
        }

        return await SendStim(StimKind.Zap, why, power, who);
    }


    [SlashCommand("buzz", "Buzz a user",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> Buzz(User who, int power, string? why)
    {
        if (!StimPermsStorage.IsAllowedTo(Context.User.Id, who.Id, StimKind.Buzz))
        {
            return NoPermsError(who, Context.User, StimKind.Buzz);
        }

        return await SendStim(StimKind.Buzz, why, power, who);
    }

    [SlashCommand("beep", "Buzz a user",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> Beep(User who, int power, string? why)
    {
        if (!StimPermsStorage.IsAllowedTo(Context.User.Id, who.Id, StimKind.Beep))
            return NoPermsError(who, Context.User, StimKind.Beep);

        return await SendStim(StimKind.Beep, why, power, who);
    }
}