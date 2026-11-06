using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Logging.ClearProviders();
builder.Services.AddControllers();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();
var app = builder.Build();
var german = new CultureInfo("de-DE");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(german),
    SupportedCultures = [german],
    SupportedUICultures = [german],
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await app.StartAsync();
try
{
    using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri(app.Urls.First()) };
    Console.WriteLine("core-frombody-binds-json-name: " + await Json(client, "/body", "{\"given\":\"Ann\"}"));
    Console.WriteLine("core-frombody-ignores-clr-name: " + await Json(client, "/body", "{\"GivenName\":\"Ann\"}"));
    Console.WriteLine("core-form-binds-clr-name: " + await Form(client, "/form", "GivenName=Ann"));
    Console.WriteLine("core-form-ignores-json-name: " + await Form(client, "/form", "given=Ann"));
    Console.WriteLine("core-form-binds-indexed-name: " + await Form(client, "/roster", "Members%5B1%5D.Name=B&Members%5B0%5D.Name=A"));
    Console.WriteLine("core-form-decimal-culture: 1,5 binds " + await Form(client, "/amount", "Value=1%2C5") + "; 1.5 binds " + await Form(client, "/amount", "Value=1.5"));
    Console.WriteLine("core-query-decimal-invariant: 1.5 binds " + await client.GetStringAsync("/amount?Value=1.5") + "; 1,5 binds " + await client.GetStringAsync("/amount?Value=1%2C5"));
    Console.WriteLine("core-json-result-names: " + await client.GetStringAsync("/person"));
    Console.WriteLine("core-cookie-auth-ajax: without the header " + await Status(client, false) + "; with X-Requested-With " + await Status(client, true));
    using (var problem = await client.PostAsync("/problem", new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded")))
    {
        Console.WriteLine("core-validation-problem-shape: " + (int)problem.StatusCode + " " + problem.Content.Headers.ContentType?.MediaType + " errors " + ErrorsOf(await problem.Content.ReadAsStringAsync()));
    }
    Console.WriteLine("core-form-empty-field: Count= binds " + await Form(client, "/counts", "Count=&Maybe=") + "; nothing sent binds " + await Form(client, "/counts", ""));
    Console.WriteLine("core-json-null-member: {\"count\":null} binds " + await Json(client, "/counts-json", "{\"count\":null}") + "; {} binds " + await Json(client, "/counts-json", "{}"));
    using (var strict = await client.PostAsync("/strict-json", null))
    using (var optional = await client.PostAsync("/counts-json", null))
    {
        Console.WriteLine("core-frombody-no-body: a required body answers " + (int)strict.StatusCode + "; a nullable body answers " + (int)optional.StatusCode + " with " + await optional.Content.ReadAsStringAsync());
    }
}
finally
{
    await app.StopAsync();
}

static async Task<string> Json(HttpClient client, string path, string body)
{
    using var response = await client.PostAsync(path, new StringContent(body, Encoding.UTF8, "application/json"));
    return await response.Content.ReadAsStringAsync();
}

static async Task<string> Status(HttpClient client, bool ajax)
{
    using var request = new HttpRequestMessage(HttpMethod.Get, "/secure");
    if (ajax) request.Headers.Add("X-Requested-With", "XMLHttpRequest");
    using var response = await client.SendAsync(request);
    return ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);
}

static string ErrorsOf(string body)
{
    using var document = System.Text.Json.JsonDocument.Parse(body);
    return document.RootElement.GetProperty("errors").GetRawText();
}

static async Task<string> Form(HttpClient client, string path, string body)
{
    using var response = await client.PostAsync(path, new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"));
    return await response.Content.ReadAsStringAsync();
}

public sealed class Person
{
    [JsonPropertyName("given")]
    public string? GivenName { get; set; }

    public string? FamilyName { get; set; }
}

public sealed class Member
{
    public string? Name { get; set; }
}

public sealed class Roster
{
    public List<Member> Members { get; set; } = [];
}

public sealed class Amount
{
    public decimal Value { get; set; }
}

public sealed class Counts
{
    public int Count { get; set; } = 5;

    public int? Maybe { get; set; } = 7;
}

public sealed class ProbeController : ControllerBase
{
    [HttpPost("/body")]
    public string Body([FromBody] Person person) => person.GivenName ?? "<null>";

    [HttpPost("/form")]
    public string Form([FromForm] Person person) => person.GivenName ?? "<null>";

    [HttpPost("/roster")]
    public string Roster([FromForm] Roster roster) => string.Join(",", roster.Members.Select(member => member.Name));

    [HttpPost("/amount")]
    public string FormAmount([FromForm] Amount amount) => Bound(amount.Value);

    [HttpGet("/amount")]
    public string QueryAmount([FromQuery] Amount amount) => Bound(amount.Value);

    [HttpGet("/person")]
    public Person Person() => new() { GivenName = "Ann", FamilyName = "Lee" };

    [Authorize]
    [HttpGet("/secure")]
    public string Secure() => "secret";

    [HttpPost("/problem")]
    public IActionResult Problem()
    {
        ModelState.AddModelError("Kin.Name", "Name the next of kin.");
        return ValidationProblem(ModelState);
    }

    [HttpPost("/counts")]
    public string FormCounts([FromForm] Counts counts) => CountsText(counts);

    [HttpPost("/counts-json")]
    public string JsonCounts([FromBody] Counts? counts) => CountsText(counts);

    [HttpPost("/strict-json")]
    public string StrictCounts([FromBody] Counts counts) => CountsText(counts);

    private string CountsText(Counts? counts)
        => counts == null ? "refused, no instance" : (ModelState.IsValid ? "valid" : "refused") + " Count=" + counts.Count.ToString(CultureInfo.InvariantCulture) + " Maybe=" + (counts.Maybe?.ToString(CultureInfo.InvariantCulture) ?? "null");

    private string Bound(decimal value) => ModelState.IsValid ? value.ToString(CultureInfo.InvariantCulture) : "refused";
}
