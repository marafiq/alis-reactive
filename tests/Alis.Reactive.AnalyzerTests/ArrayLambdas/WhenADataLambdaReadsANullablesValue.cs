using System.Threading.Tasks;
using Alis.Reactive.Analyzers.ArrayLambdas;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.ArrayLambdas;

[TestFixture]
public sealed class WhenADataLambdaReadsANullablesValue
{
    private const string MayBeNull = "CS8629";
    private const string AViewAsRazorWritesIt = "Microsoft.CodeAnalysis.Razor.Compiler/Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator/Rounds/RoutineDoses_cshtml.g.cs";

    [RefusedCase]
    public Task a_nullables_value_in_counts_predicate_is_suppressed()
        => InAView("doses.Count(d => {|#0:d.Priority|}.Value != 2);", Suppressed(0));

    [RefusedCase]
    public Task a_nullable_cast_to_its_value_in_counts_predicate_is_suppressed()
        => InAView("doses.Count(d => {|#0:(int)d.Priority|} != 2);", Suppressed(0));

    [RefusedCase]
    public Task a_nullable_cast_to_its_value_handed_as_an_argument_in_counts_predicate_is_suppressed()
        => InAView("doses.Count(d => routines.Contains({|#0:(int)d.Priority|}));", Suppressed(0));

    [RefusedCase]
    public Task a_nullables_value_in_wheres_condition_is_suppressed()
        => InAView("doses.Where(d => {|#0:d.Priority|}.Value > 2);", Suppressed(0));

    [RefusedCase]
    public Task a_nullables_value_in_anys_predicate_is_suppressed()
        => InAView("doses.Any(d => {|#0:d.Priority|}.Value > 2);", Suppressed(0));

    [RefusedCase]
    public Task a_nullables_value_in_alls_predicate_is_suppressed()
        => InAView("doses.All(d => {|#0:d.Priority|}.Value > 2);", Suppressed(0));

    [RefusedCase]
    public Task a_nullables_value_in_finds_predicate_is_suppressed()
        => InAView("doses.Find(d => {|#0:d.Priority|}.Value > 2);", Suppressed(0));

    [RefusedCase]
    public Task a_nullables_value_in_the_predicate_of_a_find_that_reads_a_field_is_suppressed()
        => InAView("doses.Find(d => {|#0:d.Priority|}.Value > 2, d => d.Medicine);", Suppressed(0));

    [RefusedCase]
    public Task a_nullables_value_in_the_field_a_find_reads_is_suppressed()
        => InAView("doses.Find(d => d.Medicine == \"Aspirin\", d => {|#0:d.Priority|}.Value);", Suppressed(0));

    [RefusedCase]
    public Task a_nullables_value_in_a_selects_field_of_text_is_suppressed()
        => InAView("doses.Select(d => {|#0:d.Priority|}.Value > 2 ? \"urgent\" : \"routine\");", Suppressed(0));

    [RefusedCase]
    public Task a_nullables_value_past_a_read_only_property_is_suppressed()
        => InAView("rescue.Read(dose => {|#0:dose.Priority|}.Value);", Suppressed(0));

    [RefusedCase]
    public Task a_nullables_value_past_a_property_through_the_read_it_inherits_is_suppressed()
        => InAView("held.Read(dose => {|#0:dose.Priority|}.Value);", Suppressed(0));

    [RefusedCase]
    public Task a_nullable_cast_to_its_value_past_what_a_method_returned_is_suppressed()
        => InAView("answered.Read(dose => {|#0:(int)dose.Priority|});", Suppressed(0));

    [RefusedCase]
    public Task a_nullables_value_in_a_count_nested_in_a_sums_term_is_suppressed()
        => InAView("doses.Sum(d => d.Checks.Count(c => {|#0:c.Minutes|}.Value > 5));", Suppressed(0));

    [RefusedCase]
    public Task a_nullable_cast_to_its_value_in_an_any_nested_in_an_order_by_key_is_suppressed()
        => InAView("doses.OrderBy(d => d.Checks.Any(c => {|#0:(int)c.Minutes|} > 5));", Suppressed(0));

