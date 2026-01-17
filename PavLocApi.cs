using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HypnoBot;

public static class PavLocApi
{
    public static async Task<HttpResponseMessage> SendPavStim(
        StimKind kind, int power, string? why, string token
    )
    {
        if (why == null) why = "No Reason Provided";
        if (0 > power) power = 0;
        if (power > 100) power = 100;

        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.pavlok.com/api/v5/stimulus/send"),
            Headers =
            {
                { "accept", "application/json" }, { "Authorization", token },
            },
            Content = new StringContent(
                $$$"""{"stimulus":{"stimulusType":"{{{kind.ToApiString()}}}","stimulusValue":{{{power}}},"reason":"{{{why}}}"}}""")
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            }
        };

        return await client.SendAsync(request);
    }
}