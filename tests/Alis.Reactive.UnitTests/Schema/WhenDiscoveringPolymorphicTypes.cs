using System.Runtime.CompilerServices;
using System.Text.Json;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.UnitTests;

/// <summary>
/// Discovers all sealed subclasses of every polymorphic descriptor base type via reflection
/// and asserts that each subclass's Kind value has a matching definition in the JSON schema.
/// Catches schema drift — a new subclass without a schema definition is a silent bug because
/// WriteOnlyPolymorphicConverter serializes any subclass via reflection.
/// </summary>
[TestFixture]
public class WhenDiscoveringPolymorphicTypes
{
    private static readonly Type[] PolymorphicBaseTypes =
    [
        typeof(Command),
        typeof(Trigger),
        typeof(Reaction),
        typeof(Mutation),
        typeof(MethodArg),
        typeof(BindSource),
        typeof(Guard),
        typeof(GatherItem)
    ];

    private static IEnumerable<TestCaseData> PolymorphicBaseTypeCases()
    {
        foreach (var baseType in PolymorphicBaseTypes)
            yield return new TestCaseData(baseType)
                .SetName($"All_{baseType.Name}_subclasses_have_schema_definitions");
    }

    [TestCaseSource(nameof(PolymorphicBaseTypeCases))]
    public void Every_kind_has_a_matching_schema_definition(Type baseType)
    {
        var subclasses = baseType.Assembly.GetTypes()
            .Where(t => t.IsSealed && !t.IsAbstract && baseType.IsAssignableFrom(t))
            .ToList();

        Assert.That(subclasses, Is.Not.Empty,
            $"No sealed subclasses found for {baseType.Name}");

        var schemaKinds = GetSchemaKindsFor(baseType.Name);

        foreach (var subclass in subclasses)
        {
            var kindProp = subclass.GetProperty("Kind");
            Assert.That(kindProp, Is.Not.Null,
                $"{subclass.Name} is a sealed subclass of {baseType.Name} but has no Kind property");

            var instance = RuntimeHelpers.GetUninitializedObject(subclass);
            var kind = (string?)kindProp!.GetValue(instance);
            Assert.That(kind, Is.Not.Null,
                $"{subclass.Name}.Kind returned null — Kind must be an expression-bodied " +
                $"property (e.g. public string Kind => \"my-kind\"), not constructor-initialized. " +
                $"GetUninitializedObject bypasses constructors, so constructor-set Kind will be null.");

            Assert.That(schemaKinds, Does.Contain(kind),
                $"{subclass.Name} has Kind=\"{kind}\" but no matching definition exists in " +
                $"schema $defs.{baseType.Name}.oneOf. Schema kinds: [{string.Join(", ", schemaKinds)}]");
        }
    }

    private static HashSet<string> GetSchemaKindsFor(string baseTypeName)
    {
        var schemaPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Schemas",
            "reactive-plan.schema.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(schemaPath));
        var defs = doc.RootElement.GetProperty("$defs");

        Assert.That(defs.TryGetProperty(baseTypeName, out var unionDef), Is.True,
            $"Schema $defs does not contain '{baseTypeName}'");

        var kinds = new HashSet<string>();

        if (!unionDef.TryGetProperty("oneOf", out var oneOf))
            return kinds;

        foreach (var refEntry in oneOf.EnumerateArray())
        {
            if (!refEntry.TryGetProperty("$ref", out var refVal))
                continue;

            var defName = refVal.GetString()!.Split('/').Last();

            if (defs.TryGetProperty(defName, out var concreteDef) &&
                concreteDef.TryGetProperty("properties", out var props) &&
                props.TryGetProperty("kind", out var kindDef) &&
                kindDef.TryGetProperty("const", out var constVal))
            {
                kinds.Add(constVal.GetString()!);
            }
        }

        return kinds;
    }
}
