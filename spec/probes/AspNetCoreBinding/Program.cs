using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Logging.ClearProviders();
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();
builder.Services.AddCors(cors => cors.AddDefaultPolicy(policy => policy.WithOrigins("http://elsewhere.test").AllowAnyMethod()));
var app = builder.Build();
var german = new CultureInfo("de-DE");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(german),
    SupportedCultures = [german],
    SupportedUICultures = [german],
});
app.UseCors();
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
    Console.WriteLine("core-form-implicit-required: Name= says " + await Form(client, "/named", "Name=") + "; nothing sent says " + await Form(client, "/named", ""));
    var web = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
    Console.WriteLine("core-stj-empty-object-start: a Lead read from {} writes " + System.Text.Json.JsonSerializer.Serialize(System.Text.Json.JsonSerializer.Deserialize<Lead>("{}", web), web));
    Console.WriteLine("core-regex-dotnet: \\d on Arabic-Indic digits " + Regex.IsMatch("\u0661\u0662\u0663", @"^\d+$") + "; $ before a final newline " + Regex.IsMatch("123\n", @"^\d+$") + "; . on a carriage return " + Regex.IsMatch("a\rb", "^a.b$") + "; \\w on e-acute " + Regex.IsMatch("\u00e9", @"^\w$") + "; \\d under ECMAScript " + Regex.IsMatch("\u0661\u0662\u0663", @"^\d+$", RegexOptions.ECMAScript));
    Console.WriteLine("core-regex-space-and-case: \\s on NEL " + Regex.IsMatch("\u0085", @"^\s$") + "; \\s on BOM " + Regex.IsMatch("\ufeff", @"^\s$") + "; IgnoreCase I against i under de-DE " + Regex.IsMatch("I", "^i$", RegexOptions.IgnoreCase) + "; under tr-TR " + Turkish(() => Regex.IsMatch("I", "^i$", RegexOptions.IgnoreCase)) + "; under tr-TR invariant " + Turkish(() => Regex.IsMatch("I", "^i$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) + "; Kelvin sign against k invariant " + Regex.IsMatch("\u212A", "^k$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
    Console.WriteLine("core-uri-absolute: " + string.Join("; ", new[] { "http://a.com/b c", "http://a b.com", "http://", "http://a", "https://exa_mple.com", "http://a.com:99999", "HTTP://A.COM" }.Select(text => text + " " + (Uri.TryCreate(text, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))));
    Console.WriteLine("core-route-segment-decoding: a%2Fb%20c%2Bd%3F%C3%A9 reaches the action as " + await client.GetStringAsync("/route/a%2Fb%20c%2Bd%3F%C3%A9"));
    Console.WriteLine("core-query-decoding: q=a+b%2Bc%26d binds " + await client.GetStringAsync("/query?q=a+b%2Bc%26d"));
    var populate = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web) { PreferredObjectCreationHandling = System.Text.Json.Serialization.JsonObjectCreationHandling.Populate };
    var populated = System.Text.Json.JsonSerializer.Deserialize<Ward>("{\"lead\":{\"phone\":\"1\"},\"leads\":[{\"phone\":\"3\"}]}", populate)!;
    Console.WriteLine("core-stj-populate-start: under Populate Lead.Name=" + populated.Lead.Name + "; Leads=" + string.Join(",", populated.Leads.Select(lead => lead.Name)));
    using (var api = await client.PostAsync("/api-named", new StringContent("Name=", Encoding.UTF8, "application/x-www-form-urlencoded")))
    {
        Console.WriteLine("core-api-controller-implicit-required: Name= answers " + (int)api.StatusCode + " " + api.Content.Headers.ContentType?.MediaType + (await api.Content.ReadAsStringAsync()).Contains("reached the action") switch { true => " from the action", false => " before the action" });
    }
#line 42 "Views/Residents/Intake.cshtml"
    var builderCall = CallSite();
#line default
    Console.WriteLine("core-caller-line-follows-line-directive: a call under #line 42 of Intake.cshtml reads " + builderCall);
    Console.WriteLine("core-form-empty-field: Count= binds " + await Form(client, "/counts", "Count=&Maybe=") + "; nothing sent binds " + await Form(client, "/counts", ""));
    Console.WriteLine("core-json-null-member: {\"count\":null} binds " + await Json(client, "/counts-json", "{\"count\":null}") + "; {} binds " + await Json(client, "/counts-json", "{}"));
    using (var strict = await client.PostAsync("/strict-json", null))
    using (var optional = await client.PostAsync("/counts-json", null))
    {
        Console.WriteLine("core-frombody-no-body: a required body answers " + (int)strict.StatusCode + "; a nullable body answers " + (int)optional.StatusCode + " with " + await optional.Content.ReadAsStringAsync());
    }
    Console.WriteLine("core-query-object-fields: Handover.Round=3&Handover.Lead=Ann&Handover.Named=Bo&Handover.Beds=1&Handover.Beds=2 binds " + await client.GetStringAsync("/q/handover?Handover.Round=3&Handover.Lead=Ann&Handover.Named=Bo&Handover.Beds=1&Handover.Beds=2"));
    Console.WriteLine("core-query-object-json-name: Handover.leader=Bo binds " + await client.GetStringAsync("/q/handover?Handover.leader=Bo") + "; Handover.Round=3&Handover.leader=Bo binds " + await client.GetStringAsync("/q/handover?Handover.Round=3&Handover.leader=Bo"));
    Console.WriteLine("core-query-object-empty-field: Handover= binds " + await client.GetStringAsync("/q/handover?Handover=") + " | Kept= binds " + await client.GetStringAsync("/q/handover?Kept=") + " | Handover.Lead= binds " + await client.GetStringAsync("/q/handover?Handover.Lead=") + " | nothing sent binds " + await client.GetStringAsync("/q/handover"));
    Console.WriteLine("core-query-list-of-objects: Lines[0].Name=A&Lines[1].Name=B&Lines[1].Beds=1&Lines[1].Beds=2 binds " + await client.GetStringAsync("/q/lines?Lines%5B0%5D.Name=A&Lines%5B1%5D.Name=B&Lines%5B1%5D.Beds=1&Lines%5B1%5D.Beds=2"));
    Console.WriteLine("core-query-list-of-objects-gap: Lines[0].Name=A&Lines[2].Name=C binds " + await client.GetStringAsync("/q/lines?Lines%5B0%5D.Name=A&Lines%5B2%5D.Name=C") + " | Lines[1].Name=B binds " + await client.GetStringAsync("/q/lines?Lines%5B1%5D.Name=B"));
    Console.WriteLine("core-query-list-of-objects-empty-item: Lines[0].Name=A&Lines[1]=&Lines[2].Name=C binds " + await client.GetStringAsync("/q/lines?Lines%5B0%5D.Name=A&Lines%5B1%5D=&Lines%5B2%5D.Name=C"));
    Console.WriteLine("core-query-read-only-lists: Held[0].Name=A&Bare=1&Bare=2&Handover.Beds=3 binds " + await client.GetStringAsync("/q/read-only?Held%5B0%5D.Name=A&Bare=1&Bare=2&Handover.Beds=3") + " | nothing sent binds " + await client.GetStringAsync("/q/read-only"));
    var named = new System.Text.Json.JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
    Console.WriteLine("core-query-enum-member-name: under a string converter System.Text.Json writes Night as " + System.Text.Json.JsonSerializer.Serialize(ProbeShift.Night, named) + "; Shift=Night binds " + await client.GetStringAsync("/q/shift?Shift=Night") + "; Shift=night-shift binds " + await client.GetStringAsync("/q/shift?Shift=night-shift") + "; Shift=night binds " + await client.GetStringAsync("/q/shift?Shift=night") + "; Shift=1 binds " + await client.GetStringAsync("/q/shift?Shift=1") + "; Shift=5 binds " + await client.GetStringAsync("/q/shift?Shift=5"));
    Console.WriteLine("core-query-nullable-enum-items: Shifts=Day&Shifts=&Shifts=Night binds " + await client.GetStringAsync("/q/shift?Shifts=Day&Shifts=&Shifts=Night") + "; Shifts=Day&Shifts=night-shift binds " + await client.GetStringAsync("/q/shift?Shifts=Day&Shifts=night-shift"));
    Console.WriteLine("core-query-flags-enum: System.Text.Json writes Mon|Tue as " + System.Text.Json.JsonSerializer.Serialize(ProbeDays.Mon | ProbeDays.Tue, named) + " under a string converter and " + System.Text.Json.JsonSerializer.Serialize(ProbeDays.Mon | ProbeDays.Tue) + " without; Days=Mon%2C%20Tue binds " + await client.GetStringAsync("/q/days?Days=Mon%2C%20Tue") + "; Days=Mon&Days=Tue binds " + await client.GetStringAsync("/q/days?Days=Mon&Days=Tue") + "; Days=3 binds " + await client.GetStringAsync("/q/days?Days=3"));
    Console.WriteLine("core-query-member-names: Ward=W&Bed=B&Room=R&Never=N binds " + await client.GetStringAsync("/q/renamed?Ward=W&Bed=B&Room=R&Never=N") + " | w=W&b=B&r=R binds " + await client.GetStringAsync("/q/renamed?w=W&b=B&r=R"));
    Console.WriteLine("core-query-bind-prefix: [Bind(Prefix = \"q\")] with Count=3 binds " + await client.GetStringAsync("/q/bind-prefix?Count=3") + "; with q.Count=3 binds " + await client.GetStringAsync("/q/bind-prefix?q.Count=3"));
    Console.WriteLine("core-query-parameter-name: [FromQuery(Name = \"q\")] with Count=3 binds " + await client.GetStringAsync("/q/query-name?Count=3") + "; with q.Count=3 binds " + await client.GetStringAsync("/q/query-name?q.Count=3"));
    Console.WriteLine("core-query-parameter-prefix: Count=3 binds " + await client.GetStringAsync("/q/plain?Count=3") + "; counts.Count=3 binds " + await client.GetStringAsync("/q/plain?counts.Count=3") + "; counts.Count=3&Count=4 binds " + await client.GetStringAsync("/q/plain?counts.Count=3&Count=4"));
    Console.WriteLine("core-query-parameter-name-collision: Collide(ProbeSearch term) with Term=a&Count=3 binds " + await client.GetStringAsync("/q/collide?Term=a&Count=3") + "; with term.Term=a&term.Count=3 binds " + await client.GetStringAsync("/q/collide?term.Term=a&term.Count=3"));
    Console.WriteLine("core-frombody-enum-forms: under the app's default options {\"shift\":1,\"watch\":\"night-shift\"} binds " + await Json(client, "/q/enum-json", "{\"shift\":1,\"watch\":\"night-shift\"}") + " | {\"shift\":\"Night\"} binds " + await Json(client, "/q/enum-json", "{\"shift\":\"Night\"}") + " | {\"watch\":\"Night\"} binds " + await Json(client, "/q/enum-json", "{\"watch\":\"Night\"}"));
    var webContract = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web) { TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver() };
    var own = webContract.GetTypeInfo(typeof(ProbeDose)).Properties.Single(property => property.Name == "maybe").CustomConverter!;
    var underOwn = new System.Text.Json.JsonSerializerOptions(webContract) { Converters = { own } };
    Console.WriteLine("core-json-enum-nullable-member-converter: a ProbeShift? member's own JsonStringEnumConverter is " + own.GetType().Name + "; under it Night writes " + System.Text.Json.JsonSerializer.Serialize<ProbeShift?>(ProbeShift.Night, underOwn) + " at ProbeShift? and " + System.Text.Json.JsonSerializer.Serialize(ProbeShift.Night, underOwn) + " at ProbeShift");
    var camelNames = new System.Text.Json.JsonSerializerOptions { Converters = { new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) } };
    Console.WriteLine("core-json-enum-held-alike: under a camel-case string converter ABC writes " + System.Text.Json.JsonSerializer.Serialize(ProbeClash.ABC, camelNames) + " and Abc writes " + System.Text.Json.JsonSerializer.Serialize(ProbeClash.Abc, camelNames));
    Console.WriteLine("core-json-null-object: {\"kept\":null} binds " + await Json(client, "/q/handover-json", "{\"kept\":null}") + " | {} binds " + await Json(client, "/q/handover-json", "{}"));
    var decimalsAsText = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web) { NumberHandling = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString };
    var written = DecimalSweep().Select(value => System.Text.Json.JsonSerializer.Serialize(value, decimalsAsText).Trim('"')).ToList();
    Console.WriteLine("core-json-decimal-text-written: " + written.Count(ReadsAsDecimalText) + " of " + written.Count + " decimals at scales 0 to 28 write as text an optional minus, digits and at most 28 places within decimal.MaxValue's digits");
    var pastPlaces = System.Text.Json.JsonSerializer.Deserialize<decimal>("\"0.10000000000000000000000000001\"", decimalsAsText);
    Console.WriteLine("core-json-decimal-text-rounded: 0.10000000000000000000000000001 as text reads " + pastPlaces.ToString(CultureInfo.InvariantCulture) + ", equal to 0.1 " + (pastPlaces == 0.1m) + "; 79228162514264337593543950336 as text reads " + DecimalTextRead("\"79228162514264337593543950336\"", decimalsAsText));
    var namedLiterals = new System.Text.Json.JsonSerializerOptions { NumberHandling = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowNamedFloatingPointLiterals };
    var noNumber = double.NaN;
    Console.WriteLine("core-json-double-text: -0.0 writes " + System.Text.Json.JsonSerializer.Serialize(-0.0, namedLiterals) + " and equals 0.0 " + (-0.0 == 0.0) + "; NaN writes " + System.Text.Json.JsonSerializer.Serialize(noNumber, namedLiterals) + " and equals NaN " + (noNumber == double.NaN));
    var flagNumbers = new System.Text.Json.JsonSerializerOptions();
    var flagNames = new System.Text.Json.JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
    var flagCamel = new System.Text.Json.JsonSerializerOptions { Converters = { new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) } };
    var monSatSun = ProbeCare.Mon | ProbeCare.Sat | ProbeCare.Sun;
    var strayBit = ProbeCare.Mon | (ProbeCare)128;
    Console.WriteLine("core-json-flags-forms: Mon|Sat|Sun writes " + System.Text.Json.JsonSerializer.Serialize(monSatSun, flagNumbers) + " under the app's numbers, " + System.Text.Json.JsonSerializer.Serialize(monSatSun, flagNames) + " under a string converter and " + System.Text.Json.JsonSerializer.Serialize(monSatSun, flagCamel) + " in camel case; Mon|Tue writes " + System.Text.Json.JsonSerializer.Serialize(ProbeMondays.Mon | ProbeMondays.Tue, flagNames) + " where JsonStringEnumMemberName names Mon monday; Mon|128, a bit no value names, writes " + System.Text.Json.JsonSerializer.Serialize(strayBit, flagNumbers) + ", " + System.Text.Json.JsonSerializer.Serialize(strayBit, flagNames) + " and " + System.Text.Json.JsonSerializer.Serialize(strayBit, flagCamel) + "; 0 of an enum with no value for zero writes " + System.Text.Json.JsonSerializer.Serialize((ProbeRota)0, flagNumbers) + ", " + System.Text.Json.JsonSerializer.Serialize((ProbeRota)0, flagNames) + " and " + System.Text.Json.JsonSerializer.Serialize((ProbeRota)0, flagCamel));
    string commaName;
    try
    {
        commaName = "writes " + System.Text.Json.JsonSerializer.Serialize(ProbeCommaName.Plain, flagNames);
    }
    catch (InvalidOperationException refused)
    {
        commaName = "throws " + refused.GetType().Name;
    }
    Console.WriteLine("core-json-flags-comma-name: under a string converter Plain of a flags enum whose other member is named \"a, b\" " + commaName);
    Console.WriteLine("core-query-flags-closure: Care=Mon%2C%20Weekend binds " + await client.GetStringAsync("/f/care?Care=Mon%2C%20Weekend") + "; Care=Weekend%2C%20Mon binds " + await client.GetStringAsync("/f/care?Care=Weekend%2C%20Mon") + "; Care=monday%2C%20Tue binds " + await client.GetStringAsync("/f/care?Care=monday%2C%20Tue") + "; Care=128 binds " + await client.GetStringAsync("/f/care?Care=128") + "; Rota=0 binds " + await client.GetStringAsync("/f/rota?Rota=0") + "; Overlap=7 binds " + await client.GetStringAsync("/f/overlap?Overlap=7") + "; Overlap=X%2C%20Y%2C%20Z binds " + await client.GetStringAsync("/f/overlap?Overlap=X%2C%20Y%2C%20Z"));
    var flagRules = new InlineValidator<ProbeFlagged>();
    flagRules.RuleFor(flagged => flagged.Care).IsInEnum();
    flagRules.RuleFor(flagged => flagged.Rota).IsInEnum();
    flagRules.RuleFor(flagged => flagged.Overlap).IsInEnum();
    flagRules.RuleFor(flagged => flagged.Access).IsInEnum();
    flagRules.RuleFor(flagged => flagged.Composite).IsInEnum();
    flagRules.RuleFor(flagged => flagged.OneValue).IsInEnum();
    flagRules.RuleFor(flagged => flagged.Shared).IsInEnum();
    string FlagVerdict(ProbeFlagged flagged, string member) => flagRules.Validate(flagged).Errors.Any(error => error.PropertyName == member) ? "fails" : "passes";
    Console.WriteLine("fv-flags-is-in-enum: FluentValidation 12.1.1's IsInEnum over a [Flags] enum: Care 3 " + FlagVerdict(new ProbeFlagged { Care = (ProbeCare)3 }, "Care") + ", 97 " + FlagVerdict(new ProbeFlagged { Care = (ProbeCare)97 }, "Care") + ", 0 " + FlagVerdict(new ProbeFlagged { Care = 0 }, "Care") + ", 128 " + FlagVerdict(new ProbeFlagged { Care = (ProbeCare)128 }, "Care") + "; Rota 0 " + FlagVerdict(new ProbeFlagged { Rota = 0 }, "Rota") + " with " + flagRules.Validate(new ProbeFlagged { Rota = 0 }).Errors.Single(error => error.PropertyName == "Rota").ErrorMessage + "; Overlap 7 " + FlagVerdict(new ProbeFlagged { Overlap = (ProbeOverlap)7 }, "Overlap") + "; over an enum with no [Flags] whose values are 1 and 2, 3 " + FlagVerdict(new ProbeFlagged { OneValue = (ProbeOneValue)3 }, "OneValue"));
    async Task<string> FlagsAgree<TEnum>(string route, Func<long, ProbeFlagged> flagged, string member, long top) where TEnum : struct, Enum
    {
        var rows = Enum.GetValues<TEnum>().Select(value => (Number: Convert.ToInt64(value, CultureInfo.InvariantCulture), Name: value.ToString())).OrderByDescending(row => row.Number).ToList();
        for (long number = 0; number <= top; number++)
        {
            var rest = number;
            var taken = new List<string>();
            if (number == 0)
            {
                var zero = rows.Where(row => row.Number == 0).Select(row => row.Name).FirstOrDefault();
                if (zero is null) rest = -1;
                else taken.Add(zero);
            }
            foreach (var row in rows.Where(row => row.Number != 0 && rest > 0 && (rest & row.Number) == row.Number))
            {
                rest &= ~row.Number;
                taken.Add(row.Name);
            }
            taken.Reverse();
            var split = rest == 0 ? string.Join(", ", taken) : null;
            var validates = FlagVerdict(flagged(number), member) == "passes";
            var bindsNumber = !(await client.GetStringAsync(route + "=" + number.ToString(CultureInfo.InvariantCulture))).Contains("(errors", StringComparison.Ordinal);
            var bindsSplit = split is null || await client.GetStringAsync(route + "=" + Uri.EscapeDataString(split)) == split + " (" + number.ToString(CultureInfo.InvariantCulture) + ")";
            var text = Enum.ToObject(typeof(TEnum), number).ToString();
            var agrees = validates == bindsNumber && bindsNumber == (split is not null) && bindsSplit && text == (split ?? number.ToString(CultureInfo.InvariantCulture));
            if (!agrees) return typeof(TEnum).Name + " differs at " + number.ToString(CultureInfo.InvariantCulture) + ": FluentValidation " + (validates ? "passes" : "fails") + ", net10 " + (bindsNumber ? "binds" : "binds nothing") + ", the split " + (split ?? "leaves bits");
        }
        return typeof(TEnum).Name + " agrees on every value from 0 to " + top.ToString(CultureInfo.InvariantCulture);
    }
    Console.WriteLine("core-flags-split-agrees: FluentValidation's IsInEnum, net10's binding of the number and of the split's text, Enum.ToString and the split: " + await FlagsAgree<ProbeCare>("/f/care?Care", number => new ProbeFlagged { Care = (ProbeCare)number }, "Care", 255) + "; " + await FlagsAgree<ProbeRota>("/f/rota?Rota", number => new ProbeFlagged { Rota = (ProbeRota)number }, "Rota", 7) + "; " + await FlagsAgree<ProbeAccess>("/f/access?Access", number => new ProbeFlagged { Access = (ProbeAccess)number }, "Access", 31) + "; " + await FlagsAgree<ProbeComposite>("/f/composite?Composite", number => new ProbeFlagged { Composite = (ProbeComposite)number }, "Composite", 7) + "; " + await FlagsAgree<ProbeOverlap>("/f/overlap?Overlap", number => new ProbeFlagged { Overlap = (ProbeOverlap)number }, "Overlap", 15) + "; " + await FlagsAgree<ProbeShared>("/f/shared?Shared", number => new ProbeFlagged { Shared = (ProbeShared)number }, "Shared", 15));
    await HttpProbes.Run(new Uri(app.Urls.First()));
}
finally
{
    await app.StopAsync();
}

