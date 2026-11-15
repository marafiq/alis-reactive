using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Vendors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAVendorIsDeclared
{
    [RefusedCase]
    public Task a_vendor_name_starts_with_an_ascii_letter()
        => Definition("ComponentVendor.Define({|#0:\"7ward\"|}, \"gauge\", \"on\", \"off\")", "Vendor name '7ward' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen");

    [RefusedCase]
    public Task a_callback_vendor_name_contains_no_dot()
        => Definition("ComponentVendor.DefineCallbackProperties({|#0:\"ward.gauge\"|}, \"gauge\")", "Vendor name 'ward.gauge' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen");

    [RefusedCase]
    public Task a_returned_unsubscriber_vendor_has_a_name()
        => Definition("ComponentVendor.DefineReturnedUnsubscriber({|#0:\"\"|}, \"gauge\", \"subscribe\")", "Vendor name '' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen");

    [RefusedCase]
    public Task a_lookup_vendor_name_uses_ascii_letters()
        => Definition("ComponentVendor.Define({|#0:\"éward\"|}, VendorLookup.ByPlugin(\"gauges\"), \"on\", \"off\")", "Vendor name 'éward' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen");

    [AcceptedCase]
    public Task a_vendor_name_takes_letters_digits_underscores_and_hyphens()
        => Definition("ComponentVendor.Define(\"Ward_7-aZ\", \"gauge\", \"on\", \"off\")");

    [AcceptedCase]
    public Task a_single_letter_is_a_vendor_name()
        => Definition("ComponentVendor.DefineCallbackProperties(\"Z\", \"gauge\")");

    [AcceptedCase]
    public Task a_name_the_source_cannot_evaluate_is_left_to_render()
        => Definition("ComponentVendor.Define(System.DateTime.Now.ToString(), \"gauge\", \"on\", \"off\")");

    [RefusedCase]
    public Task a_constant_expression_is_read_as_the_vendor_name()
        => Definition("ComponentVendor.Define({|#0:\"ward\" + \".gauge\"|}, \"gauge\", \"on\", \"off\")", "Vendor name 'ward.gauge' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen");

    [RefusedCase]
    public Task an_object_path_has_no_trailing_empty_step()
        => Definition("ComponentVendor.Define(\"ward\", {|#0:\"slate.\"|}, \"on\", \"off\")", "Path 'slate.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots.");

    [RefusedCase]
    public Task an_object_path_has_no_leading_empty_step()
        => Definition("ComponentVendor.DefineCallbackProperties(\"ward\", {|#0:\".slate\"|})", "Path '.slate' contains an empty segment at index 0. Use dot-separated property names without consecutive, leading, or trailing dots.");

    [RefusedCase]
    public Task an_element_path_has_no_middle_empty_step()
        => Definition("ComponentVendor.DefineReturnedUnsubscriber(\"ward\", \"gauge\", \"subscribe\", elementPath: {|#0:\"host..element\"|})", "Path 'host..element' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots.");

    [AcceptedCase]
    public Task an_empty_object_path_names_the_element_itself()
        => Definition("ComponentVendor.Define(\"ward\", \"\", \"on\", \"off\", elementPath: \"\")");

    [AcceptedCase]
    public Task a_path_can_hold_an_index()
        => Definition("ComponentVendor.Define(\"ward\", \"gauges.0\", \"on\", \"off\", elementPath: \"host.element\")");

    [AcceptedCase]
    public Task a_path_the_source_cannot_evaluate_is_left_to_render()
        => Definition("ComponentVendor.Define(\"ward\", System.DateTime.Now.ToString(), \"on\", \"off\")");

    [RefusedCase]
    public Task two_definitions_of_one_name_cannot_find_different_objects()
        => Two("ComponentVendor.Define(\"ward\", \"first\", \"on\", \"off\")", "ComponentVendor.Define({|#0:\"ward\"|}, \"second\", \"on\", \"off\")");

    [RefusedCase]
    public Task two_definitions_of_one_name_cannot_subscribe_differently()
        => Two("ComponentVendor.Define(\"ward\", \"gauge\", \"on\", \"off\")", "ComponentVendor.Define({|#0:\"ward\"|}, \"gauge\", \"listen\", \"off\")");

    [RefusedCase]
    public Task two_definitions_of_one_name_cannot_unsubscribe_differently()
        => Two("ComponentVendor.Define(\"ward\", \"gauge\", \"on\", \"off\")", "ComponentVendor.Define({|#0:\"ward\"|}, \"gauge\", \"on\", \"remove\")");

    [RefusedCase]
    public Task two_definitions_of_one_name_cannot_hold_different_elements()
        => Two("ComponentVendor.Define(\"ward\", \"gauge\", \"on\", \"off\")", "ComponentVendor.Define({|#0:\"ward\"|}, \"gauge\", \"on\", \"off\", elementPath: \"\")");

    [RefusedCase]
    public Task two_definitions_of_one_name_cannot_use_different_listening_kinds()
        => Two("ComponentVendor.DefineCallbackProperties(\"ward\", \"gauge\")", "ComponentVendor.DefineReturnedUnsubscriber({|#0:\"ward\"|}, \"gauge\", \"on\")");

    [RefusedCase]
    public Task two_definitions_of_one_name_cannot_use_different_lookups()
        => Two("ComponentVendor.DefineCallbackProperties(\"ward\", VendorLookup.ByPlugin(\"first\"))", "ComponentVendor.DefineCallbackProperties({|#0:\"ward\"|}, VendorLookup.ByPlugin(\"second\"))");

    [RefusedCase]
    public Task a_path_and_a_lookup_of_the_same_text_are_different()
        => Two("ComponentVendor.DefineCallbackProperties(\"ward\", \"gauge\")", "ComponentVendor.DefineCallbackProperties({|#0:\"ward\"|}, VendorLookup.ByPlugin(\"gauge\"))");

    [AcceptedCase]
    public Task equal_definitions_share_the_vendor_name()
        => Two("ComponentVendor.Define(\"ward\", \"gauge\", \"on\", \"off\")", "ComponentVendor.Define(unsubscribe: \"off\", name: \"ward\", objectPath: \"gauge\", subscribe: \"on\", elementPath: null)", false);

    [AcceptedCase]
    public Task different_names_can_have_different_definitions()
        => Two("ComponentVendor.DefineCallbackProperties(\"ward\", \"first\")", "ComponentVendor.DefineReturnedUnsubscriber(\"unit\", \"second\", \"on\")", false);

    [AcceptedCase]
    public Task an_unknown_definition_is_left_to_render()
        => Two("ComponentVendor.Define(\"ward\", \"gauge\", \"on\", \"off\")", "ComponentVendor.Define(\"ward\", System.DateTime.Now.ToString(), \"on\", \"off\")", false);

    [RefusedCase]
    public Task a_plain_objects_argument_by_position_is_refused()
        => Declaration("public ReadOnly<string> Text => {|#0:Argument<string>|}(0);", "");

    [RefusedCase]
    public Task a_plain_objects_continuation_by_position_is_refused()
        => Declaration("public void Show() => {|#0:MethodAt|}(1);", "");

    [RefusedCase]
    public Task a_continuation_with_arguments_requires_a_listener_or_callback()
        => Declaration("public void Show(Arg<string> text) => {|#0:MethodAt|}(1, [text]);", "");

    [AcceptedCase]
    public Task an_events_payload_reads_a_positional_argument()
        => Declaration("public ReadOnly<string> Text => Argument<string>(0);", "public Event<Details> Raised => Event<Details>();", false);

    [AcceptedCase]
    public Task a_callbacks_arguments_read_a_positional_argument()
        => Declaration("public ReadOnly<string> Text => Argument<string>(1);", "public Callback<Details, string> Label => Callback<Details, string>();", false);

    [AcceptedCase]
    public Task a_methods_answer_arguments_read_a_positional_argument()
        => Declaration("public ReadOnly<string> Text => Argument<string>(1);", "public void Sort(System.Func<Details, TypedSource<string>> key) => Method([Answer(key)]);", false);

    [AcceptedCase]
    public Task an_events_payload_calls_its_continuation()
        => Declaration("public void Show(Arg<string> text) => MethodAt(1, [text]);", "public Event<Details> Raised => Event<Details>();", false);

    [AcceptedCase]
    public Task a_payloads_base_can_declare_its_arguments()
        => Source("public abstract class Args : JsObject { public ReadOnly<string> Text => Argument<string>(1); } public sealed class Details : Args; public sealed class Gauge : JsObject { public Event<Details> Raised => Event<Details>(); }");

    [AcceptedCase]
    public Task a_method_with_the_same_name_is_not_a_declaration_door()
        => Source("public sealed class Details : JsObject { public string Text => Argument(0); private static string Argument(int position) => position.ToString(); }");

    [AcceptedCase]
    public Task a_project_without_the_core_has_no_vendor_declarations()
        => new Case<ALIS034VendorDeclaration>()
            .WithoutTheCore()
            .WithFile("Ward.cs", "public static class ComponentVendor { public static string Define(string name) => name; public static string Ward => Define(\"ward.gauge\"); }")
            .RunAsync();

    [AcceptedCase]
    public Task an_incomplete_vendor_keeps_its_compiler_error()
        => Definition("ComponentVendor.Define(\"ward.gauge\", (string){|CS0103:Missing|}, \"on\", \"off\")");

    [RefusedCase]
    public Task a_callback_lookup_vendor_name_contains_no_dot()
        => Definition("ComponentVendor.DefineCallbackProperties({|#0:\"ward.gauge\"|}, VendorLookup.ByPlugin(\"gauges\"))", "Vendor name 'ward.gauge' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen");

    [RefusedCase]
    public Task a_returned_lookup_vendor_name_contains_no_dot()
        => Definition("ComponentVendor.DefineReturnedUnsubscriber({|#0:\"ward.gauge\"|}, VendorLookup.ByPlugin(\"gauges\"), \"on\")", "Vendor name 'ward.gauge' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen");

    [RefusedCase]
    public Task a_vendor_name_contains_no_non_ascii_tail()
        => Definition("ComponentVendor.Define({|#0:\"wardé\"|}, \"gauge\", \"on\", \"off\")", "Vendor name 'wardé' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen");

    [RefusedCase]
    public Task a_null_vendor_name_is_refused()
        => Definition("ComponentVendor.Define({|#0:null|}, \"gauge\", \"on\", \"off\")", "Vendor name '' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen");

    [RefusedCase]
    public Task a_known_path_is_checked_when_the_name_is_unknown()
        => Definition("ComponentVendor.Define(System.DateTime.Now.ToString(), {|#0:\"gauge.\"|}, \"on\", \"off\")", "Path 'gauge.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots.");

    [AcceptedCase]
    public Task equal_lookup_definitions_share_the_vendor_name()
        => Two("ComponentVendor.Define(\"ward\", VendorLookup.ByPlugin(\"gauges\"), \"on\", \"off\")", "ComponentVendor.Define(unsubscribe: \"off\", name: \"ward\", lookup: VendorLookup.ByPlugin(\"gauges\"), subscribe: \"on\")", false);

    [AcceptedCase]
    public Task an_unknown_lookup_is_left_to_render()
        => Two("ComponentVendor.DefineCallbackProperties(\"ward\", VendorLookup.ByPlugin(\"gauges\"))", "ComponentVendor.DefineCallbackProperties(\"ward\", VendorLookup.ByPlugin(System.DateTime.Now.ToString()))", false);

    [AcceptedCase]
    public Task an_unknown_subscribe_is_left_to_render()
        => Two("ComponentVendor.Define(\"ward\", \"gauge\", \"on\", \"off\")", "ComponentVendor.Define(\"ward\", \"gauge\", System.DateTime.Now.ToString(), \"off\")", false);

    [AcceptedCase]
    public Task an_ordinary_member_of_a_plain_object_is_not_positional()
        => Declaration("public ReadOnly<string> Text => ReadOnly<string>();", "", false);

    [AcceptedCase]
    public Task a_callbacks_arguments_call_their_continuation()
        => Declaration("public void Show() => MethodAt(1);", "public Callback<Details, string> Label => Callback<Details, string>();", false);

    [AcceptedCase]
    public Task an_unrelated_factory_named_define_is_not_a_vendor()
        => Source("public static class Other { public static string Define(string name, string objectPath) => name; public static string Ward => Define(\"ward.gauge\", \"gauge.\"); }");

    [RefusedCase]
    public Task definitions_in_different_files_still_share_one_name()
        => new Case<ALIS034VendorDeclaration>()
            .WithFile("First.cs", "using Alis.Reactive.Vendors; public static class First { public static ComponentVendor Ward => ComponentVendor.Define(\"ward\", \"first\", \"on\", \"off\"); }")
            .WithFile("Second.cs", "using Alis.Reactive.Vendors; public static class Second { public static ComponentVendor Ward => ComponentVendor.Define({|#0:\"ward\"|}, \"second\", \"on\", \"off\"); }")
            .Expecting(Refused("Two different vendors are named 'ward'. Give each vendor its own name."))
            .RunAsync();

    [AcceptedCase]
    public Task a_held_lookup_is_left_to_render()
        => Source("public static class Vendors { public static VendorLookup Lookup => VendorLookup.ByPlugin(\"gauges\"); public static ComponentVendor First => ComponentVendor.DefineCallbackProperties(\"ward\", \"first\"); public static ComponentVendor Second => ComponentVendor.DefineCallbackProperties(\"ward\", Lookup); }");

    private static Task Definition(string value, string? message = null)
        => Source($"public static class Vendors {{ public static ComponentVendor Ward => {value}; }}", message is null ? [] : [Refused(message)]);

    private static Task Two(string first, string second, bool refused = true)
        => Source($"public static class Vendors {{ public static ComponentVendor First => {first}; public static ComponentVendor Second => {second}; }}", refused ? [Refused("Two different vendors are named 'ward'. Give each vendor its own name.")] : []);

    private static Task Declaration(string member, string use, bool refused = true)
        => Source($"public sealed class Details : JsObject {{ {member} }} public sealed class Gauge : JsObject {{ {use} }}", refused ? [Refused("An argument at a position is what a listener or a callback receives: declare it on an event's payload or a callback's arguments")] : []);

    private static Task Source(string source, params DiagnosticResult[] reports)
        => new Case<ALIS034VendorDeclaration>()
            .WithFile("Ward/Gauges.cs", "using Alis.Reactive.Declarations; using Alis.Reactive.ValuesAndSources; using Alis.Reactive.Vendors; " + source)
            .Expecting(reports)
            .RunAsync();

    private static DiagnosticResult Refused(string message)
        => new DiagnosticResult(ALIS034VendorDeclaration.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message);
}
