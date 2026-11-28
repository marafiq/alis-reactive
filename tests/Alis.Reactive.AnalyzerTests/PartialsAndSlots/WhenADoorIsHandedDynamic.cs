using System.Threading.Tasks;
using Alis.Reactive.Analyzers.PartialsAndSlots;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.PartialsAndSlots;

[TestFixture]
public sealed class WhenADoorIsHandedDynamic
{
    private const string AViewAsRazorWritesIt = "Microsoft.CodeAnalysis.Razor.Compiler/Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator/Home/Index_cshtml.g.cs";

    [RefusedCase]
    public Task a_plan_opened_over_dynamic_is_refused_at_its_type_argument()
        => InAViewWithNoModel(
            "var plan = Html.ReactivePlan<{|#0:dynamic|}>();",
            HandedAnUntypedType(0, "Html.ReactivePlan", "dynamic", "TModel"));

    [RefusedCase]
    public Task a_plan_a_view_with_no_model_opens_is_refused_at_the_door()
        => InAViewWithNoModel(
            "var plan = Html.{|#0:ReactivePlan|}();",
            HandedAnUntypedType(0, "Html.ReactivePlan", "dynamic", "TModel"));

    [RefusedCase]
    public Task a_plan_opened_over_object_is_refused_at_its_type_argument()
        => InAViewOverObject(
            "var plan = Html.ReactivePlan<{|#0:object|}>();",
            HandedAnUntypedType(0, "Html.ReactivePlan", "object", "TModel"));

    [RefusedCase]
    public Task a_plan_a_view_over_object_opens_is_refused_at_the_door()
        => InAViewOverObject(
            "var plan = Html.{|#0:ReactivePlan|}();",
            HandedAnUntypedType(0, "Html.ReactivePlan", "object", "TModel"));

    [RefusedCase]
    public Task another_door_handed_dynamic_is_refused_at_its_type_argument()
        => InAViewOverAWard(
            "var rang = AppEvent.Declare<{|#0:dynamic|}>();",
            HandedAnUntypedType(0, "AppEvent.Declare", "dynamic", "TPayload"));

    [RefusedCase]
    public Task every_door_after_a_plan_over_dynamic_is_refused_at_its_name()
        => InAViewWithNoModel(
            """
            var plan = Html.ReactivePlan<{|#0:dynamic|}>();
            Html.{|#1:On|}(plan, trigger => trigger.DomReady(pipeline => pipeline.{|#2:Element|}("status").Text.Set("Booted")));
            Html.{|#3:RenderPlan|}(plan);
            """,
            HandedAnUntypedType(0, "Html.ReactivePlan", "dynamic", "TModel"),
            HandedAnUntypedType(1, "Html.On", "dynamic", "TModel"),
            HandedAnUntypedType(2, "pipeline.Element", "dynamic", "TModel"),
            HandedAnUntypedType(3, "Html.RenderPlan", "dynamic", "TModel"));

    [RefusedCase]
    public Task a_door_handed_a_plan_the_apps_own_generic_method_opened_over_dynamic_is_refused_at_the_door()
        => InAViewWithNoModel(
            """
            var plan = Open(Html);
            Html.{|#0:On|}(plan, trigger => { });

            static ReactivePlan<T> Open<T>(IHtmlHelper<T> html) => html.ReactivePlan<T>();
            """,
            HandedAnUntypedType(0, "Html.On", "dynamic", "TModel"));

    [RefusedCase]
    public Task a_rule_over_a_member_typed_dynamic_is_refused_at_the_door()
        => InAValidator(
            "{|#0:RuleFor|}(ward => ward.Notes);",
            HandedAnUntypedType(0, "RuleFor", "dynamic", "TProperty"));

    [RefusedCase]
    public Task a_validator_made_over_dynamic_is_refused_at_its_type_argument()
        => InAViewOverAWard(
            "var rules = new Alis.Reactive.Validation.InlineValidator<{|#0:dynamic|}>();",
            HandedAnUntypedType(0, "InlineValidator", "dynamic", "T"));

    [RefusedCase]
    public Task a_validator_made_over_the_dynamic_its_variable_names_is_refused_at_its_creation()
        => InAViewOverAWard(
            "InlineValidator<dynamic> rules = {|#0:new()|};",
            HandedAnUntypedType(0, "InlineValidator", "dynamic", "T"));

    [RefusedCase]
    public Task a_door_of_another_assembly_under_the_frameworks_namespace_is_refused_at_its_type_argument()
        => new Case<ALIS044DoorTakesNoDynamic>()
            .WithAnotherProject("Chimes", "/Chimes/Bells.cs", "namespace Alis.Reactive.Chimes; public static class Bells { public static T Ring<T>() => default!; }")
            .WithFile("Rounds/Ward.cs", "namespace TestProject.Rounds; public static class Ward { public static object? Call() => Alis.Reactive.Chimes.Bells.Ring<{|#0:dynamic|}>(); }")
            .Expecting(HandedAnUntypedType(0, "Alis.Reactive.Chimes.Bells.Ring", "dynamic", "T"))
            .RunAsync();

