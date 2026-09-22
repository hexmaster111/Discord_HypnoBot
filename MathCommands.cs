using System;
using System.Text;
using System.Threading.Tasks;
using HaileysSpreadsheats.Expr;
using NetCord;
using NetCord.Services.ApplicationCommands;

public class MathCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("expr", "Evaluate a given expression",
        Contexts =
        [
            InteractionContextType.DMChannel, InteractionContextType.Guild, InteractionContextType.BotDMChannel
        ])]
    public async Task<string> Expr(string expression, bool debug = false)
    {
        try
        {
            var comp = CompExpr.FromString(expression);
            StringBuilder dieRes = new();
            var res = CompExpr.Evaluate(comp, (s) => RollTheDie(s, dieRes));

            if (debug)
            {
                dieRes.AppendLine("Steps:");
                dieRes.AppendLine("```");
                for (var i = 0; i < comp.Ops.Count; i++)
                {
                    var c = comp.Ops[i];
                    dieRes.AppendLine($"{i}: {c.ToString()}");
                }

                dieRes.AppendLine("```");
            }

            return $"{expression} -> {res}{(dieRes.Length != 0 ? $"\n{dieRes}" : "")}";
        }
        catch (Exception ex)
        {
            return $"Oopse: {ex.Message}";
        }
    }

    private double RollTheDie(Roll spec, StringBuilder sb)
    {
        var r = new Random(unchecked((int)DateTime.Now.Ticks));
        var sum = 0;

        for (var i = 0; i < spec.Rolls; i++)
        {
            var ro = r.Next(1, spec.Sides + 1);
            sb.AppendLine($"{i + 1}/{spec.Rolls}d{spec.Sides} => {ro}");
            sum += ro;
        }

        return sum;
    }
}