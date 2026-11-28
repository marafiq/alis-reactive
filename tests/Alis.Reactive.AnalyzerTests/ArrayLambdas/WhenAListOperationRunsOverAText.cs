using System.Threading.Tasks;
using Alis.Reactive.Analyzers.ArrayLambdas;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.ArrayLambdas;

[TestFixture]
public sealed class WhenAListOperationRunsOverAText
{
    private const string AViewAsRazorWritesIt = "Microsoft.CodeAnalysis.Razor.Compiler/Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator/Rounds/CareNote_cshtml.g.cs";

    [RefusedCase]
    public Task a_text_counted_as_a_list_is_refused_at_the_text()
        => InAView("var letters = {|#0:note|}.Count();", OverAText(0, "note", "Count"));

    [RefusedCase]
    public Task a_view_razor_compiled_that_counts_a_text_is_refused_at_the_text()
        => new Case<ALIS042ListOperationRunsOverAList>()
            .WithFile("Rounds/Ward.cs",
                """
                namespace TestProject.Rounds;

                public sealed class Ward;

                public static class Notes
                {
                    public static readonly Alis.Reactive.TriggersAndPipelines.AppEvent Written = Alis.Reactive.TriggersAndPipelines.AppEvent.Declare();
                }
                """)
            .WithFile(AViewAsRazorWritesIt,
                """
                namespace AspNetCoreGeneratedDocument
                {
                    using global::Alis.Reactive.ArrayLambdas;
                    using global::Alis.Reactive.RenderingAndDelivery;
                    using global::Alis.Reactive.TriggersAndPipelines;

                    internal sealed class Rounds_CareNote : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<TestProject.Rounds.Ward>
                    {
                        public async override global::System.Threading.Tasks.Task ExecuteAsync()
                        {
                            var plan = Html.ReactivePlan<TestProject.Rounds.Ward>();
                            Html.On(plan, trigger => trigger
                                .CustomEvent(TestProject.Rounds.Notes.Written, pipeline => pipeline.Element("letters").Text.Set({|#0:pipeline.Element("note").Text|}.Count())));
                            Write(Html.RenderPlan(plan));
                        }

                        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TestProject.Rounds.Ward> Html { get; private set; } = default!;
                    }
                }
                """)
            .Expecting(OverAText(0, "pipeline.Element(\"note\").Text", "Count"))
            .RunAsync();

    [RefusedCase]
    public Task a_text_ordered_as_a_list_is_refused_at_the_text()
        => InAView("var ordered = {|#0:note|}.OrderBy(letter => letter);", OverAText(0, "note", "OrderBy"));

    [RefusedCase]
    public Task an_elements_text_counted_as_a_list_is_refused_at_the_text()
        => InAView("var letters = {|#0:pipeline.Element(\"status\").Text|}.Count();", OverAText(0, "pipeline.Element(\"status\").Text", "Count"));

    [RefusedCase]
    public Task a_text_handed_to_the_operation_in_its_static_form_is_refused_at_the_text()
        => InAView("var any = DataLists.Any({|#0:note|});", OverAText(0, "note", "Any"));

    [RefusedCase]
    public Task a_value_typed_as_the_source_contract_of_text_is_refused_at_the_value()
        => InAView("var letters = {|#0:held|}.Count();", OverAText(0, "held", "Count"));

    [RefusedCase]
    public Task a_text_an_operation_found_counted_as_a_list_is_refused_at_the_found_text()
        => InAView("var letters = {|#0:names.Find(name => name == \"Ada\")|}.Count();", OverAText(0, "names.Find(name => name == \"Ada\")", "Count"));

    [AcceptedCase]
    public Task a_list_of_letters_counted_is_accepted()
        => InAView("var letters = chars.Count();");

    [AcceptedCase]
    public Task a_text_counted_by_the_apps_own_method_of_the_same_name_is_accepted()
        => new Case<ALIS042ListOperationRunsOverAList>()
            .WithFile("Rounds/CareNote.cs",
                """
                using Alis.Reactive.ArrayLambdas;
                using Alis.Reactive.ValuesAndSources;

                namespace TestProject.Rounds;

                public static class Texts
                {
                    public static TypedSource<int> Count(this ITypedSource<string> text) => throw new System.NotImplementedException();
                }

                public static class CareNote
                {
                    public static TypedSource<int> Letters(TypedSource<string> note) => note.Count();
                }
                """)
            .RunAsync();

    [AcceptedCase]
    public Task a_text_a_second_variable_holds_as_a_list_is_accepted()
        => InAView("ITypedSource<IEnumerable<char>> widened = note; var letters = widened.Count();");

    [AcceptedCase]
    public Task a_project_that_references_no_core_is_accepted()
        => new Case<ALIS042ListOperationRunsOverAList>()
            .WithFile("Rounds/CareNote.cs", "namespace TestProject.Rounds; public static class CareNote { public static int Letters(string note) => System.Linq.Enumerable.Count(note); }")
            .WithoutTheCore()
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => InAView("var letters = note.{|CS1061:Counts|}();");

    private static Task InAView(string statements, params DiagnosticResult[] reports)
        => new Case<ALIS042ListOperationRunsOverAList>()
            .WithFile("Rounds/CareNote.cs",
                $$"""
                using System.Collections.Generic;
                using Alis.Reactive.ArrayLambdas;
                using Alis.Reactive.TriggersAndPipelines;
                using Alis.Reactive.ValuesAndSources;

                namespace TestProject.Rounds;

                public sealed class Ward;

                public static class CareNote
                {
                    public static void Read(Pipeline<Ward> pipeline, TypedSource<string> note, ITypedSource<string> held, TypedSource<char[]> chars, TypedSource<IReadOnlyList<string>> names)
                    {
                        {{statements}}
                    }
                }
                """)
            .Expecting(reports)
            .RunAsync();

    private static DiagnosticResult OverAText(int at, string source, string operation)
        => new DiagnosticResult(ALIS042ListOperationRunsOverAList.Id, DiagnosticSeverity.Error)
            .WithLocation(at)
            .WithMessage($"'{source}' is a source of text, and a text is no list in the browser; run {operation} over a list, such as a List<T> or an array, or test the text with Contains, StartsWith, EndsWith, MinLength or Matches");
}