    [RefusedCase]
    public Task a_door_handed_a_member_of_a_dynamic_model_is_refused_at_the_argument()
        => InAViewWithNoModel(
            """
            var plan = Html.{|#0:ReactivePlan|}();
            Html.{|#1:On|}(plan, trigger => trigger.DomReady(pipeline => pipeline.{|#2:Element|}("status").Text.Set({|#3:Model.Name|})));
            """,
            HandedAnUntypedType(0, "Html.ReactivePlan", "dynamic", "TModel"),
            HandedAnUntypedType(1, "Html.On", "dynamic", "TModel"),
            HandedAnUntypedType(2, "pipeline.Element", "dynamic", "TModel"),
            BoundAtRunTime(3, "Set", "Model.Name"));

    [RefusedCase]
    public Task a_door_handed_a_value_of_the_view_bag_is_refused_at_the_argument()
        => InAViewOverAWard(
            """
            var plan = Html.ReactivePlan<Ward>();
            Html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("status").Text.Set({|#0:ViewBag.Status|})));
            """,
            BoundAtRunTime(0, "Set", "ViewBag.Status"));

    [RefusedCase]
    public Task a_door_called_on_a_dynamic_receiver_written_as_its_first_argument_is_refused_at_the_receiver()
        => InAViewWithNoModel(
            "var plan = PlanHtml.ReactivePlan<Ward>({|#0:Model.Html|});",
            BoundAtRunTime(0, "ReactivePlan", "Model.Html"));

    [RefusedCase]
    public Task a_constructor_handed_a_dynamic_argument_is_refused_at_that_argument_alone()
        => InAViewWithNoModel(
            "var mirrored = new MirrorsAttribute({|#0:Model.Module|}, \"bell\");",
            BoundAtRunTime(0, "MirrorsAttribute", "Model.Module"));

    [RefusedCase]
    public Task a_view_razor_compiled_with_no_model_is_refused_at_each_door_and_where_its_model_is_read()
        => new Case<ALIS044DoorTakesNoDynamic>()
            .WithFile(AViewAsRazorWritesIt,
                """
                namespace AspNetCoreGeneratedDocument
                {
                    using global::Alis.Reactive.RenderingAndDelivery;
                    using global::Alis.Reactive.TriggersAndPipelines;

                    internal sealed class Home_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
                    {
                        public async override global::System.Threading.Tasks.Task ExecuteAsync()
                        {
                            var plan = Html.{|#0:ReactivePlan|}();
                            WriteLiteral("<p id=\"status\">Not booted</p>");
                            Html.{|#1:On|}(plan, trigger => trigger.DomReady(pipeline => pipeline.{|#2:Element|}("status").Text.Set({|#3:Model.Name|})));
                            Write(Html.{|#4:RenderPlan|}(plan));
                        }

                        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; } = default!;
                    }
                }
                """)
            .Expecting(
                HandedAnUntypedType(0, "Html.ReactivePlan", "dynamic", "TModel"),
                HandedAnUntypedType(1, "Html.On", "dynamic", "TModel"),
                HandedAnUntypedType(2, "pipeline.Element", "dynamic", "TModel"),
                BoundAtRunTime(3, "Set", "Model.Name"),
                HandedAnUntypedType(4, "Html.RenderPlan", "dynamic", "TModel"))
            .RunAsync();

    [AcceptedCase]
    public Task a_plan_over_a_class_is_accepted()
        => InAViewOverAWard(
            """
            var plan = Html.ReactivePlan<Ward>();
            Html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("status").Text.Set("Booted")));
            Html.RenderPlan(plan);
            """);

    [AcceptedCase]
    public Task a_plan_a_view_with_a_model_opens_is_accepted()
        => InAViewOverAWard("var plan = Html.ReactivePlan();");

    [AcceptedCase]
    public Task another_door_handed_object_is_accepted()
        => InAViewOverAWard("var rang = AppEvent.Declare<object>();");

    [AcceptedCase]
    public Task a_door_handed_a_member_of_a_typed_model_is_accepted()
        => InAViewOverAWard(
            """
            var plan = Html.ReactivePlan<Ward>();
            Html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("status").Text.Set(Model.Name)));
            """);

    [AcceptedCase]
    public Task a_rule_over_a_member_typed_as_text_is_accepted()
        => InAValidator("RuleFor(ward => ward.Name);");

    [AcceptedCase]
    public Task a_validator_made_over_a_class_is_accepted()
        => InAViewOverAWard("var rules = new Alis.Reactive.Validation.InlineValidator<Ward>();");

