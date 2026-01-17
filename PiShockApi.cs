using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HypnoBot;

public static class PiShockApi
{
    public static async Task<HttpResponseMessage> SendPiShockStim(
        StimKind kind, int power, int duration, PiShockCred cred
    )
    {
        var client = new HttpClient();
        var requestUrl = "https://do.pishock.com/api/apioperate/";

        var requestBody = new
        {
            Username = cred.Username,
            Name = "HypnoBot",
            Code = cred.ShareCode,
            Apikey = cred.ApiKey,
            Intensity = power,
            Duration = duration,
            Op = kind switch
            {
                StimKind.Zap => "0",
                StimKind.Buzz => "1",
                StimKind.Beep => "2",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            }
        };

        var requestJson = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
        return await client.PostAsync(requestUrl, content);

        // var response = await client.PostAsync(requestUrl, content);
        // var responseContent = await response.Content.ReadAsStringAsync();
        // Console.WriteLine($"Response Status Code: {response.StatusCode}");
        // Console.WriteLine("Response:");
        // Console.WriteLine(responseContent);
    }
}