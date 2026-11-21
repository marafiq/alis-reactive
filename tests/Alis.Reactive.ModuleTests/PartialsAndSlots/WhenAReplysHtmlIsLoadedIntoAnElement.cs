using System;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;
using static Alis.Reactive.ModuleTests.PartialsAndSlots.CardPlans;

namespace Alis.Reactive.ModuleTests.PartialsAndSlots;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAReplysHtmlIsLoadedIntoAnElement
{
    private static readonly TestCaseData[] TheCondensedDoorAndTheFullForm =
    [
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline => pipeline.Get("/cards?ward=North").Into("card").NoDefaults())).SetArgDisplayNames("Into(elementId)"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline => pipeline.Get("/cards?ward=North")
            .OnSuccess(Content.Html, (reply, pipeline) => pipeline.Into("card", reply.Body)).NoDefaults())).SetArgDisplayNames("OnSuccess(Content.Html, Into(elementId, reply.Body))"),
    ];

    private static readonly TestCaseData[] EveryOtherVerbsCondensedDoor =
    [
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline => pipeline.Options("/cards?ward=North").Into("card").NoDefaults()), "OPTIONS").SetArgDisplayNames("OPTIONS"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline => pipeline.Post("/cards?ward=North").Into("card").NoDefaults()), "POST").SetArgDisplayNames("POST"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline => pipeline.Put("/cards?ward=North").Into("card").NoDefaults()), "PUT").SetArgDisplayNames("PUT"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline => pipeline.Delete("/cards?ward=North").Into("card").NoDefaults()), "DELETE").SetArgDisplayNames("DELETE"),
    ];

    private static readonly TestCaseData[] ANullArgument =
    [
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline => pipeline.Get("/cards").Into(null!)), "elementId").SetArgDisplayNames("Into(null)"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline => pipeline.Get("/cards").OnSuccess(Content.Html, (reply, pipeline) => pipeline.Into(null!, reply.Body))), "elementId")
            .SetArgDisplayNames("Into(null, reply.Body)"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline => pipeline.Get("/cards").OnSuccess(Content.Html, (reply, pipeline) => pipeline.Into("card", null!))), "html")
            .SetArgDisplayNames("Into(elementId, null)"),
    ];

    [TestCaseSource(nameof(TheCondensedDoorAndTheFullForm))]
    public void the_html_route_loads_its_body_into_the_slot_the_element_id_keys_by_one_step_that_is_not_awaited(Action<Pipeline<CardDesk>> write)
    {
        var expected = Loaded(Document + "," + Slot("card"), HtmlRequest("GET", "/cards", "ward=North", Load("card", HtmlOfTheReply("reply1"))));

        Assert.That(Rendered(write), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [TestCaseSource(nameof(EveryOtherVerbsCondensedDoor))]
    public void each_verbs_condensed_door_writes_the_plan_a_get_writes_but_for_its_method(Action<Pipeline<CardDesk>> write, string verb)
    {
        var expected = Loaded(Document + "," + Slot("card"), HtmlRequest(verb, "/cards", "ward=North", Load("card", HtmlOfTheReply("reply1"))));

        Assert.That(Rendered(write), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [TestCaseSource(nameof(ANullArgument))]
    public void a_null_element_id_or_html_is_refused_by_its_parameters_name(Action<Pipeline<CardDesk>> write, string parameter)
        => Assert.That(() => Rendered(write), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo(parameter));
}
