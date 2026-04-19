using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IT9GameLog.SocketIo;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Newtonsoft.Json.Linq;

namespace HypnoBot;

public class ToyListItem
{
    public required string Id;
    public required string Name;
    public required string ToyType;
    public required int Battery;
    public required bool Connected;

    public static ToyListItem Parse(JToken jo)
    {
        string id = jo["id"].Value<string>() ?? "";
        string name = jo["name"].Value<string>() ?? "";
        string toyType = jo["toyType"].Value<string>() ?? "";
        //        string nickname = jo["nickname"].Value<string>() ?? "";
        //        int fVersion = jo["fVersion"].Value<int>();
        //        string hVersion = jo["hVersion"].Value<string>() ?? "";
        int battery = jo["battery"].Value<int>();
        bool connected = jo["connected"].Value<bool>();

        return new()
        {
            Id = id,
            Battery = battery,
            Connected = connected,
            Name = name,
            ToyType = toyType
        };
    }
}

public class LovenceConnection
{
    static readonly string LovenseDevToken;
    private static readonly string Platform;

    static LovenceConnection()
    {
        LovenseDevToken = File.ReadAllText("LOVENSE_DEV_TOKEN.txt");
        Platform = File.ReadAllText("LOVENSE_PLATFORM.txt");
    }
    

    const string LovenseGetTokenUrl = "https://api.lovense-api.com/api/basicApi/getToken";
    const string LovenseGetQrCodeUrl = "https://api.lovense-api.com/api/lan/getQrCode";
    const string LovenseGetSocketUrl = "https://api.lovense-api.com/api/basicApi/getSocketUrl";
    const string AppJson = "application/json";

    // ==========================================================================================

    public DMChannel UserDm;
    public SocketIoWithWsClient? Socket;
    public HttpClient? Client = new HttpClient();

    public object ToysLock = new();
    public List<ToyListItem>? Toys;


    public LovenceConnection(DMChannel userDmChannel)
    {
        UserDm = userDmChannel;
    }
    // Device - Lovence connection and timeout stuff

    private string ReadMessageBody(string msg)
    {
        //["basicapi_get_qrcode_tc","{\"code\":0,\"data\":{\"ackId\":\"qr_req\",\"code\":\"395079\",\"qrcode\":\"{\\\"type\\\":5,\\\"data\\\":\\\"2Td5iU0YoWSpsE4fx5WSMYsgkcwwPTnxP0p3QzIOAZnivgNr3p//n4/qd1h+WrhHm1fQPMMunPlhJ3mDKKUqrw==\\\"}\",\"qrcodeUrl\":\"https://apps.lovense.com/UploadFiles/qr/20260307/868406629d1a491888014844ee3b7e9e.jpg\"}}"]
        var start = msg.IndexOf(',');
        if (start == -1) return "";
        start += 2;
        var end = msg.Length - 2;
        if (start > end) return "";

        return Regex.Unescape(msg.Substring(start, end - start));
    }

    private string ReadMessageType(string payload)
    {
        var start = payload.IndexOf('"');
        var end = payload.IndexOf('"', start + 1);


        if (start == -1 || end == -1) return "";

        return payload.Substring(start + 1, end - 2);
    }

    private string ReadQrCodeUrlFromMessage(string msg)
    {
        string bdy = ReadMessageBody(msg);
        if (string.IsNullOrEmpty(bdy)) return "Error reading body";
        var obj = JObject.Parse(bdy);
        int? ec = obj["code"]?.Value<int>();
        if (ec != 0)
        {
            return ($"Error getting you a QR Code! {ec}");
        }

        return obj["data"]?["qrcodeUrl"]?.Value<string>() ?? "Error getting you a QR Code";
    }

