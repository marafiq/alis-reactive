using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.InputsAndIds;

internal static class EnumInputRows
{
    internal static Shape Of(Shape live, Type member, JsonSerializerOptions options, ComponentInput input, string name)
    {
        var bindsAList = live is ArrayShape;
        var memberItem = bindsAList && options.GetTypeInfo(member).IsList(out var listed) ? listed.Type : member;
        var enumeration = EnumRows.EnumOf(memberItem);
        var holdsAList = options.GetTypeInfo(input.Value).IsList(out var item);
        var held = holdsAList ? item!.Type : input.Value;
        var holdsText = held == typeof(string);
        var holdsTheEnum = !holdsText && EnumRows.IsEnum(held) && EnumRows.EnumOf(held) == enumeration;
        if (bindsAList != holdsAList || !(holdsText || holdsTheEnum)) throw HoldsNoValueOf(name, member, input.Value, enumeration, bindsAList);
        if (holdsTheEnum) return live;
        var texts = Texts(enumeration, live is ArrayShape array ? array.Item : live, options, name);
        return bindsAList ? new ArrayShape(texts) : texts;
    }

    private static Shape Texts(Type enumeration, Shape item, JsonSerializerOptions options, string name)
    {
        if (item is NullableShape nullable) return new NullableShape(Texts(enumeration, nullable.Inner, options, name));
        var isFlags = ((EnumShape)item).Flags;
        var live = options.GetTypeInfo(enumeration);
        var rows = new List<LiveEnumValue>();
        var byText = new Dictionary<string, (string Value, string Form)>(StringComparer.Ordinal);
        foreach (var row in EnumRows.Of(enumeration, isFlags, name, (value, _) => JsonSerializer.SerializeToElement(value, live)))
        {
            foreach (var (text, form) in TextsOf(row))
            {
                if (byText.TryGetValue(text, out var first))
                {
                    if (first.Value == row.Name) continue;
                    throw TwoValuesInOneText(name, (row.Name, form), first, text);
                }

                byText.Add(text, (row.Name, form));
                rows.Add(new LiveEnumValue(JsonSerializer.SerializeToElement(text, PlanJson.Options), row.Held, row.Number));
            }
        }

        return new EnumShape(isFlags, isFlags ? [.. rows.OrderByDescending(row => row.Number)] : rows);
    }

    private static IEnumerable<(string Text, string Form)> TextsOf(EnumRows.HeldRow row)
    {
        yield return (row.Name, "the C# name of " + row.Name);
        yield return (row.Value.ToString("D"), "the number of " + row.Name);
        yield return (row.Held.ValueKind == JsonValueKind.String ? row.Held.GetString()! : row.Held.GetRawText(), "the name " + row.Name + "'s JSON converter writes");
    }

    private static InvalidOperationException TwoValuesInOneText(string name, (string Value, string Form) second, (string Value, string Form) first, string text)
    {
        var converterNamed = second.Form.StartsWith("the name ", StringComparison.Ordinal) ? second.Value : first.Value;
        return new($"Input '{name}' holds {second.Value} and {first.Value} in one text, '{text}', {second.Form} and {first.Form}, so the browser cannot tell which one it holds; give {converterNamed} a JsonStringEnumMemberName no value's C# name or number spells.");
    }

    private static InvalidOperationException HoldsNoValueOf(string name, Type member, Type value, Type enumeration, bool bindsAList)
    {
        var enumName = ClassNames.AsCSharpWritesIt(enumeration);
        var holds = bindsAList ? "list" : "value";
        var cure = bindsAList ? "a list of texts or of " + enumName : "text or " + enumName;
        return new($"Input '{name}' binds {ClassNames.AsCSharpWritesIt(member)}, and its component's value is {ClassNames.AsCSharpWritesIt(value)}, which holds no {holds} of {enumName}: render the field with a component whose value is {cure}.");
    }
}
