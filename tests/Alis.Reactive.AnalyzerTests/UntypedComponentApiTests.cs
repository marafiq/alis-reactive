using Alis.Reactive.Analyzers.TypedDsl;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

/// <summary>
/// ALIS009: the whole exposed surface of a shipped package, and a component declaration wherever it is
/// written, is typed. A deliberate exception names its reason with [TypedDslExemption].
/// </summary>
[TestFixture]
public class UntypedComponentApiTests
{
    private const string ShippedAssembly = "Alis.Reactive.Fusion";
    private const string ShippedFile = "Components/FusionThermostat.cs";
    private const string BroughtFile = "Components/Thermostat.cs";

    private static string Message(string member, string offending) =>
        $"Public API '{member}' exposes untyped '{offending}'. Give it a typed form (a generic value, an expression, an enum, a declaration) and keep wire types internal; developers never author raw object, System.Type, member-naming strings or plan wire types.";

    [Test]
    public Task an_object_parameter_on_a_shipped_surface_is_reported() =>
        Fixture.Reports<UntypedComponentApiAnalyzer>("ALIS009", Message("Label", "object"), ShippedFile, Shipped(
            """
            public static string Label(object [|value|]) => value.ToString() ?? "";
            """), ShippedAssembly);

    [Test]
    public Task an_object_return_on_a_shipped_surface_is_reported() =>
        Fixture.Reports<UntypedComponentApiAnalyzer>("ALIS009", Message("Raw", "object"), ShippedFile, Shipped(
            """
            public static object [|Raw|]() => new object();
            """), ShippedAssembly);

    [Test]
    public Task a_system_type_parameter_on_a_shipped_surface_is_reported() =>
        Fixture.Reports<UntypedComponentApiAnalyzer>("ALIS009", Message("Describe", "System.Type"), ShippedFile, Shipped(
            """
            public static string Describe(Type [|component|]) => component.Name;
            """), ShippedAssembly);

    [Test]
    public Task a_string_that_names_a_member_on_a_shipped_surface_is_reported() =>
        Fixture.Reports<UntypedComponentApiAnalyzer>("ALIS009", Message("Read", "string propertyName"), ShippedFile, Shipped(
            """
            public static string Read(string [|propertyName|]) => propertyName;
            """), ShippedAssembly);

    [Test]
    public Task an_untyped_argument_on_a_public_shipped_member_is_reported() =>
        Fixture.Reports<UntypedComponentApiAnalyzer>("ALIS009", Message("Call", "Alis.Reactive.Arg"), ShippedFile, Shipped(
            """
            public static void Call(Arg [|first|]) { }
            """), ShippedAssembly);

    [Test]
    public Task an_untyped_property_on_a_brought_component_is_reported() =>
        Fixture.Reports<UntypedComponentApiAnalyzer>("ALIS009", Message("Degrees", "Alis.Reactive.Property<object>"), BroughtFile,
            """
            using Alis.Reactive;

            namespace MapleGrove.Web.Components
            {
                public sealed class ThermostatReading : JsObject
                {
                    public Property<object> [|Degrees|] => Property<object>();
                }
            }
            """);

    [Test]
    public Task an_exemption_that_names_its_reason_is_not_reported() =>
        Fixture.ReportsNothing<UntypedComponentApiAnalyzer>(ShippedFile, Shipped(
            """
            [TypedDslExemption("The vendor's slot is typed object; the typed form is owed with its next release.")]
            public static string Label(object value) => value.ToString() ?? "";
            """), ShippedAssembly);

    [Test]
    public Task a_typed_form_is_not_reported() =>
        Fixture.ReportsNothing<UntypedComponentApiAnalyzer>(ShippedFile, Shipped(
            """
            public static string Label<TValue>(TValue value) where TValue : notnull => value.ToString() ?? "";
            """), ShippedAssembly);

    [Test]
    public Task an_applications_own_helper_is_left_alone() =>
        Fixture.ReportsNothing<UntypedComponentApiAnalyzer>("Helpers/Labels.cs",
            """
            namespace MapleGrove.Web
            {
                public static class Labels
                {
                    public static string Label(object value) => value.ToString() ?? "";
                }
            }
            """);

    // A render helper in a shipped package, with the member given.
    private static string Shipped(string member) => $$"""
        using System;
        using Alis.Reactive;
        using Alis.Reactive.Components;

        namespace Alis.Reactive.Fusion
        {
            public static class FusionThermostatExtensions
            {
                {{member}}
            }
        }
        """;
}
