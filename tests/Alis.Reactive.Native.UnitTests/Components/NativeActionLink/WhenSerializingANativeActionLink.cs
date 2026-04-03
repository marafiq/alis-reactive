using System.Text.Json;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenSerializingANativeActionLink
{
    [Test]
    public void One_request_chain_serializes_as_a_native_action_link_payload()
    {
        var contract = NativeActionLinkSerializer.CreateContract<NativeTestModel>(
            "/orders/page/2",
            p => p.Post("/orders/page/2", g => g.Static("page", 2))
                .WhileLoading(x => x.Element("paging-spinner").Show())
                .Response(r => r.OnSuccess(x => x.Element("orders-grid").SetText("loaded"))));

        using var doc = JsonDocument.Parse(contract.PayloadJson);
        var root = doc.RootElement;
        var request = FindFirstRequest(root.GetProperty("action"));

        Assert.That(root.TryGetProperty("plan", out _), Is.True);
        Assert.That(request.GetProperty("url").GetString(), Is.EqualTo("/orders/page/2"));
    }

    [Test]
    public void Href_is_the_runtime_url_truth_for_a_native_action_link_request()
    {
        var contract = NativeActionLinkSerializer.CreateContract<NativeTestModel>(
            "/orders/page/2",
            p => p.Post("/orders/page/2"));

        using var doc = JsonDocument.Parse(contract.PayloadJson);
        var request = FindFirstRequest(doc.RootElement.GetProperty("action"));
        Assert.That(request.GetProperty("url").GetString(), Is.EqualTo("/orders/page/2"));
    }

    [Test]
    public void Href_must_match_the_configured_request_url()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            NativeActionLinkSerializer.CreateContract<NativeTestModel>(
                "/orders/page/2",
                p => p.Post("/orders/page/3")));

        Assert.That(ex!.Message, Does.Contain("href must match"));
    }

    [Test]
    public void Chained_requests_are_rejected()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            NativeActionLinkSerializer.CreateContract<NativeTestModel>(
                "/orders/save",
                p => p.Post("/orders/save")
                    .Response(r => r.Chained(c => c.Get("/orders/after-save")))));

        Assert.That(ex!.Message, Does.Contain("Response.Chained(...) is not supported"));
    }

    [Test]
    public void Nested_http_in_response_handlers_is_rejected()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            NativeActionLinkSerializer.CreateContract<NativeTestModel>(
                "/orders/save",
                p => p.Post("/orders/save")
                    .Response(r => r.OnSuccess(x => x.Post("/orders/after-save")))));

        Assert.That(ex!.Message, Does.Contain("response handlers cannot start a second HTTP request"));
    }

    [Test]
    public void Confirm_wrapped_single_request_chain_serializes_as_a_native_action_link_payload()
    {
        var contract = NativeActionLinkSerializer.CreateContract<NativeTestModel>(
            "/orders/delete/42",
            p => p.Confirm("Delete row?")
                .Then(t => t.Delete("/orders/delete/42")
                    .Response(r => r.OnSuccess(x => x.Dispatch("deleted")))));

        using var doc = JsonDocument.Parse(contract.PayloadJson);
        var action = doc.RootElement.GetProperty("action");
        Assert.That(action.GetProperty("kind").GetString(), Is.EqualTo("branch"));

        var request = FindFirstRequest(action);
        Assert.That(request.GetProperty("url").GetString(), Is.EqualTo("/orders/delete/42"));
    }

    [Test]
    public void Include_all_gather_is_rejected_for_native_action_link()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            NativeActionLinkSerializer.CreateContract<NativeTestModel>(
                "/orders/search",
                p => p.Get("/orders/search")
                    .Gather(g => g.IncludeAll())));

        Assert.That(ex!.Message, Does.Contain("IncludeAll"));
    }

    [Test]
    public void Validation_is_rejected_for_native_action_link()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            NativeActionLinkSerializer.CreateContract<NativeTestModel>(
                "/orders/save",
                p => p.Post("/orders/save")
                    .Validate(new Alis.Reactive.Validation.FormValidation("form", new System.Collections.Generic.List<Alis.Reactive.Validation.ValidationField>()))));

        Assert.That(ex!.Message, Does.Contain("validation"));
    }

    [Test]
    public void Multiple_requests_inside_a_confirm_wrapped_native_action_link_are_rejected()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            NativeActionLinkSerializer.CreateContract<NativeTestModel>(
                "/orders/delete/42",
                p => p.Confirm("Delete row?")
                    .Then(t => t.Delete("/orders/delete/42")
                        .Response(r => r.OnSuccess(x => x.Post("/orders/after-delete"))))));

        Assert.That(ex!.Message, Does.Contain("response handlers cannot start a second HTTP request"));
    }

    private static JsonElement FindFirstRequest(JsonElement action)
    {
        var kind = action.GetProperty("kind").GetString();
        return kind switch
        {
            "request" => action.GetProperty("request"),
            "sequence" => action.GetProperty("steps").EnumerateArray()
                .Select(FindFirstRequestOrDefault)
                .First(x => x.ValueKind != JsonValueKind.Undefined),
            "branch" => action.GetProperty("cases").EnumerateArray()
                .Select(x => FindFirstRequestOrDefault(x.GetProperty("run")))
                .First(x => x.ValueKind != JsonValueKind.Undefined),
            _ => default
        };
    }

    private static JsonElement FindFirstRequestOrDefault(JsonElement action)
    {
        var kind = action.GetProperty("kind").GetString();
        return kind switch
        {
            "request" => action.GetProperty("request"),
            "sequence" => action.GetProperty("steps").EnumerateArray()
                .Select(FindFirstRequestOrDefault)
                .FirstOrDefault(x => x.ValueKind != JsonValueKind.Undefined),
            "branch" => action.GetProperty("cases").EnumerateArray()
                .Select(x => FindFirstRequestOrDefault(x.GetProperty("run")))
                .FirstOrDefault(x => x.ValueKind != JsonValueKind.Undefined),
            _ => default
        };
    }
}
