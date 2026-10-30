using Alis.Reactive.Analyzers.ConditionalChain;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

/// <summary>ALIS001: a condition a view starts and never completes with .Then() is not in the plan.</summary>
[TestFixture]
public class IncompleteConditionalChainTests
{
    private const string Message = "Incomplete conditional chain — call .Then() to complete the condition";

    [Test]
    public Task a_source_chosen_without_an_operator_is_reported() =>
        Fixture.Reports<IncompleteConditionalChainAnalyzer>("ALIS001", Message, Fixture.View, View(
            """
            var link = p.Url<ShiftQuery>();
            [|p.When(link.Get(q => q.Resident))|];
            """));

    [Test]
    public Task a_guard_without_then_is_reported() =>
        Fixture.Reports<IncompleteConditionalChainAnalyzer>("ALIS001", Message, Fixture.View, View(
            """
            var link = p.Url<ShiftQuery>();
            [|p.When(link.Get(q => q.Resident)).Eq("Maple")|];
            """));

    [Test]
    public Task an_all_guard_without_then_is_reported() =>
        Fixture.Reports<IncompleteConditionalChainAnalyzer>("ALIS001", Message, Fixture.View, View(
            """
            var link = p.Url<ShiftQuery>();
            [|p.When(link.Get(q => q.Resident)).Eq("Maple").And(link.Get(q => q.Wing)).Eq("East")|];
            """));

    [Test]
    public Task an_any_guard_without_then_is_reported() =>
        Fixture.Reports<IncompleteConditionalChainAnalyzer>("ALIS001", Message, Fixture.View, View(
            """
            var link = p.Url<ShiftQuery>();
            [|p.When(link.Get(q => q.Resident)).Eq("Maple").Or(link.Get(q => q.Wing)).Eq("East")|];
            """));

    [Test]
    public Task a_confirm_without_then_is_reported() =>
        Fixture.Reports<IncompleteConditionalChainAnalyzer>("ALIS001", Message, Fixture.View, View(
            """
            [|p.Confirm("Discharge this resident?")|];
            """));

    [Test]
    public Task a_condition_completed_with_then_is_not_reported() =>
        Fixture.ReportsNothing<IncompleteConditionalChainAnalyzer>(Fixture.View, View(
            """
            var link = p.Url<ShiftQuery>();
            p.When(link.Get(q => q.Resident)).Eq("Maple").Then(maple => maple.Get("/wings/maple"));
            """));

    [Test]
    public Task a_file_that_is_not_a_view_is_left_alone() =>
        Fixture.ReportsNothing<IncompleteConditionalChainAnalyzer>(Fixture.Controller, View(
            """
            var link = p.Url<ShiftQuery>();
            p.When(link.Get(q => q.Resident)).Eq("Maple");
            """));

    // The C# the Razor compiler generates for a view whose DomReady pipeline holds the lines given.
    private static string View(string pipeline) => $$"""
        using System.Threading.Tasks;
        using Alis.Reactive;
        using Microsoft.AspNetCore.Mvc.Razor;
        using Microsoft.AspNetCore.Mvc.Rendering;

        public class Shift
        {
            public string Resident { get; set; } = "";
        }

        public class ShiftQuery
        {
            public string Resident { get; set; } = "";
            public string Wing { get; set; } = "";
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