static string CallSite([System.Runtime.CompilerServices.CallerFilePath] string file = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
    => Path.GetFileName(file) + ":" + line;

static bool Turkish(Func<bool> test)
{
    var before = CultureInfo.CurrentCulture;
    CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
    try
    {
        return test();
    }
    finally
    {
        CultureInfo.CurrentCulture = before;
    }
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

static IEnumerable<decimal> DecimalSweep()
{
    var random = new Random(15);
    for (byte scale = 0; scale <= 28; scale++)
    {
        foreach (var negative in new[] { false, true })
        {
            yield return new decimal(0, 0, 0, negative, scale);
            yield return new decimal(1, 0, 0, negative, scale);
            yield return new decimal(-1, -1, -1, negative, scale);
            yield return new decimal(int.MaxValue, int.MaxValue, int.MaxValue, negative, scale);
            for (var n = 0; n < 20; n++) yield return new decimal(random.Next(), random.Next(), random.Next(), negative, scale);
        }
    }
}

static bool ReadsAsDecimalText(string text)
    => Regex.IsMatch(text, @"^-?[0-9]+(\.[0-9]{1,28})?$")
       && System.Numerics.BigInteger.Parse(text.TrimStart('-').Replace(".", "", StringComparison.Ordinal), CultureInfo.InvariantCulture) <= System.Numerics.BigInteger.Parse("79228162514264337593543950335", CultureInfo.InvariantCulture);

static string DecimalTextRead(string json, System.Text.Json.JsonSerializerOptions options)
{
    try
    {
        return System.Text.Json.JsonSerializer.Deserialize<decimal>(json, options).ToString(CultureInfo.InvariantCulture);
    }
    catch (System.Text.Json.JsonException)
    {
        return "refused";
    }
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

public sealed class Named
{
    public string Name { get; set; } = "start";
}

public sealed class Ward
{
    public Lead Lead { get; set; } = new() { Name = "outer" };

    public Lead? Deputy { get; set; }

    public List<Lead> Leads { get; set; } = [new() { Name = "seed" }];
}

public sealed class ProbeHandover
{
    public int Round { get; init; }

    public DateTimeOffset StartsAt { get; init; }

    public DateTimeOffset? EndsAt { get; init; }

    public IReadOnlyList<int> Beds { get; init; } = [];

    public string? Lead { get; init; }

    [JsonPropertyName("leader")]
    public string? Named { get; init; }

    public int? Target { get; init; }
}

public sealed class ProbeLine
{
    public string? Name { get; set; }

    public List<int> Beds { get; set; } = [];
}

public enum ProbeShift
{
    Day,
    [JsonStringEnumMemberName("night-shift")]
    Night,
}

public enum ProbeClash
{
    ABC,
    Abc,
}

public sealed class ProbeDose
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProbeShift? Maybe { get; set; }
}

public sealed class ProbeEnumBody
{
    public ProbeShift Shift { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProbeShift Watch { get; set; }
}

[Flags]
public enum ProbeCare
{
    None = 0,
    Mon = 1,
    Tue = 2,
    Sat = 32,
    Sun = 64,
    Weekend = Sat | Sun,
}

[Flags]
public enum ProbeRota
{
    Early = 1,
    Late = 2,
}

[Flags]
public enum ProbeOverlap
{
    X = 5,
    Y = 6,
    Z = 3,
}

[Flags]
public enum ProbeAccess
{
    None = 0,
    Read = 1,
    Write = 2,
    ReadWrite = Read | Write,
    Admin = 4,
    All = ReadWrite | Admin,
    Audit = 8,
}

[Flags]
public enum ProbeShared
{
    X = 1,
    Y = 3,
    Z = 6,
}

[Flags]
public enum ProbeComposite
{
    Both = 3,
}

public enum ProbeOneValue
{
    A = 1,
    B = 2,
}

[Flags]
public enum ProbeMondays
{
    [JsonStringEnumMemberName("monday")]
    Mon = 1,
    Tue = 2,
}

[Flags]
public enum ProbeCommaName
{
    [JsonStringEnumMemberName("a, b")]
    Both = 1,
    Plain = 2,
}

public sealed class ProbeFlagged
{
    public ProbeCare Care { get; set; }

    public ProbeRota Rota { get; set; } = ProbeRota.Early;

    public ProbeOverlap Overlap { get; set; } = ProbeOverlap.X;

    public ProbeAccess Access { get; set; }

    public ProbeComposite Composite { get; set; } = ProbeComposite.Both;

    public ProbeOneValue OneValue { get; set; } = ProbeOneValue.A;

    public ProbeShared Shared { get; set; } = ProbeShared.X;
}

public sealed class FlagsProbeController : ControllerBase
{
    [HttpGet("/f/care")]
    public string Care(ProbeFlagged query) => Bound(query.Care);

    [HttpGet("/f/rota")]
    public string Rota(ProbeFlagged query) => Bound(query.Rota);

    [HttpGet("/f/overlap")]
    public string Overlap(ProbeFlagged query) => Bound(query.Overlap);

    [HttpGet("/f/access")]
    public string Access(ProbeFlagged query) => Bound(query.Access);

    [HttpGet("/f/shared")]
    public string Shared(ProbeFlagged query) => Bound(query.Shared);

    [HttpGet("/f/composite")]
    public string Composite(ProbeFlagged query) => Bound(query.Composite);

    private string Bound<TEnum>(TEnum value) where TEnum : struct, Enum
        => value + " (" + Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) + ")" + (ModelState.IsValid ? "" : " (errors " + string.Join(",", ModelState.Where(entry => entry.Value!.Errors.Count > 0).Select(entry => entry.Key)) + ")");
}

[Flags]
public enum ProbeDays
{
    None = 0,
    Mon = 1,
    Tue = 2,
}

public sealed class HandoverQuery
{
    public int Round { get; set; }

    public ProbeHandover? Handover { get; set; }

    public ProbeHandover Kept { get; set; } = new() { Lead = "start" };

    public List<ProbeLine> Lines { get; set; } = [];

    public IReadOnlyList<ProbeLine> Held { get; init; } = [];

    public IReadOnlyList<int>? Bare { get; init; }

    public ProbeShift Shift { get; set; }

    public List<ProbeShift?> Shifts { get; set; } = [];

    public ProbeDays Days { get; set; }
}

public sealed class ProbeRenamed
{
    [FromQuery(Name = "w")]
    public string? Ward { get; set; }

    [BindProperty(Name = "b")]
    public string? Bed { get; set; }

    [ModelBinder(Name = "r")]
    public string? Room { get; set; }

    [BindNever]
    public string? Never { get; set; }
}

public sealed class ProbeSearch
{
    public string? Term { get; set; }

    public int Count { get; set; } = 5;
}

public sealed class QueryProbeController : ControllerBase
{
    [HttpGet("/q/handover")]
    public string Handover(HandoverQuery query) => HandoverText(query.Handover) + "; Kept " + HandoverText(query.Kept) + Errors();

    [HttpPost("/q/handover-json")]
    public string HandoverJson([FromBody] HandoverQuery query) => "Kept " + HandoverText(query.Kept) + Errors();

    [HttpGet("/q/lines")]
    public string Lines(HandoverQuery query) => "Lines=[" + string.Join(", ", query.Lines.Select(LineText)) + "]" + Errors();

    [HttpGet("/q/read-only")]
    public string ReadOnlyLists(HandoverQuery query)
        => "Held=[" + string.Join(", ", query.Held.Select(LineText)) + "] Bare=" + (query.Bare is null ? "null" : "[" + string.Join(",", query.Bare) + "]")
           + " Handover=" + HandoverText(query.Handover) + Errors();

    [HttpPost("/q/enum-json")]
    public string EnumJson([FromBody] ProbeEnumBody? body) => (body is null ? "nothing" : "Shift=" + body.Shift + " Watch=" + body.Watch) + Errors();

    [HttpGet("/q/shift")]
    public string Shift(HandoverQuery query) => "Shift=" + query.Shift + " Shifts=[" + string.Join(",", query.Shifts.Select(shift => shift?.ToString() ?? "null")) + "]" + Errors();

    [HttpGet("/q/days")]
    public string Days(HandoverQuery query) => "Days=" + query.Days + Errors();

    [HttpGet("/q/renamed")]
    public string Renamed(ProbeRenamed renamed)
        => "Ward=" + (renamed.Ward ?? "null") + " Bed=" + (renamed.Bed ?? "null") + " Room=" + (renamed.Room ?? "null") + " Never=" + (renamed.Never ?? "null") + Errors();

    [HttpGet("/q/bind-prefix")]
    public string BindPrefix([Bind(Prefix = "q")] Counts counts) => CountText(counts);

    [HttpGet("/q/query-name")]
    public string QueryName([FromQuery(Name = "q")] Counts counts) => CountText(counts);

    [HttpGet("/q/plain")]
    public string Plain(Counts counts) => CountText(counts);

    [HttpGet("/q/collide")]
    public string Collide(ProbeSearch term) => "Term=" + (term.Term ?? "null") + " Count=" + term.Count.ToString(CultureInfo.InvariantCulture) + Errors();

    private string CountText(Counts counts) => "Count=" + counts.Count.ToString(CultureInfo.InvariantCulture) + Errors();

    private static string HandoverText(ProbeHandover? handover)
        => handover is null
            ? "null"
            : "Round=" + handover.Round.ToString(CultureInfo.InvariantCulture) + " Lead=" + (handover.Lead ?? "null") + " Named=" + (handover.Named ?? "null")
              + " Beds=[" + string.Join(",", handover.Beds) + "]";

    private static string LineText(ProbeLine? line)
        => line is null ? "null" : (line.Name ?? "null") + (line.Beds.Count == 0 ? "" : " Beds=" + string.Join(",", line.Beds));

    private string Errors()
    {
        var failed = ModelState.Where(entry => entry.Value!.Errors.Count > 0).Select(entry => entry.Key).Order(StringComparer.Ordinal).ToList();
        return failed.Count == 0 ? "" : " (errors " + string.Join(",", failed) + ")";
    }
}

[ApiController]
public sealed class ApiProbeController : ControllerBase
{
    [HttpPost("/api-named")]
    public string Named([FromForm] Named named) => "reached the action";
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

    [HttpGet("/route/{name}")]
    public string Route(string name) => System.Text.Json.JsonSerializer.Serialize(name);

    [HttpGet("/query")]
    public string Query([FromQuery] string q) => System.Text.Json.JsonSerializer.Serialize(q);

    [HttpPost("/named")]
    public string FormNamed([FromForm] Named named)
        => (ModelState.IsValid ? "valid" : string.Join(" | ", ModelState.Where(entry => entry.Value!.Errors.Count > 0).Select(entry => entry.Key + ": " + entry.Value!.Errors[0].ErrorMessage))) + " Name=" + (named.Name ?? "null");

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
