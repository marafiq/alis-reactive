using System.Threading.Tasks;
using Alis.Reactive.Analyzers.PartialsAndSlots;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.PartialsAndSlots;

[TestFixture]
public sealed class WhenAViewOpensAPlan
{
    private const string AViewAsRazorWritesIt = "Microsoft.CodeAnalysis.Razor.Compiler/Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator/Rounds/Board_cshtml.g.cs";

    [RefusedCase]
    public Task a_plan_no_line_hands_to_render_plan_is_refused_at_its_name()
        => InAView(
            """
            var {|#0:plan|} = Html.ReactivePlan<Ward>();
            Html.On(plan, trigger => { });
            """,
            NeverRendered(0, "plan", "TestProject.Rounds.Ward"));

    [RefusedCase]
    public Task a_view_razor_compiled_that_never_renders_its_plan_is_refused_at_its_name()
        => new Case<ALIS041PlanIsRenderedOnceLast>()
            .WithFile("Rounds/Ward.cs", "namespace TestProject.Rounds; public sealed class Ward;")
            .WithFile(AViewAsRazorWritesIt,
                """
                namespace AspNetCoreGeneratedDocument
                {
                    using global::Alis.Reactive.RenderingAndDelivery;

                    internal sealed class Rounds_Board : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<TestProject.Rounds.Ward>
                    {
                        public async override global::System.Threading.Tasks.Task ExecuteAsync()
                        {
                            var {|#0:plan|} = Html.ReactivePlan<TestProject.Rounds.Ward>();
                            WriteLiteral("<h1>");
                            Write(Model);
                            WriteLiteral("</h1>");
                            DefineSection("Scripts", async () =>
                            {
                                Html.On(plan, trigger => { });
                            });
                        }

                        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TestProject.Rounds.Ward> Html { get; private set; } = default!;
                    }
                }
                """)
            .Expecting(NeverRendered(0, "plan", "TestProject.Rounds.Ward"))
            .RunAsync();

    [RefusedCase]
    public Task a_plan_rendered_a_second_time_is_refused_at_the_second_render()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            Html.RenderPlan(plan);
            {|#0:Html.RenderPlan(plan)|};
            """,
            RenderedTwice(0, "TestProject.Rounds.Ward"));

    [RefusedCase]
    public Task a_plan_rendered_in_a_loop_is_refused_at_its_render()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            foreach (var bed in beds) {|#0:Html.RenderPlan(plan)|};
            """,
            RenderedTwice(0, "TestProject.Rounds.Ward"));

    [RefusedCase]
    public Task a_door_that_takes_the_plan_after_its_render_is_refused_at_its_call()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            Html.RenderPlan(plan);
            {|#0:Html.On(plan, trigger => { })|};
            """,
            TakenAfterItRendered(0, "TestProject.Rounds.Ward"));

    [RefusedCase]
    public Task a_door_of_the_plan_itself_called_after_its_render_is_refused_at_its_call()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            Html.RenderPlan(plan);
            {|#0:plan.Rows<Dose>("doses")|};
            """,
            TakenAfterItRendered(0, "TestProject.Rounds.Ward"));

    [RefusedCase]
    public Task a_door_after_a_render_only_one_branch_runs_is_refused_at_its_call()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            if (busy) Html.RenderPlan(plan);
            {|#0:Html.On(plan, trigger => { })|};
            """,
            TakenAfterItRendered(0, "TestProject.Rounds.Ward"));

    [RefusedCase]
    public Task a_door_in_a_lambda_written_after_the_render_is_refused_at_its_call()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            Html.RenderPlan(plan);
            System.Action late = () => {|#0:Html.On(plan, trigger => { })|};
            late();
            """,
            TakenAfterItRendered(0, "TestProject.Rounds.Ward"));

    [RefusedCase]
    public Task a_door_after_the_render_and_an_early_return_is_refused_at_its_call()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            Html.RenderPlan(plan);
            if (busy) return;
            {|#0:Html.On(plan, trigger => { })|};
            """,
            TakenAfterItRendered(0, "TestProject.Rounds.Ward"));

    [RefusedCase]
    public Task a_plan_a_local_function_renders_twice_is_refused_at_the_second_render()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            void Scripts()
            {
                Html.RenderPlan(plan);
                {|#0:Html.RenderPlan(plan)|};
            }
            Scripts();
            """,
            RenderedTwice(0, "TestProject.Rounds.Ward"));

