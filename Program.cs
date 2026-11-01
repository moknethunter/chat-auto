var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string verifyToken = Environment.GetEnvironmentVariable("VERIFY_TOKEN") ?? "Mokhtar123";
string waToken = Environment.GetEnvironmentVariable("WA_TOKEN") ?? "";
string phoneId = Environment.GetEnvironmentVariable("WA_PHONE_ID") ?? "";
string defaultNumber = Environment.GetEnvironmentVariable("DEFAULT_NUMBER") ?? "";

app.MapGet("/", () => "Chat Platform Running - Mokhtar");

app.MapGet("/webhook", (HttpRequest req) => {
    var mode = req.Query["hub.mode"];
    var token = req.Query["hub.verify_token"];
    var challenge = req.Query["hub.challenge"];
    if (mode == "subscribe" && token == verifyToken) {
        return Results.Text(challenge, "text/plain");
    }
    return Results.Unauthorized();
});

app.MapPost("/webhook", async (HttpRequest req) => {
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    Console.WriteLine(body);
    // هنا يشتغل الآلي - ترد تلقائيا
    return Results.Ok();
});

app.MapGet("/send-test", async () => {
    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {waToken}");
    var url = $"https://graph.facebook.com/v20.0/{phoneId}/messages";
    var payload = new {
        messaging_product = "whatsapp",
        to = defaultNumber,
        type = "text",
        text = new { body = "مرحبا يا مختار! المنصة شغالة آليا ✅" }
    };
    var res = await client.PostAsJsonAsync(url, payload);
    var txt = await res.Content.ReadAsStringAsync();
    return Results.Text(txt);
});

app.Run($"http://+:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}");
