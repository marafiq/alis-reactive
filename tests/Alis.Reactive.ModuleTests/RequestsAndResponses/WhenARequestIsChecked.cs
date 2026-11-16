using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenARequestIsChecked
{
    private const string Rules = "Alis.Reactive.ModuleTests.RequestsAndResponses.TransferRules";
    private const string DoseRules = "Alis.Reactive.ModuleTests.RequestsAndResponses.DoseOrderRules";
    private const string WardNorth = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"North"}}]}""";

    [Test]
    public void a_request_whose_model_has_a_registered_validator_is_checked_under_the_default_rule_set_on_what_it_sends()
        => Assert.That(Check(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Set(r => r.Ward, "North"))), Is.EqualTo(
            $$$$"""[{"kind":"let","name":"checked1","await":false,"binds":"line","expr":{"kind":"call","path":["checked"],"members":"required","target":{"kind":"obj","id":"$model"},"args":[{"kind":"get","path":["body","inputs"],"members":"required","target":{"kind":"ref","name":"argument1"}},{"kind":"lit","value":null},{"kind":"list","items":[{"kind":"record","fields":[{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"ward"}]}},{"name":"value","value":{{{{WardNorth}}}}}]}]}]}},{"kind":"if","when":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"checked1"},{"kind":"lit","value":null}]},"then":[{"kind":"let","name":"failures1","await":false,"binds":"line","expr":{"kind":"call","path":["validate"],"members":"required","target":{"kind":"obj","id":"$form:{{{{Rules}}}}:default"},"args":[{"kind":"record","fields":[{"name":"registration","value":{"kind":"lit","value":"{{{{Rules}}}}:default"}},{"name":"body","value":{"kind":"record","fields":[{"name":"format","value":{"kind":"lit","value":"multipart"}},{"name":"value","value":{"kind":"get","path":["body"],"members":"required","target":{"kind":"ref","name":"checked1"}}}]}},{"name":"inputs","value":{"kind":"get","path":["inputs"],"members":"required","target":{"kind":"ref","name":"checked1"}}},{"name":"place","value":{"kind":"record","fields":[{"name":"destination","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"notice"}},{"name":"separator","value":{"kind":"lit","value":" "}}]}}]}}]}]}},{"kind":"if","when":{"kind":"op","op":"not","args":[{"kind":"op","op":"empty","args":[{"kind":"ref","name":"failures1"}]}]},"then":[{"kind":"stop","reason":"The body \u0027POST /transfers\u0027 sends fails the rules Transfer is registered with."}],"else":[]}],"else":[]}]"""));

    [Test]
    public void a_json_body_is_checked_on_the_value_it_sends_and_a_get_on_its_query_as_form_data()
    {
        Assert.That(Check(pipeline => pipeline.Put<Transfer>("/transfers", request => request.Json(body => body.Set(r => r.Ward, "North")))), Does.StartWith(
            """[{"kind":"let","name":"checked1","await":false,"binds":"line","expr":{"kind":"call","path":["checked"],"members":"required","target":{"kind":"obj","id":"$model"},"args":[{"kind":"get","path":["body","inputs"],"members":"required","target":{"kind":"ref","name":"argument1"}},{"kind":"get","path":["body","value"],"members":"required","target":{"kind":"ref","name":"argument1"}},{"kind":"list","items":[]}]}}"""));
        Assert.That(Check(pipeline => pipeline.Get<Transfer>("/transfers", request => request.Set(r => r.Ward, "North"))), Does.StartWith(
            $$$$"""[{"kind":"let","name":"checked1","await":false,"binds":"line","expr":{"kind":"call","path":["checked"],"members":"required","target":{"kind":"obj","id":"$model"},"args":[{"kind":"get","path":["query","inputs"],"members":"required","target":{"kind":"ref","name":"argument1"}},{"kind":"lit","value":null},{"kind":"list","items":[{"kind":"record","fields":[{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"ward"}]}},{"name":"value","value":{{{{WardNorth}}}}}]}]}]}}"""));
        Assert.That(Check(pipeline => pipeline.Get<Transfer>("/transfers", request => request.Set(r => r.Ward, "North"))), Does.Contain(
            """{"name":"format","value":{"kind":"lit","value":"urlEncoded"}}"""));
    }

    [Test]
    public void a_request_that_sends_a_container_shows_its_failures_in_that_containers_summary()
        => Assert.That(Check(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Inputs(pipeline.Container<Transfer>("move")))), Does.Contain(
            """{"name":"place","value":{"kind":"record","fields":[{"name":"destination","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"summary"}},{"name":"containerId","value":{"kind":"lit","value":"move"}}]}}]}}"""));

    [Test]
    public void a_check_whose_failures_show_at_the_notice_names_the_notice_contract()
    {
        Assert.That(Contracts(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Set(r => r.Ward, "North"))), Is.EqualTo("""{"kind":"list","items":[{"kind":"lit","value":"notice"}]}"""));
        Assert.That(Contracts(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Inputs(pipeline.Container<Transfer>("move")))), Is.EqualTo("""{"kind":"list","items":[]}"""));
        Assert.That(Contracts(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Set(r => r.Ward, "North")).NoValidation()), Is.EqualTo("""{"kind":"list","items":[]}"""));
    }

    [Test]
    public void a_rule_set_named_or_no_check_writes_no_default_check_so_a_default_rule_the_browser_refuses_refuses_nothing()
    {
        Assert.That(DoseForms(pipeline => pipeline.Post<DoseOrder>("/doses", request => request.Set(r => r.Medicine, "Aspirin")).NoValidation()), Is.Empty);
        Assert.That(DoseForms(pipeline => pipeline.Post<DoseOrder>("/doses", request => request.Set(r => r.Medicine, "Aspirin")).Validate<DoseOrderRules>("stock")),
            Is.EqualTo(new[] { "$form:" + DoseRules + ":stock" }));
        Assert.That(() => DoseForms(pipeline => pipeline.Post<DoseOrder>("/doses", request => request.Set(r => r.Medicine, "Aspirin"))),
            Throws.InvalidOperationException.With.Message.Contains("The pattern 'aspirin' ignores case, which no browser flag folds as .NET does"));
    }

    [Test]
    public void a_named_rule_set_checks_by_the_registered_validator_under_that_set()
        => Assert.That(Check(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Set(r => r.Ward, "North")).Validate<TransferRules>("moving")), Does.Contain(
            $$$$"""{"kind":"call","path":["validate"],"members":"required","target":{"kind":"obj","id":"$form:{{{{Rules}}}}:moving"},"args":[{"kind":"record","fields":[{"name":"registration","value":{"kind":"lit","value":"{{{{Rules}}}}:moving"}}"""));

    [Test]
    public void the_default_rule_set_named_writes_the_default_check()
        => Assert.That(Check(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Set(r => r.Ward, "North")).Validate<TransferRules>()),
            Is.EqualTo(Check(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Set(r => r.Ward, "North")))));

    [Test]
    public void no_validation_names_no_check_even_when_the_model_has_a_validator()
        => Assert.That(Check(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Set(r => r.Ward, "North")).NoValidation()), Is.EqualTo("[]"));

    [Test]
    public void a_request_whose_model_has_no_registered_validator_is_not_checked()
        => Assert.That(Check(pipeline => pipeline.Post<IntakeSend>("/intakes", request => request.Set(r => r.Ward, "North"))), Is.EqualTo("[]"));

    [Test]
    public void a_second_check_on_one_request_is_refused_where_the_view_renders()
        => Assert.That(() => Check(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Set(r => r.Ward, "North")).Validate<TransferRules>().NoValidation()),
            Throws.InvalidOperationException.With.Message.EqualTo("The request already names its check; write one Validate or NoValidation for the request."));

    [Test]
    public void a_validator_the_app_has_not_registered_for_the_model_is_refused_where_the_view_renders()
        => Assert.That(() => Check(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Set(r => r.Ward, "North")).Validate<StrictTransferRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Transfer has no registered StrictTransferRules validator. Register it with services.AddReactiveFluentValidation(validation => validation.Add<StrictTransferRules>())."));

    private static readonly TestCaseData[] EachVerbAndStage =
    [
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Get<Transfer>("/t", request => request.Set(r => r.Ward, "North")).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Get<Transfer>("/t", request => request.Set(r => r.Ward, "North")).Validate<TransferRules>("moving"))).SetArgDisplayNames("GET"),
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Get<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Get<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).Validate<TransferRules>("moving"))).SetArgDisplayNames("GET answered"),
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Head<Transfer>("/t", request => request.Set(r => r.Ward, "North")).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Head<Transfer>("/t", request => request.Set(r => r.Ward, "North")).Validate<TransferRules>("moving"))).SetArgDisplayNames("HEAD"),
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Head<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Head<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).Validate<TransferRules>("moving"))).SetArgDisplayNames("HEAD answered"),
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Options<Transfer>("/t", request => request.Set(r => r.Ward, "North")).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Options<Transfer>("/t", request => request.Set(r => r.Ward, "North")).Validate<TransferRules>("moving"))).SetArgDisplayNames("OPTIONS"),
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Options<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Options<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).Validate<TransferRules>("moving"))).SetArgDisplayNames("OPTIONS answered"),
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Post<Transfer>("/t", request => request.Set(r => r.Ward, "North")).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Post<Transfer>("/t", request => request.Set(r => r.Ward, "North")).Validate<TransferRules>("moving"))).SetArgDisplayNames("POST"),
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Post<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Post<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).Validate<TransferRules>("moving"))).SetArgDisplayNames("POST answered"),
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Put<Transfer>("/t", request => request.Set(r => r.Ward, "North")).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Put<Transfer>("/t", request => request.Set(r => r.Ward, "North")).Validate<TransferRules>("moving"))).SetArgDisplayNames("PUT"),
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Put<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Put<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).Validate<TransferRules>("moving"))).SetArgDisplayNames("PUT answered"),
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Delete<Transfer>("/t", request => request.Set(r => r.Ward, "North")).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Delete<Transfer>("/t", request => request.Set(r => r.Ward, "North")).Validate<TransferRules>("moving"))).SetArgDisplayNames("DELETE"),
        new TestCaseData((Action<Pipeline<Transfer>>)(pipeline => pipeline.Delete<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).NoValidation()), (Action<Pipeline<Transfer>>)(pipeline => pipeline.Delete<Transfer>("/t", request => request.Set(r => r.Ward, "North")).OnSuccess(_ => { }).Validate<TransferRules>("moving"))).SetArgDisplayNames("DELETE answered"),
    ];

    [TestCaseSource(nameof(EachVerbAndStage))]
    public void each_verb_in_each_stage_names_no_check_or_another_rule_set(Action<Pipeline<Transfer>> noCheck, Action<Pipeline<Transfer>> moving)
    {
        Assert.That(Check(noCheck), Is.EqualTo("[]"));
        Assert.That(Check(moving), Does.Contain($$$"""{"name":"registration","value":{"kind":"lit","value":"{{{Rules}}}:moving"}}"""));
    }

    [Test]
    public void each_check_reads_against_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(Plan(pipeline => pipeline.Post<Transfer>("/transfers", request => request.Inputs(pipeline.Container<Transfer>("move")).Set(r => r.Ward, "North")))), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(Plan(pipeline => pipeline.Put<Transfer>("/transfers", request => request.Json(body => body.Set(r => r.Ward, "North"))))), Is.Empty);
    }

    private static string Check(Action<Pipeline<Transfer>> write) => RequestPage.Request(Plan(write)).GetProperty("check").GetRawText();

    private static string Contracts(Action<Pipeline<Transfer>> write) => RequestPage.Send(Plan(write), "contracts").GetRawText();

    private static string[] DoseForms(Action<Pipeline<DoseOrder>> write)
    {
        var html = Views.Of(new DoseOrder(), _ => { }, app =>
        {
            app.AddReactiveRequests(requests => requests.NoAntiforgery());
            app.AddReactiveFluentValidation(validation => validation.Add<DoseOrderRules>());
        });
        var plan = html.ReactivePlan<DoseOrder>();
        html.On(plan, trigger => trigger.DomReady(write));
        var rendered = Views.TextOf(html.RenderPlan(plan))["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
        return [.. JsonDocument.Parse(rendered).RootElement.GetProperty("objects").EnumerateObject().Select(member => member.Name).Where(name => name.StartsWith("$form:", StringComparison.Ordinal))];
    }

    private static string Plan(Action<Pipeline<Transfer>> write)
    {
        var html = Views.Of(new Transfer(), _ => { }, app =>
        {
            app.AddReactiveRequests(requests => requests.NoAntiforgery());
            app.AddReactiveFluentValidation(validation => validation.Add<TransferRules>());
        });
        var plan = html.ReactivePlan<Transfer>();
        html.On(plan, trigger => trigger.DomReady(write));
        return Views.TextOf(html.RenderPlan(plan))["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }
}

public sealed class Transfer
{
    public string Ward { get; init; } = "";

    public string Room { get; init; } = "";
}

public sealed class TransferRules : ReactiveValidator<Transfer>
{
    public TransferRules()
    {
        RuleFor(transfer => transfer.Ward).NotEmpty();
        RuleSet("moving", () => RuleFor(transfer => transfer.Room).NotEmpty());
    }
}

public sealed class DoseOrder
{
    public string? Medicine { get; init; }
}

public sealed class DoseOrderRules : ReactiveValidator<DoseOrder>
{
    public DoseOrderRules()
    {
        RuleFor(order => order.Medicine).Matches("aspirin", RegexOptions.IgnoreCase);
        RuleSet("stock", () => RuleFor(order => order.Medicine).NotEmpty());
    }
}

public sealed class StrictTransferRules : ReactiveValidator<Transfer>
{
    public StrictTransferRules() => RuleFor(transfer => transfer.Room).NotEmpty();
}
