using System.Threading.Tasks;
using Alis.Reactive.Analyzers.ValuesAndSources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.ValuesAndSources;

[TestFixture]
public sealed class WhenDotNetMakesATextOfASource
{
    private const string AViewAsRazorWritesIt = "Microsoft.CodeAnalysis.Razor.Compiler/Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator/Rounds/CallBell_cshtml.g.cs";

    [RefusedCase]
    public Task a_text_joined_with_a_source_by_plus_is_refused_at_the_source()
        => InAView("var status = \"rang \" + {|#0:bed|};", MadeIntoText(0, "bed"));

    [RefusedCase]
    public Task a_view_razor_compiled_that_joins_a_text_with_a_source_is_refused_at_the_source()
        => new Case<ALIS043TextIsJoinedByConcat>()
            .WithFile("Rounds/Ward.cs",
                """
                namespace TestProject.Rounds;

                public sealed class Ward;

                public static class Bells
                {
                    public static readonly Alis.Reactive.TriggersAndPipelines.AppEvent Rang = Alis.Reactive.TriggersAndPipelines.AppEvent.Declare();
                }
                """)
            .WithFile(AViewAsRazorWritesIt,
                """
                namespace AspNetCoreGeneratedDocument
                {
                    using global::Alis.Reactive.RenderingAndDelivery;
                    using global::Alis.Reactive.TriggersAndPipelines;

                    internal sealed class Rounds_CallBell : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<TestProject.Rounds.Ward>
                    {
                        public async override global::System.Threading.Tasks.Task ExecuteAsync()
                        {
                            var plan = Html.ReactivePlan<TestProject.Rounds.Ward>();
                            Html.On(plan, trigger => trigger
                                .CustomEvent(TestProject.Rounds.Bells.Rang, pipeline => pipeline.Element("status").Text.Set("rang " + {|#0:pipeline.Element("bed").Text|})));
                            Write(Html.RenderPlan(plan));
                        }

                        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TestProject.Rounds.Ward> Html { get; private set; } = default!;
                    }
                }
                """)
            .Expecting(MadeIntoText(0, "pipeline.Element(\"bed\").Text"))
            .RunAsync();

    [RefusedCase]
    public Task a_value_typed_as_the_source_contract_is_refused_at_the_value()
        => InAView("var status = \"rang \" + {|#0:held|};", MadeIntoText(0, "held"));

    [RefusedCase]
    public Task a_source_on_the_left_of_plus_is_refused_at_the_source()
        => InAView("var status = {|#0:bed|} + \" rang\";", MadeIntoText(0, "bed"));

    [RefusedCase]
    public Task each_source_of_a_longer_join_is_refused_at_its_own_place()
        => InAView("var status = \"rang \" + {|#0:bed|} + {|#1:count|};", MadeIntoText(0, "bed"), MadeIntoText(1, "count"));

    [RefusedCase]
    public Task a_source_joined_onto_a_text_in_place_is_refused_at_the_source()
        => InAView("text += {|#0:bed|};", MadeIntoText(0, "bed"));

    [RefusedCase]
    public Task a_source_in_an_interpolation_hole_is_refused_at_the_hole()
        => InAView("var status = $\"rang {{|#0:bed|}}\";", MadeIntoText(0, "bed"));

    [RefusedCase]
    public Task a_source_made_a_text_by_its_own_method_is_refused_at_the_source()
        => InAView("var status = {|#0:bed|}.ToString();", MadeIntoText(0, "bed"));

    [RefusedCase]
    public Task a_source_made_a_text_through_a_null_check_is_refused_at_the_source()
        => InAView("var status = {|#0:bed|}?.ToString();", MadeIntoText(0, "bed"));

    [RefusedCase]
    public Task a_source_a_static_method_of_text_joins_is_refused_at_its_argument()
        => InAView("var status = string.Concat(\"rang \", {|#0:bed|});", MadeIntoText(0, "bed"));

    [RefusedCase]
    public Task a_source_among_the_values_a_format_takes_is_refused_at_its_place()
        => InAView("var status = string.Format(\"{0} {1} {2} {3}\", name, text, beds, {|#0:bed|});", MadeIntoText(0, "bed"));

