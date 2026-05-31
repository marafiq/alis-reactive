using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel;

/// <summary>
/// Declares the expected structural type for a value in the plan
/// (string, number, boolean, date, array, object, nullable, raw, any, or none).
/// Null is unrepresentable: there is no null Shape — use <see cref="None"/>.
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

    // The eleven built-in numerics — exactly these. nint/nuint are NOT here -> Any.
    private static readonly HashSet<Type> NumericTypes =
    [
        typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
        typeof(int), typeof(uint), typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
    ];

    private static readonly HashSet<Type> StringSerializedTypes =
    [
        typeof(Guid), typeof(TimeSpan), typeof(TimeOnly),
    ];

    private static readonly HashSet<Type> DateTypes =
    [
        typeof(DateTime), typeof(DateTimeOffset), typeof(DateOnly),
    ];

    // The generic collection interfaces whose single distinct T resolves a supported collection.
    private static readonly HashSet<Type> CollectionInterfaceDefinitions =
    [
        typeof(IEnumerable<>),
        typeof(IReadOnlyCollection<>),
        typeof(IReadOnlyList<>),
        typeof(ICollection<>),
        typeof(IList<>),
        typeof(List<>),
        typeof(HashSet<>),
        typeof(ISet<>),
    ];

    private readonly ShapeStructure _structure;

    private Shape(string kind, ShapeStructure structure)
    {
        Kind = kind;
        _structure = structure ?? throw new ArgumentNullException(nameof(structure));
    }

    private static Shape Scalar(string kind) => new(kind, ShapeStructure.None);

    /// <summary>The discriminator token matched by the TS Shape union.</summary>
    public string Kind { get; }

    internal bool IsNone => Kind == "none";

    internal bool IsScalar
    {
        get
        {
            if (Kind is "string" or "number" or "boolean" or "date")
            {
                return true;
            }

            return _structure.IsScalar;
        }
    }

    internal static Shape ArrayOf(Shape item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.IsNone)
        {
            throw new ArgumentException("An array of None is unrepresentable.", nameof(item));
        }

        return new Shape("array", ShapeStructure.Array(item));
    }

    internal static Shape ObjectOf(Dictionary<string, Shape> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        return new Shape("object", ShapeStructure.Object(new Dictionary<string, Shape>(fields)));
    }

    internal static Shape OpenObject() => new("object", ShapeStructure.OpenObject);

    internal static Shape Nullable(Shape inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        if (inner.IsNone)
        {
            throw new ArgumentException("A nullable of None is unrepresentable.", nameof(inner));
        }

        return new Shape("nullable", ShapeStructure.Nullable(inner));
    }

    internal static Shape FromValue(object? value) =>
        value is null ? None : FromClrType(value.GetType());

    internal static Shape FromClrType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        Type? underlying = System.Nullable.GetUnderlyingType(type);
        if (underlying is not null)
        {
            return Nullable(FromClrType(underlying));
        }

        if (type == typeof(string))
        {
            return String;
        }

        if (type == typeof(bool))
        {
            return Boolean;
        }

        if (DateTypes.Contains(type))
        {
            return Date;
        }

        if (NumericTypes.Contains(type))
        {
            return Number;
        }

        if (StringSerializedTypes.Contains(type))
        {
            return String;
        }

        if (type.IsEnum)
        {
            return String;
        }

        if (TryGetCollectionItemShape(type, out Shape itemShape))
        {
            return ArrayOf(itemShape);
        }

        return Any;
    }

    internal static Shape CollectionItemShapeOrNone(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return TryGetCollectionItemShape(type, out Shape itemShape) ? itemShape : None;
    }

    private static bool TryGetCollectionItemShape(Type type, out Shape itemShape)
    {
        itemShape = None;

        // string is itself an IEnumerable<char> but is never a supported collection.
        if (type == typeof(string))
        {
            return false;
        }

        // Dictionaries (and IDictionary) are not supported collections.
        if (IsDictionary(type))
        {
            return false;
        }

        if (type.IsArray)
        {
            Type? elementType = type.GetElementType();
            if (elementType is null)
            {
                return false;
            }

            itemShape = FromClrType(elementType);
            return true;
        }

        // Gather every generic-collection-interface T the type exposes; exactly one distinct T qualifies.
        HashSet<Type> distinctItemTypes = [];

        AddIfCollectionInterface(type, distinctItemTypes);
        foreach (Type iface in type.GetInterfaces())
        {
            AddIfCollectionInterface(iface, distinctItemTypes);
        }

        if (distinctItemTypes.Count != 1)
        {
            // zero distinct T (non-generic IEnumerable only, e.g. ArrayList) -> not supported
            // two-or-more distinct T (ambiguous) -> not supported
            return false;
        }

        itemShape = FromClrType(distinctItemTypes.Single());
        return true;
    }

    private static void AddIfCollectionInterface(Type candidate, HashSet<Type> distinctItemTypes)
    {
        if (!candidate.IsGenericType)
        {
            return;
        }

        Type definition = candidate.GetGenericTypeDefinition();
        if (CollectionInterfaceDefinitions.Contains(definition))
        {
            distinctItemTypes.Add(candidate.GetGenericArguments()[0]);
        }
    }

    private static bool IsDictionary(Type type)
    {
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            return true;
        }

        foreach (Type iface in type.GetInterfaces())
        {
            if (iface.IsGenericType
                && iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                return true;
            }
        }

        return false;
    }

    internal bool TryGetArrayItemShape(out Shape itemShape) => _structure.TryGetArrayItemShape(out itemShape);

    internal bool TryGetObjectContract(out ShapeObjectContract contract) => _structure.TryGetObjectContract(out contract);

    internal bool TryGetNullableInnerShape(out Shape inner) => _structure.TryGetNullableInnerShape(out inner);

    internal bool IsNullableOf(Shape inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        return _structure.IsNullableWrapping(inner);
    }

    internal string DescribeContract()
    {
        if (TryGetArrayItemShape(out Shape item))
        {
            return $"array<{item.DescribeContract()}>";
        }

        if (TryGetNullableInnerShape(out Shape inner))
        {
            return $"nullable<{inner.DescribeContract()}>";
        }

        if (TryGetObjectContract(out ShapeObjectContract contract))
        {
            return contract.Describe();
        }

        return Kind;
    }

    internal void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options) =>
        _structure.WriteContractDetails(writer, options);

    public bool Equals(Shape? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Kind != other.Kind)
        {
            return false;
        }

        return _structure.HasSameContract(other._structure);
    }

    public override bool Equals(object? obj) => Equals(obj as Shape);

    public override int GetHashCode() => HashCode.Combine(Kind, _structure.GetContractHashCode());

    public static bool operator ==(Shape? left, Shape? right) => Equals(left, right);

    public static bool operator !=(Shape? left, Shape? right) => !Equals(left, right);
}

