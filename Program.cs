using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HaileysSpreadsheats.Expr;
using HypnoBot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

internal class Program
{
    private static void RunRepl()
    {
        const string Prompt = "> ";
        string? line = "";
        bool lexDebug = false;

        Console.Write(Prompt);

        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0) goto CommandDone;
            if (line.StartsWith("."))
            {
                if (line.Equals(".lex")) lexDebug = !lexDebug;
                if(line.Equals(".clear")) Console.Clear();

                goto CommandDone;
            }

            try
            {
                Lexer l = new(line);

                if (lexDebug)
                {
                    while (l.More()) Console.WriteLine(l.Consume().Kind);
                    l = new(line);
                }

                Parser p = new(l);
                AstNode root = p.GetRoot();
                Compiler c = new(root);

                var comp = c.Compile();
                if (comp.error != null) throw new Exception(comp.error);
                var cExpr = new CompiledExpression(line, comp.output!);
                var res = CompExpr.Evaluate(cExpr, spec => -1);
                Console.WriteLine(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            CommandDone:

            Console.Write(Prompt);
        }
    }

    public static async Task Main(string[] args)
    {
        //Console.WriteLine($"Res {CompExpr.Evaluate(CompExpr.FromString("TRUE"), spec => 1)}");
        RunRepl();

        PavCreds.Load();
        PiShockCreds.Load();
        StimPermsStorage.Load();
        TimeZoneDataStorage.Load();
        ZapMaxStorage.Load();

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

        var client = host.Services.GetRequiredService<GatewayClient>();
        client.MessageCreate += async (message) =>
        {
            if (message.Author.Id == client.Cache.User!.Id) return;

            if (message.Content.StartsWith("!buzz ")) await BuzzHandler(client, message);
            if (message.Content.StartsWith("!zap ")) await ZapHandler(client, message);
        };


        await host.RunAsync();
    }

    private static bool TryParseWearableCommand(string input, out ulong userId, out int level, out string message)
    {
        userId = 0;
        var userIdStr = string.Empty;
        level = 0;
        message = string.Empty;

        // match !beep/!buzz/!zap <@userid> <level> <message>
        var match = Regex.Match(input, @"^!(beep|buzz|zap)\s+<@(\d+)>\s+(\d+)\s+(.+)$");
        if (!match.Success) return false;

        userIdStr = match.Groups[2].Value;
        message = match.Groups[4].Value.Trim();

        return ulong.TryParse(userIdStr, out userId) && int.TryParse(match.Groups[3].Value, out level);
    }

    private static async Task BuzzHandler(GatewayClient client, Message message)
    {
        if (!TryParseWearableCommand(message.Content, out var who, out var power, out var why))
        {
            await message.ReplyAsync("Oopse i need <@USERID> <level> <message>");
            return;
        }

        if (!StimPermsStorage.IsAllowedTo(message.Author.Id, who, StimKind.Buzz))
        {
            await message.ReplyAsync("No Perms!");
            return;
        }

        await ZapCommands.SendStim(StimKind.Buzz, why, power, who);
        await message.ReplyAsync("Task Complete");
    }

    private static async Task ZapHandler(GatewayClient client, Message message)
    {
        if (!TryParseWearableCommand(message.Content, out var who, out var power, out var why))
        {
            await message.ReplyAsync("Oopse i need <@USERID> <level> <message>");
            return;
        }

        if (!StimPermsStorage.IsAllowedTo(message.Author.Id, who, StimKind.Zap))
        {
            await message.ReplyAsync("No Perms!");
            return;
        }

        var maxZap = ZapMaxStorage.GetMaxZap(who);

        power = ZapCommands.Map(power, 0, 100, 0, maxZap);

        await ZapCommands.SendStim(StimKind.Zap, why, power, who);
        await message.ReplyAsync("Task Complete");
    }
}