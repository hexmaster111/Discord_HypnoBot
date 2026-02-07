using System.Threading.Tasks;
using NetCord;
using NetCord.Services.ApplicationCommands;

public class UtilityCommands :
    ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("f_to_c", "Convert Deg F to deg C",
        Contexts =
            [InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel])]
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
    
    
    
}