    [AcceptedCase]
    public Task a_method_outside_the_framework_handed_dynamic_is_accepted()
        => InAViewOverAWard("var read = System.Text.Json.JsonSerializer.Deserialize<dynamic>(\"{}\");");

    [AcceptedCase]
    public Task a_constructor_outside_the_framework_handed_dynamic_is_accepted()
        => InAViewOverAWard("var held = new System.Collections.Generic.List<dynamic>();");

    [AcceptedCase]
    public Task a_call_outside_the_framework_bound_at_run_time_is_accepted()
        => InAViewOverAWard("System.Console.WriteLine(ViewBag.Status);");

    [AcceptedCase]
    public Task a_constructor_outside_the_framework_bound_at_run_time_is_accepted()
        => InAViewOverAWard("var text = new System.Text.StringBuilder(ViewBag.Status);");

    [AcceptedCase]
    public Task a_method_the_app_declares_under_the_frameworks_namespace_is_accepted()
        => new Case<ALIS044DoorTakesNoDynamic>()
            .WithFile("Chimes/Bells.cs", "namespace Alis.Reactive.Chimes; public static class Bells { public static T Ring<T>() => default!; }")
            .WithFile("Rounds/Ward.cs", "namespace TestProject.Rounds; public static class Ward { public static object? Call() => Alis.Reactive.Chimes.Bells.Ring<dynamic>(); }")
            .RunAsync();

    [AcceptedCase]
    public Task a_project_that_references_no_core_is_read_by_no_rule()
        => new Case<ALIS044DoorTakesNoDynamic>()
            .WithFile("Rounds/Ward.cs", "namespace TestProject.Rounds; public static class Ward { public static int Count() => new System.Collections.Generic.List<dynamic>().Count; }")
            .WithoutTheCore()
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => InAViewOverAWard(
            """
            var plan = Html.ReactivePlan<Ward>();
            Html.On(plan, trigger => trigger.DomReady(pipeline => {|CS1973:pipeline.Element(ViewBag.Status)|}.Text.Set("Booted")));
            """);

    private static Task InAViewWithNoModel(string statements, params DiagnosticResult[] reports)
        => InAView("IHtmlHelper<dynamic> Html, dynamic Model", statements, reports);

    private static Task InAViewOverObject(string statements, params DiagnosticResult[] reports)
        => InAView("IHtmlHelper<object> Html, object Model", statements, reports);

    private static Task InAViewOverAWard(string statements, params DiagnosticResult[] reports)
        => InAView("IHtmlHelper<Ward> Html, Ward Model, dynamic ViewBag", statements, reports);

    private static Task InAView(string parameters, string statements, DiagnosticResult[] reports)
        => new Case<ALIS044DoorTakesNoDynamic>()
            .WithFile("Rounds/View.cs",
                $$"""
                using Alis.Reactive.Declarations;
                using Alis.Reactive.RenderingAndDelivery;
                using Alis.Reactive.TriggersAndPipelines;
                using Alis.Reactive.Validation;
                using Microsoft.AspNetCore.Mvc.Rendering;

                namespace TestProject.Rounds;

                public sealed class Ward
                {
                    public string Name { get; set; } = "";
                }

                public static class View
                {
                    public static void Execute({{parameters}})
                    {
                        {{statements}}
                    }
                }
                """)
            .Expecting(reports)
            .RunAsync();

    private static Task InAValidator(string rules, params DiagnosticResult[] reports)
        => new Case<ALIS044DoorTakesNoDynamic>()
            .WithFile("Rounds/WardValidator.cs",
                $$"""
                using Alis.Reactive.Validation;

                namespace TestProject.Rounds;

                public sealed class Ward
                {
                    public string Name { get; set; } = "";

                    public dynamic Notes { get; set; } = "";
                }

                public sealed class WardValidator : ReactiveValidator<Ward>
                {
                    public WardValidator()
                    {
                        {{rules}}
                    }
                }
                """)
            .Expecting(reports)
            .RunAsync();

    private static DiagnosticResult HandedAnUntypedType(int at, string door, string untyped, string typeParameter)
        => Refused(at, $"{door} is handed {untyped} as {typeParameter}: a door is typed by the type it is handed, and {untyped} leaves the compiler nothing to check; name a class the app wrote, the view's @model for a plan");

    private static DiagnosticResult BoundAtRunTime(int at, string door, string dynamicValue)
        => Refused(at, $"{door} is bound at run time because {dynamicValue} is dynamic, so the compiler checks none of it; give the view its model with @model, and its Model is typed");

    private static DiagnosticResult Refused(int at, string message)
        => new DiagnosticResult(ALIS044DoorTakesNoDynamic.Id, DiagnosticSeverity.Error).WithLocation(at).WithMessage(message);
}