    private void OnQrCodeMessage(string msg)
    {
        Console.WriteLine("OnQrCodeMessage");

        var urlOrErr = ReadQrCodeUrlFromMessage(msg);
        Console.WriteLine("OnQrCodeMessage ReadQrCodeUrlFromMessage");
        UserDm.SendMessageAsync(urlOrErr).Wait();
        Console.WriteLine("UserDm.SendMessageAsync(urlOrErr).Wait()");
        Console.WriteLine(urlOrErr);
    }


    private void OnUpdateDeviceInfo(string msg)
    {
        // ["basicapi_update_device_info_tc","{\"deviceCode\":\"remote_3ceda5aab4e076e62d1eccb6bd674635\",\"online\":true,\"domain\":\"192-168-0-248.lovense.club\",\"httpsPort\":30010,\"wssPort\":30010,\"platform\":\"ios\",\"appVersion\":\"7.69.0\",\"appType\":\"remote\",\"toyList\":[{\"id\":\"0487275354ff\",\"name\":\"Hush 2\",\"toyType\":\"hush\",\"nickname\":\"\",\"fVersion\":331,\"hVersion\":\"2\",\"battery\":100,\"connected\":true}]}"]
        var body = ReadMessageBody(msg);
        var jobj = JObject.Parse(body);

        var online = jobj["online"]?.Value<bool>() == true;
        var items = jobj["toyList"]?.Value<JArray>();

        Toys = items?.Select(ToyListItem.Parse)?.ToList();
    }

    private void OnUpdateAppStatus(string msg)
    {
        // ["basicapi_update_app_status_tc","{\"status\":1}"]
    }

