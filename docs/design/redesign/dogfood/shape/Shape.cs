// Spec-only reconstruction of the Shape kernel.
// Source of truth: docs/design/redesign/scaffold/Shape.md (§2, §3, §5a, §6 A/B/C/D)
// + the Shape fixtures (Module 1) in docs/design/redesign/scaffold/_fixtures.md.
// No file under Alis.Reactive/** was read while writing this.

using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel;

/// <summary>
/// Declares the expected structural type for a value in the plan
/// (string, number, boolean, date, array, object, nullable, raw, any, or none).
/// Null is unrepresentable: there is no null Shape — use <see cref="None"/> for
/// "absence of a typed value".
/// </summary>
[JsonConverter(typeof(ShapeJsonConverter))]
public sealed class Shape : IEquatable<Shape>
{
    internal static readonly Shape String  = Scalar("string");
    internal static readonly Shape Number  = Scalar("number");
    internal static readonly Shape Boolean = Scalar("boolean");
    internal static readonly Shape Date    = Scalar("date");
    internal static readonly Shape Raw     = Scalar("raw");
    internal static readonly Shape Any     = Scalar("any");
    internal static readonly Shape None    = Scalar("none");

    public string Kind { get; }
    private readonly ShapeStructure _structure;

    private Shape(string kind, ShapeStructure structure)
    {
        Kind = kind;
        _structure = structure ?? throw new ArgumentNullException(nameof(structure));
    }

    private static Shape Scalar(string kind) => new Shape(kind, ShapeStructure.None);

    // --- Construction (fixtures: array_of_none_is_rejected, nullable_of_none_is_rejected, object_of_fields_serializes_closed) ---

    internal static Shape ArrayOf(Shape item)
    {
        if (item is null || item.IsNone)
            throw new ArgumentException("An array of None is unrepresentable.", nameof(item));
        return new Shape("array", ShapeStructure.Array(item));
    }

    internal static Shape ObjectOf(Dictionary<string, Shape> fields)
    {
        if (fields is null) throw new ArgumentNullException(nameof(fields));
        // copy into a read-only snapshot so the contract is immutable
        var copy = new Dictionary<string, Shape>(fields);
        return new Shape("object", ShapeStructure.Object(copy));
    }

    internal static Shape OpenObject() => new Shape("object", ShapeStructure.OpenObject);

    internal static Shape Nullable(Shape inner)
    {
        if (inner is null || inner.IsNone)
            throw new ArgumentException("A nullable of None is unrepresentable.", nameof(inner));
        return new Shape("nullable", ShapeStructure.Nullable(inner));
    }

    // --- Authoring inference (fixtures: A. CLR inference) ---

