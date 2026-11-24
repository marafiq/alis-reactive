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
            return (held, new ListWireForm(itemForm, SentForms.ItemAbsence(item.Type)));
        }

        return (LiteralValue(value, type, writer, member), LiteralForm(type));
    }

    internal static (Expression Value, WireForm Form, RefExpression Captured) Source<TValue>(ITypedSource<TValue> source, BlockWriter writer, string member, Action<LetStatement> capture)
    {
        var form = Sources.FormOf(source);
        var wire = SentForms.SourceForm(typeof(TValue), form, writer.Content, member);
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

    private static ScalarWireForm LiteralForm(Type type)
    {
        if (EnumRows.IsEnum(type)) return new TextScalarWireForm();
        return SentForms.Scalar(type, false);
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

    private static bool ReadsDate(WireForm form) => form is DateScalarWireForm or InstantScalarWireForm ||
        form is ListWireForm { Item: DateScalarWireForm or InstantScalarWireForm };

}