    [RefusedCase]
    public Task each_source_a_join_takes_is_refused_at_its_place()
        => InAView("var status = string.Join(\", \", {|#0:bed|}, {|#1:count|});", MadeIntoText(0, "bed"), MadeIntoText(1, "count"));

    [RefusedCase]
    public Task each_source_of_an_array_a_join_takes_is_refused_at_its_place()
        => InAView("var status = string.Join(\", \", new object[] { {|#0:bed|}, {|#1:count|} });", MadeIntoText(0, "bed"), MadeIntoText(1, "count"));

    [RefusedCase]
    public Task a_text_joined_with_plus_inside_pipeline_concat_is_refused_at_the_source()
        => InAView("var status = pipeline.Concat(\"rang \" + {|#0:bed|});", MadeIntoText(0, "bed"));

    [AcceptedCase]
    public Task a_source_passed_to_pipeline_concat_as_a_part_is_accepted()
        => InAView("var status = pipeline.Concat(\"rang \", bed);");

    [AcceptedCase]
    public Task a_text_the_view_holds_joined_by_plus_and_in_a_hole_is_accepted()
        => InAView("var status = \"rang \" + name + $\"bed-{name}\";");

    [AcceptedCase]
    public Task a_number_the_view_holds_made_a_text_by_its_own_method_is_accepted()
        => InAView("var status = beds.ToString();");

    [AcceptedCase]
    public Task a_text_the_view_holds_given_to_a_format_is_accepted()
        => InAView("var status = string.Format(\"rang {0}\", name);");

    [AcceptedCase]
    public Task a_method_of_a_source_other_than_its_own_text_is_accepted()
        => InAView("var code = bed.GetHashCode();");

    [AcceptedCase]
    public Task a_source_compared_by_reference_makes_no_text_and_is_accepted()
        => InAView("var same = bed == other;");

    [AcceptedCase]
    public Task a_source_given_to_an_instance_method_of_text_is_accepted()
        => InAView("var same = name.Equals(bed);");

    [AcceptedCase]
    public Task a_source_given_to_a_method_of_another_type_is_accepted()
        => InAView("log.Append(bed); System.Console.WriteLine(bed);");

    [AcceptedCase]
    public Task a_source_a_second_variable_holds_is_accepted()
        => InAView("object boxed = bed; var status = \"rang \" + boxed;");

    [AcceptedCase]
    public Task a_source_an_operator_of_the_apps_own_joins_is_accepted()
        => InAView("var status = shift + bed;");

    [AcceptedCase]
    public Task a_project_that_references_no_core_is_accepted()
        => new Case<ALIS043TextIsJoinedByConcat>()
            .WithFile("Rounds/CallBell.cs", "namespace TestProject.Rounds; public static class CallBell { public static string Ring(int bed) => \"rang \" + bed; }")
            .WithoutTheCore()
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => InAView("string status = {|CS0029:bed|};");

    private static Task InAView(string statements, params DiagnosticResult[] reports)
        => new Case<ALIS043TextIsJoinedByConcat>()
            .WithFile("Rounds/CallBell.cs",
                $$"""
                using System.Text;
                using Alis.Reactive.TriggersAndPipelines;
                using Alis.Reactive.ValuesAndSources;

                namespace TestProject.Rounds;

                public sealed class Ward;

                public readonly struct Shift
                {
                    public static string operator +(Shift shift, TypedSource<string> bed) => "";
                }

                public static class CallBell
                {
                    public static void Ring(Pipeline<Ward> pipeline, TypedSource<string> bed, TypedSource<int> count, TypedSource<string> other, ITypedSource<string> held, string name, int beds, string text, StringBuilder log, Shift shift)
                    {
                        {{statements}}
                    }
                }
                """)
            .Expecting(reports)
            .RunAsync();

    private static DiagnosticResult MadeIntoText(int at, string source)
        => new DiagnosticResult(ALIS043TextIsJoinedByConcat.Id, DiagnosticSeverity.Error)
            .WithLocation(at)
            .WithMessage($"'{source}' is a source, read in the browser when its step runs, and .NET makes a text of it here, the name of its type; pass '{source}' itself where a source is taken, and join it into a text with pipeline.Concat");
}
