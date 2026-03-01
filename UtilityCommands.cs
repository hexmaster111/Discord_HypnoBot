using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCord;
using NetCord.Services.ApplicationCommands;
using Newtonsoft.Json;

public class UtilityCommands :
    ApplicationCommandModule<ApplicationCommandContext>
{


    [SlashCommand("f_to_c", "Convert Deg F to deg C", Contexts = [InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel])]
    public async Task<string> FToC(float degF)
    {
        var degC = degF;
        degC -= 32;
        degC *= 5.0f / 9.0f;
        return $"`{degF:0.00}°F -> {degC:0.00}°C`";
    }

    [SlashCommand("c_to_f", "Convert Deg C to deg F",
        Contexts =
            [InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel])]
    public async Task<string> CToF(float degC)
    {
        var degF = degC;
        degF *= 9.0f / 5.0f;
        degF += 32;

        return $"`{degC:0.00}°C -> {degF:0.00}°F`";
    }

    [SlashCommand("set_time_zone", "Set your timezone", Contexts = [InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel])]
    public async Task<string> SetMyTimeZone(string myTz)
    {
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(myTz, out var tzi))
        {
            var options = TimeZoneInfo.GetSystemTimeZones();
            StringBuilder sb = new();

            sb.AppendLine("Thats not a valid timezone, but these are like the one you just send me:");

            const int max = 30;
            var items = options.Where(x => x.Id.Contains(myTz)).ToArray();

            for (int idx = 0; idx < Math.Min(items.Length, max); idx++)
            {
                sb.AppendLine(items[idx].Id);
            }

            return sb.ToString();
        }

        TimeZoneDataStorage.UserTimeZone[Context.User.Id] = tzi.Id;
        TimeZoneDataStorage.Save();

        return $"Timezone: {tzi} set";
    }

    [SlashCommand("date_time_convert", "convert time + user tz to discord time", Contexts = [InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel])]
    public async Task<string> ConvertDateTime(string time)
    {
        if (!DateTime.TryParse(time, out DateTime o)) return $"oopse! {time} is not a valid time";
        if (!TimeZoneDataStorage.UserTimeZone.TryGetValue(Context.User.Id, out var tzi)) return "Oopse, you need to run /set_time_zone first";
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(tzi, out var tz)) return "Oopse, you need to run /set_time_zone; I seem to not be able to find your tz!";

        DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(o, tz);
        return $"<t:{new DateTimeOffset(utcTime).ToUnixTimeSeconds()}:F>"; //:t for short
    }

        [SlashCommand("time_convert", "convert time + user tz to discord time", Contexts = [InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel])]
    public async Task<string> ConvertTimeShort(string time)
    {
        if (!DateTime.TryParse(time, out DateTime o)) return $"oopse! {time} is not a valid time";
        if (!TimeZoneDataStorage.UserTimeZone.TryGetValue(Context.User.Id, out var tzi)) return "Oopse, you need to run /set_time_zone first";
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(tzi, out var tz)) return "Oopse, you need to run /set_time_zone; I seem to not be able to find your tz!";

        DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(o, tz);
        return $"<t:{new DateTimeOffset(utcTime).ToUnixTimeSeconds()}:t>"; //:t for short
    }
}

public static class TimeZoneDataStorage
{
    public static Dictionary<ulong, string> UserTimeZone = new();


    public static void Save()
    {
        File.WriteAllText("UserTimeZone.txt", JsonConvert.SerializeObject(UserTimeZone, Formatting.Indented));
    }

    public static void Load()
    {
        if (File.Exists("UserTimeZone.txt"))
        {
            var ft = File.ReadAllText("UserTimeZone.txt");
            var obj = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(ft);
            if (obj != null) UserTimeZone = obj;
        }
    }
}