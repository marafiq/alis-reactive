using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Schema;

namespace Alis.Reactive.PathsAndWire;

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(TextScalarWireForm))]
[JsonDerivedType(typeof(NumberScalarWireForm))]
[JsonDerivedType(typeof(BooleanScalarWireForm))]
[JsonDerivedType(typeof(DateScalarWireForm))]
[JsonDerivedType(typeof(InstantScalarWireForm))]
[JsonDerivedType(typeof(WrittenScalarWireForm))]
[JsonDerivedType(typeof(EnumScalarWireForm))]
[JsonDerivedType(typeof(FlagsScalarWireForm))]
[JsonDerivedType(typeof(ListWireForm))]
internal abstract class WireForm
{
    private protected WireForm()
    {
    }

    public abstract string Kind { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(TextScalarWireForm))]
[JsonDerivedType(typeof(NumberScalarWireForm))]
[JsonDerivedType(typeof(BooleanScalarWireForm))]
[JsonDerivedType(typeof(DateScalarWireForm))]
[JsonDerivedType(typeof(InstantScalarWireForm))]
[JsonDerivedType(typeof(WrittenScalarWireForm))]
[JsonDerivedType(typeof(EnumScalarWireForm))]
[JsonDerivedType(typeof(FlagsScalarWireForm))]
internal abstract class ScalarWireForm : WireForm
{
    private protected ScalarWireForm()
    {
    }
}

internal sealed class TextScalarWireForm : ScalarWireForm
{
    internal TextScalarWireForm()
    {
    }

    public override string Kind => "text";
}

internal sealed class NumberScalarWireForm : ScalarWireForm
{
    internal NumberScalarWireForm()
    {
    }

    public override string Kind => "number";
}

internal sealed class BooleanScalarWireForm : ScalarWireForm
{
    internal BooleanScalarWireForm()
    {
    }

    public override string Kind => "boolean";
}

internal sealed class DateScalarWireForm : ScalarWireForm
{
    internal DateScalarWireForm()
    {
    }

    public override string Kind => "date";
}

internal sealed class InstantScalarWireForm : ScalarWireForm
{
    internal InstantScalarWireForm()
    {
    }

    public override string Kind => "instant";
}

internal sealed class WrittenScalarWireForm : ScalarWireForm
{
    internal WrittenScalarWireForm()
    {
    }

    public override string Kind => "written";
}

internal sealed class EnumScalarWireForm : ScalarWireForm
{
    internal EnumScalarWireForm(IReadOnlyList<EnumValue> values)
    {
        Values = values;
    }

    public override string Kind => "enum";

    public IReadOnlyList<EnumValue> Values { get; }
}

internal sealed class FlagsScalarWireForm : ScalarWireForm
{
    internal FlagsScalarWireForm(IReadOnlyList<FlagValue> values)
    {
        Values = values;
    }

    public override string Kind => "flags";

    public IReadOnlyList<FlagValue> Values { get; }
}

internal sealed class ListWireForm : WireForm
{
    internal ListWireForm(ScalarWireForm item, ItemWhenNothing itemWhenNothing)
    {
        Item = item;
        ItemWhenNothing = itemWhenNothing;
    }

    public override string Kind => "list";

    public ScalarWireForm Item { get; }

    public ItemWhenNothing ItemWhenNothing { get; }
}

internal sealed class EnumValue
{
    internal EnumValue(JsonElement held, string name)
    {
        Held = held;
        Name = name;
    }

    public JsonElement Held { get; }

    public string Name { get; }
}

internal sealed class FlagValue
{
    internal FlagValue(JsonElement held, string name, long number)
    {
        Held = held;
        Name = name;
        Number = number;
    }

    public JsonElement Held { get; }

    public string Name { get; }

    public long Number { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(EnumJsonForm))]
[JsonDerivedType(typeof(FlagsJsonForm))]
[JsonDerivedType(typeof(ListJsonForm))]
internal abstract class JsonForm
{
    private protected JsonForm()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class EnumJsonForm : JsonForm
{
    internal EnumJsonForm(IReadOnlyList<JsonEnumValue> values)
    {
        Values = values;
    }

    public override string Kind => "enum";

    public IReadOnlyList<JsonEnumValue> Values { get; }
}

internal sealed class FlagsJsonForm : JsonForm
{
    internal FlagsJsonForm(IReadOnlyList<JsonFlagValue> values)
    {
        Values = values;
    }

    public override string Kind => "flags";

    public IReadOnlyList<JsonFlagValue> Values { get; }
}

internal sealed class ListJsonForm : JsonForm
{
    internal ListJsonForm(JsonForm item, ItemWhenNothing itemWhenNothing)
    {
        Item = item;
        ItemWhenNothing = itemWhenNothing;
    }

    public override string Kind => "list";

    public JsonForm Item { get; }

    public ItemWhenNothing ItemWhenNothing { get; }
}

internal sealed class JsonEnumValue
{
    internal JsonEnumValue(JsonElement held, JsonElement sent)
    {
        Held = held;
        Sent = sent;
    }

    public JsonElement Held { get; }

    public JsonElement Sent { get; }
}

internal sealed class JsonFlagValue
{
    internal JsonFlagValue(JsonElement held, long number, JsonElement sent)
    {
        Held = held;
        Number = number;
        Sent = sent;
    }

    public JsonElement Held { get; }

    public long Number { get; }

    public JsonElement Sent { get; }
}
