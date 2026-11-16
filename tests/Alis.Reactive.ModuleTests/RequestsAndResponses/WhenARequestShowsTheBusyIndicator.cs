using System;
using Alis.Reactive.Conditions;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenARequestShowsTheBusyIndicator
{
    private const string NoBusy = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}""";
    private const string PageBusy = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"page"}}]}""";
    private const string PanelAndList = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"targets"}},{"name":"values","value":{"kind":"list","items":[""" +
        """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"element"}},{"name":"id","value":{"kind":"lit","value":"panel"}}]},""" +
        """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"element"}},{"name":"id","value":{"kind":"lit","value":"list"}}]}]}}]}""";
    private const string BusyContract = """{"kind":"list","items":[{"kind":"lit","value":"busy"}]}""";
    private const string NoContract = """{"kind":"list","items":[]}""";
    private const string TakesNoDefaults = "Busy says where the default busy indicator shows, and this request takes no defaults; take out NoDefaults() or Busy.";
    private const string SaidTwice = "A request says once where its busy indicator shows; write one Busy or NoBusy.";

    private static readonly TestCaseData[] EveryBusyOnTwoElements =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").Busy("panel", "list"))).SetArgDisplayNames("GET"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).Busy("panel", "list"))).SetArgDisplayNames("GET after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Head("/beds").Busy("panel", "list"))).SetArgDisplayNames("HEAD"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Head("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).Busy("panel", "list"))).SetArgDisplayNames("HEAD after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Options("/beds").Busy("panel", "list"))).SetArgDisplayNames("OPTIONS"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Options("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).Busy("panel", "list"))).SetArgDisplayNames("OPTIONS after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post("/beds").Busy("panel", "list"))).SetArgDisplayNames("POST"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).Busy("panel", "list"))).SetArgDisplayNames("POST after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put("/beds").Busy("panel", "list"))).SetArgDisplayNames("PUT"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).Busy("panel", "list"))).SetArgDisplayNames("PUT after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/beds").Busy("panel", "list"))).SetArgDisplayNames("DELETE"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).Busy("panel", "list"))).SetArgDisplayNames("DELETE after success"),
    ];

    private static readonly TestCaseData[] EveryBusyOnThePage =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).Busy())).SetArgDisplayNames("GET after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post("/beds").Busy().OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")))).SetArgDisplayNames("POST"),
    ];

    private static readonly TestCaseData[] EveryNoBusy =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").NoBusy())).SetArgDisplayNames("GET"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).NoBusy())).SetArgDisplayNames("GET after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Head("/beds").NoBusy())).SetArgDisplayNames("HEAD"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Head("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).NoBusy())).SetArgDisplayNames("HEAD after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Options("/beds").NoBusy())).SetArgDisplayNames("OPTIONS"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Options("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).NoBusy())).SetArgDisplayNames("OPTIONS after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post("/beds").NoBusy())).SetArgDisplayNames("POST"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).NoBusy())).SetArgDisplayNames("POST after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put("/beds").NoBusy())).SetArgDisplayNames("PUT"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).NoBusy())).SetArgDisplayNames("PUT after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/beds").NoBusy())).SetArgDisplayNames("DELETE"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).NoBusy())).SetArgDisplayNames("DELETE after success"),
    ];

    private static readonly TestCaseData[] RefusedWhereTheViewRenders =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").Busy("panel").NoDefaults()), TakesNoDefaults).SetArgDisplayNames("Busy, then NoDefaults"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post("/beds").NoDefaults().Busy()), TakesNoDefaults).SetArgDisplayNames("NoDefaults, then Busy"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/beds").OnSuccess(pipeline => { }).NoDefaults().Busy("panel")), TakesNoDefaults).SetArgDisplayNames("NoDefaults, then Busy after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").Busy("panel").Busy("list")), SaidTwice).SetArgDisplayNames("Busy twice"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put("/beds").Busy().NoBusy()), SaidTwice).SetArgDisplayNames("Busy, then NoBusy"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Head("/beds").NoBusy().OnSuccess(pipeline => { }).Busy("panel")), SaidTwice).SetArgDisplayNames("NoBusy, then Busy"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Options("/beds").NoBusy().NoBusy()), SaidTwice).SetArgDisplayNames("NoBusy twice"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").Busy("panel", "panel")),
            "Busy names element 'panel' twice; name each element once.").SetArgDisplayNames("an element twice"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").Busy("panel", "")),
            "Busy names an element by the id the view gave it, and one id is empty; name each element by its id.").SetArgDisplayNames("an empty id"),
    ];

    [Test]
    public void the_elements_and_components_its_success_route_writes_outside_a_condition_hold_it()
    {
        var plan = Rendered(pipeline => pipeline.Get("/beds").OnSuccess(Content.None, (reply, pipeline) =>
        {
            pipeline.Element("beds").Text.Set("Full");
            pipeline.Component<BedBoard>("board").Refresh();
            pipeline.Component<BedBoard>("board").Note.Set("Full");
            pipeline.Element("beds").Html.Set("<b>Full</b>");
            pipeline.Element("log").Text.Append("Booked");
            pipeline.When(reply.Status.Eq(204)).Then(pipeline => pipeline.Element("quiet").Text.Set("Quiet"));
            pipeline.Notice().Success("Saved");
        }));
        Assert.That(RequestPage.Send(plan, "busy").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"targets"}},{"name":"values","value":{"kind":"list","items":[""" +
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"element"}},{"name":"id","value":{"kind":"lit","value":"beds"}}]},""" +
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"element"}},{"name":"id","value":{"kind":"lit","value":"board"}}]},""" +
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"element"}},{"name":"id","value":{"kind":"lit","value":"log"}}]}]}}]}"""));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(BusyContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void the_element_its_success_route_loads_holds_it_in_its_place_and_an_element_it_unloads_does_not()
    {
        var plan = Rendered(pipeline => pipeline.Get("/beds").OnSuccess(Content.Html, (reply, pipeline) =>
        {
            pipeline.Unload("ward");
            pipeline.Element("beds").Text.Set("Loaded");
            pipeline.Into("bed-card", reply.Body);
        }));
        Assert.That(RequestPage.Send(plan, "busy").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"targets"}},{"name":"values","value":{"kind":"list","items":[""" +
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"element"}},{"name":"id","value":{"kind":"lit","value":"beds"}}]},""" +
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"element"}},{"name":"id","value":{"kind":"lit","value":"bed-card"}}]}]}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_request_whose_route_writes_only_inside_a_condition_holds_the_page()
    {
        var plan = Rendered(pipeline => pipeline.Get("/beds").OnSuccess(Content.None, (reply, pipeline) =>
            pipeline.When(reply.Status.Eq(204)).Then(pipeline => pipeline.Element("quiet").Text.Set("Quiet"))));
        Assert.That(RequestPage.Send(plan, "busy").GetRawText(), Is.EqualTo(PageBusy));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(BusyContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_request_with_no_success_route_holds_the_page()
    {
        var plan = Rendered(pipeline => pipeline.Delete("/beds").OnError((failed, pipeline) => pipeline.Element("beds").Text.Set("Failed")));
        Assert.That(RequestPage.Send(plan, "busy").GetRawText(), Is.EqualTo(PageBusy));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [TestCaseSource(nameof(EveryBusyOnTwoElements))]
    public void busy_names_its_targets_in_place_of_what_its_route_writes(Action<Pipeline<RequestDesk>> write)
    {
        var plan = Rendered(write);
        Assert.That(RequestPage.Send(plan, "busy").GetRawText(), Is.EqualTo(PanelAndList));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(BusyContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [TestCaseSource(nameof(EveryBusyOnThePage))]
    public void busy_with_no_element_holds_the_page(Action<Pipeline<RequestDesk>> write)
    {
        var plan = Rendered(write);
        Assert.That(RequestPage.Send(plan, "busy").GetRawText(), Is.EqualTo(PageBusy));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [TestCaseSource(nameof(EveryNoBusy))]
    public void no_busy_holds_nothing_and_looks_up_no_indicator(Action<Pipeline<RequestDesk>> write)
    {
        var plan = Rendered(write);
        Assert.That(RequestPage.Send(plan, "busy").GetRawText(), Is.EqualTo(NoBusy));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_request_that_takes_no_defaults_holds_nothing()
    {
        var plan = Rendered(pipeline => pipeline.Get("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")).NoDefaults());
        Assert.That(RequestPage.Send(plan, "busy").GetRawText(), Is.EqualTo(NoBusy));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void without_show_busy_a_request_holds_nothing()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/beds").OnSuccess(pipeline => pipeline.Element("beds").Text.Set("Full")));
        Assert.That(RequestPage.Send(plan, "busy").GetRawText(), Is.EqualTo(NoBusy));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void without_show_busy_busy_is_refused()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get("/beds").Busy("panel")), Throws.InvalidOperationException.With.Message.EqualTo(
            "Busy says where the default busy indicator shows, and the app's defaults show none; call ShowBusy() in AddReactiveRequests, or take out Busy."));

    [Test]
    public void busy_on_a_request_a_default_route_sends_is_refused()
        => Assert.That(() => RequestPage.RenderedIn(app => app.AddReactiveRequests(requests => requests.NoAntiforgery().ShowBusy()
                .OnError(401, Content.None, (reply, pipeline) => pipeline.Get("/session").Busy("panel"))), pipeline => pipeline.Get("/beds")),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Busy says where the default busy indicator shows, and a request a default's block sends takes no defaults; take out Busy."));

    [TestCaseSource(nameof(RefusedWhereTheViewRenders))]
    public void a_busy_choice_that_cannot_hold_is_refused_where_the_view_renders(Action<Pipeline<RequestDesk>> write, string refusal)
        => Assert.That(() => Rendered(write), Throws.InvalidOperationException.With.Message.EqualTo(refusal));

    private static string Rendered(Action<Pipeline<RequestDesk>> write)
        => RequestPage.RenderedIn(app => app.AddReactiveRequests(requests => requests.NoAntiforgery().ShowBusy()), write);
}

public sealed class BedBoard : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<string?> Note => Property<string?>();

    public void Refresh() => Method();
}
