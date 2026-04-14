using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.Shapes;

[TestFixture]
public class WhenBuildingShapes : PlanTestBase
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string SerializeShape(Shape shape) =>
        JsonSerializer.Serialize<Shape>(shape, Options);

    // ── ScalarShape leaves (string|number|boolean|date) ──────────

    [Test]
    public void ScalarShape_string_renders_kind_only()
    {
        var json = SerializeShape(Shape.String);
        Assert.That(json, Is.EqualTo("{\"kind\":\"string\"}"));
    }

    [Test]
    public void ScalarShape_number_renders_kind_only()
    {
        var json = SerializeShape(Shape.Number);
        Assert.That(json, Is.EqualTo("{\"kind\":\"number\"}"));
    }

    [Test]
    public void ScalarShape_boolean_renders_kind_only()
    {
        var json = SerializeShape(Shape.Boolean);
        Assert.That(json, Is.EqualTo("{\"kind\":\"boolean\"}"));
    }

    [Test]
    public void ScalarShape_date_renders_kind_only()
    {
        var json = SerializeShape(Shape.Date);
        Assert.That(json, Is.EqualTo("{\"kind\":\"date\"}"));
    }

    // ── OpaqueShape leaves (raw|any) ─────────────────────────────

    [Test]
    public void OpaqueShape_raw_renders_kind_only()
    {
        var json = SerializeShape(Shape.Raw);
        Assert.That(json, Is.EqualTo("{\"kind\":\"raw\"}"));
    }

    [Test]
    public void OpaqueShape_any_renders_kind_only()
    {
        var json = SerializeShape(Shape.Any);
        Assert.That(json, Is.EqualTo("{\"kind\":\"any\"}"));
    }

    // ── NoneShape ────────────────────────────────────────────────

    [Test]
    public void NoneShape_renders_kind_only()
    {
        var json = SerializeShape(Shape.None);
        Assert.That(json, Is.EqualTo("{\"kind\":\"none\"}"));
    }

    // ── ArrayShape ───────────────────────────────────────────────

    [Test]
    public void ArrayShape_renders_kind_and_item()
    {
        var arrayOfStrings = Shape.ArrayOf(Shape.String);
        var json = SerializeShape(arrayOfStrings);
        Assert.That(json, Is.EqualTo("{\"kind\":\"array\",\"item\":{\"kind\":\"string\"}}"));
    }

    [Test]
    public void ArrayShape_throws_when_item_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => Shape.ArrayOf(null!));
    }

    [Test]
    public void ArrayShape_throws_when_item_is_NoneShape_singleton()
    {
        var ex = Assert.Throws<ArgumentException>(() => Shape.ArrayOf(Shape.None));
        Assert.That(ex!.ParamName, Is.EqualTo("item"));
    }

    // ── NullableShape ────────────────────────────────────────────

    [Test]
    public void NullableShape_renders_kind_and_inner()
    {
        var nullableDate = Shape.Nullable(Shape.Date);
        var json = SerializeShape(nullableDate);
        Assert.That(json, Is.EqualTo("{\"kind\":\"nullable\",\"inner\":{\"kind\":\"date\"}}"));
    }

    [Test]
    public void NullableShape_throws_when_inner_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => Shape.Nullable(null!));
    }

    [Test]
    public void NullableShape_throws_when_inner_is_NoneShape_singleton()
    {
        var ex = Assert.Throws<ArgumentException>(() => Shape.Nullable(Shape.None));
        Assert.That(ex!.ParamName, Is.EqualTo("inner"));
    }

    [Test]
    public void NullableShape_IsScalar_delegates_to_inner_when_inner_is_scalar()
    {
        var nullableString = (NullableShape)Shape.Nullable(Shape.String);
        Assert.That(nullableString.IsScalar, Is.True);
    }

    [Test]
    public void NullableShape_of_array_is_not_scalar()
    {
        var nullableArray = (NullableShape)Shape.Nullable(Shape.ArrayOf(Shape.String));
        Assert.That(nullableArray.IsScalar, Is.False);
    }

    [Test]
    public void NullableShape_of_nullable_recurses_correctly()
    {
        var nullableNullableString = (NullableShape)Shape.Nullable(Shape.Nullable(Shape.String));
        // nullable(nullable(string)) — IsScalar bottoms out at the inner-most scalar
        Assert.That(nullableNullableString.IsScalar, Is.True);
    }

    // ── ObjectShape ──────────────────────────────────────────────

    [Test]
    public void ObjectShape_empty_renders_with_empty_fields()
    {
        var emptyObject = Shape.ObjectOf(new Dictionary<string, Shape>());
        var json = SerializeShape(emptyObject);
        Assert.That(json, Is.EqualTo("{\"kind\":\"object\",\"fields\":{}}"));
    }

    [Test]
    public void ObjectShape_with_named_fields_renders_correctly()
    {
        var person = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["name"] = Shape.String
        });
        var json = SerializeShape(person);
        Assert.That(json, Is.EqualTo("{\"kind\":\"object\",\"fields\":{\"name\":{\"kind\":\"string\"}}}"));
    }

    [Test]
    public void ObjectShape_throws_when_fields_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => Shape.ObjectOf(null!));
    }

    [Test]
    public void ObjectShape_no_additional_field_in_json()
    {
        var emptyObject = Shape.ObjectOf(new Dictionary<string, Shape>());
        var json = SerializeShape(emptyObject);
        Assert.That(json, Does.Not.Contain("additional"));
    }

    // ── FromClrType singleton mapping ────────────────────────────

    [Test]
    public void Shape_FromClrType_string_returns_singleton_String()
    {
        Assert.That(Shape.FromClrType(typeof(string)), Is.SameAs(Shape.String));
    }

    [Test]
    public void Shape_FromClrType_int_returns_singleton_Number()
    {
        Assert.That(Shape.FromClrType(typeof(int)), Is.SameAs(Shape.Number));
    }

    [Test]
    public void Shape_FromClrType_nullable_int_wraps_in_NullableShape()
    {
        var shape = Shape.FromClrType(typeof(int?));
        Assert.That(shape, Is.InstanceOf<NullableShape>());
        var nullable = (NullableShape)shape;
        Assert.That(nullable.Inner, Is.SameAs(Shape.Number));
    }

    [Test]
    public void Shape_FromClrType_string_array_wraps_in_ArrayShape()
    {
        var shape = Shape.FromClrType(typeof(string[]));
        Assert.That(shape, Is.InstanceOf<ArrayShape>());
        var array = (ArrayShape)shape;
        Assert.That(array.Item, Is.SameAs(Shape.String));
    }

    // ── Equality ─────────────────────────────────────────────────

    [Test]
    public void Shape_equality_is_structural_for_arrays()
    {
        var a1 = Shape.ArrayOf(Shape.String);
        var a2 = Shape.ArrayOf(Shape.String);
        Assert.That(a1.Equals(a2), Is.True);
        Assert.That(a1.GetHashCode(), Is.EqualTo(a2.GetHashCode()));
    }

    [Test]
    public void Shape_equality_distinguishes_array_items()
    {
        var arrayOfStrings = Shape.ArrayOf(Shape.String);
        var arrayOfNumbers = Shape.ArrayOf(Shape.Number);
        Assert.That(arrayOfStrings.Equals(arrayOfNumbers), Is.False);
    }

    [Test]
    public void Shape_equality_distinguishes_subclass_kinds()
    {
        Assert.That(Shape.String.Equals(Shape.Raw), Is.False);
        Assert.That(Shape.ArrayOf(Shape.String).Equals(Shape.String), Is.False);
        Assert.That(Shape.ArrayOf(Shape.String).Equals(Shape.Nullable(Shape.String)), Is.False);
    }

    // ── Type-based factory guard (R15) ───────────────────────────

    [Test]
    public void Constructing_a_freshly_allocated_NoneShape_does_not_pass_factory_guards()
    {
        // R15 invariant: factory guards are TYPE-based (`is NoneShape`), not identity-based.
        // A freshly-allocated NoneShape (not the Shape.None singleton) is still rejected.
        var freshNone = new NoneShape();
        Assert.That(freshNone, Is.Not.SameAs(Shape.None), "test pre-condition: fresh instance is not the singleton");

        var arrayEx = Assert.Throws<ArgumentException>(() => Shape.ArrayOf(freshNone));
        Assert.That(arrayEx!.ParamName, Is.EqualTo("item"));

        var nullableEx = Assert.Throws<ArgumentException>(() => Shape.Nullable(freshNone));
        Assert.That(nullableEx!.ParamName, Is.EqualTo("inner"));
    }

    // ── Singleton equality semantics ─────────────────────────────

    [Test]
    public void NoneShape_singleton_compares_equal_to_freshly_constructed_NoneShape()
    {
        // The singleton invariant is type-enforced, not identity-enforced.
        // Two NoneShape instances are structurally equal (same GetType, same EqualsSameType => true).
        var freshNone = new NoneShape();
        Assert.That(Shape.None.Equals(freshNone), Is.True);
        Assert.That(Shape.None.GetHashCode(), Is.EqualTo(freshNone.GetHashCode()));
    }

    // ── Ctor allow-list validation (Type-design BLOCK 1, 2 fix) ──

    [Test]
    public void ScalarShape_throws_when_kind_is_not_a_legal_scalar_kind()
    {
        var ex = Assert.Throws<ArgumentException>(() => new ScalarShape("banana"));
        Assert.That(ex!.ParamName, Is.EqualTo("kind"));
        Assert.That(ex.Message, Does.Contain("banana"));
    }

    [Test]
    public void ScalarShape_throws_when_kind_is_a_non_scalar_legal_kind_like_raw()
    {
        // "raw" is a legal kind for OpaqueShape but illegal for ScalarShape — proves the
        // class-level invariant is enforced, not just "any non-empty string".
        Assert.Throws<ArgumentException>(() => new ScalarShape("raw"));
        Assert.Throws<ArgumentException>(() => new ScalarShape("any"));
        Assert.Throws<ArgumentException>(() => new ScalarShape("none"));
        Assert.Throws<ArgumentException>(() => new ScalarShape("array"));
        Assert.Throws<ArgumentException>(() => new ScalarShape("nullable"));
        Assert.Throws<ArgumentException>(() => new ScalarShape("object"));
    }

    [Test]
    public void OpaqueShape_throws_when_kind_is_not_a_legal_opaque_kind()
    {
        var ex = Assert.Throws<ArgumentException>(() => new OpaqueShape("nope"));
        Assert.That(ex!.ParamName, Is.EqualTo("kind"));
        Assert.That(ex.Message, Does.Contain("nope"));
    }

    [Test]
    public void OpaqueShape_throws_when_kind_is_a_non_opaque_legal_kind_like_string()
    {
        // "string" is a legal kind for ScalarShape but illegal for OpaqueShape.
        Assert.Throws<ArgumentException>(() => new OpaqueShape("string"));
        Assert.Throws<ArgumentException>(() => new OpaqueShape("number"));
        Assert.Throws<ArgumentException>(() => new OpaqueShape("none"));
        Assert.Throws<ArgumentException>(() => new OpaqueShape("array"));
    }

    // ── ObjectShape ctor null-value validation (self-discovered hole) ──

    [Test]
    public void ObjectShape_throws_when_a_field_value_is_null()
    {
        var fields = new Dictionary<string, Shape>
        {
            ["name"] = Shape.String,
            ["broken"] = null!
        };
        var ex = Assert.Throws<ArgumentException>(() => Shape.ObjectOf(fields));
        Assert.That(ex!.ParamName, Is.EqualTo("fields"));
        Assert.That(ex.Message, Does.Contain("broken"));
    }

    // ── FromClrType branch coverage (Test analyzer BLOCKs 1-7) ──

    [Test]
    public void Shape_FromClrType_null_returns_singleton_Any()
    {
        Assert.That(Shape.FromClrType(null!), Is.SameAs(Shape.Any));
    }

    [Test]
    public void Shape_FromClrType_bool_returns_singleton_Boolean()
    {
        Assert.That(Shape.FromClrType(typeof(bool)), Is.SameAs(Shape.Boolean));
    }

    [Test]
    public void Shape_FromClrType_DateTime_returns_singleton_Date()
    {
        Assert.That(Shape.FromClrType(typeof(DateTime)), Is.SameAs(Shape.Date));
        Assert.That(Shape.FromClrType(typeof(DateTimeOffset)), Is.SameAs(Shape.Date));
        Assert.That(Shape.FromClrType(typeof(DateOnly)), Is.SameAs(Shape.Date));
    }

    [Test]
    public void Shape_FromClrType_Guid_TimeSpan_TimeOnly_return_singleton_String()
    {
        Assert.That(Shape.FromClrType(typeof(Guid)), Is.SameAs(Shape.String));
        Assert.That(Shape.FromClrType(typeof(TimeSpan)), Is.SameAs(Shape.String));
        Assert.That(Shape.FromClrType(typeof(TimeOnly)), Is.SameAs(Shape.String));
    }

    private enum SampleEnum { A, B, C }

    [Test]
    public void Shape_FromClrType_enum_returns_singleton_String()
    {
        Assert.That(Shape.FromClrType(typeof(SampleEnum)), Is.SameAs(Shape.String));
    }

    [Test]
    public void Shape_FromClrType_List_of_int_wraps_in_ArrayShape_of_Number()
    {
        var shape = Shape.FromClrType(typeof(List<int>));
        Assert.That(shape, Is.InstanceOf<ArrayShape>());
        var array = (ArrayShape)shape;
        Assert.That(array.Item, Is.SameAs(Shape.Number));
    }

    [Test]
    public void Shape_FromClrType_typeof_object_falls_through_to_Any()
    {
        // Unknown types (not string/bool/date/numeric/string-serialized/enum/collection)
        // fall through to the final `return Any` branch.
        Assert.That(Shape.FromClrType(typeof(object)), Is.SameAs(Shape.Any));
    }

    [Test]
    public void Shape_FromClrType_nested_generic_List_of_List_recurses_correctly()
    {
        var shape = Shape.FromClrType(typeof(List<List<string>>));
        Assert.That(shape, Is.InstanceOf<ArrayShape>());
        var outer = (ArrayShape)shape;
        Assert.That(outer.Item, Is.InstanceOf<ArrayShape>());
        var inner = (ArrayShape)outer.Item;
        Assert.That(inner.Item, Is.SameAs(Shape.String));
    }

    [TestCase(typeof(byte))]
    [TestCase(typeof(sbyte))]
    [TestCase(typeof(short))]
    [TestCase(typeof(ushort))]
    [TestCase(typeof(int))]
    [TestCase(typeof(uint))]
    [TestCase(typeof(long))]
    [TestCase(typeof(ulong))]
    [TestCase(typeof(float))]
    [TestCase(typeof(double))]
    [TestCase(typeof(decimal))]
    public void Shape_FromClrType_numeric_types_all_return_singleton_Number(Type numericType)
    {
        Assert.That(Shape.FromClrType(numericType), Is.SameAs(Shape.Number));
    }

    // ── ObjectShape equality coverage (Test analyzer BLOCK 8) ──

    [Test]
    public void ObjectShape_equality_returns_false_when_field_counts_differ()
    {
        var oneField = Shape.ObjectOf(new Dictionary<string, Shape> { ["a"] = Shape.String });
        var twoFields = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["a"] = Shape.String,
            ["b"] = Shape.Number
        });
        Assert.That(oneField.Equals(twoFields), Is.False);
        Assert.That(twoFields.Equals(oneField), Is.False);
    }

    [Test]
    public void ObjectShape_equality_returns_false_when_keys_differ()
    {
        var withA = Shape.ObjectOf(new Dictionary<string, Shape> { ["a"] = Shape.String });
        var withB = Shape.ObjectOf(new Dictionary<string, Shape> { ["b"] = Shape.String });
        Assert.That(withA.Equals(withB), Is.False);
        Assert.That(withB.Equals(withA), Is.False);
    }

    [Test]
    public void ObjectShape_equality_returns_false_when_field_shapes_differ()
    {
        var aIsString = Shape.ObjectOf(new Dictionary<string, Shape> { ["a"] = Shape.String });
        var aIsNumber = Shape.ObjectOf(new Dictionary<string, Shape> { ["a"] = Shape.Number });
        Assert.That(aIsString.Equals(aIsNumber), Is.False);
    }

    [Test]
    public void ObjectShape_equality_returns_true_for_same_keys_and_shapes_regardless_of_insertion_order()
    {
        var ab = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["a"] = Shape.String,
            ["b"] = Shape.Number
        });
        var ba = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["b"] = Shape.Number,
            ["a"] = Shape.String
        });
        Assert.That(ab.Equals(ba), Is.True);
        Assert.That(ab.GetHashCode(), Is.EqualTo(ba.GetHashCode()));
    }

    // ── ObjectShape order-independent hash (Test analyzer BLOCK 9, R20 contract) ──

    [Test]
    public void ObjectShape_GetHashCode_is_order_independent_across_three_permutations()
    {
        var abc = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["a"] = Shape.String,
            ["b"] = Shape.Number,
            ["c"] = Shape.Boolean
        });
        var cab = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["c"] = Shape.Boolean,
            ["a"] = Shape.String,
            ["b"] = Shape.Number
        });
        var bca = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["b"] = Shape.Number,
            ["c"] = Shape.Boolean,
            ["a"] = Shape.String
        });
        Assert.That(abc.GetHashCode(), Is.EqualTo(cab.GetHashCode()));
        Assert.That(abc.GetHashCode(), Is.EqualTo(bca.GetHashCode()));
        // All three are also Equals-equal (per the order-independent equality contract)
        Assert.That(abc.Equals(cab), Is.True);
        Assert.That(abc.Equals(bca), Is.True);
    }

    // ── Within-subclass Kind discrimination (Test analyzer BLOCK 10) ──

    [Test]
    public void ScalarShape_String_and_Number_are_not_equal_within_the_same_subclass()
    {
        // Both are ScalarShape — base GetType() check passes — EqualsSameType compares Kind.
        // Test 27 only crossed subclass boundaries (String vs Raw); this exercises the
        // within-ScalarShape Kind discriminator branch.
        Assert.That(Shape.String.Equals(Shape.Number), Is.False);
        Assert.That(Shape.Number.Equals(Shape.Boolean), Is.False);
        Assert.That(Shape.Boolean.Equals(Shape.Date), Is.False);
        // Reflexive
        Assert.That(Shape.String.Equals(Shape.String), Is.True);
    }

    [Test]
    public void OpaqueShape_Raw_and_Any_are_not_equal_within_the_same_subclass()
    {
        Assert.That(Shape.Raw.Equals(Shape.Any), Is.False);
        Assert.That(Shape.Any.Equals(Shape.Raw), Is.False);
        // Reflexive
        Assert.That(Shape.Raw.Equals(Shape.Raw), Is.True);
        Assert.That(Shape.Any.Equals(Shape.Any), Is.True);
        // Hash equality contract: structurally-equal OpaqueShapes hash identically.
        // Verified via fresh OpaqueShape instances (not the singletons) to prove the
        // hash code is structural, not identity-based.
        Assert.That(new OpaqueShape("raw").GetHashCode(), Is.EqualTo(Shape.Raw.GetHashCode()));
        Assert.That(new OpaqueShape("any").GetHashCode(), Is.EqualTo(Shape.Any.GetHashCode()));
    }

    // ── NullableShape equality (Test analyzer BLOCK 11) ──

    [Test]
    public void NullableShape_equality_is_structural_on_inner()
    {
        var nullableString1 = Shape.Nullable(Shape.String);
        var nullableString2 = Shape.Nullable(Shape.String);
        var nullableNumber = Shape.Nullable(Shape.Number);

        Assert.That(nullableString1.Equals(nullableString2), Is.True);
        Assert.That(nullableString1.GetHashCode(), Is.EqualTo(nullableString2.GetHashCode()));
        Assert.That(nullableString1.Equals(nullableNumber), Is.False);
    }

    // ── IsScalar constants per non-Nullable subclass (Test analyzer WEAK-2) ──

    [Test]
    public void ScalarShape_IsScalar_is_always_true()
    {
        Assert.That(((ScalarShape)Shape.String).IsScalar, Is.True);
        Assert.That(((ScalarShape)Shape.Number).IsScalar, Is.True);
        Assert.That(((ScalarShape)Shape.Boolean).IsScalar, Is.True);
        Assert.That(((ScalarShape)Shape.Date).IsScalar, Is.True);
    }

    [Test]
    public void OpaqueShape_IsScalar_is_always_false()
    {
        Assert.That(((OpaqueShape)Shape.Raw).IsScalar, Is.False);
        Assert.That(((OpaqueShape)Shape.Any).IsScalar, Is.False);
    }

    [Test]
    public void NoneShape_IsScalar_is_false()
    {
        Assert.That(((NoneShape)Shape.None).IsScalar, Is.False);
    }

    [Test]
    public void ArrayShape_IsScalar_is_always_false()
    {
        var arrayOfString = (ArrayShape)Shape.ArrayOf(Shape.String);
        var arrayOfNumber = (ArrayShape)Shape.ArrayOf(Shape.Number);
        Assert.That(arrayOfString.IsScalar, Is.False);
        Assert.That(arrayOfNumber.IsScalar, Is.False);
    }

    [Test]
    public void ObjectShape_IsScalar_is_always_false()
    {
        var emptyObject = (ObjectShape)Shape.ObjectOf(new Dictionary<string, Shape>());
        var withFields = (ObjectShape)Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["a"] = Shape.String
        });
        Assert.That(emptyObject.IsScalar, Is.False);
        Assert.That(withFields.IsScalar, Is.False);
    }

    // ── ObjectShape multi-field rendering (Test analyzer WEAK-3) ──

    [Test]
    public void ObjectShape_with_multiple_named_fields_of_mixed_shapes_renders_correctly()
    {
        var resident = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["id"] = Shape.Number,
            ["name"] = Shape.String,
            ["isActive"] = Shape.Boolean,
            ["roomTypes"] = Shape.ArrayOf(Shape.String)
        });
        var json = SerializeShape(resident);
        // Field order in the output reflects insertion order of the underlying Dictionary.
        Assert.That(json, Does.Contain("\"kind\":\"object\""));
        Assert.That(json, Does.Contain("\"fields\":"));
        Assert.That(json, Does.Contain("\"id\":{\"kind\":\"number\"}"));
        Assert.That(json, Does.Contain("\"name\":{\"kind\":\"string\"}"));
        Assert.That(json, Does.Contain("\"isActive\":{\"kind\":\"boolean\"}"));
        Assert.That(json, Does.Contain("\"roomTypes\":{\"kind\":\"array\",\"item\":{\"kind\":\"string\"}}"));
        Assert.That(json, Does.Not.Contain("additional"));
    }

    // ── Negative tests for public Equals/operator API (Test analyzer "missing negative tests") ──

    [Test]
    public void Shape_Equals_Shape_returns_false_when_other_is_null()
    {
        Assert.That(Shape.String.Equals((Shape?)null), Is.False);
    }

    [Test]
    public void Shape_Equals_object_returns_false_when_other_is_not_a_Shape()
    {
        // Intentional null + non-Shape comparisons exercise the negative branches of Equals(object?).
        // CS8602 is a false positive here — Shape.String is non-null at runtime (internal static readonly
        // with a non-null initializer) but the compiler's flow analysis cannot prove this through the
        // static-field-then-method-call chain. Suppressed locally.
#pragma warning disable CS8602
        Assert.That(Shape.String.Equals((object?)null), Is.False);
        Assert.That(Shape.String.Equals("string"), Is.False);
        Assert.That(Shape.String.Equals(42), Is.False);
#pragma warning restore CS8602
    }

    [Test]
    public void Shape_operator_equals_handles_both_null_via_ReferenceEquals()
    {
        Shape? left = null;
        Shape? right = null;
        Assert.That(left == right, Is.True);
        Assert.That(left != right, Is.False);
    }

    [Test]
    public void Shape_operator_equals_handles_left_null_right_non_null()
    {
        Shape? left = null;
        Assert.That(left == Shape.String, Is.False);
        Assert.That(left != Shape.String, Is.True);
    }

    [Test]
    public void Shape_operator_not_equals_returns_inverse_of_operator_equals()
    {
        Assert.That(Shape.String != Shape.Number, Is.True);
#pragma warning disable CS1718 // intentional self-comparison verifies operator!= reflexivity
        Assert.That(Shape.String != Shape.String, Is.False);
#pragma warning restore CS1718
        var arrayOfString1 = Shape.ArrayOf(Shape.String);
        var arrayOfString2 = Shape.ArrayOf(Shape.String);
        Assert.That(arrayOfString1 != arrayOfString2, Is.False);
    }

    // ── Deeply nested composition (Test analyzer "missing edge cases") ──

    [Test]
    public void Deeply_nested_Array_of_Nullable_of_Object_renders_and_compares_equal()
    {
        // Array(Nullable(Object{a: Array(String)}))
        var inner = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["a"] = Shape.ArrayOf(Shape.String)
        });
        var nullable = Shape.Nullable(inner);
        var outer1 = Shape.ArrayOf(nullable);

        var inner2 = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["a"] = Shape.ArrayOf(Shape.String)
        });
        var outer2 = Shape.ArrayOf(Shape.Nullable(inner2));

        Assert.That(outer1.Equals(outer2), Is.True);
        Assert.That(outer1.GetHashCode(), Is.EqualTo(outer2.GetHashCode()));

        var json = SerializeShape(outer1);
        Assert.That(json, Is.EqualTo(
            "{\"kind\":\"array\",\"item\":{\"kind\":\"nullable\",\"inner\":{\"kind\":\"object\",\"fields\":{\"a\":{\"kind\":\"array\",\"item\":{\"kind\":\"string\"}}}}}}"));
    }
}