    internal static Shape FromClrType(Type type)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));

        // Nullable<T> first → Nullable(FromClrType(underlying))
        var underlying = System.Nullable.GetUnderlyingType(type);
        if (underlying is not null)
            return Nullable(FromClrType(underlying));

        if (type == typeof(string)) return String;
        if (type == typeof(bool))   return Boolean;

        if (IsDateType(type))    return Date;
        if (IsNumericType(type)) return Number;

        // Guid / TimeSpan / TimeOnly / enum → string-serialized scalar
        if (IsStringSerialized(type)) return String;

        // supported collection → ArrayOf(item)
        if (TryGetCollectionItemShape(type, out var item))
            return ArrayOf(item);

        // unclassifiable → any (never null)
        return Any;
    }

    internal static Shape FromValue(object? value) =>
        value is null ? None : FromClrType(value.GetType()); // fixture: from_value_null_is_none

    internal static Shape CollectionItemShapeOrNone(Type type) =>
        TryGetCollectionItemShape(type, out var item) ? item : None; // fixture: collection_item_shape_or_none_for_non_collection

    // --- Predicates / accessors ---

    internal bool IsNone => Kind == "none";

    internal bool IsScalar =>
        Kind switch
        {
            "string" or "number" or "boolean" or "date" => true,
            _ => _structure.IsScalar, // Nullable<scalar> is scalar; object/array/raw/any/none are not
        };

    internal bool TryGetArrayItemShape(out Shape itemShape)         => _structure.TryGetArrayItemShape(out itemShape);
    internal bool TryGetObjectContract(out ShapeObjectContract c)   => _structure.TryGetObjectContract(out c);
    internal bool TryGetNullableInnerShape(out Shape inner)         => _structure.TryGetNullableInnerShape(out inner);
    internal bool IsNullableOf(Shape inner)                         => _structure.IsNullableWrapping(inner ?? throw new ArgumentNullException(nameof(inner)));

    // --- Diagnostics (fixture: describe_contract_nested) ---

    internal string DescribeContract()
    {
        if (TryGetArrayItemShape(out var item))
            return $"array<{item.DescribeContract()}>";
        if (TryGetNullableInnerShape(out var inner))
            return $"nullable<{inner.DescribeContract()}>";
        if (TryGetObjectContract(out var contract))
            return $"object{contract.Describe()}";
        return Kind;
    }

    // --- Serialization (write-only; fixture: C. Serialization) ---

    internal void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options) =>
        _structure.WriteContractDetails(writer, options);

    // --- Equality (fixtures: equal_array_shapes_are_equal, different_object_fields_are_unequal) ---

    public bool Equals(Shape? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Kind != other.Kind) return false;
        return _structure.HasSameContract(other._structure);
    }

    public override bool Equals(object? obj) => Equals(obj as Shape);
    public override int GetHashCode() => HashCode.Combine(Kind, _structure.GetContractHashCode());
    public static bool operator ==(Shape? l, Shape? r) => Equals(l, r);
    public static bool operator !=(Shape? l, Shape? r) => !Equals(l, r);

    public override string ToString() => DescribeContract();

    // --- inference helpers (spec gap fill: exact CLR sets — see README "Invented") ---

    private static bool IsDateType(Type t) =>
        t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(DateOnly);

    private static bool IsNumericType(Type t) =>
        t == typeof(byte) || t == typeof(sbyte) ||
        t == typeof(short) || t == typeof(ushort) ||
        t == typeof(int) || t == typeof(uint) ||
        t == typeof(long) || t == typeof(ulong) ||
        t == typeof(float) || t == typeof(double) || t == typeof(decimal) ||
        t == typeof(nint) || t == typeof(nuint);

    private static bool IsStringSerialized(Type t) =>
        t.IsEnum || t == typeof(Guid) || t == typeof(TimeSpan) || t == typeof(TimeOnly);

    /// <summary>
    /// A "supported collection" is an enumerable element sequence that is NOT a
    /// string and NOT a dictionary (per fixtures clr_list_of_t_is_array_of_t and
    /// clr_dictionary_is_any). Element shape comes from the generic argument, or the
    /// array element type, else Any.
    /// </summary>
    private static bool TryGetCollectionItemShape(Type type, out Shape itemShape)
    {
        itemShape = None;
        if (type == typeof(string)) return false; // string is a scalar, not a char[]

        // dictionaries are explicitly NOT supported collections → fall through to Any
        if (IsDictionary(type)) return false;

        if (type.IsArray)
        {
            var elem = type.GetElementType();
            itemShape = elem is null ? Any : FromClrType(elem);
            return true;
        }

        // IEnumerable<T> (List<T>, IEnumerable<string>, etc.)
        var enumerableArg = GetEnumerableElementType(type);
        if (enumerableArg is not null)
        {
            itemShape = FromClrType(enumerableArg);
            return true;
        }

        // non-generic IEnumerable (but not string/dictionary) → array<any>
        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            itemShape = Any;
            return true;
        }

        return false;
    }

    private static bool IsDictionary(Type type)
    {
        foreach (var i in AllInterfacesIncludingSelf(type))
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                return true;
        }
        return typeof(IDictionary).IsAssignableFrom(type);
    }

    private static Type? GetEnumerableElementType(Type type)
    {
        foreach (var i in AllInterfacesIncludingSelf(type))
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return i.GetGenericArguments()[0];
        }
        return null;
    }

    private static IEnumerable<Type> AllInterfacesIncludingSelf(Type type)
    {
        if (type.IsInterface) yield return type;
        foreach (var i in type.GetInterfaces()) yield return i;
    }
}

/// <summary>The structure axis. Closed family of five sealed subclasses reached only through factories.</summary>
internal abstract class ShapeStructure
{
    internal static ShapeStructure None { get; } = new EmptyShapeStructure();
    internal static ShapeStructure OpenObject { get; } = new OpenObjectShapeStructure();
    internal static ShapeStructure Array(Shape item) => new ArrayShapeStructure(item);
    internal static ShapeStructure Nullable(Shape inner) => new NullableShapeStructure(inner);
    internal static ShapeStructure Object(IReadOnlyDictionary<string, Shape> fields) => new ObjectShapeStructure(fields);

    internal virtual bool IsScalar => false;
    internal virtual bool HasSameContract(ShapeStructure other) => other.GetType() == GetType();
    internal virtual int GetContractHashCode() => GetType().GetHashCode();
    internal virtual bool IsNullableWrapping(Shape inner) => false;

    internal virtual bool TryGetArrayItemShape(out Shape itemShape) { itemShape = Shape.None; return false; }
    internal virtual bool TryGetObjectContract(out ShapeObjectContract c) { c = ShapeObjectContract.None; return false; }
    internal virtual bool TryGetNullableInnerShape(out Shape inner) { inner = Shape.None; return false; }

