using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel;

/// <summary>
/// Declares the expected structural type for a value in the plan
/// (string, number, boolean, date, array, object, nullable, raw, any, or none).
/// Carried on every value expression, condition operand, gather assignment,
/// and contract member so both C# and the runtime convert the same bytes the
/// same way. Null is unrepresentable: there is no null Shape — use
/// <see cref="None"/> for "absence of a typed value".
/// </summary>
[JsonConverter(typeof(ShapeJsonConverter))]
public sealed class Shape : IEquatable<Shape>
{
    internal static readonly Shape String = Scalar("string");
    internal static readonly Shape Number = Scalar("number");
    internal static readonly Shape Boolean = Scalar("boolean");
    internal static readonly Shape Date = Scalar("date");
    internal static readonly Shape Raw = Scalar("raw");
    internal static readonly Shape Any = Scalar("any");
    internal static readonly Shape None = Scalar("none");

    private readonly ShapeStructure _structure;

    private Shape(string kind, ShapeStructure structure)
    {
        Kind = kind;
        _structure = structure ?? throw new ArgumentNullException(nameof(structure));
    }

    private static Shape Scalar(string kind) => new(kind, ShapeStructure.None);

    internal static Shape ArrayOf(Shape item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.IsNone)
            throw new ArgumentException("An array of None is unrepresentable.", nameof(item));
        return new Shape("array", ShapeStructure.Array(item));
    }

    internal static Shape ObjectOf(Dictionary<string, Shape> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        var readOnly = new Dictionary<string, Shape>(fields);
        return new Shape("object", ShapeStructure.Object(readOnly));
    }

    internal static Shape OpenObject() => new("object", ShapeStructure.OpenObject);

    internal static Shape Nullable(Shape inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        if (inner.IsNone)
            throw new ArgumentException("A nullable of None is unrepresentable.", nameof(inner));
        return new Shape("nullable", ShapeStructure.Nullable(inner));
    }

    // The eleven built-in numerics. nint/nuint are deliberately NOT here.
    private static readonly HashSet<Type> NumericTypes =
    [
        typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
        typeof(int), typeof(uint), typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal)
    ];

    private static readonly Type[] GenericCollectionInterfaces =
    [
        typeof(IEnumerable<>), typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>),
        typeof(ICollection<>), typeof(IList<>), typeof(List<>),
        typeof(HashSet<>), typeof(ISet<>)
    ];

    internal static Shape FromClrType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var underlying = System.Nullable.GetUnderlyingType(type);
        if (underlying is not null)
            return Nullable(FromClrType(underlying));

        if (type == typeof(string))
            return String;

        if (type == typeof(bool))
            return Boolean;

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly))
            return Date;

        if (NumericTypes.Contains(type))
            return Number;

        if (type == typeof(Guid) || type == typeof(TimeSpan) || type == typeof(TimeOnly))
            return String;

        if (type.IsEnum)
            return String;

        if (TryGetCollectionItemShape(type, out var item))
            return ArrayOf(item);

        return Any;
    }

    internal static Shape CollectionItemShapeOrNone(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return TryGetCollectionItemShape(type, out var item) ? item : None;
    }

    private static bool TryGetCollectionItemShape(Type type, out Shape itemShape)
    {
        itemShape = None;

        // A string is enumerable but is never a "collection" for shape purposes.
        if (type == typeof(string))
            return false;

        if (type.IsArray)
        {
            var element = type.GetElementType();
            if (element is null)
                return false;
            itemShape = FromClrType(element);
            return true;
        }

        // A dictionary is not a supported collection.
        if (IsDictionary(type))
            return false;

        // Collect every distinct T across the recognized generic-collection interfaces
        // that the type implements (including the type itself when it is one of them).
        var elementTypes = new HashSet<Type>();
        foreach (var candidate in EnumerateTypeAndInterfaces(type))
        {
            if (!candidate.IsGenericType)
                continue;
            var definition = candidate.GetGenericTypeDefinition();
            if (Array.IndexOf(GenericCollectionInterfaces, definition) < 0)
                continue;
            elementTypes.Add(candidate.GetGenericArguments()[0]);
        }

        if (elementTypes.Count != 1)
            return false; // zero (non-generic IEnumerable) or ambiguous (two distinct T) → not supported.

        Type theOne = default!;
        foreach (var t in elementTypes)
            theOne = t;

        itemShape = FromClrType(theOne);
        return true;
    }

    private static bool IsDictionary(Type type)
    {
        foreach (var candidate in EnumerateTypeAndInterfaces(type))
        {
            if (!candidate.IsGenericType)
                continue;
            var definition = candidate.GetGenericTypeDefinition();
            if (definition == typeof(IDictionary<,>) || definition == typeof(IReadOnlyDictionary<,>))
                return true;
        }
        return false;
    }

    private static IEnumerable<Type> EnumerateTypeAndInterfaces(Type type)
    {
        yield return type;
        foreach (var iface in type.GetInterfaces())
            yield return iface;
    }

    internal static Shape FromValue(object? value) =>
        value is null ? None : FromClrType(value.GetType());

    public string Kind { get; }

    internal bool IsNone => Kind == "none";

    internal bool IsScalar => Kind switch
    {
        "string" or "number" or "boolean" or "date" => true,
        _ => _structure.IsScalar
    };

    internal bool TryGetArrayItemShape(out Shape itemShape) => _structure.TryGetArrayItemShape(out itemShape);

    internal bool TryGetObjectContract(out ShapeObjectContract contract) => _structure.TryGetObjectContract(out contract);

    internal bool TryGetNullableInnerShape(out Shape inner) => _structure.TryGetNullableInnerShape(out inner);

    internal bool IsNullableOf(Shape inner) =>
        _structure.IsNullableWrapping(inner ?? throw new ArgumentNullException(nameof(inner)));

    internal string DescribeContract()
    {
        if (TryGetArrayItemShape(out var item))
            return $"array<{item.DescribeContract()}>";
        if (TryGetNullableInnerShape(out var inner))
            return $"nullable<{inner.DescribeContract()}>";
        if (TryGetObjectContract(out var contract))
            return contract.Describe();
        return Kind;
    }

    internal void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options) =>
        _structure.WriteContractDetails(writer, options);

    public bool Equals(Shape? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        if (Kind != other.Kind)
            return false;
        return _structure.HasSameContract(other._structure);
    }

    public override bool Equals(object? obj) => Equals(obj as Shape);

    public override int GetHashCode() => HashCode.Combine(Kind, _structure.GetContractHashCode());

    public static bool operator ==(Shape? l, Shape? r) => Equals(l, r);

    public static bool operator !=(Shape? l, Shape? r) => !Equals(l, r);
}

