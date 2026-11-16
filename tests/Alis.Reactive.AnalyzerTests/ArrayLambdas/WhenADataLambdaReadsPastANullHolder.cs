using System.Threading.Tasks;
using Alis.Reactive.Analyzers.ArrayLambdas;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.ArrayLambdas;

[TestFixture]
public sealed class WhenADataLambdaReadsPastANullHolder
{
    private const string MayBeNull = "CS8602";
    private const string MayBeANullArgument = "CS8604";
    private const string AViewAsRazorWritesIt = "Microsoft.CodeAnalysis.Razor.Compiler/Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator/Rounds/DoseCount_cshtml.g.cs";

    [RefusedCase]
    public Task a_read_past_a_holder_in_counts_predicate_is_suppressed()
        => InAView("doses.Count(d => {|#0:d.Carer|}.Name == \"Ruth\");", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_wheres_condition_is_suppressed()
        => InAView("doses.Where(d => {|#0:d.Carer|}.Name == \"Ruth\");", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_anys_predicate_is_suppressed()
        => InAView("doses.Any(d => {|#0:d.Carer|}.Name == \"Ruth\");", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_alls_predicate_is_suppressed()
        => InAView("doses.All(d => {|#0:d.Carer|}.Name == \"Ruth\");", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_finds_predicate_is_suppressed()
        => InAView("doses.Find(d => {|#0:d.Carer|}.Name == \"Ruth\");", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_the_predicate_of_a_find_that_reads_a_field_is_suppressed()
        => InAView("doses.Find(d => {|#0:d.Carer|}.Name == \"Ruth\", d => d.Medicine);", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_the_field_a_find_reads_is_suppressed()
        => InAView("doses.Find(d => d.Medicine == \"Aspirin\", d => {|#0:d.Carer|}.Name);", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_a_selects_field_of_text_is_suppressed()
        => InAView("doses.Select(d => {|#0:d.Carer|}.Name);", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_a_selects_field_of_a_nullable_number_is_suppressed()
        => InAView("doses.Select(d => (int?){|#0:d.Supply|}.Vials);", Suppressed(0));

    [RefusedCase]
    public Task a_member_of_a_text_that_holds_null_in_counts_predicate_is_suppressed()
        => InAView("doses.Count(d => {|#0:d.Ward|}.Contains(\"th\"));", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_the_value_a_read_only_property_holds_is_suppressed()
        => InAView("rescue.Read(dose => {|#0:dose|}.Medicine);", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_the_value_a_property_holds_through_the_read_it_inherits_is_suppressed()
        => InAView("held.Read(dose => {|#0:dose|}.Medicine);", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_the_value_a_method_returned_is_suppressed()
        => InAView("answered.Read(dose => {|#0:dose|}.Medicine);", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_a_count_nested_in_a_sums_term_is_suppressed()
        => InAView("doses.Sum(d => d.Checks.Count(c => {|#0:c.By|}.Name == \"Ruth\"));", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_an_any_nested_in_an_order_by_key_is_suppressed()
        => InAView("doses.OrderBy(d => d.Checks.Any(c => {|#0:c.By|}.Name == \"Ruth\"));", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_an_all_nested_in_counts_predicate_is_suppressed()
        => InAView("doses.Count(d => d.Checks.All(c => {|#0:c.By|}.Name == \"Ruth\"));", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_a_holder_in_an_any_nested_in_a_read_is_suppressed()
        => InAView("answered.Read(dose => dose != null && dose.Checks.Any(c => {|#0:c.By|}.Name == \"Ruth\"));", Suppressed(0));

    [RefusedCase]
    public Task a_read_past_the_outer_items_holder_in_a_nested_predicate_is_suppressed()
        => InAView("doses.Count(d => d.Checks.Any(c => c.By != null && c.By.Name == {|#0:d.Carer|}.Name));", Suppressed(0));

    [RefusedCase]
    public Task a_view_razor_compiled_that_reads_past_a_holder_in_counts_predicate_is_suppressed()
        => AnAppsProject()
            .WithFile("Rounds/Round.cs",
                """
                namespace TestProject.Rounds;

                public sealed class Carer
                {
                    public string Name { get; set; } = "";
                }

                public sealed class Dose
                {
                    public Carer? Carer { get; set; }
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

                    internal sealed class Rounds_DoseCount : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<TestProject.Rounds.Round>
                    {
                        public async override global::System.Threading.Tasks.Task ExecuteAsync()
                        {
                #nullable restore
                            var plan = Html.ReactivePlan<TestProject.Rounds.Round>();
                            Html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Get("/Rounds/Doses")
                                .OnSuccess<List<TestProject.Rounds.Dose>>((doses, pipeline) => pipeline.Element("ruth-count").Text.Set(doses.Body.Count(d => {|#0:d.Carer|}.Name == "Ruth")))));
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
    public Task a_read_past_a_holder_in_a_sums_term_keeps_its_warning()
        => InAView("doses.Sum(d => {|#0:d.Supply|}.Vials);", Kept(0));

    [AcceptedCase]
    public Task a_read_past_a_holder_in_an_order_by_key_keeps_its_warning()
        => InAView("doses.OrderBy(d => {|#0:d.Carer|}.Name);", Kept(0));

    [AcceptedCase]
    public Task a_read_past_a_holder_in_a_descending_order_by_key_keeps_its_warning()
        => InAView("doses.OrderByDescending(d => {|#0:d.Carer|}.Name);", Kept(0));

    [AcceptedCase]
    public Task a_read_past_a_holder_in_a_selects_field_of_a_number_keeps_its_warning()
        => InAView("doses.Select(d => {|#0:d.Supply|}.Vials);", Kept(0));

    [AcceptedCase]
    public Task a_read_past_a_value_the_view_holds_keeps_its_warning()
        => InAView("doses.Count(d => d.Medicine == {|#0:cover|}.Name);", Kept(0));

    [AcceptedCase]
    public Task a_read_past_a_value_the_view_holds_in_a_nested_predicate_keeps_its_warning()
        => InAView("doses.Count(d => d.Checks.Any(c => c.By != null && c.By.Name == {|#0:cover|}.Name));", Kept(0));

    [AcceptedCase]
    public Task an_argument_that_may_hold_null_keeps_its_warning()
        => InAView("doses.Count(d => d.Medicine.Contains({|#0:d.Ward|}));", DiagnosticResult.CompilerWarning(MayBeANullArgument).WithLocation(0).WithIsSuppressed(false));

    [AcceptedCase]
    public Task a_read_past_a_holder_over_a_list_the_server_holds_keeps_its_warning()
        => InAView("served.Count(d => {|#0:d.Carer|}.Name == \"Ruth\");", Kept(0));

    [AcceptedCase]
    public Task a_read_past_a_holder_in_a_predicate_nested_over_a_list_the_server_holds_keeps_its_warning()
        => InAView("served.Count(d => d.Checks.Any(c => {|#0:c.By|}.Name == \"Ruth\"));", Kept(0));

    [AcceptedCase]
    public Task a_read_past_a_holder_in_a_select_nested_in_counts_predicate_keeps_its_warning()
        => InAView("doses.Count(d => d.Checks.Select(c => {|#0:c.By|}.Name).Contains(\"Ruth\"));", Kept(0));

    [AcceptedCase]
    public Task an_input_a_pipeline_names_past_a_holder_keeps_its_warning()
        => InAView("pipeline.Input(m => {|#0:m.Cover|}.Name);", Kept(0));

    [AcceptedCase]
    public Task an_input_field_named_past_a_holder_keeps_its_warning()
        => InAView("html.InputField(plan, m => {|#0:m.Cover|}.Name);", Kept(0));

    [AcceptedCase]
    public Task a_read_past_a_holder_in_the_apps_own_method_of_the_same_name_keeps_its_warning()
        => InAView("ledger.Count(d => {|#0:d.Carer|}.Name == \"Ruth\");", Kept(0));

    [AcceptedCase]
    public Task a_read_past_a_holder_in_a_lambda_a_variable_holds_keeps_its_warning()
        => InAView("Expression<Func<Dose, bool>> byRuth = d => {|#0:d.Carer|}.Name == \"Ruth\"; doses.Count(byRuth);", Kept(0));

    [AcceptedCase]
    public Task a_project_that_references_no_core_keeps_its_warning()
        => AnAppsProject()
            .WithFile("Rounds/DoseCount.cs",
                """
                using System.Collections.Generic;
                using System.Linq;

                namespace TestProject.Rounds;

                public sealed class Carer
                {
                    public string Name { get; set; } = "";
                }

                public sealed class Dose
                {
                    public Carer? Carer { get; set; }
                }

                public static class DoseCount
                {
                    public static int Ruths(List<Dose> doses) => doses.Count(d => {|#0:d.Carer|}.Name == "Ruth");
                }
                """)
            .WithoutTheCore()
            .Expecting(Kept(0))
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_keeps_its_warning_in_the_door_it_does_not_bind()
        => InAView("doses.{|CS1501:Find|}(d => d.Medicine == \"Aspirin\", d => {|#0:d.Carer|}.Name, 3);", Kept(0));

    private static Task InAView(string statements, params DiagnosticResult[] reports)
        => AnAppsProject()
            .WithFile("Rounds/DoseCount.cs",
                $$"""
                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Linq.Expressions;
                using Alis.Reactive.ArrayLambdas;
                using Alis.Reactive.Declarations;
                using Alis.Reactive.RenderingAndDelivery;
                using Alis.Reactive.TriggersAndPipelines;
                using Alis.Reactive.ValuesAndSources;
                using Microsoft.AspNetCore.Mvc.Rendering;

                namespace TestProject.Rounds;

                public sealed class Carer
                {
                    public string Name { get; set; } = "";
                }

                public sealed class Check
                {
                    public Carer? By { get; set; }
                }

                public sealed class Supply
                {
                    public int Vials { get; set; }
                }

                public sealed class Dose
                {
                    public string Medicine { get; set; } = "";

                    public string? Ward { get; set; }

                    public Carer? Carer { get; set; }

                    public Supply? Supply { get; set; }

                    public List<Check> Checks { get; set; } = [];
                }

                public sealed class Round
                {
                    public Carer? Cover { get; set; }
                }

                public sealed class Ledger<T>
                {
                    public int Count(Expression<Func<T, bool>> keep) => 0;
                }

                public static class DoseCount
                {
                    public static void Write(IHtmlHelper<Round> html, ReactivePlan<Round> plan, Pipeline<Round> pipeline, TypedSource<List<Dose>> doses, ReadOnly<Dose?> rescue, Property<Dose?> held, MethodResult<Dose?> answered, Carer? cover, List<Dose> served, Ledger<Dose> ledger)
                    {
                        {{statements}}
                    }
                }
                """)
            .Expecting(reports)
            .RunAsync();

    private static Case<ALIS026ReadPastANullHolderReadsNothing> AnAppsProject()
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
