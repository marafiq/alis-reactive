namespace Alis.Reactive.UnitTests;

/// <summary>
/// Tests IdGenerator — collision-free element IDs from model FullName + expression path.
/// Format: "{Namespace_TypeName}__{MemberPath}" — double underscore separates scope from property.
/// Split on "__" to recover: [0] = model scope, [1] = property path.
/// </summary>
[TestFixture]
public class WhenGeneratingUniqueIds
{
    // ── Test models ──

    public class Address
    {
        public string? City { get; set; }
        public string? State { get; set; }
        public int PostalCode { get; set; }
    }

    public class ModelA
    {
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public Address? Address { get; set; }
    }

    public class ModelB
    {
        public string? Name { get; set; }
        public decimal Amount { get; set; }
    }

    // ── Format ──

    [Test]
    public void Flat_property_uses_full_name_double_underscore_separator()
    {
        var id = IdGenerator.For<ModelA>(m => m.Name);
        var scope = IdGenerator.TypeScope(typeof(ModelA));
        Assert.That(id, Is.EqualTo(scope + "__Name"));
        Assert.That(id, Does.Contain("__"));
    }

    [Test]
    public void Value_type_property_uses_full_name_scope()
    {
        var id = IdGenerator.For<ModelA>(m => m.Amount);
        var scope = IdGenerator.TypeScope(typeof(ModelA));
        Assert.That(id, Is.EqualTo(scope + "__Amount"));
    }

    [Test]
    public void Nested_property_uses_underscores_after_delimiter()
    {
        var id = IdGenerator.For<ModelA>(m => m.Address!.City);
        var scope = IdGenerator.TypeScope(typeof(ModelA));
        Assert.That(id, Is.EqualTo(scope + "__Address_City"));
    }

    [Test]
    public void Nested_value_type_uses_full_name_scope()
    {
        var id = IdGenerator.For<ModelA, int>(m => m.Address!.PostalCode);
        var scope = IdGenerator.TypeScope(typeof(ModelA));
        Assert.That(id, Is.EqualTo(scope + "__Address_PostalCode"));
    }

    // ── Splittable ──

    [Test]
    public void Id_splits_into_scope_and_property_path()
    {
        var id = IdGenerator.For<ModelA>(m => m.Address!.City);
        var parts = id.Split("__");
        Assert.That(parts, Has.Length.EqualTo(2));
        Assert.That(parts[1], Is.EqualTo("Address_City"));
    }

    // ── Collision resistance ──

    [Test]
    public void Different_models_with_same_property_produce_different_ids()
    {
        var idA = IdGenerator.For<ModelA>(m => m.Name);
        var idB = IdGenerator.For<ModelB>(m => m.Name);
        Assert.That(idA, Is.Not.EqualTo(idB));
    }

    [Test]
    public void Same_model_same_property_is_deterministic()
    {
        var id1 = IdGenerator.For<ModelA>(m => m.Name);
        var id2 = IdGenerator.For<ModelA>(m => m.Name);
        Assert.That(id1, Is.EqualTo(id2));
    }

    // ── TypeScope format ──

    [Test]
    public void TypeScope_replaces_dots_with_underscores()
    {
        var scope = IdGenerator.TypeScope(typeof(ModelA));
        Assert.That(scope, Does.Not.Contain("."));
        Assert.That(scope, Does.Contain("WhenGeneratingUniqueIds_ModelA"));
    }

    [Test]
    public void Different_types_produce_different_scopes()
    {
        var scopeA = IdGenerator.TypeScope(typeof(ModelA));
        var scopeB = IdGenerator.TypeScope(typeof(ModelB));
        Assert.That(scopeA, Is.Not.EqualTo(scopeB));
    }

    // ── Typed overload ──

    [Test]
    public void Typed_overload_matches_object_overload_for_reference_types()
    {
        var fromObject = IdGenerator.For<ModelA>(m => m.Name);
        var fromTyped = IdGenerator.For<ModelA, string?>(m => m.Name);
        Assert.That(fromTyped, Is.EqualTo(fromObject));
    }
}