internal sealed class ShapeJsonConverter : JsonConverter<Shape>
{
    public override void Write(Utf8JsonWriter writer, Shape value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("kind", value.Kind);
        value.WriteContractDetails(writer, options);
        writer.WriteEndObject();
    }

    public override Shape Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException("Plan types are write-only.");
}

internal abstract class ShapeStructure
{
    internal static ShapeStructure None { get; } = new EmptyShapeStructure();
    internal static ShapeStructure OpenObject { get; } = new OpenObjectShapeStructure();
    internal static ShapeStructure Array(Shape item) => new ArrayShapeStructure(item);
    internal static ShapeStructure Nullable(Shape inner) => new NullableShapeStructure(inner);
    internal static ShapeStructure Object(IReadOnlyDictionary<string, Shape> fields) => new ObjectShapeStructure(fields);

    internal virtual bool IsScalar => false;

    internal virtual bool HasSameContract(ShapeStructure other) => GetType() == other.GetType();

    internal virtual int GetContractHashCode() => GetType().GetHashCode();

    internal virtual bool IsNullableWrapping(Shape inner) => false;

    internal virtual bool TryGetArrayItemShape(out Shape itemShape)
    {
        itemShape = Shape.None;
        return false;
    }

    internal virtual bool TryGetObjectContract(out ShapeObjectContract contract)
    {
        contract = ShapeObjectContract.None;
        return false;
    }

    internal virtual bool TryGetNullableInnerShape(out Shape inner)
    {
        inner = Shape.None;
        return false;
    }

