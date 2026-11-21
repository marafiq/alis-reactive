using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Vendors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenACallbackAnswersItsVendor
{
    private const string Step = "A step is written inside a callback's answer, which answers its vendor with one value. Write the step in a pipeline.";
    private const string Wait = "A callback answers its vendor at once, so its answer cannot wait for an async method or a request";

    [RefusedCase]
    public Task an_answer_cannot_write_a_property()
        => InAnAnswer("gauge.Value.{|#0:Set|}(\"changed\"); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_write_a_write_only_property()
        => InAnAnswer("gauge.Output.{|#0:Set|}(\"changed\"); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_call_a_method_that_writes_a_step()
        => InAnAnswer("gauge.{|#0:Ping|}(); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_call_a_continuation()
        => InAnAnswer("args.{|#0:Show|}(); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_write_element_content()
        => InAnAnswer("pipeline.Element(\"status\").Text.{|#0:Set|}(\"changed\"); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_write_a_pipeline_step()
        => InAnAnswer("pipeline.{|#0:Unload|}(\"card\"); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_wait_for_a_method_result()
        => InAnAnswer("return gauge.{|#0:Load|}();", Wait);

    [RefusedCase]
    public Task an_answer_cannot_wait_for_a_method_with_no_result()
        => InAnAnswer("gauge.{|#0:Refresh|}(); return args.Text;", Wait);

    [RefusedCase]
    public Task an_answer_cannot_send_a_request()
        => InAnAnswer("pipeline.{|#0:Get|}(\"/reading\"); return args.Text;", Wait);

    [RefusedCase]
    public Task a_method_argument_used_as_an_answer_cannot_write_steps()
        => InAView("gauge.Sort(args => { gauge.{|#0:Ping|}(); return args.Text; });", Step);

    [RefusedCase]
    public Task a_method_argument_used_as_an_answer_cannot_wait()
        => InAView("gauge.Sort(args => gauge.{|#0:Load|}());", Wait);

    [RefusedCase]
    public Task a_direct_answer_argument_cannot_write_steps()
        => Source("public sealed class Gauge : JsObject { public void Sort() => Method([Answer<Details, string>(args => { {|#0:Method|}(); return args.Text; })]); } public sealed class Details : JsObject { public ReadOnly<string> Text => ReadOnly<string>(); }", Step);

    [RefusedCase]
    public Task a_razor_generated_view_cannot_write_a_step_in_an_answer()
        => InAView("gauge.Label.Set(args => { gauge.{|#0:Ping|}(); return args.Text; });", Step, "Microsoft.CodeAnalysis.Razor.Compiler/Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator/Bench/Answers_cshtml.g.cs");

    [AcceptedCase]
    public Task an_answer_can_read_a_property()
        => InAnAnswer("return gauge.Value;");

    [AcceptedCase]
    public Task an_answer_can_read_a_synchronous_method_result()
        => InAnAnswer("return gauge.Read();");

    [AcceptedCase]
    public Task a_method_argument_can_answer_with_a_synchronous_result()
        => InAView("gauge.Sort(args => gauge.Read());");

    [AcceptedCase]
    public Task an_answer_can_read_the_page()
        => InAnAnswer("return pipeline.Component<GaugeComponent>(\"gauge\").Value;");

    [AcceptedCase]
    public Task a_step_before_the_answer_is_outside_its_body()
        => InAView("gauge.Ping(); gauge.Label.Set(args => args.Text);");

    [AcceptedCase]
    public Task an_async_call_before_the_answer_is_outside_its_body()
        => InAView("gauge.Refresh(); gauge.Label.Set(args => args.Text);");

    [AcceptedCase]
    public Task a_lambda_not_used_as_an_answer_is_not_read()
        => InAView("gauge.Ignore(args => { gauge.Ping(); return args.Text; });");

    [AcceptedCase]
    public Task an_uninvoked_nested_lambda_does_not_write_a_step()
        => InAnAnswer("System.Action later = () => gauge.Ping(); return args.Text;");

    [AcceptedCase]
    public Task a_same_named_method_of_an_unrelated_type_is_not_a_step()
        => InAnAnswer("Other.Set(\"changed\"); return args.Text;");

    [AcceptedCase]
    public Task an_answer_held_by_an_unknown_helper_is_left_to_render()
        => InAView("gauge.Label.Set(gauge.BuildAnswer());");

    [AcceptedCase]
    public Task a_project_without_the_core_has_no_callback_answers()
        => new Case<ALIS032CallbackAnswer>()
            .WithoutTheCore()
            .WithFile("Answers.cs", "public sealed class Callback { public void Set(System.Func<string> answer) { } public void Build() => Set(() => { Set(() => \"value\"); return \"value\"; }); }")
            .RunAsync();

    [AcceptedCase]
    public Task an_incomplete_answer_keeps_its_compiler_error()
        => InAnAnswer("return args.{|CS1061:Missing|};");

    [RefusedCase]
    public Task an_answer_cannot_send_a_head_request()
        => InAnAnswer("pipeline.{|#0:Head|}(\"/reading\"); return args.Text;", Wait);

    [RefusedCase]
    public Task an_answer_cannot_send_an_options_request()
        => InAnAnswer("pipeline.{|#0:Options|}(\"/reading\"); return args.Text;", Wait);

    [RefusedCase]
    public Task an_answer_cannot_append_element_content()
        => InAnAnswer("pipeline.Element(\"status\").Text.{|#0:Append|}(\"changed\"); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_set_another_callback()
        => InAnAnswer("gauge.Label.{|#0:Set|}(other => other.Text); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_write_a_nullable_property_from_a_source()
        => InAnAnswer("gauge.OptionalCount.{|#0:Set|}(gauge.Count); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_open_a_branch()
        => InAnAnswer("pipeline.When(gauge.Ready).{|#0:Then|}(pipeline => { }); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_write_an_else_branch()
        => InAView("var branches = pipeline.When(gauge.Ready).Then(pipeline => { }); gauge.Label.Set(args => { branches.{|#0:Else|}(pipeline => { }); return args.Text; });", Step);

    [RefusedCase]
    public Task an_answer_cannot_run_parallel_steps()
        => InAnAnswer("pipeline.{|#0:Parallel|}(pipeline => { }); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_call_a_function_without_a_result()
        => InAnAnswer("gauge.{|#0:Invoke|}(); return args.Text;", Step);

    [RefusedCase]
    public Task an_answer_cannot_wait_for_a_block_bodied_method()
        => InAnAnswer("return gauge.{|#0:LoadBlock|}();", Wait);

    [RefusedCase]
    public Task a_callback_parameter_can_follow_another_parameter()
        => InAView("gauge.SortAfter(1, args => { gauge.{|#0:Ping|}(); return args.Text; });", Step);

    [AcceptedCase]
    public Task an_unproven_declaration_method_is_left_to_render()
        => InAnAnswer("gauge.Unfollowed(); return args.Text;");

    [AcceptedCase]
    public Task an_uninvoked_local_function_does_not_write_a_step()
        => InAnAnswer("void Later() => gauge.Ping(); return args.Text;");

    [AcceptedCase]
    public Task an_aliased_callback_parameter_is_left_to_render()
        => InAView("gauge.Aliased(args => { gauge.Ping(); return args.Text; });");

    [AcceptedCase]
    public Task a_callback_parameter_with_an_unknown_use_is_left_to_render()
        => InAView("gauge.Escapes(args => { gauge.Ping(); return args.Text; });");

    [RefusedCase]
    public Task an_answer_cannot_send_a_post_request()
        => InAnAnswer("pipeline.{|#0:Post|}(\"/reading\"); return args.Text;", Wait);

    [RefusedCase]
    public Task an_answer_cannot_send_a_put_request()
        => InAnAnswer("pipeline.{|#0:Put|}(\"/reading\"); return args.Text;", Wait);

    [RefusedCase]
    public Task an_answer_cannot_send_a_delete_request()
        => InAnAnswer("pipeline.{|#0:Delete|}(\"/reading\"); return args.Text;", Wait);

    [RefusedCase]
    public Task a_method_argument_cannot_write_a_property()
        => InAView("gauge.Sort(args => { gauge.Value.{|#0:Set|}(\"changed\"); return args.Text; });", Step);

    [RefusedCase]
    public Task an_answer_cannot_wait_for_a_partial_method()
        => Source("public sealed partial class Gauge : JsObject { public partial MethodResult<string> Load(); public partial MethodResult<string> Load() => MethodAsync<string>(); public void Sort() => Method([Answer<Details, string>(args => {|#0:Load|}())]); } public sealed class Details : JsObject;", Wait);

    [RefusedCase]
    public Task an_answer_cannot_call_a_generic_declarations_step()
        => Source("public sealed class Gauge<T> : JsObject { public void Ping() => Method(); public void Sort() => Method([Answer<Details, string>(args => { {|#0:Ping|}(); return args.Text; })]); } public sealed class Details : JsObject { public ReadOnly<string> Text => Argument<string>(0); }", Step);

    [AcceptedCase]
    public Task a_callback_parameter_in_a_field_keeps_its_unknown_use()
        => InAView("gauge.Stored(args => { gauge.Ping(); return args.Text; });");

    [AcceptedCase]
    public Task an_unused_callback_parameter_is_not_an_answer()
        => InAView("gauge.Unused(args => { gauge.Ping(); return args.Text; });");

    [AcceptedCase]
    public Task every_use_of_a_callback_parameter_must_be_an_answer()
        => InAView("gauge.Mixed(args => { gauge.Ping(); return args.Text; });");

    private static Task InAnAnswer(string body, string? message = null)
        => InAView($"gauge.Label.Set(args => {{ {body} }});", message);

    private static Task InAView(string body, string? message = null, string path = "Ward/Answers.cs")
        => new Case<ALIS032CallbackAnswer>()
            .WithFile("Ward/Gauge.cs",
                """
                using System;
                using Alis.Reactive.Declarations;
                using Alis.Reactive.ValuesAndSources;
                using Alis.Reactive.Vendors;

                public sealed class Gauge : JsObject
                {
                    public Property<string> Value => Property<string>();
                    public Property<int?> OptionalCount => Property<int?>();
                    public ReadOnly<int> Count => ReadOnly<int>();
                    public ReadOnly<bool> Ready => ReadOnly<bool>();
                    public WriteOnly<string> Output => WriteOnly<string>();
                    public Callback<Details, string> Label => Callback<Details, string>();
                    public void Ping() => Method();
                    public void Invoke() => CallItself();
                    public void Unfollowed() { }
                    public MethodResult<string> LoadBlock() { return MethodAsync<string>(); }
                    public void SortAfter(Arg<int> count, Func<Details, TypedSource<string>> key) => Method([count, Answer(key)]);
                    public void Aliased(Func<Details, TypedSource<string>> key) { var other = key; Method([Answer(other)]); }
                    public void Escapes(Func<Details, TypedSource<string>> key) { Ignore(key); Method([Answer(key)]); }
                    public MethodResult<string> Read() => Method<string>();
                    public MethodResult<string> Load() => MethodAsync<string>();
                    public void Refresh() => MethodAsync();
                    public void Sort(Func<Details, TypedSource<string>> key) => Method([Answer(key)]);
                    public void Ignore(Func<Details, TypedSource<string>> key) { }
                    public void Unused(Func<Details, TypedSource<string>> key) => Method();
                    public void Mixed(Func<Details, TypedSource<string>> key) => Method([Answer(key), Fixed(key)]);
                    private Func<Details, TypedSource<string>> _stored;
                    public void Stored(Func<Details, TypedSource<string>> key) => Method([Answer(_stored = key)]);
                    public Func<Details, TypedSource<string>> BuildAnswer() => args => args.Text;
                }

                public sealed class Details : JsObject
                {
                    public ReadOnly<string> Text => Argument<string>(0);
                    public void Show() => MethodAt(1);
                }

                public sealed class GaugeComponent : Component
                {
                    public override ComponentVendor Vendor => ComponentVendor.Native;
                    public ReadOnly<string> Value => ReadOnly<string>();
                }

                public static class Other
                {
                    public static void Set(string text) { }
                }
                """)
            .WithFile(path,
                $$"""
                using Alis.Reactive.TriggersAndPipelines;
                using Alis.Reactive.Declarations;

                public static class Answers
                {
                    public static void Build(Gauge gauge, Pipeline<int> pipeline)
                    {
                        {{body}}
                    }
                }
                """)
            .Expecting(message is null ? [] : [Refused(message)])
            .RunAsync();

    private static Task Source(string source, string message)
        => new Case<ALIS032CallbackAnswer>()
            .WithFile("Ward/Answers.cs", "using Alis.Reactive.Declarations; " + source)
            .Expecting(Refused(message))
            .RunAsync();

    private static DiagnosticResult Refused(string message)
        => new DiagnosticResult(ALIS032CallbackAnswer.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message);
}
