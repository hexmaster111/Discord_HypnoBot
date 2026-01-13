using System;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class ZapTokens
{
    public static ConcurrentDictionary<ulong, string> UserAuthTokens = new();

    public static void Load()
    {
        if (File.Exists("UserSecrets.txt"))
        {
            UserAuthTokens.Clear();
            var jobj = JObject.Parse(File.ReadAllText("UserSecrets.txt"));
            var dict = jobj.ToObject<ConcurrentDictionary<ulong, string>>();
            if (dict == null) return;
            UserAuthTokens = dict;
        }

        Console.WriteLine($"Loaded {UserAuthTokens.Count} user tokens");
    }

    public static void Save()
    {
        File.WriteAllText("UserSecrets.txt", JObject.FromObject(UserAuthTokens).ToString(Formatting.Indented));
    }

    public static void UpsertUserToken(ulong ut, string token)
    {
        UserAuthTokens[ut] = token;
        Save();
    }



}