using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Execution;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.RequestsAndResponses;

internal static class RequestValues
{
    internal static (Expression Value, WireForm Form) Literal<TValue>(TValue value, BlockWriter writer, string member)
    {
        var (held, form) = LiteralHeld(value, writer, member);
        return (Held(held), form);
    }

    internal static (Expression Held, WireForm Form) LiteralHeld<TValue>(TValue value, BlockWriter writer, string member)
    {
        var type = typeof(TValue);
        var contract = writer.Content.Json.LiveForm.GetTypeInfo(type);
        if (contract.IsList(out var item))
        {
            var itemForm = LiteralForm(item.Type);
            var held = value is IEnumerable list
                ? new ListExpression(list.Cast<object?>().Select(entry => LiteralValue(entry, item.Type, writer, member)).ToArray())
                : (Expression)ContractRecord<ValueRead>.Literal<string?>(null);
            return (held, new ListWireForm(itemForm, ItemAbsence(item.Type)));
        }

        return (LiteralValue(value, type, writer, member), LiteralForm(type));
    }

    internal static (Expression Value, WireForm Form, RefExpression Captured) Source<TValue>(ITypedSource<TValue> source, BlockWriter writer, string member, Action<LetStatement> capture)
    {
        var form = Sources.FormOf(source);
        var wire = SourceForm(typeof(TValue), form, writer, member);
        var captured = Captured(ReadsDate(wire) ? form.NumbersAndDatesIntoLive<TValue>(Sources.Of(source)) : Sources.Of(source), writer, capture);
        return (Read(captured), wire, captured);
    }

    internal static RefExpression Captured(Expression value, BlockWriter writer, Action<LetStatement> capture)
    {
        var captured = writer.NextBinding("requestValue");
        capture(new LetStatement(captured.Name, false, LetBinds.Line, value));
        return captured;
    }

    internal static OpExpression Read(Expression captured) => Read(captured, captured);

    internal static OpExpression ReadUnlessNull(Expression captured, Expression held)
        => new(Operator.Cond,
        [
            new OpExpression(Operator.Neq, [captured, ContractRecord<ValueRead>.Literal<string?>(null)]),
            Held(held),
            new ContractRecord<NothingValueRead>().Set(read => read.Kind, "nothing").Written,
        ]);

    internal static OpExpression Read(Expression captured, Expression held)
        => new(Operator.Cond,
        [
            ReadsSomething(captured),
            Held(held),
            new ContractRecord<NothingValueRead>().Set(read => read.Kind, "nothing").Written,
        ]);

    internal static OpExpression ReadsSomething(Expression captured)
    {
        var nil = ContractRecord<ValueRead>.Literal<string?>(null);
        return new OpExpression(Operator.Or, [new OpExpression(Operator.Eq, [captured, nil]), new OpExpression(Operator.Neq, [captured, nil])]);
    }

    internal static RecordExpression Held(Expression value)
        => new ContractRecord<HeldValueRead>().Set(read => read.Kind, "held").Set(read => read.Value, value).Written;

    internal static WhenNothing Absence(Type type) => type == typeof(string) || Nullable.GetUnderlyingType(type) is not null ? WhenNothing.Empty : WhenNothing.Leave;

    private static ItemWhenNothing ItemAbsence(Type type) => Absence(type) == WhenNothing.Empty ? ItemWhenNothing.Empty : ItemWhenNothing.Stop;

    private static ScalarWireForm LiteralForm(Type type)
    {
        if (EnumRows.IsEnum(type)) return new TextScalarWireForm();
        return Scalar(type, false);
    }

