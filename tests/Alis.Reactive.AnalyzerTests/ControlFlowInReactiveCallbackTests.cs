using Alis.Reactive.Analyzers.ControlFlow;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

/// <summary>
/// ALIS004: a reactive callback declares plan entries at render time, so a condition written in C# decides
/// nothing in the browser; a loop over render-time data and a local function declare entries and are fine.
/// </summary>
[TestFixture]
public class ControlFlowInReactiveCallbackTests
{
    private static string Message(string construct) =>
        $"'{construct}' is not allowed inside a reactive callback — a callback declares plan entries at render time; a condition is p.When(...).Then(...).Else(...)";

    [Test]
    public Task an_if_statement_in_a_pipeline_is_reported() =>
        Fixture.Reports<ControlFlowInReactiveCallbackAnalyzer>("ALIS004", Message("if/else"), Fixture.View, View(
            """
            [|if (Model.Resident == "Maple") p.Get("/wings/maple");|]
            """));

    [Test]
    public Task a_ternary_in_a_pipeline_is_reported() =>
        Fixture.Reports<ControlFlowInReactiveCallbackAnalyzer>("ALIS004", Message("ternary ?:"), Fixture.View, View(
            """
            p.Get([|Model.Resident == "Maple" ? "/wings/maple" : "/wings/grove"|]);
            """));

    [Test]
    public Task an_if_statement_inside_a_loop_is_reported() =>
        Fixture.Reports<ControlFlowInReactiveCallbackAnalyzer>("ALIS004", Message("if/else"), Fixture.View, View(
            """
            foreach (var wing in Model.Wings)
            {
                [|if (wing == "Maple") p.Get("/wings/maple");|]
            }
            """));

    [Test]
    public Task a_condition_the_plan_carries_is_not_reported() =>
        Fixture.ReportsNothing<ControlFlowInReactiveCallbackAnalyzer>(Fixture.View, View(
            """
            var link = p.Url<ShiftQuery>();
            p.When(link.Get(q => q.Resident)).Eq("Maple")
                .Then(maple => maple.Get("/wings/maple"))
                .Else(other => other.Get("/wings/grove"));
            """));

    [Test]
    public Task a_loop_declaring_one_condition_per_wing_is_not_reported() =>
        Fixture.ReportsNothing<ControlFlowInReactiveCallbackAnalyzer>(Fixture.View, View(
            """
            var link = p.Url<ShiftQuery>();
            foreach (var wing in Model.Wings)
            {
                p.When(link.Get(q => q.Resident)).Eq(wing).Then(chosen => chosen.Element("wing-" + wing).Show());
            }
            """));

    [Test]
    public Task a_local_function_declaring_into_the_branch_it_is_given_is_not_reported() =>
        Fixture.ReportsNothing<ControlFlowInReactiveCallbackAnalyzer>(Fixture.View, View(
            """
            var link = p.Url<ShiftQuery>();
            void TellTheDesk(PipelineBuilder<Shift> branch, string wing) => branch.Element("desk").Text.Set(wing);
            p.When(link.Get(q => q.Resident)).Eq("Maple")
                .Then(maple => TellTheDesk(maple, "Maple"))
                .Else(other => TellTheDesk(other, "Grove"));
            """));

    [Test]
    public Task a_file_that_is_not_a_view_is_left_alone() =>
        Fixture.ReportsNothing<ControlFlowInReactiveCallbackAnalyzer>(Fixture.Controller, View(
            """
            if (Model.Resident == "Maple") p.Get("/wings/maple");
            """));

    // The C# the Razor compiler generates for a view whose DomReady pipeline holds the lines given.
    private static string View(string pipeline) => $$"""
        using System.Threading.Tasks;
        using Alis.Reactive;
        using Alis.Reactive.Builders;
        using Microsoft.AspNetCore.Mvc.Razor;
        using Microsoft.AspNetCore.Mvc.Rendering;

        public class Shift
        {
            public string Resident { get; set; } = "";
            public string[] Wings { get; set; } = new string[0];
        }

        public class ShiftQuery
        {
            public string Resident { get; set; } = "";
        }

        public class Views_Home_Index_cshtml : RazorPage<Shift>
        {
            public IHtmlHelper<Shift> Html { get; private set; } = default!;

            public override Task ExecuteAsync()
            {
                var plan = Html.ReactivePlan();
                Html.On(plan, t => t.DomReady(p =>
                {
                    {{pipeline}}
                }));
                return Task.CompletedTask;
            }
        }
        """;
}
