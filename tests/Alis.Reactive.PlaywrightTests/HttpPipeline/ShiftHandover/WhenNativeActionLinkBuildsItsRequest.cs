using System.Net;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Alis.Reactive.Builders;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.Native.AppLevel;
using Alis.Reactive.Native.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.ShiftHandover;

/// <summary>
/// The request a NativeActionLink serializes into its anchor keeps what the link's pipeline declares, and
/// what the link cannot carry is refused when the view renders instead of being dropped.
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
            .WhileLoading(l => l.Component<NativeLoader>().Show())
            .Finally(f => f.Component<NativeLoader>().Hide()));

        using var payload = PayloadOf(link);
        var request = FirstRequestIn(payload.RootElement.GetProperty("reaction"));
        var input = request.GetProperty("input");
        Assert.That(input.TryGetProperty("assignments", out var gathered), Is.True,
            $"The link's request lost its gather: {input.GetRawText()}");
        var assignments = gathered.EnumerateArray()
            .Select(assignment => assignment.GetProperty("target"))
            .Select(target => (Kind: target.GetProperty("kind").GetString(), Name: target.GetProperty("name").GetString()))
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(assignments, Is.EqualTo(new[] { (Kind: (string?)"header", Name: (string?)"X-Shift") }));
            Assert.That(request.GetProperty("finally").GetArrayLength(), Is.GreaterThan(0));
            Assert.That(request.GetProperty("finally").GetRawText(),
                Is.EqualTo(FinallyOf(p => p.Component<NativeLoader>().Hide())));
        });
    }

    [Test]
    public void a_route_parameter_is_refused_because_the_href_must_be_a_url_a_modifier_click_can_open()
    {
        const string template = "/Sandbox/HttpPipeline/ShiftHandover/MarkRead/{resident}";

        var refusal = Assert.Throws<InvalidOperationException>(() => Html().NativeActionLink("Mark Read", template, p => p
            .Post(template)
            .Gather(g => g.RouteParam("resident", "helen"))));

        Assert.That(refusal!.Message, Does.StartWith("NativeActionLink does not support route parameters"));
    }

    [Test]
    public void include_all_is_refused_because_the_link_does_not_see_the_page_inputs()
    {
        var refusal = Assert.Throws<InvalidOperationException>(() => Html().NativeActionLink("Mark Read", MarkReadUrl, p => p
            .Post(MarkReadUrl)
            .Gather(g => g.IncludeAll())));

        Assert.That(refusal!.Message, Does.StartWith("NativeActionLink does not support IncludeAll"));
    }

    [Test]
    public void validation_on_the_link_request_itself_is_refused()
    {
        var refusal = Assert.Throws<InvalidOperationException>(() => Html().NativeActionLink("Mark Read", MarkReadUrl, p => p
            .Post(MarkReadUrl)
            .Validate<HandoverValidator>("handover-form")));

        Assert.That(refusal!.Message, Is.EqualTo("NativeActionLink does not support validation."));
    }

    private sealed class HandoverValidator : ReactiveValidator<ShiftHandoverPageModel>
    {
    }

    // The Finally a link with only that Finally serializes, so the assertion compares like with like.
    private static string FinallyOf(Action<PipelineBuilder<ShiftHandoverPageModel>> finallyPipeline)
    {
        var link = Html().NativeActionLink("Mark Read", MarkReadUrl, p => p.Post(MarkReadUrl).Finally(finallyPipeline));
        using var payload = PayloadOf(link);
        return FirstRequestIn(payload.RootElement.GetProperty("reaction")).GetProperty("finally").GetRawText();
    }

    private static IHtmlHelper<ShiftHandoverPageModel> Html() =>
        DispatchProxy.Create<IHtmlHelper<ShiftHandoverPageModel>, ViewContextOnlyHtmlHelper>();

    private static JsonDocument PayloadOf(NativeActionLinkBuilder<ShiftHandoverPageModel> link)
    {
        using var writer = new StringWriter();
        link.WriteTo(writer, HtmlEncoder.Default);
        var encodedPayload = Regex.Match(writer.ToString(), "data-reactive-link=\"([^\"]*)\"").Groups[1].Value;
        return JsonDocument.Parse(WebUtility.HtmlDecode(encodedPayload));
    }

    private static JsonElement FirstRequestIn(JsonElement reaction)
    {
        if (reaction.GetProperty("kind").GetString() == "request") return reaction.GetProperty("request");

        return reaction.GetProperty("steps").EnumerateArray().Select(FirstRequestIn).First();
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
