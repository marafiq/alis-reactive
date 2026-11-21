using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Vendors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenADeclarationListens
{
    private const string Type = "format\t1\ntype\tg\tapi\tGauge\tinterface\tGauge\n";
    private const string Methods = Type + "member\ton\tg\ton\tmethod\tfalse\tfalse\tfalse\ttrue\nmember\toff\tg\toff\tmethod\tfalse\tfalse\tfalse\ttrue\n";
    private const string Returned = Methods + "signature\ton/s0\tmember\ton\t0\tnone\tfalse\tfalse\nholds\ton/s0/returns/0\tsignature\ton/s0\treturns\t0\tfunction\tremover\t\n";
    private const string Callback = Type + "return-type\traised/s0\tvoid\nmember\traised\tg\traised\tproperty\tfalse\tfalse\tfalse\ttrue\nsignature\traised/s0\tmember\traised\t0\tnone\tfalse\ttrue\nholds\traised/s0/returns/0\tsignature\traised/s0\treturns\t0\tother\t\tvoid\n";

    [RefusedCase]
    public Task on_must_be_public()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.Define(\"g\", \"\", {|#0:\"on\"|}, \"off\")", Methods.Replace("member\ton\tg\ton\tmethod\tfalse\tfalse\tfalse\ttrue\n", ""), "ALIS016", "Declaration 'Gauge' listens through 'on', which type 'Gauge' in module 'api' and its bases do not make public; name a public listening member");

    [AcceptedCase]
    public Task on_can_be_public()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.Define(\"g\", \"\", \"on\", \"off\")", Methods, "ALIS016");

    [RefusedCase]
    public Task on_must_be_callable()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.Define(\"g\", \"\", {|#0:\"on\"|}, \"off\")", Methods.Replace("\ton\tmethod\tfalse\tfalse\tfalse\ttrue", "\ton\tproperty\tfalse\tfalse\tfalse\tfalse"), "ALIS018", "Declaration 'Gauge' listens through 'on', which cannot be called; name a callable listening member");

    [AcceptedCase]
    public Task on_can_be_a_callable_property()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.Define(\"g\", \"\", \"on\", \"off\")", Methods.Replace("\ton\tmethod\t", "\ton\tproperty\t"), "ALIS018");

    [RefusedCase]
    public Task off_must_be_public()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.Define(\"g\", \"\", \"on\", {|#0:\"off\"|})", Methods.Replace("member\toff\tg\toff\tmethod\tfalse\tfalse\tfalse\ttrue\n", ""), "ALIS016", "Declaration 'Gauge' listens through 'off', which type 'Gauge' in module 'api' and its bases do not make public; name a public listening member");

    [AcceptedCase]
    public Task off_can_be_public()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.Define(\"g\", \"\", \"on\", \"off\")", Methods, "ALIS016");

    [RefusedCase]
    public Task off_must_be_callable()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.Define(\"g\", \"\", \"on\", {|#0:\"off\"|})", Methods.Replace("\toff\tmethod\tfalse\tfalse\tfalse\ttrue", "\toff\tproperty\tfalse\tfalse\tfalse\tfalse"), "ALIS018", "Declaration 'Gauge' listens through 'off', which cannot be called; name a callable listening member");

    [AcceptedCase]
    public Task off_can_be_a_callable_property()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.Define(\"g\", \"\", \"on\", \"off\")", Methods.Replace("\toff\tmethod\t", "\toff\tproperty\t"), "ALIS018");

    [RefusedCase]
    public Task a_returned_subscribe_must_be_public()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|#0:\"on\"|})", Type, "ALIS016", "Declaration 'Gauge' listens through 'on', which type 'Gauge' in module 'api' and its bases do not make public; name a public listening member");

    [AcceptedCase]
    public Task a_returned_subscribe_can_be_public()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Returned, "ALIS016");

    [RefusedCase]
    public Task a_returned_subscribe_must_be_callable()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|#0:\"on\"|})", Returned.Replace("\ton\tmethod\tfalse\tfalse\tfalse\ttrue", "\ton\tmethod\tfalse\tfalse\tfalse\tfalse"), "ALIS018", "Declaration 'Gauge' listens through 'on', which cannot be called; name a callable listening member");

    [AcceptedCase]
    public Task a_returned_subscribe_can_be_callable()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Returned, "ALIS018");

    [RefusedCase]
    public Task a_returned_subscribe_must_return_a_function()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|#0:\"on\"|})", Returned.Replace("\tfunction\tremover\t", "\tother\t\tnumber"), "ALIS018", "Declaration 'Gauge' listens through 'on', whose return is not only a remover function; name a subscribe method that returns a function and nothing else");

    [AcceptedCase]
    public Task a_returned_subscribe_can_return_a_function()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Returned, "ALIS018");

    [RefusedCase]
    public Task a_returned_subscribe_cannot_return_a_promise()
        => Component<ALIS017MethodMatchesPromise>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|#0:\"on\"|})", Returned.Replace("\tnone\t", "\tpromise\t"), "ALIS017", "Declaration 'Gauge' listens through 'on', whose signatures return a promise; name a subscribe method that returns its remover synchronously");

    [AcceptedCase]
    public Task a_returned_subscribe_returns_synchronously()
        => Component<ALIS017MethodMatchesPromise>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Returned, "ALIS017");

    [RefusedCase]
    public Task a_returned_subscribe_cannot_return_null()
        => Component<ALIS019ValueNullability>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|#0:\"on\"|})", Returned.Replace("\tnone\tfalse\tfalse", "\tnone\ttrue\tfalse"), "ALIS019", "Declaration 'Gauge' listens through 'on', whose return admits null or undefined; name a subscribe method that always returns its remover");

    [AcceptedCase]
    public Task a_returned_subscribe_always_returns_its_remover()
        => Component<ALIS019ValueNullability>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Returned, "ALIS019");

    [RefusedCase]
    public Task a_callback_event_must_be_writable()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Callback.Replace("\tproperty\t", "\treadonly\t"), "ALIS018", "Declaration member 'Gauge.Raised' listens by assignment to 'raised', which is not a writable callable property; name a writable callable event property", "public Event Raised => {|#0:Event|}();");

    [RefusedCase]
    public Task callback_listening_needs_its_property_even_when_a_method_names_the_event()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Methods + "event\ton\ton/s0\tname\traised\tlistener/s0\n", "ALIS018", "Declaration member 'Gauge.Raised' listens by assignment to 'raised', which is not a writable callable property; name a writable callable event property", "public Event Raised => {|#0:Event|}();");

    [AcceptedCase]
    public Task a_callback_event_can_be_writable()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Callback, "ALIS018");

    [RefusedCase]
    public Task a_callback_event_must_be_a_property()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Callback.Replace("\tproperty\t", "\tmethod\t"), "ALIS018", "Declaration member 'Gauge.Raised' listens by assignment to 'raised', which is not a writable callable property; name a writable callable event property", "public Event Raised => {|#0:Event|}();");

    [RefusedCase]
    public Task a_callback_event_must_hold_a_function()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Callback.Replace("\tfalse\tfalse\tfalse\ttrue", "\tfalse\tfalse\tfalse\tfalse"), "ALIS018", "Declaration member 'Gauge.Raised' listens by assignment to 'raised', which is not a writable callable property; name a writable callable event property", "public Event Raised => {|#0:Event|}();");

    [RefusedCase]
    public Task a_callback_answer_is_not_an_event()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Callback.Replace("\tnone\tfalse\ttrue", "\tnone\tfalse\tfalse").Replace("\tvoid\n", "\tnumber\n"), "ALIS018", "'raised' returns number, an answer its vendor reads, and a handler answers nothing. Declare it as Callback<TArgs, TResult> and write the answer as an expression.", "public Event Raised => {|#0:Event|}();");

    [AcceptedCase]
    public Task a_callback_return_can_be_void()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Callback, "ALIS018");

    [AcceptedCase]
    public Task a_callback_return_can_be_undefined()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Callback.Replace("\tvoid\n", "\tundefined\n"), "ALIS018");

    [AcceptedCase]
    public Task a_callback_return_can_be_any()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Callback.Replace("\tvoid\n", "\tany\n"), "ALIS018");

    [AcceptedCase]
    public Task a_callback_return_can_be_unknown()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Callback.Replace("\tvoid\n", "\tunknown\n"), "ALIS018");

    [AcceptedCase]
    public Task a_callback_return_can_be_number_or_undefined()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Callback.Replace("\tvoid\n", "\tnumber | undefined\n"), "ALIS018");

    [AcceptedCase]
    public Task callback_nullability_is_not_its_answer()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Callback.Replace("\tfalse\tfalse\tfalse\ttrue", "\tfalse\ttrue\ttrue\ttrue"), "ALIS018");

    [AcceptedCase]
    public Task method_listening_does_not_assign_a_callback()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.Define(\"g\", \"\", \"on\", \"off\")", Callback.Replace("\tnone\tfalse\ttrue", "\tnone\tfalse\tfalse").Replace("\tvoid\n", "\tnumber\n"), "ALIS018");

    [AcceptedCase]
    public Task a_callback_without_signatures_proves_no_answer()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", Type + "member\traised\tg\traised\tproperty\tfalse\tfalse\tfalse\ttrue\n", "ALIS018");

    [AcceptedCase]
    public Task alis016_no_table_means_no_listening_check()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", null, "ALIS016");

    [AcceptedCase]
    public Task alis016_a_declaration_without_events_needs_no_listening()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Type, "ALIS016", null, "");

    [AcceptedCase]
    public Task alis016_invalid_listening_keeps_the_compiler_error()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|CS0103:Unknown|})", Returned, "ALIS016");

    [AcceptedCase]
    public Task alis018_no_table_means_no_listening_check()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", null, "ALIS018");

    [AcceptedCase]
    public Task alis018_a_declaration_without_events_needs_no_listening()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Type, "ALIS018", null, "");

    [AcceptedCase]
    public Task alis018_invalid_listening_keeps_the_compiler_error()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|CS0103:Unknown|})", Returned, "ALIS018");

    [AcceptedCase]
    public Task alis017_no_table_means_no_listening_check()
        => Component<ALIS017MethodMatchesPromise>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", null, "ALIS017");

    [AcceptedCase]
    public Task alis017_a_declaration_without_events_needs_no_listening()
        => Component<ALIS017MethodMatchesPromise>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Type, "ALIS017", null, "");

    [AcceptedCase]
    public Task alis017_invalid_listening_keeps_the_compiler_error()
        => Component<ALIS017MethodMatchesPromise>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|CS0103:Unknown|})", Returned, "ALIS017");

    [AcceptedCase]
    public Task alis019_no_table_means_no_listening_check()
        => Component<ALIS019ValueNullability>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", null, "ALIS019");

    [AcceptedCase]
    public Task alis019_a_declaration_without_events_needs_no_listening()
        => Component<ALIS019ValueNullability>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Type, "ALIS019", null, "");

    [AcceptedCase]
    public Task alis019_invalid_listening_keeps_the_compiler_error()
        => Component<ALIS019ValueNullability>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|CS0103:Unknown|})", Returned, "ALIS019");

    [AcceptedCase]
    public Task a_dynamic_subscribe_is_left_to_the_browser()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.Define(\"g\", \"\", Name, \"off\")", Type, "ALIS016", null, "private string Name => \"on\"; public Event Raised => Event();");

    [AcceptedCase]
    public Task a_dynamic_vendor_is_left_to_the_browser()
        => Component<ALIS016PublicMemberExists>("Make()", Type, "ALIS016", null, "private ComponentVendor Make() => ComponentVendor.Define(\"g\", \"\", \"on\", \"off\"); public Event Raised => Event();");

    [RefusedCase]
    public Task native_listening_uses_event_target_members()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.{|#0:Native|}", Methods.Replace("\ton\t", "\taddEventListener\t"), "ALIS016", "Declaration 'Gauge' listens through 'removeEventListener', which type 'Gauge' in module 'api' and its bases do not make public; name a public listening member");

    [AcceptedCase]
    public Task native_listening_finds_event_target_members()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.Native", Methods.Replace("\ton\t", "\taddEventListener\t").Replace("\toff\t", "\tremoveEventListener\t"), "ALIS016");

    [RefusedCase]
    public Task a_static_vendor_property_initializer_is_read()
        => Component<ALIS016PublicMemberExists>("Declared", Methods.Replace("member\toff\tg\toff\tmethod\tfalse\tfalse\tfalse\ttrue\n", ""), "ALIS016", "Declaration 'Gauge' listens through 'off', which type 'Gauge' in module 'api' and its bases do not make public; name a public listening member", "public static ComponentVendor Declared { get; } = ComponentVendor.Define(\"g\", \"\", \"on\", {|#0:\"off\"|}); public Event Raised => Event();");

    [AcceptedCase]
    public Task a_computed_static_vendor_property_is_not_followed()
        => Component<ALIS016PublicMemberExists>("Declared", Methods.Replace("member\toff\tg\toff\tmethod\tfalse\tfalse\tfalse\ttrue\n", ""), "ALIS016", null, "public static ComponentVendor Declared => ComponentVendor.Define(\"g\", \"\", \"on\", \"off\"); public Event Raised => Event();");

    [AcceptedCase]
    public Task a_static_vendor_field_initializer_is_read()
        => Component<ALIS016PublicMemberExists>("Declared", Methods, "ALIS016", null, "public static readonly ComponentVendor Declared = ComponentVendor.Define(\"g\", \"\", \"on\", \"off\"); public Event Raised => Event();");

    [RefusedCase]
    public Task a_plugins_explicit_methods_are_read()
        => Source<ALIS016PublicMemberExists>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : Plugin { public Gauge() : base(\"g\", EventListening.Methods(\"on\", {|#0:\"off\"|})) { } public Event Raised => Event(); }", Methods.Replace("member\toff\tg\toff\tmethod\tfalse\tfalse\tfalse\ttrue\n", ""), "ALIS016", "Declaration 'Gauge' listens through 'off', which type 'Gauge' in module 'api' and its bases do not make public; name a public listening member");

    [AcceptedCase]
    public Task a_plugins_explicit_methods_can_match()
        => Source<ALIS016PublicMemberExists>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : Plugin { public Gauge() : base(\"g\", EventListening.Methods(\"on\", \"off\")) { } public Event Raised => Event(); }", Methods, "ALIS016");

    [RefusedCase]
    public Task a_plugins_default_listening_is_event_target()
        => Source<ALIS016PublicMemberExists>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : Plugin { public Gauge() : {|#0:base|}(\"g\") { } public Event Raised => Event(); }", Methods.Replace("\ton\t", "\taddEventListener\t"), "ALIS016", "Declaration 'Gauge' listens through 'removeEventListener', which type 'Gauge' in module 'api' and its bases do not make public; name a public listening member");

    [AcceptedCase]
    public Task a_plugins_default_listening_can_match()
        => Source<ALIS016PublicMemberExists>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : Plugin { public Gauge() : base(\"g\") { } public Event Raised => Event(); }", Methods.Replace("\ton\t", "\taddEventListener\t").Replace("\toff\t", "\tremoveEventListener\t"), "ALIS016");

    [RefusedCase]
    public Task an_evented_objects_override_is_read()
        => Source<ALIS016PublicMemberExists>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : EventedObject { protected override EventListening Listening => EventListening.Methods(\"on\", {|#0:\"off\"|}); public Event Raised => Event(); }", Methods.Replace("member\toff\tg\toff\tmethod\tfalse\tfalse\tfalse\ttrue\n", ""), "ALIS016", "Declaration 'Gauge' listens through 'off', which type 'Gauge' in module 'api' and its bases do not make public; name a public listening member");

    [AcceptedCase]
    public Task an_evented_objects_override_can_match()
        => Source<ALIS016PublicMemberExists>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : EventedObject { protected override EventListening Listening => EventListening.Methods(\"on\", \"off\"); public Event Raised => Event(); }", Methods, "ALIS016");

    [AcceptedCase]
    public Task a_vendor_from_another_assembly_is_not_followed()
    {
        var test = new Case<ALIS016PublicMemberExists>()
            .WithAnotherProject("Vendor", "/Vendor/HeldVendor.cs", "using Alis.Reactive.Vendors; public static class HeldVendor { public static ComponentVendor Value { get; } = ComponentVendor.Define(\"g\", \"\", \"on\", \"off\"); }")
            .WithFile("Gauge.cs", "using Alis.Reactive.Declarations; using Alis.Reactive.Vendors; [Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : Component { public override ComponentVendor Vendor => HeldVendor.Value; public Event Raised => Event(); }");
        test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", Type));
        return test.RunAsync();
    }

    [RefusedCase]
    public Task a_nullable_return_in_a_later_overload_is_refused()
        => Component<ALIS019ValueNullability>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|#0:\"on\"|})", Returned + "signature\ton/s1\tmember\ton\t1\tnone\ttrue\tfalse\n", "ALIS019", "Declaration 'Gauge' listens through 'on', whose return admits null or undefined; name a subscribe method that always returns its remover");

    [AcceptedCase]
    public Task nonnullable_returns_in_every_overload_are_accepted()
        => Component<ALIS019ValueNullability>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Returned + "signature\ton/s1\tmember\ton\t1\tnone\tfalse\tfalse\n", "ALIS019");

    [RefusedCase]
    public Task a_nonfunction_union_member_is_refused()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|#0:\"on\"|})", Returned + "holds\ton/s0/returns/1\tsignature\ton/s0\treturns\t1\tother\t\tnumber\n", "ALIS018", "Declaration 'Gauge' listens through 'on', whose return is not only a remover function; name a subscribe method that returns a function and nothing else");

    [AcceptedCase]
    public Task a_function_union_member_is_accepted()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Returned + "holds\ton/s0/returns/1\tsignature\ton/s0\treturns\t1\tfunction\tsecond\t\n", "ALIS018");

    [RefusedCase]
    public Task a_later_promised_overload_is_refused()
        => Component<ALIS017MethodMatchesPromise>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", {|#0:\"on\"|})", Returned + "signature\ton/s1\tmember\ton\t1\tpromise\tfalse\tfalse\n", "ALIS017", "Declaration 'Gauge' listens through 'on', whose signatures return a promise; name a subscribe method that returns its remover synchronously");

    [AcceptedCase]
    public Task synchronous_returns_in_every_overload_are_accepted()
        => Component<ALIS017MethodMatchesPromise>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Returned + "signature\ton/s1\tmember\ton\t1\tnone\tfalse\tfalse\n", "ALIS017");

    [RefusedCase]
    public Task a_method_pair_cannot_call_a_writeonly_member()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.Define(\"g\", \"\", {|#0:\"on\"|}, \"off\")", Methods.Replace("\ton\tmethod", "\ton\twriteonly"), "ALIS018", "Declaration 'Gauge' listens through 'on', which cannot be called; name a callable listening member");

    [AcceptedCase]
    public Task a_method_pair_can_call_a_readonly_function()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.Define(\"g\", \"\", \"on\", \"off\")", Methods.Replace("\ton\tmethod", "\ton\treadonly"), "ALIS018");

    [AcceptedCase]
    public Task a_method_pair_does_not_keep_its_return()
        => Component<ALIS019ValueNullability>("ComponentVendor.Define(\"g\", \"\", \"on\", \"off\")", Returned.Replace("\tnone\tfalse\tfalse", "\tnone\ttrue\tfalse"), "ALIS019");

    [AcceptedCase]
    public Task a_method_pair_does_not_require_a_function_return()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.Define(\"g\", \"\", \"on\", \"off\")", Returned.Replace("\tfunction\tremover\t", "\tother\t\tnumber"), "ALIS018");

    [AcceptedCase]
    public Task a_method_pair_does_not_await_its_return()
        => Component<ALIS017MethodMatchesPromise>("ComponentVendor.Define(\"g\", \"\", \"on\", \"off\")", Returned.Replace("\tnone\t", "\tpromise\t"), "ALIS017");

    [AcceptedCase]
    public Task a_returned_subscribe_without_signatures_is_not_guessed()
        => Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineReturnedUnsubscriber(\"g\", \"\", \"on\")", Methods, "ALIS018");

    [AcceptedCase]
    public Task a_listening_member_can_be_inherited()
        => Component<ALIS016PublicMemberExists>("ComponentVendor.Define(\"g\", \"\", \"on\", \"off\")", Type + Methods.Replace("format\t1\n", "").Replace("\tg\t", "\tbase\t").Replace("\tGauge\t", "\tBase\t") + "base\tg\t0\tbase\n", "ALIS016");

    [RefusedCase]
    public Task a_static_vendor_field_reports_its_name_argument()
        => Component<ALIS016PublicMemberExists>("Declared", Methods.Replace("member\toff\tg\toff\tmethod\tfalse\tfalse\tfalse\ttrue\n", ""), "ALIS016", "Declaration 'Gauge' listens through 'off', which type 'Gauge' in module 'api' and its bases do not make public; name a public listening member", "private static readonly ComponentVendor Declared = ComponentVendor.Define(\"g\", \"\", \"on\", {|#0:\"off\"|}); public Event Raised => Event();");

    [AcceptedCase]
    public Task a_static_vendor_property_can_match()
        => Component<ALIS016PublicMemberExists>("Declared", Methods, "ALIS016", null, "private static ComponentVendor Declared { get; } = ComponentVendor.Define(\"g\", \"\", \"on\", \"off\"); public Event Raised => Event();");

    [RefusedCase]
    public Task a_primary_plugin_constructor_is_read()
        => Source<ALIS016PublicMemberExists>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge() : Plugin(\"g\", EventListening.ReturnedUnsubscriber({|#0:\"on\"|})) { public Event Raised => Event(); }", Type, "ALIS016", "Declaration 'Gauge' listens through 'on', which type 'Gauge' in module 'api' and its bases do not make public; name a public listening member");

    [AcceptedCase]
    public Task a_primary_plugin_constructor_can_match()
        => Source<ALIS016PublicMemberExists>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge() : Plugin(\"g\", EventListening.ReturnedUnsubscriber(\"on\")) { public Event Raised => Event(); }", Returned, "ALIS016");

    [RefusedCase]
    public Task an_evented_callback_cannot_answer_its_vendor()
        => Source<ALIS018DoorMatchesMemberKind>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : EventedObject { protected override EventListening Listening => EventListening.CallbackProperties; public Event Raised => {|#0:Event|}(); }", Callback.Replace("\tnone\tfalse\ttrue", "\tnone\tfalse\tfalse").Replace("\tvoid\n", "\tnumber\n"), "ALIS018", "'raised' returns number, an answer its vendor reads, and a handler answers nothing. Declare it as Callback<TArgs, TResult> and write the answer as an expression.");

    [AcceptedCase]
    public Task an_evented_callback_can_answer_nothing()
        => Source<ALIS018DoorMatchesMemberKind>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : EventedObject { protected override EventListening Listening => EventListening.CallbackProperties; public Event Raised => Event(); }", Callback, "ALIS018");

    [RefusedCase]
    public Task a_plugin_callback_cannot_answer_its_vendor()
        => Source<ALIS018DoorMatchesMemberKind>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge() : Plugin(\"g\", EventListening.CallbackProperties) { public Event Raised => {|#0:Event|}(); }", Callback.Replace("\tnone\tfalse\ttrue", "\tnone\tfalse\tfalse").Replace("\tvoid\n", "\tnumber\n"), "ALIS018", "'raised' returns number, an answer its vendor reads, and a handler answers nothing. Declare it as Callback<TArgs, TResult> and write the answer as an expression.");

    [AcceptedCase]
    public Task a_plugin_callback_can_answer_nothing()
        => Source<ALIS018DoorMatchesMemberKind>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge() : Plugin(\"g\", EventListening.CallbackProperties) { public Event Raised => Event(); }", Callback, "ALIS018");

    [RefusedCase]
    public Task a_promised_void_callback_is_an_answer()
        => CallbackContract("Promise<void>", "'raised' returns Promise<void>, an answer its vendor reads, and a handler answers nothing. Declare it as Callback<TArgs, TResult> and write the answer as an expression.");

    [RefusedCase]
    public Task a_mixed_promised_callback_without_undefined_is_an_answer()
        => CallbackContract("Promise<void> | number", "'raised' returns number | Promise<void>, an answer its vendor reads, and a handler answers nothing. Declare it as Callback<TArgs, TResult> and write the answer as an expression.");

    [AcceptedCase]
    public Task a_mixed_promised_callback_with_undefined_can_answer_nothing()
        => CallbackContract("Promise<void> | undefined");

    [AcceptedCase]
    public Task a_real_union_that_takes_undefined_is_an_event()
        => CallbackContract("number | undefined");

    [RefusedCase]
    public Task a_real_union_that_only_takes_null_is_an_answer()
        => CallbackContract("number | null", "'raised' returns number | null, an answer its vendor reads, and a handler answers nothing. Declare it as Callback<TArgs, TResult> and write the answer as an expression.");

    private static async Task CallbackContract(string returned, string? message = null)
    {
        using var program = new VendorProgram("interface Promise<T> { then(done: (value: T) => unknown): unknown; } export interface Gauge { raised: () => " + returned + "; }");
        await Component<ALIS018DoorMatchesMemberKind>("ComponentVendor.DefineCallbackProperties(\"g\", \"\")", string.Join("\n", await program.Extract()), "ALIS018", message, message is null ? "public Event Raised => Event();" : "public Event Raised => {|#0:Event|}();");
    }

    private static Task Component<TAnalyzer>(string definition, string? table, string id, string? message = null, string members = "public Event Raised => Event();")
        where TAnalyzer : DiagnosticAnalyzer, new()
        => Source<TAnalyzer>("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : Component { public override ComponentVendor Vendor => " + definition + "; " + members + " }", table, id, message);

    private static Task Source<TAnalyzer>(string source, string? table, string id, string? message = null)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new Case<TAnalyzer>().WithFile("Gauge.cs", "using Alis.Reactive.Declarations; using Alis.Reactive.Vendors; using Alis.Reactive.Plugins; " + source);
        if (table is not null) test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", table));
        return test.Expecting(message is null ? [] : [new DiagnosticResult(id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message)]).RunAsync();
    }
}