    internal virtual void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options) { /* scalar: no body */ }

    private sealed class EmptyShapeStructure : ShapeStructure { }

    private sealed class OpenObjectShapeStructure : ShapeStructure
    {
        internal override bool TryGetObjectContract(out ShapeObjectContract c) { c = ShapeObjectContract.Open; return true; }
        internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options)
            => ShapeObjectContract.Open.WriteTo(writer, options);
    }

    private sealed class ArrayShapeStructure : ShapeStructure
    {
        private readonly Shape _item;
        internal ArrayShapeStructure(Shape item) => _item = item;

        internal override bool TryGetArrayItemShape(out Shape itemShape) { itemShape = _item; return true; }
        internal override bool HasSameContract(ShapeStructure other)
            => other is ArrayShapeStructure a && _item.Equals(a._item);
        internal override int GetContractHashCode() => HashCode.Combine("array", _item.GetHashCode());
        internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WritePropertyName("item");
            JsonSerializer.Serialize(writer, _item, options);
        }
    }

    private sealed class NullableShapeStructure : ShapeStructure
    {
        private readonly Shape _inner;
        internal NullableShapeStructure(Shape inner) => _inner = inner;

        internal override bool IsScalar => _inner.IsScalar; // Nullable<scalar> is scalar
        internal override bool IsNullableWrapping(Shape inner) => _inner.Equals(inner);
        internal override bool TryGetNullableInnerShape(out Shape inner) { inner = _inner; return true; }
        internal override bool HasSameContract(ShapeStructure other)
            => other is NullableShapeStructure n && _inner.Equals(n._inner);
        internal override int GetContractHashCode() => HashCode.Combine("nullable", _inner.GetHashCode());
        internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WritePropertyName("inner");
            JsonSerializer.Serialize(writer, _inner, options);
        }
    }

    private sealed class ObjectShapeStructure : ShapeStructure
    {
        private readonly ShapeObjectContract _contract;
        internal ObjectShapeStructure(IReadOnlyDictionary<string, Shape> fields)
            => _contract = ShapeObjectContract.Closed(fields);

        internal override bool TryGetObjectContract(out ShapeObjectContract c) { c = _contract; return true; }
        internal override bool HasSameContract(ShapeStructure other)
            => other is ObjectShapeStructure o && FieldsEqual(_contract, o._contract);
        internal override int GetContractHashCode()
        {
            var hash = new HashCode();
            hash.Add("object");
            // order-independent field hashing
            int acc = 0;
            foreach (var (k, v) in _contract.Fields)
                acc ^= HashCode.Combine(k, v.GetHashCode());
            hash.Add(acc);
            hash.Add(_contract.AllowsAdditionalFields);
            return hash.ToHashCode();
        }
        internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options)
            => _contract.WriteTo(writer, options);

        private static bool FieldsEqual(ShapeObjectContract a, ShapeObjectContract b)
        {
            if (a.AllowsAdditionalFields != b.AllowsAdditionalFields) return false;
            if (a.Fields.Count != b.Fields.Count) return false;
            foreach (var (k, v) in a.Fields)
            {
                if (!b.Fields.TryGetValue(k, out var bv)) return false;
                if (!v.Equals(bv)) return false;
            }
            return true;
        }
    }
}

/// <summary>A closed/open object field contract.</summary>
internal sealed class ShapeObjectContract
{
    internal static readonly ShapeObjectContract Open =
        new ShapeObjectContract(new Dictionary<string, Shape>(), allowsAdditional: true);
    internal static readonly ShapeObjectContract None =
        new ShapeObjectContract(new Dictionary<string, Shape>(), allowsAdditional: false);

    internal static ShapeObjectContract Closed(IReadOnlyDictionary<string, Shape> fields)
        => new ShapeObjectContract(new Dictionary<string, Shape>(fields), allowsAdditional: false);

    private ShapeObjectContract(IReadOnlyDictionary<string, Shape> fields, bool allowsAdditional)
    {
        Fields = fields;
        AllowsAdditionalFields = allowsAdditional;
    }

    internal IReadOnlyDictionary<string, Shape> Fields { get; }
    internal bool AllowsAdditionalFields { get; }

    internal string Describe()
    {
        // object{a:string,b:number}
        var parts = new List<string>();
        foreach (var (k, v) in Fields)
            parts.Add($"{k}:{v.DescribeContract()}");
        return "{" + string.Join(",", parts) + "}";
    }

    internal void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WritePropertyName("fields");
        writer.WriteStartObject();
        foreach (var (k, v) in Fields)
        {
            writer.WritePropertyName(k);
            JsonSerializer.Serialize(writer, v, options);
        }
        writer.WriteEndObject();
        writer.WriteBoolean("additional", AllowsAdditionalFields);
    }
}

/// <summary>Write-only STJ converter. `kind` first, then the structure body. Read is unsupported.</summary>
internal sealed class ShapeJsonConverter : JsonConverter<Shape>
{
    public override void Write(Utf8JsonWriter writer, Shape value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("kind", value.Kind);
        value.WriteContractDetails(writer, options);
        writer.WriteEndObject();
    }

    public override Shape Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) =>
        throw new NotSupportedException("Plan types are write-only.");
}