    internal virtual void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        // scalar / none / raw / any write no body.
    }

    private sealed class EmptyShapeStructure : ShapeStructure
    {
    }

    private sealed class OpenObjectShapeStructure : ShapeStructure
    {
        internal override bool TryGetObjectContract(out ShapeObjectContract contract)
        {
            contract = ShapeObjectContract.Open;
            return true;
        }

        internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options) =>
            ShapeObjectContract.Open.WriteTo(writer, options);
    }

    private sealed class ArrayShapeStructure(Shape item) : ShapeStructure
    {
        private readonly Shape _item = item;

        internal override bool TryGetArrayItemShape(out Shape itemShape)
        {
            itemShape = _item;
            return true;
        }

        internal override bool HasSameContract(ShapeStructure other) =>
            other is ArrayShapeStructure a && _item.Equals(a._item);

        internal override int GetContractHashCode() => HashCode.Combine("array", _item.GetHashCode());

        internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WritePropertyName("item");
            JsonSerializer.Serialize(writer, _item, options);
        }
    }

    private sealed class NullableShapeStructure(Shape inner) : ShapeStructure
    {
        private readonly Shape _inner = inner;

        internal override bool IsScalar => _inner.IsScalar;

        internal override bool IsNullableWrapping(Shape inner) => _inner.Equals(inner);

        internal override bool TryGetNullableInnerShape(out Shape inner)
        {
            inner = _inner;
            return true;
        }

        internal override bool HasSameContract(ShapeStructure other) =>
            other is NullableShapeStructure n && _inner.Equals(n._inner);

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

        internal ObjectShapeStructure(IReadOnlyDictionary<string, Shape> fields) =>
            _contract = ShapeObjectContract.Closed(fields);

        internal override bool TryGetObjectContract(out ShapeObjectContract contract)
        {
            contract = _contract;
            return true;
        }

        internal override bool HasSameContract(ShapeStructure other) =>
            other is ObjectShapeStructure o && ContractsEqual(_contract, o._contract);

        internal override int GetContractHashCode()
        {
            var hash = new HashCode();
            hash.Add("object");
            hash.Add(_contract.AllowsAdditionalFields);
            foreach (var pair in _contract.Fields.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                hash.Add(pair.Key);
                hash.Add(pair.Value);
            }
            return hash.ToHashCode();
        }

        internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options) =>
            _contract.WriteTo(writer, options);

        private static bool ContractsEqual(ShapeObjectContract a, ShapeObjectContract b)
        {
            if (a.AllowsAdditionalFields != b.AllowsAdditionalFields)
                return false;
            if (a.Fields.Count != b.Fields.Count)
                return false;
            foreach (var pair in a.Fields)
            {
                if (!b.Fields.TryGetValue(pair.Key, out var other))
                    return false;
                if (!pair.Value.Equals(other))
                    return false;
            }
            return true;
        }
    }
}

internal sealed class ShapeObjectContract
{
    private static IReadOnlyDictionary<string, Shape> EmptyFields { get; } =
        new Dictionary<string, Shape>();

    internal static readonly ShapeObjectContract Open = new(EmptyFields, allowsAdditional: true);
    internal static readonly ShapeObjectContract None = new(EmptyFields, allowsAdditional: false);

    private ShapeObjectContract(IReadOnlyDictionary<string, Shape> fields, bool allowsAdditional)
    {
        Fields = fields;
        AllowsAdditionalFields = allowsAdditional;
    }

    internal static ShapeObjectContract Closed(IReadOnlyDictionary<string, Shape> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        return new ShapeObjectContract(fields, allowsAdditional: false);
    }

    internal IReadOnlyDictionary<string, Shape> Fields { get; }

    internal bool AllowsAdditionalFields { get; }

    internal string Describe()
    {
        if (Fields.Count == 0)
            return AllowsAdditionalFields ? "object<open>" : "object{}";

        var body = string.Join(", ", Fields.Select(f => $"{f.Key}:{f.Value.DescribeContract()}"));
        var additional = AllowsAdditionalFields ? ", ..." : string.Empty;
        return $"object{{{body}{additional}}}";
    }

    internal void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WritePropertyName("fields");
        writer.WriteStartObject();
        foreach (var pair in Fields)
        {
            writer.WritePropertyName(pair.Key);
            JsonSerializer.Serialize(writer, pair.Value, options);
        }
        writer.WriteEndObject();
        writer.WriteBoolean("additional", AllowsAdditionalFields);
    }
}
