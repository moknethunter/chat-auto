using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var verifyToken = Environment.GetEnvironmentVariable("VERIFY_TOKEN") ?? "Mokhtar123";
var waToken = Environment.GetEnvironmentVariable("WA_TOKEN") ?? "";
var phoneId = Environment.GetEnvironmentVariable("WA_PHONE_ID") ?? "";

app.MapGet("/", () => "Mokhtar Chat Auto - Running ✅");

app.MapGet("/webhook", (HttpRequest req) =>
{
    var mode = req.Query["hub.mode"];
    var token = req.Query["hub.verify_token"];
    var challenge = req.Query["hub.challenge"];
    Console.WriteLine($"Verify: {mode} {token}");
    if (mode == "subscribe" && token == verifyToken)
    {
        return Results.Text(challenge.ToString(), "text/plain");
    }
    return Results.Unauthorized();
});

app.MapPost("/webhook", async (HttpRequest req) =>
{
    try
    {
        using var reader = new StreamReader(req.Body);
        var body = await reader.ReadToEndAsync();
        Console.WriteLine(body);

        if (string.IsNullOrWhiteSpace(body) || string.IsNullOrWhiteSpace(waToken) || string.IsNullOrWhiteSpace(phoneId))
            return Results.Ok();

        var doc = JsonDocument.Parse(body);
        var entry = doc.RootElement.GetProperty("entry")[0];
        var changes = entry.GetProperty("changes")[0];
        var value = changes.GetProperty("value");
        
        if (!value.TryGetProperty("messages", out var messages)) 
            return Results.Ok();

        var from = messages[0].GetProperty("from").GetString();
        var text = messages[0].GetProperty("text").GetProperty("body").GetString();

        Console.WriteLine($"Message from {from}: {text}");

        // رد آلي
        var replyText = $"أهلاً! وصلتني رسالتك: {text} \nالمنصة شغالة آلياً مع مختار ✅";

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {waToken}");
        var url = $"https://graph.facebook.com/v20.0/{phoneId}/messages";
        
        var payload = new
        {
            messaging_product = "whatsapp",
            to = from,
            type = "text",
            text = new { body = replyText }
        };

        var res = await client.PostAsJsonAsync(url, payload);
        var resBody = await res.Content.ReadAsStringAsync();
        Console.WriteLine(resBody);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    return Results.Ok();
});

app.MapGet("/health", () => Results.Ok("ok"));

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://+:{port}");
