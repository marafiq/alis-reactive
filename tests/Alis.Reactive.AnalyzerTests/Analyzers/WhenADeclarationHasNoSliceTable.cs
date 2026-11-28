using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Analyzers;

[TestFixture]
public sealed class WhenADeclarationHasNoSliceTable
{
    [RefusedCase]
    public Task a_live_objects_public_door_is_typed_without_a_table()
        => InADeclaration("public object {|#0:Value|} => 1;", Refused("'Gauge.Value' is a public door and names 'object': a door is typed; write the value's own type, a type parameter or a typed source"));

    [AcceptedCase]
    public Task a_live_objects_typed_door_is_accepted_without_a_table()
        => InADeclaration("public int Value => 1;");

    [RefusedCase]
    public Task a_components_public_door_is_typed_without_a_table()
        => InADeclaration("public void Set(object {|#0:value|}) { }", Refused("'Gauge.Set' is a public door and names 'object': a door is typed; write the value's own type, a type parameter or a typed source"), "Component", "public override ComponentVendor Vendor => ComponentVendor.Native;");

    [RefusedCase]
    public Task a_declaration_inherits_its_reach_through_an_app_base()
        => Source("public abstract class Base : JsObject; public sealed class Gauge : Base { public object {|#0:Value|} => 1; }", Refused("'Gauge.Value' is a public door and names 'object': a door is typed; write the value's own type, a type parameter or a typed source"));

    [RefusedCase]
    public Task a_declarations_protected_door_is_typed()
        => Source("public abstract class Gauge : JsObject { protected abstract void Set(object {|#0:value|}); }", Refused("'Gauge.Set' is a public door and names 'object': a door is typed; write the value's own type, a type parameter or a typed source"));

    [RefusedCase]
    public Task a_declarations_base_arguments_are_typed()
        => Source("public abstract class Base<T> : JsObject; public sealed class {|#0:Gauge|} : Base<object>;", Refused("'Gauge' is a public door and names 'object': a door is typed; write the value's own type, a type parameter or a typed source"));

    [RefusedCase]
    public Task a_render_helpers_signature_is_typed_without_a_table()
        => Source("public static class Helpers { public static Rendered<Gauge, int> Draw(object {|#0:options|}) => throw new System.NotImplementedException(); } public sealed class Gauge : Component { public override ComponentVendor Vendor => ComponentVendor.Native; }", Refused("'Helpers.Draw' is a public door and names 'object': a door is typed; write the value's own type, a type parameter or a typed source"));

    [RefusedCase]
    public Task a_render_helper_takes_no_member_naming_string()
        => Source("public static class Helpers { public static Rendered<Gauge, int> Draw(string {|#0:field|}) => throw new System.NotImplementedException(); } public sealed class Gauge : Component { public override ComponentVendor Vendor => ComponentVendor.Native; }", Refused("'Helpers.Draw' takes the string 'field', named for 'field': a public door names a member, field, event, method or path by a typed lambda or a declaration, never by a string"));

    [AcceptedCase]
    public Task a_render_helpers_typed_signature_is_accepted()
        => Source("public static class Helpers { public static Rendered<Gauge, int> Draw(int options) => throw new System.NotImplementedException(); } public sealed class Gauge : Component { public override ComponentVendor Vendor => ComponentVendor.Native; }");

    [AcceptedCase]
    public Task an_unrelated_door_beside_a_render_helper_stays_outside_the_rule()
        => Source("public static class Helpers { public static Rendered<Gauge, int> Draw(int options) => throw new System.NotImplementedException(); public static object Read(object value) => value; } public sealed class Gauge : Component { public override ComponentVendor Vendor => ComponentVendor.Native; }");

    [AcceptedCase]
    public Task an_unrelated_public_class_stays_outside_the_rule()
        => Source("public sealed class Gauge { public object Value => 1; }");

    [AcceptedCase]
    public Task a_private_door_stays_outside_the_rule()
        => InADeclaration("private object Value => 1;");

    [AcceptedCase]
    public Task a_reason_exempts_a_declarations_door()
        => InADeclaration("[TypedDslExemption(\"The vendor requires an opaque token.\")] public object Value => 1;");

    [RefusedCase]
    public Task an_empty_reason_does_not_exempt_a_declaration()
        => InADeclaration("[TypedDslExemption({|#0:\"\"|})] public int Value => 1;", Refused("An exemption from ALIS009 says why; give TypedDslExemption its reason"));

    [AcceptedCase]
    public Task a_generated_declaration_is_not_read()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithFile("Gauge.g.cs", "public sealed class Gauge : Alis.Reactive.Declarations.JsObject { public object Value => 1; }")
            .RunAsync();

    [AcceptedCase]
    public Task a_project_without_the_core_is_not_a_declaration()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithoutTheCore()
            .WithFile("Gauge.cs", "public class JsObject; public sealed class Gauge : JsObject { public object Value => 1; }")
            .RunAsync();

    [AcceptedCase]
    public Task an_incomplete_declaration_keeps_its_compiler_error()
        => InADeclaration("public {|CS0246:Missing|} Value => throw new System.NotImplementedException();");

    private static Task InADeclaration(string member, DiagnosticResult? report = null, string parent = "JsObject", string vendor = "")
        => Source($"public sealed class Gauge : {parent} {{ {vendor} {member} }}", report is null ? [] : [report.Value]);

    private static Task Source(string source, params DiagnosticResult[] reports)
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithFile("Ward/Gauge.cs", "using Alis.Reactive.Declarations; using Alis.Reactive.Vendors; using Alis.Reactive.RenderingAndDelivery; " + source)
            .Expecting(reports)
            .RunAsync();

    private static DiagnosticResult Refused(string message)
        => new DiagnosticResult(ALIS009PublicDoorIsTyped.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message);
}
