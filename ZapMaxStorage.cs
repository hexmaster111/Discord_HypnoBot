using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HypnoBot;

public static class ZapMaxStorage
{
    public static ConcurrentDictionary<ulong, int> Storage = new();

    public const string Filename = nameof(ZapMaxStorage) + ".txt";

    public static void Save()
    {
        var so = JsonConvert.SerializeObject(Storage, Formatting.Indented);
        File.WriteAllText(Filename, so);
    }

    public static void Load()
    {
        if (!File.Exists(Filename)) return;
        var ft = File.ReadAllText(Filename);
        var obj = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, int>>(ft);
        if (obj != null) Storage = obj;
    }

    public static void UpsertMaxZap(ulong userId, int maxZap)
    {
        Storage[userId] = maxZap;
        Save();
    }

    public static int GetMaxZap(ulong userId)
    {
        return Storage.GetValueOrDefault(userId, 100);
    }
}