/// <summary>
/// The structure axis behind a Shape. Closed family of five private sealed subclasses,
/// reached only through the static factories.
/// </summary>
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
        // no-op: scalars carry no body.
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
            other is ArrayShapeStructure array && _item.Equals(array._item);

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
            other is NullableShapeStructure nullable && _inner.Equals(nullable._inner);

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
            other is ObjectShapeStructure obj && _contract.HasSameContract(obj._contract);

        internal override int GetContractHashCode() => _contract.GetContractHashCode();

        internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options) =>
            _contract.WriteTo(writer, options);
    }
}

/// <summary>
/// The closed/open field set of an object shape.
/// </summary>
internal sealed class ShapeObjectContract
{
    internal static readonly ShapeObjectContract Open = new(new Dictionary<string, Shape>(), allowsAdditionalFields: true);

    internal static readonly ShapeObjectContract None = new(new Dictionary<string, Shape>(), allowsAdditionalFields: false);

    private ShapeObjectContract(IReadOnlyDictionary<string, Shape> fields, bool allowsAdditionalFields)
    {
        Fields = fields;
        AllowsAdditionalFields = allowsAdditionalFields;
    }

    internal static ShapeObjectContract Closed(IReadOnlyDictionary<string, Shape> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        return new ShapeObjectContract(new Dictionary<string, Shape>(fields), allowsAdditionalFields: false);
    }

    internal IReadOnlyDictionary<string, Shape> Fields { get; }

    internal bool AllowsAdditionalFields { get; }

    internal string Describe()
    {
        if (Fields.Count == 0)
        {
            return AllowsAdditionalFields ? "object<open>" : "object{}";
        }

        string body = string.Join(", ", Fields.Select(kvp => $"{kvp.Key}:{kvp.Value.DescribeContract()}"));
        string suffix = AllowsAdditionalFields ? ", ..." : string.Empty;
        return $"object{{{body}{suffix}}}";
    }

    internal bool HasSameContract(ShapeObjectContract other)
    {
        if (AllowsAdditionalFields != other.AllowsAdditionalFields)
        {
            return false;
        }

        if (Fields.Count != other.Fields.Count)
        {
            return false;
        }

        foreach ((string key, Shape shape) in Fields)
        {
            if (!other.Fields.TryGetValue(key, out Shape? otherShape) || !shape.Equals(otherShape))
            {
                return false;
            }
        }

        return true;
    }

    internal int GetContractHashCode()
    {
        // Order-independent field hash so equal contracts hash equally.
        int fieldHash = 0;
        foreach ((string key, Shape shape) in Fields)
        {
            fieldHash ^= HashCode.Combine(key, shape.GetHashCode());
        }

        return HashCode.Combine("object", AllowsAdditionalFields, fieldHash);
    }

    internal void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WritePropertyName("fields");
        writer.WriteStartObject();
        foreach ((string key, Shape shape) in Fields)
        {
            writer.WritePropertyName(key);
            JsonSerializer.Serialize(writer, shape, options);
        }

        writer.WriteEndObject();
        writer.WriteBoolean("additional", AllowsAdditionalFields);
    }
}

/// <summary>Write-only polymorphic emission of a Shape — plan types are write-only.</summary>
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