    [RefusedCase]
    public Task a_nullables_value_in_an_all_nested_in_counts_predicate_is_suppressed()
        => InAView("doses.Count(d => d.Checks.All(c => {|#0:c.Minutes|}.Value > 5));", Suppressed(0));

    [RefusedCase]
    public Task a_view_razor_compiled_that_casts_a_nullable_in_counts_predicate_is_suppressed()
        => AnAppsProject()
            .WithFile("Rounds/Round.cs",
                """
                namespace TestProject.Rounds;

                public sealed class Dose
                {
                    public int? Priority { get; set; }
                }

                public sealed class Round;
                """)
            .WithFile(AViewAsRazorWritesIt,
                """
                #pragma warning disable 1998
                namespace AspNetCoreGeneratedDocument
                {
                    using global::System.Collections.Generic;
                    using global::Alis.Reactive.ArrayLambdas;
                    using global::Alis.Reactive.RenderingAndDelivery;
                    using global::Alis.Reactive.TriggersAndPipelines;

                    internal sealed class Rounds_RoutineDoses : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<TestProject.Rounds.Round>
                    {
                        public async override global::System.Threading.Tasks.Task ExecuteAsync()
                        {
                #nullable restore
                            var plan = Html.ReactivePlan<TestProject.Rounds.Round>();
                            Html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Get("/Rounds/Doses")
                                .OnSuccess<List<TestProject.Rounds.Dose>>((doses, pipeline) => pipeline.Element("off-routine-count").Text.Set(doses.Body.Count(d => {|#0:(int)d.Priority|} != 2)))));
                            Write(Html.RenderPlan(plan));
                #nullable disable
                        }

                        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TestProject.Rounds.Round> Html { get; private set; } = default!;
                    }
                }
                """)
            .Expecting(Suppressed(0))
            .RunAsync();

    [AcceptedCase]
    public Task a_nullables_value_in_a_sums_term_keeps_its_warning()
        => InAView("doses.Sum(d => {|#0:d.Priority|}.Value);", Kept(0));

    [AcceptedCase]
    public Task a_nullable_cast_to_its_value_in_a_sums_term_keeps_its_warning()
        => InAView("doses.Sum(d => {|#0:(int)d.Priority|});", Kept(0));

    [AcceptedCase]
    public Task a_nullable_cast_to_its_value_in_an_order_by_key_keeps_its_warning()
        => InAView("doses.OrderBy(d => {|#0:(int)d.Priority|});", Kept(0));

    [AcceptedCase]
    public Task a_nullables_value_in_a_descending_order_by_key_keeps_its_warning()
        => InAView("doses.OrderByDescending(d => {|#0:d.Priority|}.Value);", Kept(0));

    [AcceptedCase]
    public Task a_nullable_cast_to_its_value_in_a_selects_field_of_a_number_keeps_its_warning()
        => InAView("doses.Select(d => {|#0:(int)d.Priority|});", Kept(0));

    [AcceptedCase]
    public Task a_nullables_value_in_a_selects_field_of_a_number_keeps_its_warning()
        => InAView("doses.Select(d => {|#0:d.Priority|}.Value);", Kept(0));

    [AcceptedCase]
    public Task a_nullable_the_view_holds_cast_to_its_value_keeps_its_warning()
        => InAView("doses.Count(d => d.Priority == {|#0:(int)limit|});", Kept(0));

    [AcceptedCase]
    public Task the_value_of_a_nullable_the_view_holds_keeps_its_warning()
        => InAView("doses.Count(d => d.Priority == {|#0:limit|}.Value);", Kept(0));

    [AcceptedCase]
    public Task a_nullable_cast_to_a_wider_number_keeps_its_warning()
        => InAView("doses.Count(d => {|#0:(long)d.Priority|} != 2);", Kept(0));

    [AcceptedCase]
    public Task a_nullable_date_converted_to_an_instant_keeps_its_warning()
        => InAView("doses.Count(d => ((DateTimeOffset){|#0:d.Given|}).Year == 2026);", Kept(0));

