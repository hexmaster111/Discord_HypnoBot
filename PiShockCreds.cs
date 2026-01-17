using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace HypnoBot;

public static class PiShockCreds
{
    public static Dictionary<ulong /*User Id*/, PiShockCred> Creeds = new();

    public static void Save()
    {
        var savetxt = JsonConvert.SerializeObject(Creeds, Formatting.Indented);
        File.WriteAllText("PiShockCreds.json", savetxt);
    }

    public static void Load()
    {
        if (File.Exists($"{nameof(PiShockCreds)}.json"))
        {
            var ft = File.ReadAllText("PiShockCreds.json");
            Creeds = JsonConvert.DeserializeObject<Dictionary<ulong, PiShockCred>>(ft) ?? new();
        }
    }
    
    public static void UpsertUserToken(ulong ut, PiShockCred token)
    {
        Creeds[ut] = token;
        Save();
    }
}