using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Alis.Reactive.Conformance.App;
using Alis.Reactive.Conformance.World;
using Json.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAnAppRendersItsPages
{
    private const string Html = "text/html";
    private const string WhatTheRequestIsFor = "Sec-Fetch-Dest";
    private const string ADocument = "document";
    private const string AnEndpointOfTheApp = "";

    private static readonly Regex Link = new(
        @"<[a-z][^<>]*?\shref\s*=\s*(?:""(?<address>[^""]*)""|'(?<address>[^']*)'|(?<address>[^\s""'=<>`]+))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private const string AWardsPlan =
        "{\"version\":7,\"planId\":\"Ward\",\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"},\"vendors\":{},\"objects\":{},\"handlers\":[]}";

    private static readonly Regex Script = new(
        @"<script\b(?<attributes>[^>]*)>(?<text>.*?)</script>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex HoldsJson = new(@"\btype\s*=\s*[""']application/json[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex IsMarkedAPlan = new(@"\bdata-reactive-plan\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly JsonSchema ThePlansSchema = JsonSchema.FromText(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "plan.schema.json")));

    private sealed record Swept(IReadOnlyList<string> Opened, IReadOnlyList<string> Problems, IReadOnlyList<string> NotBuilt, IReadOnlyList<string> RefusedByDesign);

    private sealed record Answer(int Status, IReadOnlyList<Exception> Thrown, IReadOnlyList<string> Links, IReadOnlyList<string> WhatThePlansSchemaRefuses);

    [Test]
    public async Task every_page_of_the_conformance_app_answers_the_person_who_opens_it()
    {
        await using var app = new WebApplicationFactory<Program>();

        var swept = await Sweep(app.WithWebHostBuilder(host => host.ConfigureTestServices(Watch)).Server);

        Assert.That(swept.Opened, Is.Not.Empty, "The conformance app maps no address a GET opens, so the sweep read nothing");
        Assert.That(
            swept.Problems,
            Is.Empty,
            "A page of the conformance app did not answer the person who opened it. Open the address under scripts/run.sh, read what it answers and cure the page.");
    }

    [RefusedCase]
    public async Task a_page_that_fails_is_refused_by_its_address()
    {
        await using var app = await AnApp(ward => ward.MapGet("/ward", string () => throw new InvalidOperationException("The ward's census is not loaded.")));

        Assert.That((await Sweep(app.GetTestServer())).Problems, Is.EqualTo(new[] { "GET /ward answered 500" }));
    }

    [RefusedCase]
    public async Task a_link_to_an_address_the_app_does_not_serve_is_refused_with_the_page_that_holds_it()
    {
        await using var app = await AnApp(ward => ward.MapGet("/ward", () => Page("<a href=\"/ward/night\">The night round</a> <a href=round>This round</a>")));

        Assert.That(
            (await Sweep(app.GetTestServer())).Problems,
            Is.EqualTo(new[] { "GET /ward/night answered 404, a link on /ward", "GET /round answered 404, a link on /ward" }));
    }

    [RefusedCase]
    public async Task a_route_with_a_parameter_no_page_links_to_is_refused_as_unopened()
    {
        await using var app = await AnApp(ward =>
        {
            ward.MapGet("/ward", () => Page("<h1>The ward</h1>"));
            ward.MapGet("/residents/{id}", (string id) => Page($"<h1>Resident {id}</h1>"));
        });

        Assert.That(
            (await Sweep(app.GetTestServer())).Problems,
            Is.EqualTo(new[] { "GET /residents/{id} was not opened: no page links to an address it serves" }));
    }

    [AcceptedCase]
    public async Task a_route_with_a_parameter_is_opened_with_the_value_a_link_gives_it()
    {
        await using var app = await AnApp(ward =>
        {
            ward.MapGet("/ward", () => Page("<a href=\"/residents/7\">Ada</a>"));
            ward.MapGet("/residents/{id}", (string id) => Page($"<h1>Resident {id}</h1>"));
        });

        var swept = await Sweep(app.GetTestServer());

        Assert.That(swept.Problems, Is.Empty);
        Assert.That(swept.Opened, Is.EqualTo(new[] { "/ward", "/residents/7" }));
    }

    [RefusedCase]
    public async Task a_refusal_page_that_fails_with_no_refusal_is_refused_by_its_address()
    {
        await using var app = await AnApp(ward => ward.MapGet(RefusalPages.Area + "census", string () => throw new FormatException("The census holds no number.")));

        Assert.That((await Sweep(app.GetTestServer())).Problems, Is.EqualTo(new[] { "GET /refusals/census answered 500" }));
    }

    [AcceptedCase]
    public async Task a_refusal_page_that_refuses_is_accepted_as_refused_by_design()
    {
        await using var app = await AnApp(ward => ward.MapGet(RefusalPages.Area + "census", string () => throw new InvalidOperationException("The ward's census is not loaded.")));

        var swept = await Sweep(app.GetTestServer());

        Assert.That(swept.Problems, Is.Empty);
        Assert.That(swept.RefusedByDesign, Is.EqualTo(new[] { "/refusals/census" }));
    }

    [AcceptedCase]
    public async Task a_page_whose_door_is_not_built_is_listed_apart()
    {
        await using var app = await AnApp(ward => ward.MapGet("/ward", string () => throw new NotImplementedException()));

        var swept = await Sweep(app.GetTestServer());

        Assert.That(swept.Problems, Is.Empty);
        Assert.That(swept.NotBuilt, Is.EqualTo(new[] { "/ward" }));
    }

    [RefusedCase]
    public async Task a_page_that_fails_beside_one_whose_door_is_not_built_is_refused_by_its_address()
    {
        await using var app = await AnApp(ward =>
        {
            ward.MapGet("/ward", string () => throw new NotImplementedException());
            ward.MapGet("/ward/census", string () => throw new FormatException("The census holds no number."));
        });

        var swept = await Sweep(app.GetTestServer());

        Assert.That(swept.Problems, Is.EqualTo(new[] { "GET /ward/census answered 500" }));
        Assert.That(swept.NotBuilt, Is.EqualTo(new[] { "/ward" }));
    }

    [RefusedCase]
    public async Task an_action_of_a_controller_that_fails_is_refused_by_its_address()
    {
        await using var app = await AnAppOfActions(typeof(LostRoundController));

        Assert.That((await Sweep(app.GetTestServer())).Problems, Is.EqualTo(new[] { "GET /ward/rounds/lost answered 500" }));
    }

    [AcceptedCase]
    public async Task an_action_of_a_controller_is_opened_at_its_route()
    {
        await using var app = await AnAppOfActions(typeof(NightRoundController));

        var swept = await Sweep(app.GetTestServer());

        Assert.That(swept.Problems, Is.Empty);
        Assert.That(swept.Opened, Is.EqualTo(new[] { "/ward/rounds/night" }));
    }

    [RefusedCase]
    public async Task a_plan_the_plans_schema_refuses_is_refused_with_the_page_that_renders_it()
    {
        var withNoTrace = AWardsPlan.Replace("\"trace\":{\"kind\":\"off\"},", string.Empty);
        await using var app = await AnApp(ward => ward.MapGet("/ward", () => Page($"<script type=\"application/json\" data-reactive-plan>{withNoTrace}</script>")));

        Assert.That(
            (await Sweep(app.GetTestServer())).Problems,
            Is.EqualTo(new[] { "GET /ward renders a plan the plan's schema refuses: the plan: required" }));
    }

    [RefusedCase]
    public async Task a_plan_that_is_no_json_is_refused_with_the_page_that_renders_it()
    {
        await using var app = await AnApp(ward => ward.MapGet("/ward", () => Page("<script data-reactive-plan type='application/json'>{\"version\":</script>")));

        Assert.That(
            (await Sweep(app.GetTestServer())).Problems,
            Is.EqualTo(new[] { "GET /ward renders a plan the plan's schema refuses: its text is not JSON" }));
    }

    [AcceptedCase]
    public async Task a_plan_the_plans_schema_takes_and_a_script_that_is_no_plan_are_accepted()
    {
        await using var app = await AnApp(ward => ward.MapGet("/ward", () => Page(
            $"<script type=\"application/json\" data-reactive-plan>{AWardsPlan}</script><script type=\"application/json\">{{\"beds\":4}}</script>"
            + "<script type=\"text/plain\" data-reactive-plan>Four beds</script>")));

        Assert.That((await Sweep(app.GetTestServer())).Problems, Is.Empty);
    }

    [AcceptedCase]
    public async Task pages_that_answer_and_hold_links_that_answer_are_accepted()
    {
        await using var app = await AnApp(ward =>
        {
            ward.MapGet("/ward", () => Page(
                "<a href=\"/ward/rounds?wing=north&amp;floor=2#night\">Rounds</a> <a href='/ward/rounds?wing=south'>The south wing</a> "
                + "<a href=\"https://example.org/another-home\">Another home</a> <a href=\"mailto:ward@example.org\">Write</a> <a href=\"#top\">Top</a>"));
            ward.MapGet("/ward/rounds", (string? wing, int? floor) => Page($"<h1>Rounds {wing} {floor}</h1>"));
            ward.MapGet("/ward/moved", () => Results.Redirect("/ward/rounds"));
            ward.MapGet("/ward/door", (HttpRequest request) => request.Headers[WhatTheRequestIsFor] == ADocument ? Page("<h1>The door</h1>") : Results.BadRequest());
            ward.MapGet("/ward/note", () => Results.Text("<a href=\"/ward/closed\">The closed wing</a>", "text/plain"));
            ward.MapPost("/ward/discharge", string () => throw new InvalidOperationException("No resident was named."));
        });

        var swept = await Sweep(app.GetTestServer());

        Assert.That(swept.Problems, Is.Empty);
        Assert.That(
            swept.Opened,
            Is.EqualTo(new[] { "/ward", "/ward/rounds", "/ward/moved", "/ward/door", "/ward/note", "/ward/rounds?wing=north&floor=2", "/ward/rounds?wing=south" }));
    }

    private static async Task<Swept> Sweep(TestServer app)
    {
        using var browser = new HttpClient(new RedirectHandler { InnerHandler = app.CreateHandler() }) { BaseAddress = app.BaseAddress };
        var watched = app.Services.GetRequiredService<Watched>();
        var opened = new List<string>();
        var problems = new List<string>();
        var notBuilt = new List<string>();
        var refusedByDesign = new List<string>();
        var toOpen = new Queue<(string Address, string LinkedFrom)>();
        var routes = app.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>().Where(AnswersGet).ToList();

        foreach (var route in routes.Where(route => route.RoutePattern.Parameters.Count == 0)) toOpen.Enqueue((AddressOf(route), AnEndpointOfTheApp));

        while (toOpen.TryDequeue(out var next))
        {
            if (opened.Contains(next.Address)) continue;

            opened.Add(next.Address);
            var answer = await Open(browser, watched, next.Address);
            if (answer.Thrown.OfType<NotImplementedException>().Any())
                notBuilt.Add(next.Address);
            else if (IsARefusalPage(next.Address) && answer.Thrown.OfType<InvalidOperationException>().Any())
                refusedByDesign.Add(next.Address);
            else if (answer.Status is < 200 or > 299)
                problems.Add($"GET {next.Address} answered {answer.Status}" + (next.LinkedFrom == AnEndpointOfTheApp ? "" : $", a link on {next.LinkedFrom}"));
            problems.AddRange(answer.WhatThePlansSchemaRefuses.Select(refused => $"GET {next.Address} renders a plan the plan's schema refuses: {refused}"));
            foreach (var link in answer.Links) toOpen.Enqueue((link, next.Address));
        }

        problems.AddRange(routes
            .Where(route => route.RoutePattern.Parameters.Count > 0 && !watched.Served(route))
            .Select(route => $"GET {AddressOf(route)} was not opened: no page links to an address it serves"));
        return new Swept(opened, problems, notBuilt, refusedByDesign);
    }

    private static async Task<Answer> Open(HttpClient browser, Watched watched, string address)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, address);
        request.Headers.Add(WhatTheRequestIsFor, ADocument);
        using var answer = await browser.SendAsync(request);

        var thrown = watched.TakeWhatWasThrown();
        if (answer.Content.Headers.ContentType?.MediaType != Html) return new Answer((int)answer.StatusCode, thrown, [], []);

        var page = await answer.Content.ReadAsStringAsync();
        return new Answer((int)answer.StatusCode, thrown, LinksOn(page, new Uri(browser.BaseAddress!, address)), WhatThePlansSchemaRefusesOn(page));
    }

    private static List<string> WhatThePlansSchemaRefusesOn(string page)
        => Script.Matches(page)
            .Where(script => HoldsJson.IsMatch(script.Groups["attributes"].Value) && IsMarkedAPlan.IsMatch(script.Groups["attributes"].Value))
            .SelectMany(script => WhatThePlansSchemaRefuses(script.Groups["text"].Value))
            .ToList();

    private static IEnumerable<string> WhatThePlansSchemaRefuses(string plan)
    {
        try
        {
            using var read = JsonDocument.Parse(plan);
            return WhatFails(ThePlansSchema.Evaluate(read.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical })).ToList();
        }
        catch (JsonException)
        {
            return ["its text is not JSON"];
        }
    }

    private static IEnumerable<string> WhatFails(EvaluationResults evaluated)
    {
        if (evaluated.IsValid) return [];
        var here = evaluated.Errors == null ? [] : evaluated.Errors.Keys.Select(keyword => "the plan" + evaluated.InstanceLocation + ": " + keyword);
        var below = evaluated.Details == null ? [] : evaluated.Details.SelectMany(WhatFails);
        return here.Concat(below);
    }

    private static bool IsARefusalPage(string address) => address.StartsWith(RefusalPages.Area, StringComparison.Ordinal);

    private static string AddressOf(RouteEndpoint route) => "/" + RouteOf(route).TrimStart('/');

    private static void Watch(IServiceCollection services)
    {
        services.AddSingleton<Watched>();
        services.AddSingleton<IDeveloperPageExceptionFilter>(held => held.GetRequiredService<Watched>());
        services.AddSingleton<IStartupFilter>(held => held.GetRequiredService<Watched>());
    }

    private static List<string> LinksOn(string page, Uri address)
        => Link.Matches(page)
            .Select(link => new Uri(address, WebUtility.HtmlDecode(link.Groups["address"].Value)))
            .Where(linked => linked.Scheme == address.Scheme && linked.Authority == address.Authority)
            .Select(linked => linked.PathAndQuery)
            .ToList();

    private static bool AnswersGet(RouteEndpoint endpoint)
        => endpoint.Metadata.GetMetadata<IHttpMethodMetadata>() is not { } methods || methods.HttpMethods.Contains(HttpMethods.Get);

    private static string RouteOf(RouteEndpoint endpoint)
        => endpoint.RoutePattern.RawText ?? throw new InvalidOperationException($"The endpoint {endpoint.DisplayName} was mapped with no route text, so the sweep cannot open it.");

    private static Task<WebApplication> AnApp(Action<WebApplication> map) => AnApp(map, _ => { });

    private static Task<WebApplication> AnAppOfActions(params Type[] controllers)
        => AnApp(
            app => app.MapControllers(),
            services => services.AddControllers().ConfigureApplicationPartManager(parts =>
            {
                parts.ApplicationParts.Clear();
                parts.ApplicationParts.Add(new TheCasesControllers(controllers));
            }));

    private static async Task<WebApplication> AnApp(Action<WebApplication> map, Action<IServiceCollection> add)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = Environments.Development });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        Watch(builder.Services);
        add(builder.Services);
        var app = builder.Build();
        map(app);
        await app.StartAsync();
        return app;
    }

    private static IResult Page(string body)
        => Results.Content($"<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>The ward</title></head><body>{body}</body></html>", Html + "; charset=utf-8");

    private sealed class Watched : IDeveloperPageExceptionFilter, IStartupFilter
    {
        private readonly List<Exception> _thrown = [];
        private readonly HashSet<string> _served = [];

        internal bool Served(RouteEndpoint route) => _served.Contains(RouteOf(route));

        internal IReadOnlyList<Exception> TakeWhatWasThrown()
        {
            var thrown = _thrown.ToList();
            _thrown.Clear();
            return thrown;
        }

        public Task HandleExceptionAsync(ErrorContext errorContext, Func<ErrorContext, Task> next)
        {
            _thrown.Add(errorContext.Exception);
            return next(errorContext);
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => app =>
            {
                app.Use(async (HttpContext context, RequestDelegate answer) =>
                {
                    await answer(context);
                    if (context.GetEndpoint() is RouteEndpoint route) _served.Add(RouteOf(route));
                });
                next(app);
            };
    }

    private sealed class TheCasesControllers(IReadOnlyList<Type> controllers) : ApplicationPart, IApplicationPartTypeProvider
    {
        public override string Name => "The case's controllers";

        public IEnumerable<TypeInfo> Types => controllers.Select(controller => controller.GetTypeInfo());
    }
}

public sealed class NightRoundController : Controller
{
    [HttpGet("ward/rounds/night")]
    public IActionResult Night() => Content("<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>The night round</title></head><body><h1>The night round</h1></body></html>", "text/html");
}

public sealed class LostRoundController : Controller
{
    [HttpGet("ward/rounds/lost")]
    public IActionResult Lost() => throw new InvalidOperationException("The round's chart is lost.");
}
