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

        Assert.That(root.GetProperty("reaction").GetProperty("kind").GetString(), Is.EqualTo("http"));
        Assert.That(root.GetProperty("reaction").GetProperty("request").GetProperty("url").GetString(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Href_is_the_runtime_url_truth_for_a_native_action_link_request()
    {
        var contract = NativeActionLinkSerializer.CreateContract<NativeTestModel>(
            "/orders/page/2",
            p => p.Post("/orders/page/2"));

        using var doc = JsonDocument.Parse(contract.PayloadJson);
        var request = doc.RootElement.GetProperty("reaction").GetProperty("request");
        Assert.That(request.GetProperty("url").GetString(), Is.EqualTo(string.Empty));
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

        Assert.That(ex!.Message, Does.Contain("exactly one request"));
    }

    [Test]
    public void Nested_http_in_response_handlers_is_rejected()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            NativeActionLinkSerializer.CreateContract<NativeTestModel>(
                "/orders/save",
                p => p.Post("/orders/save")
                    .Response(r => r.OnSuccess(x => x.Post("/orders/after-save")))));

        Assert.That(ex!.Message, Does.Contain("exactly one request"));
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
        var reaction = doc.RootElement.GetProperty("reaction");
        Assert.That(reaction.GetProperty("kind").GetString(), Is.EqualTo("conditional"));

        var branchReaction = reaction.GetProperty("branches")[0].GetProperty("reaction");
        Assert.That(branchReaction.GetProperty("kind").GetString(), Is.EqualTo("http"));
        Assert.That(branchReaction.GetProperty("request").GetProperty("url").GetString(), Is.EqualTo(string.Empty));
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
                    .Validate(new Alis.Reactive.Validation.ValidationDescriptor("form", new System.Collections.Generic.List<Alis.Reactive.Validation.ValidationField>()))));

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

        Assert.That(ex!.Message, Does.Contain("exactly one request"));
    }
}
