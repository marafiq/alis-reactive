using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alis.Reactive.TypeKnowledge;

internal sealed class WrittenMember
{
    private const JsonNumberHandling WritingBits = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowNamedFloatingPointLiterals;

    private static readonly Type[] Numbers =
    [
        typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
        typeof(byte?), typeof(sbyte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?),
        typeof(float?), typeof(double?), typeof(decimal?),
    ];

    private readonly JsonPropertyInfo _property;
    private readonly Func<object, object?> _get;

    private WrittenMember(JsonTypeInfo holder, JsonPropertyInfo property, Func<object, object?> get)
    {
        Holder = holder;
        _property = property;
        _get = get;
    }

    internal JsonTypeInfo Holder { get; }

    internal string Name => _property.Name;

    internal JsonTypeInfo Contract => Holder.Options.GetTypeInfo(_property.PropertyType);

    internal object? ReadFrom(object holder) => _get(holder);

    internal JsonElement Written(object? value) => JsonSerializer.SerializeToElement(value, OwnForm().GetTypeInfo(_property.PropertyType));

    internal static WrittenMember Of(MemberExpression access, JsonTypeInfo holder, LambdaExpression read)
    {
        var declared = Declared(access, holder, read);
        var written = holder.Properties.FirstOrDefault(property => property.Get is not null && property.Name == declared.Name);
        return written is { Get: { } get } ? new WrittenMember(holder, written, get) : throw NotWritten(read, holder);
    }

    private JsonSerializerOptions OwnForm()
    {
        var options = Holder.Options;
        if (_property.CustomConverter is { } converter)
        {
            var byItsConverter = new JsonSerializerOptions(options);
            byItsConverter.Converters.Insert(0, converter);
            return byItsConverter;
        }

        var handling = _property.NumberHandling ?? Holder.NumberHandling ?? options.NumberHandling;
        var writesAsTheOptionsDo = (handling & WritingBits) == (options.NumberHandling & WritingBits) || !HoldsNumbers(Contract);
        return writesAsTheOptionsDo ? options : new JsonSerializerOptions(options) { NumberHandling = handling };
    }

    private static bool HoldsNumbers(JsonTypeInfo type)
    {
        if (Numbers.Contains(type.Type)) return true;
        if (type.IsList(out var item)) return Numbers.Contains(item.Type);
        return type.IsDictionary(out _, out var value) && Numbers.Contains(value.Type);
    }

    private static JsonPropertyInfo Declared(MemberExpression access, JsonTypeInfo holder, LambdaExpression read)
    {
        var declaring = DeclaringClass(access) ?? throw NotWritten(read, holder);
        return holder.Options.GetTypeInfo(declaring).Properties.FirstOrDefault(property => Equals(property.AttributeProvider, access.Member))
            ?? throw NotWritten(read, holder);
    }

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's exception, 2026-11-06: the class that declares a lambda's member is a type's identity, not a discovery of members (D18.1, D18.3).")]
    private static Type? DeclaringClass(MemberExpression access) => access.Member.DeclaringType;

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "A type's name is its identity (D18.1), and X18.8's refusal names the class a read starts from by it.")]
    private static string NameOf(JsonTypeInfo holder) => holder.Type.Name;

    private static InvalidOperationException NotWritten(LambdaExpression read, JsonTypeInfo holder)
        => new($"'{read}' reads a member of '{NameOf(holder)}' that the app's JSON options do not write, so the browser never holds it.");
}
