using Alis.Reactive.PlanTests.Support;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.PlanTests.Conditions;

/// <summary>A condition guards a branch; arms run in authored order and the first match wins.</summary>
[TestFixture]
public class WhenAPageAuthorsConditions
{
    [Test]
    public void a_branch_guards_its_pipeline_with_the_value_it_compares()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Name);
        plan.Triggers().DomReady(p =>
            p.When(p.Component<TextInput>(m => m.Name).Value).Eq("Helen").Then(t => t.Element("greeting").Text.Set("Hello Helen")));

        var rendered = plan.Rendered();

        var branch = rendered.Handlers()[0]!["do"]![0]!;
        Assert.That(branch["kind"]!.GetValue<string>(), Is.EqualTo("if"));
        Assert.That(branch["arms"]!.AsArray().Single()!["when"]!["op"]!.GetValue<string>(), Is.EqualTo("eq"));
        Golden.Matches(rendered, "Conditions", nameof(a_branch_guards_its_pipeline_with_the_value_it_compares));
    }

    [Test]
    public void else_if_and_else_follow_in_authored_order_and_the_first_match_wins()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Name);
        plan.Triggers().DomReady(p =>
            p.When(p.Component<TextInput>(m => m.Name).Value).Eq("Helen").Then(t => t.Element("greeting").Text.Set("Hello Helen"))
             .ElseIf(p.Component<TextInput>(m => m.Name).Value).Eq("Tom").Then(t => t.Element("greeting").Text.Set("Hello Tom"))
             .Else(t => t.Element("greeting").Text.Set("Hello")));

        var rendered = plan.Rendered();

        var branch = rendered.Handlers()[0]!["do"]![0]!;
        Assert.That(branch["arms"]!.AsArray().Count, Is.EqualTo(2));
        Assert.That(branch["else"]!.AsArray().Count, Is.EqualTo(1));
        Golden.Matches(rendered, "Conditions", nameof(else_if_and_else_follow_in_authored_order_and_the_first_match_wins));
    }

    [Test]
    public void a_confirm_asks_and_the_branches_wait_for_the_answer()
    {
        var plan = Page.Plan<Admission>();
        plan.Triggers().DomReady(p => p.Confirm("Admit Helen?").Then(t => t.Element("status").Text.Set("Admitted")));

        var rendered = plan.Rendered();

        var statements = rendered.Handlers()[0]!["do"]!.AsArray();
        Assert.That(statements[0]!["kind"]!.GetValue<string>(), Is.EqualTo("let"));
        Assert.That(statements[0]!["await"]!.GetValue<bool>(), Is.True);
        Assert.That(statements[1]!["kind"]!.GetValue<string>(), Is.EqualTo("if"));
        Golden.Matches(rendered, "Conditions", nameof(a_confirm_asks_and_the_branches_wait_for_the_answer));
    }

    [Test]
    public void an_awaited_method_result_is_asked_only_when_its_term_is_reached()
    {
        var plan = Page.Plan<Admission>();
        plan.Render<Desk>("desk", HtmlString.Empty);
        plan.Triggers().DomReady(p =>
            p.When(p.Component<TextInput>(m => m.Name).Value).Eq("Helen")
             .Or(p.Component<Desk>("desk").IsFree("214")).Truthy()
             .Then(t => t.Element("status").Text.Set("Admit")));

        var rendered = plan.Rendered();

        var outer = rendered.Handlers()[0]!["do"]![0]!;
        Assert.That(outer["kind"]!.GetValue<string>(), Is.EqualTo("if"), "the synchronous term is asked first");
        Assert.That(outer["else"]![0]!["kind"]!.GetValue<string>(), Is.EqualTo("let"), "the desk is asked only when the name did not match");
        Golden.Matches(rendered, "Conditions", nameof(an_awaited_method_result_is_asked_only_when_its_term_is_reached));
    }
}