    [AcceptedCase]
    public Task a_plan_rendered_once_after_its_doors_is_accepted()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            Html.On(plan, trigger => { });
            Html.RenderPlan(plan);
            """);

    [AcceptedCase]
    public Task a_plan_rendered_once_in_each_branch_is_accepted()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            if (busy) Html.RenderPlan(plan);
            else Html.RenderPlan(plan);
            """);

    [AcceptedCase]
    public Task a_plan_opened_and_rendered_in_each_turn_of_a_loop_is_accepted()
        => InAView(
            """
            foreach (var bed in beds)
            {
                var plan = Html.ReactivePlan<Ward>();
                Html.RenderPlan(plan);
            }
            """);

    [AcceptedCase]
    public Task two_plans_each_rendered_once_are_accepted()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            var chart = doses.ReactivePlan<Dose>();
            Html.RenderPlan(plan);
            Html.RenderPlan(chart);
            """);

    [AcceptedCase]
    public Task a_plan_only_a_lambda_renders_is_accepted()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            System.Action scripts = () => Html.RenderPlan(plan);
            scripts();
            """);

    [AcceptedCase]
    public Task a_door_in_a_lambda_written_before_the_render_is_left_to_the_render()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            System.Action section = () => Html.On(plan, trigger => { });
            Html.RenderPlan(plan);
            section();
            """);

    [AcceptedCase]
    public Task a_plan_a_second_local_holds_is_not_followed()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            var same = plan;
            Html.RenderPlan(same);
            Html.RenderPlan(same);
            """);

    [AcceptedCase]
    public Task a_plan_handed_by_reference_is_not_followed()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            static void Reopen(ref ReactivePlan<Ward> held) { }
            Reopen(ref plan);
            """);

    [AcceptedCase]
    public Task a_plan_no_local_holds_is_not_followed()
        => InAView("Html.RenderPlan(Html.ReactivePlan<Ward>());");

    [AcceptedCase]
    public Task a_local_another_call_fills_is_no_plan()
        => InAView(
            """
            var listed = System.Linq.Enumerable.ToList(beds);
            listed.Clear();
            """);

    [AcceptedCase]
    public Task a_project_that_references_no_core_is_read_by_no_rule()
        => new Case<ALIS041PlanIsRenderedOnceLast>()
            .WithFile("Rounds/View.cs", "namespace TestProject.Rounds; public static class View { public static int Execute() { var plan = 1; return plan; } }")
            .WithoutTheCore()
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => InAView(
            """
            var plan = Html.ReactivePlan<Ward>();
            Html.{|CS1061:Renders|}(plan);
            """);

    private static Task InAView(string statements, params DiagnosticResult[] reports)
        => new Case<ALIS041PlanIsRenderedOnceLast>()
            .WithFile("Rounds/View.cs",
                $$"""
                using Alis.Reactive.RenderingAndDelivery;
                using Microsoft.AspNetCore.Mvc.Rendering;

                namespace TestProject.Rounds;

                public sealed class Ward;

                public sealed class Dose;

                public static class View
                {
                    public static void Execute(IHtmlHelper<Ward> Html, IHtmlHelper<Dose> doses, bool busy, int[] beds)
                    {
                        {{statements}}
                    }
                }
                """)
            .Expecting(reports)
            .RunAsync();

    private static DiagnosticResult NeverRendered(int at, string plan, string model)
        => Refused(at, $"'{plan}', the plan of '{model}', is opened and never handed to Html.RenderPlan; write Html.RenderPlan({plan}) as the view's last line");

    private static DiagnosticResult RenderedTwice(int at, string model)
        => Refused(at, $"The plan of '{model}' is rendered twice, so every handler it holds would run twice; render it once, as the view's last line");

    private static DiagnosticResult TakenAfterItRendered(int at, string model)
        => Refused(at, $"A door takes the plan of '{model}' after it rendered, so the page never runs what the door writes; write every door above Html.RenderPlan, and where a @section holds one, render the plan in a section the layout renders after it");

    private static DiagnosticResult Refused(int at, string message)
        => new DiagnosticResult(ALIS041PlanIsRenderedOnceLast.Id, DiagnosticSeverity.Error).WithLocation(at).WithMessage(message);
}
