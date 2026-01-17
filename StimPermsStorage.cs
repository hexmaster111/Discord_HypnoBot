using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HypnoBot;

public static class StimPermsStorage
{
    public static Dictionary<ulong /*victem*/, Dictionary<ulong /*attacker*/, StimPerms>> Perms = new();

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
            var dict = jobj.ToObject<Dictionary<ulong /*victem*/, Dictionary<ulong /*attacker*/, StimPerms>>>();
            if (dict == null) return;
            Perms = dict;
        }

        Console.WriteLine($"Loaded {Perms.Count} User permissions");
    }

    public static StimPerms GetOrCreatePermFor(ulong attacker, ulong victem)
    {
        if (!Perms.ContainsKey(victem))
        {
            Perms.Add(victem, new Dictionary<ulong, StimPerms>());
        }

        if (!Perms[victem].ContainsKey(attacker))
        {
            Perms[victem][attacker] = new();
        }

        return Perms[victem][attacker];
    }

    public static void AddPerm(ulong attacker, ulong victem, StimKind what)
    {
        var perm = GetOrCreatePermFor(attacker, victem);

        switch (what)
        {
            case StimKind.Zap:
                perm.CanZap = true;
                break;
            case StimKind.Buzz:
                perm.CanBuzz = true;
                break;
            case StimKind.Beep:
                perm.CanBeep = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(what), what, null);
        }

        Save();
    }

    public static void RemovePerm(ulong attacker, ulong victem, StimKind what)
    {
        var perm = GetOrCreatePermFor(attacker, victem);

        switch (what)
        {
            case StimKind.Zap:
                perm.CanZap = false;
                break;
            case StimKind.Buzz:
                perm.CanBuzz = false;
                break;
            case StimKind.Beep:
                perm.CanBeep = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(what), what, null);
        }

        Save();
    }

    public static bool IsAllowedTo(ulong attacker, ulong victem, StimKind what)
    {
        var perms = GetOrCreatePermFor(attacker, victem);

        return what switch
        {
            StimKind.Zap => perms.CanZap,
            StimKind.Buzz => perms.CanBuzz,
            StimKind.Beep => perms.CanBeep,
            _ => throw new ArgumentOutOfRangeException(nameof(what), what, null)
        };
    }
}