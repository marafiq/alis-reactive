using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.ValuesAndSources;

internal static class SentForms
{
    internal static WhenNothing Absence(Type type) => type == typeof(string) || Nullable.GetUnderlyingType(type) is not null ? WhenNothing.Empty : WhenNothing.Leave;

    internal static ItemWhenNothing ItemAbsence(Type type) => Absence(type) == WhenNothing.Empty ? ItemWhenNothing.Empty : ItemWhenNothing.Stop;

    internal static WireForm SourceForm(Type type, HeldForm held, PlanContent content, string member)
    {
        var contract = held.DataOptions.GetTypeInfo(type);
        if (contract.IsList(out var item))
        {
            var form = SourceForm(item.Type, held, content, member);
            if (form is not ScalarWireForm scalar) throw Object(member);
            return new ListWireForm(scalar, ItemAbsence(item.Type));
        }

        if (EnumRows.IsEnum(type)) return EnumForm(type, held, content, member);
        if (!held.WritesAsAScalar(type)) throw Object(member);
        var writesNumberAsText = ComparedTypes.IsNumber(type) && ((contract.NumberHandling ?? held.DataOptions.NumberHandling) & JsonNumberHandling.WriteAsString) != 0;
        return Scalar(type, held.WritesOwnScalar(type) || writesNumberAsText);
    }

    internal static ScalarWireForm Scalar(Type type, bool written)
    {
        if (written) return new WrittenScalarWireForm();
        if (type == typeof(string) || ComparedTypes.IsChar(type)) return new TextScalarWireForm();
        if (ComparedTypes.IsNumber(type)) return new NumberScalarWireForm();
        if (type == typeof(bool) || type == typeof(bool?)) return new BooleanScalarWireForm();
        if (type == typeof(DateTime) || type == typeof(DateTime?)) return new DateScalarWireForm();
        if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?)) return new InstantScalarWireForm();
        return new WrittenScalarWireForm();
    }

    internal static ScalarWireForm EnumForm(Type type, HeldForm held, PlanContent content, string member)
    {
        var rows = HeldRows(type, held, content, member);
        return content.NamesFlags(Nullable.GetUnderlyingType(type) ?? type)
            ? new FlagsScalarWireForm(rows.Select(row => new FlagValue(row.Held, row.Name, row.Number)).OrderByDescending(row => row.Number).ToArray())
            : new EnumScalarWireForm(rows.Select(row => new EnumValue(row.Held, row.Name)).ToArray());
    }

    internal static List<EnumRows.HeldRow> HeldRows(Type type, HeldForm held, PlanContent content, string member)
        => EnumRows.Of(type, content.NamesFlags(EnumRows.EnumOf(type)), member, (value, name) =>
        {
            var written = held.Beside(value, type).Value;
            if (EnumRows.IsOneTextNumberOrBoolean(written)) return written;
            throw new InvalidOperationException($"'{member}' holds {name} as {written.GetRawText()} under its own JSON converter, and a request sends one text or number per member; send a member written as text or a number.");
        });

    internal static InvalidOperationException Object(string member)
        => new($"'{member}' holds an object or a nested list; send its scalar members by their paths, or send it in a JSON body.");

    internal static JsonForm JsonFormOf(Type type, HeldForm held, PlanContent content, string member)
    {
        if (held.DataOptions.GetTypeInfo(type).IsList(out var item)) return new ListJsonForm(JsonFormOf(item.Type, held, content, member), ItemAbsence(item.Type));
        var enumeration = Nullable.GetUnderlyingType(type) ?? type;
        var rows = HeldRows(type, held, content, member);
        var json = content.Json;
        if (!content.NamesFlags(enumeration))
            return new EnumJsonForm(rows.Select(row => new JsonEnumValue(row.Held, EnumEndpoints.Sent(row.Value, enumeration, json))).ToArray());
        var flags = rows.Select(row => new JsonFlagValue(row.Held, row.Number, EnumEndpoints.Sent(row.Value, enumeration, json))).OrderByDescending(row => row.Number).ToArray();
        var sentAsTexts = flags.All(row => row.Sent.ValueKind == JsonValueKind.String);
        var sentAsNumbers = flags.All(row => row.Sent.ValueKind == JsonValueKind.Number);
        if (!sentAsTexts && !sentAsNumbers)
            throw new InvalidOperationException($"'{member}' is a flags enum its endpoint's JSON options write as texts and numbers apart, and a JSON body joins a combination's texts or sends its number; write each value of it as text, or each as its number.");
        return new FlagsJsonForm(flags);
    }
}
