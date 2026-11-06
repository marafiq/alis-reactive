using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Logging.ClearProviders();
builder.Services.AddControllersWithViews();
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
    Console.WriteLine("core-form-duplicate-name: GivenName=Ann&GivenName=Bob binds " + await Form(client, "/form", "GivenName=Ann&GivenName=Bob"));
    Console.WriteLine("core-json-duplicate-name: {\"given\":\"Ann\",\"given\":\"Bob\"} binds " + await Json(client, "/body", "{\"given\":\"Ann\",\"given\":\"Bob\"}"));
    Console.WriteLine("core-sanitized-id: NextOfKin.Name is " + Microsoft.AspNetCore.Mvc.Rendering.TagBuilder.CreateSanitizedId("NextOfKin.Name", "_") + "; Contacts[0].Name is " + Microsoft.AspNetCore.Mvc.Rendering.TagBuilder.CreateSanitizedId("Contacts[0].Name", "_"));
    Console.WriteLine("core-template-prefix-name: " + new Microsoft.AspNetCore.Mvc.ViewFeatures.TemplateInfo { HtmlFieldPrefix = "NextOfKin" }.GetFullHtmlFieldName("Name"));
    Console.WriteLine("core-form-binds-indexed-name: " + await Form(client, "/roster", "Members%5B1%5D.Name=B&Members%5B0%5D.Name=A"));
    Console.WriteLine("core-form-decimal-culture: 1,5 binds " + await Form(client, "/amount", "Value=1%2C5") + "; 1.5 binds " + await Form(client, "/amount", "Value=1.5"));
    Console.WriteLine("core-form-decimal-group: 1.234,5 binds " + await Form(client, "/amount", "Value=1.234%2C5") + "; 1,5e3 binds " + await Form(client, "/amount", "Value=1%2C5e3") + "; ' 1,5 ' binds " + await Form(client, "/amount", "Value=%201%2C5%20"));
    Console.WriteLine("core-form-int-culture: Count=1.234 binds " + await Form(client, "/counts", "Count=1.234") + "; Count=1,0 binds " + await Form(client, "/counts", "Count=1%2C0"));
    Console.WriteLine("core-form-date-kind: 2026-11-06T10:00:00 binds " + await Form(client, "/when", "When=2026-11-06T10%3A00%3A00") + "; 2026-11-06 binds " + await Form(client, "/when", "When=2026-11-06"));
    Console.WriteLine("core-number-text-culture: decimal 1234.5 shows " + 1234.5m.ToString(german) + "; double 0.1+0.2 shows " + (0.1 + 0.2).ToString(german) + "; double 1e21 shows " + 1e21.ToString(german) + "; double 1e15 shows " + 1e15.ToString(german));
    Console.WriteLine("core-number-text-exponent: 1e16 " + 1e16.ToString(german) + "; 1e20 " + 1e20.ToString(german) + "; 1e17 " + 1e17.ToString(german) + "; 1e-4 " + 1e-4.ToString(german) + "; 1e-5 " + 1e-5.ToString(german) + "; 123456.5e12 " + 123456.5e12.ToString(german) + "; 1e-7 " + 1e-7.ToString(german));
    Console.WriteLine("core-windows-zone-to-iana: Eastern Standard Time converts " + (TimeZoneInfo.TryConvertWindowsIdToIanaId("Eastern Standard Time", out var iana) ? iana : "to nothing") + "; India Standard Time converts " + (TimeZoneInfo.TryConvertWindowsIdToIanaId("India Standard Time", out var india) ? india : "to nothing"));
    var page = await client.GetStringAsync("/page");
    Console.WriteLine("core-partial-section-dropped: the card renders " + (page.Contains("card body") ? "its body" : "nothing") + " and " + (page.Contains("from-partial-section") ? "its section" : "not its section"));
    var original = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary());
    original.TemplateInfo.HtmlFieldPrefix = "Page";
    var copy = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(original);
    copy.TemplateInfo.HtmlFieldPrefix = "Card";
    Console.WriteLine("core-viewdata-copy-template-info: after the copy takes Card, the original's prefix is " + original.TemplateInfo.HtmlFieldPrefix);
    Console.WriteLine("core-stj-date-forms: DateTime " + System.Text.Json.JsonSerializer.Serialize(new DateTime(1990, 5, 1)) + "; DateTimeOffset " + System.Text.Json.JsonSerializer.Serialize(new DateTimeOffset(2026, 11, 6, 10, 0, 0, TimeSpan.FromHours(-8))) + "; UTC " + System.Text.Json.JsonSerializer.Serialize(new DateTime(2026, 11, 6, 18, 0, 0, DateTimeKind.Utc)));
    Console.WriteLine("core-stj-offset-into-datetime: reads as kind " + System.Text.Json.JsonSerializer.Deserialize<DateTime>("\"2026-11-06T10:00:00-08:00\"").Kind);
    Console.WriteLine("core-query-decimal-invariant: 1.5 binds " + await client.GetStringAsync("/amount?Value=1.5") + "; 1,5 binds " + await client.GetStringAsync("/amount?Value=1%2C5"));
    Console.WriteLine("core-json-result-names: " + await client.GetStringAsync("/person"));
    Console.WriteLine("core-cookie-auth-ajax: without the header " + await Status(client, false) + "; with X-Requested-With " + await Status(client, true));
    using (var problem = await client.PostAsync("/problem", new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded")))
    {
        Console.WriteLine("core-validation-problem-shape: " + (int)problem.StatusCode + " " + problem.Content.Headers.ContentType?.MediaType + " errors " + ErrorsOf(await problem.Content.ReadAsStringAsync()));
    }
    Console.WriteLine("core-form-nested-start: " + await Form(client, "/ward", "Lead.Phone=1&Deputy.Phone=2&Leads%5B0%5D.Phone=3"));
    Console.WriteLine("core-json-nested-start: " + await Json(client, "/ward-json", "{\"lead\":{\"phone\":\"1\"},\"deputy\":{\"phone\":\"2\"},\"leads\":[{\"phone\":\"3\"}]}"));
    Console.WriteLine("core-form-unsent-keeps-start: " + await Form(client, "/ward", "") + " | json {} " + await Json(client, "/ward-json", "{}"));
    Console.WriteLine("core-form-value-messages: Count=12a says " + await Form(client, "/counts-message", "Count=12a") + "; Count= says " + await Form(client, "/counts-message", "Count="));
    Console.WriteLine("core-regex-dotnet: \\d on Arabic-Indic digits " + Regex.IsMatch("\u0661\u0662\u0663", @"^\d+$") + "; $ before a final newline " + Regex.IsMatch("123\n", @"^\d+$") + "; . on a carriage return " + Regex.IsMatch("a\rb", "^a.b$") + "; \\w on e-acute " + Regex.IsMatch("\u00e9", @"^\w$") + "; \\d under ECMAScript " + Regex.IsMatch("\u0661\u0662\u0663", @"^\d+$", RegexOptions.ECMAScript));
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

public sealed class Visit
{
    public DateTime When { get; set; }
}

public sealed class Counts
{
    public int Count { get; set; } = 5;

    public int? Maybe { get; set; } = 7;
}

public sealed class Lead
{
    public string? Name { get; set; } = "inner";

    public string? Phone { get; set; }
}

public sealed class Ward
{
    public Lead Lead { get; set; } = new() { Name = "outer" };

    public Lead? Deputy { get; set; }

    public List<Lead> Leads { get; set; } = [new() { Name = "seed" }];
}

public sealed class PageController : Controller
{
    [HttpGet("/page")]
    public IActionResult Page() => View("~/Views/Probe/Page.cshtml");
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

    [HttpPost("/when")]
    public string FormWhen([FromForm] Visit visit) => ModelState.IsValid ? visit.When.Kind + " " + visit.When.ToString("s", CultureInfo.InvariantCulture) : "refused";

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

    [HttpPost("/ward")]
    public string FormWard([FromForm] Ward ward) => WardText(ward);

    [HttpPost("/ward-json")]
    public string JsonWard([FromBody] Ward ward) => WardText(ward);

    [HttpPost("/counts-message")]
    public string CountsMessage([FromForm] Counts counts)
        => string.Join(" | ", ModelState.Where(entry => entry.Value!.Errors.Count > 0).Select(entry => entry.Key + ": " + entry.Value!.Errors[0].ErrorMessage));

    private static string WardText(Ward ward)
        => "Lead.Name=" + ward.Lead.Name + "; Deputy.Name=" + (ward.Deputy?.Name ?? "null") + "; Leads=" + string.Join(",", ward.Leads.Select(lead => lead.Name));

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
