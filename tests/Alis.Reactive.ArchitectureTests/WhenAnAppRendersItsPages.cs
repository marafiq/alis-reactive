using System.Net;
using System.Text.RegularExpressions;
using Alis.Reactive.Conformance.App;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

    private sealed record Swept(IReadOnlyList<string> Opened, IReadOnlyList<string> Problems);

    private sealed record Answer(int Status, IReadOnlyList<string> Links);

    [Test]
    public async Task every_page_of_the_conformance_app_answers_the_person_who_opens_it()
    {
        await using var app = new WebApplicationFactory<Program>();

        var swept = await Sweep(app.Server);

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
    public async Task a_route_that_takes_a_parameter_is_refused_as_unopened()
    {
        await using var app = await AnApp(ward => ward.MapGet("/residents/{id}", (string id) => Page($"<h1>Resident {id}</h1>")));

        Assert.That(
            (await Sweep(app.GetTestServer())).Problems,
            Is.EqualTo(new[] { "GET /residents/{id} was not opened: the sweep has no value for its parameter id" }));
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
        var opened = new List<string>();
        var problems = new List<string>();
        var toOpen = new Queue<(string Address, string LinkedFrom)>();

        foreach (var endpoint in app.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>().Where(AnswersGet))
        {
            var route = "/" + RouteOf(endpoint).TrimStart('/');
            if (endpoint.RoutePattern.Parameters is [var parameter, ..])
                problems.Add($"GET {route} was not opened: the sweep has no value for its parameter {parameter.Name}");
            else
                toOpen.Enqueue((route, AnEndpointOfTheApp));
        }

        while (toOpen.TryDequeue(out var next))
        {
            if (opened.Contains(next.Address)) continue;

            opened.Add(next.Address);
            var answer = await Open(browser, next.Address);
            if (answer.Status is < 200 or > 299)
                problems.Add($"GET {next.Address} answered {answer.Status}" + (next.LinkedFrom == AnEndpointOfTheApp ? "" : $", a link on {next.LinkedFrom}"));
            foreach (var link in answer.Links) toOpen.Enqueue((link, next.Address));
        }

        return new Swept(opened, problems);
    }

    private static async Task<Answer> Open(HttpClient browser, string address)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, address);
        request.Headers.Add(WhatTheRequestIsFor, ADocument);
        using var answer = await browser.SendAsync(request);

        var isAPage = answer.Content.Headers.ContentType?.MediaType == Html;
        return new Answer((int)answer.StatusCode, isAPage ? LinksOn(await answer.Content.ReadAsStringAsync(), new Uri(browser.BaseAddress!, address)) : []);
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

    private static async Task<WebApplication> AnApp(Action<WebApplication> map)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = Environments.Development });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        var app = builder.Build();
        map(app);
        await app.StartAsync();
        return app;
    }

    private static IResult Page(string body)
        => Results.Content($"<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>The ward</title></head><body>{body}</body></html>", Html + "; charset=utf-8");
}