    [AcceptedCase]
    public Task a_nullables_value_over_a_list_the_server_holds_keeps_its_warning()
        => InAView("served.Count(d => {|#0:d.Priority|}.Value != 2);", Kept(0));

    [AcceptedCase]
    public Task an_input_a_pipeline_names_by_a_nullables_value_keeps_its_warning()
        => InAView("pipeline.Input(m => {|#0:m.Limit|}.Value);", Kept(0));

    [AcceptedCase]
    public Task a_nullables_value_in_the_apps_own_method_of_the_same_name_keeps_its_warning()
        => InAView("ledger.Count(d => {|#0:d.Priority|}.Value != 2);", Kept(0));

    [AcceptedCase]
    public Task a_project_that_references_no_core_keeps_its_warning()
        => AnAppsProject()
            .WithFile("Rounds/RoutineDoses.cs",
                """
                using System.Collections.Generic;
                using System.Linq;

                namespace TestProject.Rounds;

                public sealed class Dose
                {
                    public int? Priority { get; set; }
                }

                public static class RoutineDoses
                {
                    public static int OffRoutine(List<Dose> doses) => doses.Count(d => {|#0:(int)d.Priority|} != 2);
                }
                """)
            .WithoutTheCore()
            .Expecting(Kept(0))
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_keeps_its_warning_in_the_door_it_does_not_bind()
        => InAView("doses.{|CS1501:Find|}(d => d.Medicine == \"Aspirin\", d => {|#0:d.Priority|}.Value, 3);", Kept(0));

    private static Task InAView(string statements, params DiagnosticResult[] reports)
        => AnAppsProject()
            .WithFile("Rounds/RoutineDoses.cs",
                $$"""
                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Linq.Expressions;
                using Alis.Reactive.ArrayLambdas;
                using Alis.Reactive.Declarations;
                using Alis.Reactive.TriggersAndPipelines;
                using Alis.Reactive.ValuesAndSources;

                namespace TestProject.Rounds;

                public sealed class Check
                {
                    public int? Minutes { get; set; }
                }

                public sealed class Dose
                {
                    public string Medicine { get; set; } = "";

                    public int? Priority { get; set; }

                    public DateTime? Given { get; set; }

                    public List<Check> Checks { get; set; } = [];
                }

                public sealed class Round
                {
                    public int? Limit { get; set; }
                }

                public sealed class Ledger<T>
                {
                    public int Count(Expression<Func<T, bool>> keep) => 0;
                }

                public static class RoutineDoses
                {
                    public static void Write(Pipeline<Round> pipeline, TypedSource<List<Dose>> doses, ReadOnly<Dose> rescue, Property<Dose> held, MethodResult<Dose> answered, int? limit, List<int> routines, List<Dose> served, Ledger<Dose> ledger)
                    {
                        {{statements}}
                    }
                }
                """)
            .Expecting(reports)
            .RunAsync();

    private static Case<ALIS027NullablesValueReadsNothing> AnAppsProject()
        => new() { CompilerDiagnostics = CompilerDiagnostics.Warnings, SolutionTransforms = { AsAnAppBuildsIt } };

    private static Solution AsAnAppBuildsIt(Solution solution, ProjectId id)
    {
        var project = solution.GetProject(id)!;
        var nullableOn = ((CSharpCompilationOptions)project.CompilationOptions!).WithNullableContextOptions(NullableContextOptions.Enable);
        var noDocumentationFile = project.ParseOptions!.WithDocumentationMode(DocumentationMode.Parse);
        return solution.WithProjectCompilationOptions(id, nullableOn).WithProjectParseOptions(id, noDocumentationFile);
    }

    private static DiagnosticResult Suppressed(int at) => DiagnosticResult.CompilerWarning(MayBeNull).WithLocation(at).WithIsSuppressed(true);

    private static DiagnosticResult Kept(int at) => DiagnosticResult.CompilerWarning(MayBeNull).WithLocation(at).WithIsSuppressed(false);
}
