using System;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Conditions;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAPipelineBranchesOnWhatItReads
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string OnLoad =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.Conditions.Round\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"pain\":{\"kind\":\"element\",\"id\":\"pain\",\"vendor\":\"native\"}," +
        "\"plan\":{\"kind\":\"element\",\"id\":\"plan\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[";

    private const string Ends = "]}]}";

    private const string PainText = "{\"kind\":\"get\",\"path\":[\"textContent\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"pain\"}}";

    private const string CallTheDoctor =
        "{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true,\"target\":{\"kind\":\"obj\",\"id\":\"plan\"},\"value\":{\"kind\":\"lit\",\"value\":\"Call the doctor\"}}";

    private const string GivePainRelief =
        "{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true,\"target\":{\"kind\":\"obj\",\"id\":\"plan\"},\"value\":{\"kind\":\"lit\",\"value\":\"Give pain relief\"}}";

    private const string Rest =
        "{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true,\"target\":{\"kind\":\"obj\",\"id\":\"plan\"},\"value\":{\"kind\":\"lit\",\"value\":\"Rest\"}}";

    private const string ThreeBranches = OnLoad +
        "{\"kind\":\"if\",\"when\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + PainText + ",{\"kind\":\"lit\",\"value\":\"4\"}]}," +
        "\"then\":[" + CallTheDoctor + "]," +
        "\"else\":[{\"kind\":\"if\",\"when\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + PainText + ",{\"kind\":\"lit\",\"value\":\"2\"}]}," +
        "\"then\":[" + GivePainRelief + "],\"else\":[" + Rest + "]}]}" + Ends;

    private const string JoinedTerms = OnLoad +
        "{\"kind\":\"if\",\"when\":{\"kind\":\"op\",\"op\":\"or\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"and\",\"args\":[{\"kind\":\"op\",\"op\":\"neq\",\"args\":[" + PainText + ",{\"kind\":\"lit\",\"value\":null}]}," +
        "{\"kind\":\"op\",\"op\":\"not\",\"args\":[{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + PainText + ",{\"kind\":\"lit\",\"value\":\"0\"}]}]}]}," +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + PainText + ",{\"kind\":\"get\",\"path\":[\"textContent\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"plan\"}}]}]}," +
        "\"then\":[" + CallTheDoctor + "],\"else\":[]}" + Ends;

    [Test]
    public void an_else_if_is_the_else_block_of_the_if_before_it_holding_an_if_of_its_own()
    {
        var html = Views.Of(new Round("North"));
        var plan = html.ReactivePlan<Round>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.When(pipeline.Element("pain").Text.Eq("4"))
                .Then(pipeline => pipeline.Element("plan").Text.Set("Call the doctor"))
                .ElseIf(pipeline.Element("pain").Text.Eq("2"))
                .Then(pipeline => pipeline.Element("plan").Text.Set("Give pain relief"))
                .Else(pipeline => pipeline.Element("plan").Text.Set("Rest"))));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ThreeBranches + ScriptClose));
    }

    [Test]
    public void the_plan_of_three_branches_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(ThreeBranches), Is.Empty);

    [Test]
    public void and_or_and_not_hold_their_terms_in_the_order_the_view_writes_them_and_a_when_with_no_else_has_an_empty_else()
    {
        var html = Views.Of(new Round("North"));
        var plan = html.ReactivePlan<Round>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pain = pipeline.Element("pain").Text;
            string? none = null;
            pipeline.When(pain.NotEq(none).And(pain.Eq("0").Not()).Or(pain.Eq(pipeline.Element("plan").Text)))
                .Then(pipeline => pipeline.Element("plan").Text.Set("Call the doctor"));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + JoinedTerms + ScriptClose));
    }

    [Test]
    public void the_plan_of_joined_terms_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(JoinedTerms), Is.Empty);

    [Test]
    public void an_else_written_after_another_step_of_its_block_is_refused_since_it_no_longer_continues_its_when()
    {
        var html = Views.Of(new Round("North"));
        var plan = html.ReactivePlan<Round>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var branches = pipeline.When(pipeline.Element("pain").Text.Eq("4")).Then(pipeline => pipeline.Element("plan").Text.Set("Call the doctor"));
                pipeline.Element("plan").Text.Set("Checked");
                branches.Else(pipeline => pipeline.Element("plan").Text.Set("Rest"));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "An ElseIf or an Else continues the When it follows, before any other step of its block: write it in the same sentence."));
    }

    [Test]
    public void an_else_if_after_the_else_of_its_when_is_refused_since_the_else_ended_it()
    {
        var html = Views.Of(new Round("North"));
        var plan = html.ReactivePlan<Round>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var branches = pipeline.When(pipeline.Element("pain").Text.Eq("4")).Then(pipeline => pipeline.Element("plan").Text.Set("Call the doctor"));
                branches.Else(pipeline => pipeline.Element("plan").Text.Set("Rest"));
                branches.ElseIf(pipeline.Element("pain").Text.Eq("2")).Then(pipeline => pipeline.Element("plan").Text.Set("Give pain relief"));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "An Else ends its When: write each ElseIf before it, and every step of the remaining case in that one Else."));
    }

    [Test]
    public void an_ordering_of_true_and_false_is_refused_since_the_browser_does_not_order_them_as_dotnet_does()
    {
        var html = Views.Of(new Round("North"));
        var plan = html.ReactivePlan<Round>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
                pipeline.When(pipeline.Element("pain").Text.Eq("4").Gt(false)).Then(pipeline => pipeline.Element("plan").Text.Set("Call the doctor"))));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition orders values the browser cannot order as .NET does; order numbers, a char, or a date or a time, or order on the server."));
    }
}

public sealed record Round(string Ward);
