namespace HypnoBot;

public record PiShockCred(string Username, string ApiKey, string ShareCode);

// internal class Program
// {
//     public static async Task Main(string[] args)
//     {
//         var client = new HttpClient();
//         var requestUrl = "https://do.pishock.com/api/apioperate/";
//         var requestBody = new
//         {
//             Username = "hypnobot457",
//             Name = "droppy-script",
//             Code = "6F62E1FF21E",
//             Apikey = "685da444-8e19-4f98-a3d8-9d51d498a7b1",
//             Intensity = "10",
//             Duration = "1",
//             Op = "1" // 0 = zap, 1 = vibrate, 2 = beep
//         };
//
//         var requestJson = System.Text.Json.JsonSerializer.Serialize(requestBody);
//         var content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
//         var response = await client.PostAsync(requestUrl, content);
//
//         var responseContent = await response.Content.ReadAsStringAsync();
//         Console.WriteLine($"Response Status Code: {response.StatusCode}");
//         Console.WriteLine("Response:");
//         Console.WriteLine(responseContent);
//     }
// }