    private static Expression LiteralValue(object? value, Type type, BlockWriter writer, string member)
    {
        if (value is Enum enumeration)
        {
            var name = enumeration.ToString();
            if (name.Length == 0 || name[0] == '-' || char.IsDigit(name[0]))
                throw new InvalidOperationException($"'{member}' is set to {name}, which no value of its enum names: net10 refuses it and MVC 5 binds it, so the hosts bind it apart; set a value its enum names.");
            return ContractRecord<ValueRead>.Literal(name);
        }

        Literals.RefuseWhereJavaScriptReadsAnother(value, member);
        var written = Literals.Of(value, type, writer.Content.Json);
        if (written.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
            throw new InvalidOperationException($"'{member}' holds an object or a nested list; send its scalar members by their paths, or send it in a JSON body.");
        return LiveShapes.Converts(writer.Content.Json.LiveForm.GetTypeInfo(type), out var shape) ? new AsExpression(written, shape) : written;
    }

    private static WireForm SourceForm(Type type, HeldForm held, BlockWriter writer, string member)
    {
        var contract = held.DataOptions.GetTypeInfo(type);
        if (contract.IsList(out var item))
        {
            var form = SourceForm(item.Type, held, writer, member);
            if (form is not ScalarWireForm scalar) throw Object(member);
            return new ListWireForm(scalar, ItemAbsence(item.Type));
        }

        if (EnumRows.IsEnum(type)) return EnumForm(type, held, writer, member);
        if (!held.WritesAsAScalar(type)) throw Object(member);
        var writesNumberAsText = ComparedTypes.IsNumber(type) && ((contract.NumberHandling ?? held.DataOptions.NumberHandling) & JsonNumberHandling.WriteAsString) != 0;
        return Scalar(type, held.WritesOwnScalar(type) || writesNumberAsText);
    }

    private static bool ReadsDate(WireForm form) => form is DateScalarWireForm or InstantScalarWireForm ||
        form is ListWireForm { Item: DateScalarWireForm or InstantScalarWireForm };

    private static ScalarWireForm Scalar(Type type, bool written)
    {
        if (written) return new WrittenScalarWireForm();
        if (type == typeof(string) || ComparedTypes.IsChar(type)) return new TextScalarWireForm();
        if (ComparedTypes.IsNumber(type)) return new NumberScalarWireForm();
        if (type == typeof(bool) || type == typeof(bool?)) return new BooleanScalarWireForm();
        if (type == typeof(DateTime) || type == typeof(DateTime?)) return new DateScalarWireForm();
        if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?)) return new InstantScalarWireForm();
        return new WrittenScalarWireForm();
    }

    internal static JsonForm JsonFormOf(Type type, HeldForm held, BlockWriter writer, string member)
    {
        if (held.DataOptions.GetTypeInfo(type).IsList(out var item)) return new ListJsonForm(JsonFormOf(item.Type, held, writer, member), ItemAbsence(item.Type));
        var enumeration = Nullable.GetUnderlyingType(type) ?? type;
        var rows = HeldRows(type, held, writer, member);
        var json = writer.Content.Json;
        if (!writer.Content.NamesFlags(enumeration))
            return new EnumJsonForm(rows.Select(row => new JsonEnumValue(row.Held, EnumEndpoints.Sent(row.Value, enumeration, json))).ToArray());
        var flags = rows.Select(row => new JsonFlagValue(row.Held, row.Number, EnumEndpoints.Sent(row.Value, enumeration, json))).OrderByDescending(row => row.Number).ToArray();
        var sentAsTexts = flags.All(row => row.Sent.ValueKind == JsonValueKind.String);
        var sentAsNumbers = flags.All(row => row.Sent.ValueKind == JsonValueKind.Number);
        if (!sentAsTexts && !sentAsNumbers)
            throw new InvalidOperationException($"'{member}' is a flags enum its endpoint's JSON options write as texts and numbers apart, and a JSON body joins a combination's texts or sends its number; write each value of it as text, or each as its number.");
        return new FlagsJsonForm(flags);
    }

    private static ScalarWireForm EnumForm(Type type, HeldForm held, BlockWriter writer, string member)
    {
        var rows = HeldRows(type, held, writer, member);
        return writer.Content.NamesFlags(Nullable.GetUnderlyingType(type) ?? type)
            ? new FlagsScalarWireForm(rows.Select(row => new FlagValue(row.Held, row.Name, row.Number)).OrderByDescending(row => row.Number).ToArray())
            : new EnumScalarWireForm(rows.Select(row => new EnumValue(row.Held, row.Name)).ToArray());
    }

    private static List<EnumRows.HeldRow> HeldRows(Type type, HeldForm held, BlockWriter writer, string member)
        => EnumRows.Of(type, writer.Content.NamesFlags(EnumRows.EnumOf(type)), member, (value, name) =>
        {
            var written = held.Beside(value, type).Value;
            if (EnumRows.IsOneTextNumberOrBoolean(written)) return written;
            throw new InvalidOperationException($"'{member}' holds {name} as {written.GetRawText()} under its own JSON converter, and a request sends one text or number per member; send a member written as text or a number.");
        });

    private static InvalidOperationException Object(string member)
        => new($"'{member}' holds an object or a nested list; send its scalar members by their paths, or send it in a JSON body.");
}
