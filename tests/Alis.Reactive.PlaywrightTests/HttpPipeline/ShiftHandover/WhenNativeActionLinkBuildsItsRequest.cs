using System.Net;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Alis.Reactive.Builders;
using Alis.Reactive.Native.AppLevel;
using Alis.Reactive.Native.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.ShiftHandover;

/// <summary>
/// The request a NativeActionLink's own plan sends keeps what the link's pipeline declares. What a
/// link cannot carry (the form's inputs, validation, more than one request) is refused in the view by
/// analyzer ALIS002.
/// </summary>
[TestFixture]
public sealed class WhenNativeActionLinkBuildsItsRequest
{
    private const string MarkReadUrl = "/Sandbox/HttpPipeline/ShiftHandover/MarkRead/helen";

    [Test]
    public void the_link_request_keeps_its_headers_and_its_finally()
    {
        var link = Html().NativeActionLink("Mark Read", MarkReadUrl, p => p
            .Post(MarkReadUrl)
            .Gather(g => g.Header("X-Shift", "Night"))
            .WhileLoading(l => l.Component<NativeLoader>().Call(c => c.Show))
            .Finally(f => f.Component<NativeLoader>().Call(c => c.Hide)));

        using var plan = PlanOf(link);
        var request = SentRequestIn(plan.RootElement);
        var headers = Member(request, "headers").GetProperty("entries").EnumerateArray()
            .Select(entry => entry.GetProperty("name").GetString())
            .ToArray();
        var cleanup = FinallyIn(plan.RootElement);

        Assert.Multiple(() =>
        {
            Assert.That(headers, Is.EqualTo(new[] { "X-Shift" }), $"The link's request lost its gather: {request.GetRawText()}");
            Assert.That(cleanup.GetArrayLength(), Is.GreaterThan(0));
            Assert.That(cleanup.GetRawText(),
                Is.EqualTo(FinallyOf(p => p.Component<NativeLoader>().Call(c => c.Hide))));
        });
    }

    // The Finally a link with only that Finally serializes, so the assertion compares like with like.
    private static string FinallyOf(Action<PipelineBuilder<ShiftHandoverPageModel>> finallyPipeline)
    {
        var link = Html().NativeActionLink("Mark Read", MarkReadUrl, p => p.Post(MarkReadUrl).Finally(finallyPipeline));
        using var plan = PlanOf(link);
        return FinallyIn(plan.RootElement).GetRawText();
    }

    private static IHtmlHelper<ShiftHandoverPageModel> Html() =>
        DispatchProxy.Create<IHtmlHelper<ShiftHandoverPageModel>, ViewContextOnlyHtmlHelper>();

    // The link renders its anchor, then its own plan script.
    private static JsonDocument PlanOf(NativeActionLinkBuilder<ShiftHandoverPageModel> link)
    {
        using var writer = new StringWriter();
        link.WriteTo(writer, HtmlEncoder.Default);
        var planJson = Regex.Match(writer.ToString(), "<script type=\"application/json\"[^>]*data-reactive-plan[^>]*>(.*)</script>").Groups[1].Value;
        return JsonDocument.Parse(planJson);
    }

    // The record the link's request hands alis.http: { method, url, route, values, arrays, headers, format }.
    private static JsonElement SentRequestIn(JsonElement plan) =>
        Nodes(plan).First(node => node.TryGetProperty("kind", out var kind) && kind.GetString() == "call"
                && node.GetProperty("target").TryGetProperty("id", out var id) && id.GetString() == "$http")
            .GetProperty("args")[0];

    private static JsonElement FinallyIn(JsonElement plan) =>
        Nodes(plan).First(node => node.TryGetProperty("kind", out var kind) && kind.GetString() == "try")
            .GetProperty("finally");

    private static JsonElement Member(JsonElement record, string name) =>
        record.GetProperty("entries").EnumerateArray()
            .Single(entry => entry.GetProperty("name").GetString() == name)
            .GetProperty("value");

    private static IEnumerable<JsonElement> Nodes(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            yield return element;
            foreach (var property in element.EnumerateObject())
                foreach (var node in Nodes(property.Value))
                    yield return node;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                foreach (var node in Nodes(item))
                    yield return node;
        }
    }
}

/// <summary>An HTML helper that offers only a view context, which is all NativeActionLink reads.</summary>
public class ViewContextOnlyHtmlHelper : DispatchProxy
{
    private readonly ViewContext _viewContext = new() { HttpContext = new DefaultHttpContext() };

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
        targetMethod?.Name == "get_ViewContext"
            ? _viewContext
            : throw new NotSupportedException($"NativeActionLink is not expected to call {targetMethod?.Name}.");
}