    private void OnMessage(
        object? sender,
        IncomingEventEventArgs inEvent
    )
    {
        try
        {
            string payloadKind = ReadMessageType(inEvent.Payload);

            switch (payloadKind)
            {
                case "basicapi_get_qrcode_tc": OnQrCodeMessage(inEvent.Payload); break;
                case "basicapi_update_app_online_tc": OnUpdateAppOnline(inEvent.Payload); break;
                case "basicapi_update_app_status_tc": OnUpdateAppStatus(inEvent.Payload); break;
                case "basicapi_update_device_info_tc": OnUpdateDeviceInfo(inEvent.Payload); break;
                default: Console.WriteLine("unhandled payload: " + payloadKind); break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void OnUpdateAppOnline(string msg)
    {
        // ["basicapi_update_app_online_tc","{\"status\":1}"]
        Console.WriteLine($"OnUpdateAppOnline: {msg}");
    }


    // Reutnrs message to send to discord
    public async Task<(string msg, bool err)> StartSession(User user)
    {
        var authTokenReq = new
        {
            token = LovenseDevToken,
            uid = user.Id, // user id
            uname = user.Username // username
        };

        var authTokenReqStr = new StringContent(JsonSerializer.Serialize(authTokenReq), Encoding.UTF8, AppJson);
        var tokenReqResponce = await Client.PostAsync(LovenseGetTokenUrl, authTokenReqStr);
        var authTokenResp = await tokenReqResponce.Content.ReadAsStringAsync();
// {"code":0,"message":"Success","data":{"authToken":"token string"}}
        var authTokenJobj = JObject.Parse(authTokenResp);
        if (authTokenJobj["code"]?.Value<int>() != 0) return ("error getting auth token", true);
        var LovenseAuthToken = authTokenJobj["data"]?["authToken"]?.Value<string>();
        if (string.IsNullOrEmpty(LovenseAuthToken)) return ("no auth token", true);


        var socketRequest = new
        {
            platform = Platform,
            authToken = LovenseAuthToken
        };
        var socketRequestStr = new StringContent(JsonSerializer.Serialize(socketRequest), Encoding.UTF8, AppJson);
        var socketReqResp = await Client.PostAsync(LovenseGetSocketUrl, socketRequestStr);
        var socketReqRespStr = await socketReqResp.Content.ReadAsStringAsync();
// {"code":0,"message":"Success","data":{"socketIoPath":"/developer.io","socketIoUrl":"https://developerio.lovense-api.com?ntoken=tokentokentoken"}}
        var socketReqRespJobj = JObject.Parse(socketReqRespStr);
        if (socketReqRespJobj["code"]?.Value<int>() != 0) return ("error getting socket", true); // error getting socket

        var SocketIoPath = socketReqRespJobj["data"]?["socketIoPath"]?.Value<string>();
        var SocketIoUrl = socketReqRespJobj["data"]?["socketIoUrl"]?.Value<string>();
        if (string.IsNullOrEmpty(SocketIoPath) || string.IsNullOrEmpty(SocketIoUrl))
            return ("no io path or url", true); // no io path or url


        var uri = new Uri(SocketIoUrl);

        Socket = new();
        Socket.IncomingEvent += OnMessage;

        var cts = new CancellationTokenSource();
        Console.WriteLine($"connecting...");


        if (!SocketIoPath.EndsWith('/')) SocketIoPath += "/";
        await Socket.ConnectAsync(uri, SocketIoPath, cts.Token);

        Socket.SendEventPayload("""["basicapi_get_qrcode_ts",{"ackId":"qr_req"}]""");


        return ($"connected", false);
    }


    public void BuzzAllConnectedDevices()
    {
        if (Toys == null) return;
        if (Socket == null) return;

        lock (ToysLock)
        {
            // todo: buzz them

            foreach (var toy in Toys)
            {
                Socket.SendEventPayload(
                    $$"""["basicapi_send_toy_command_ts",{"command":"Function","apiVer":1,"timeSec":1,"toyId":"{{toy.Id}}","action":"Vibrate:75"}]"""
                );
            }
        }
    }

    public string[] ListToyNames()
    {
        if (Toys == null) return [];

        List<string> t = new();

        lock (ToysLock)
        {
            foreach (var toy in Toys)
            {
                t.Add($"{toy.Name} {toy.Battery} {(toy.Connected ? "🟢" : "🔴")}");
            }
        }

        return t.ToArray();
    }
}

// Discord Commands
public class LovenceControlCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    public static ConcurrentDictionary<ulong, LovenceConnection> Connections = new();

    [SlashCommand("start_session", "Starts a lovence session", Contexts =
    [
        InteractionContextType.BotDMChannel,
        InteractionContextType.DMChannel,
        InteractionContextType.Guild
    ])]
    public async Task<string> StartLovenceSession()
    {
        var dmc = await Context.User.GetDMChannelAsync();
        var con = new LovenceConnection(dmc);
        var (msg, err) = await con.StartSession(Context.User);

        Connections[Context.User.Id] = con;
        return "Session Started, check your dms from this bot.";
    }


    public static LovenceConnection? GetConnectionFor(ulong id)
    {
        // todo: check that connection is still valid 
        if (!Connections.TryGetValue(id, out var v)) return null;
        return v;
    }


    [SlashCommand("buzz_lovense_device", "mmmmpphh.", Contexts =
    [
        InteractionContextType.BotDMChannel,
        InteractionContextType.DMChannel,
        InteractionContextType.Guild
    ])]
    public async Task<string> VibLovenceDevice(User who)
    {
        var con = GetConnectionFor(who.Id);
        if (con == null)
        {
            return $"`Error:` No Connection!, {who} should run /start_session";
        }

        con.BuzzAllConnectedDevices();

        return "*smirks*";
    }


    [SlashCommand("lovense_list_toys", "mmmmpphh.", Contexts =
    [
        InteractionContextType.BotDMChannel,
        InteractionContextType.DMChannel,
        InteractionContextType.Guild
    ])]
    public string ListLovenseToys(User who)
    {
        var con = GetConnectionFor(who.Id);
        if (con == null)
        {
            return $"`Error:` No Connection!, {who} should run /start_session";
        }

        var toys = con.ListToyNames();

        var msg = "Toys:\n";
        foreach (var t in toys)
        {
            msg += t + "\n";
        }

        return msg;
    }
}


/*































 */