using System;
using System.Linq;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.RequestsAndResponses;

internal static class WireRecords
{
    internal static RecordExpression Of(WireForm form)
    {
        var record = new ContractRecord<WireForm>().Set(wire => wire.Kind, form.Kind);
        return form switch
        {
            TextScalarWireForm or NumberScalarWireForm or BooleanScalarWireForm or DateScalarWireForm or InstantScalarWireForm or WrittenScalarWireForm => record.Written,
            EnumScalarWireForm enumeration => new ContractRecord<EnumScalarWireForm>()
                .Set(wire => wire.Kind, enumeration.Kind)
                .Set(wire => wire.Values, new ListExpression(enumeration.Values.Select(value => new ContractRecord<EnumValue>()
                    .Set(row => row.Held, new LitExpression(value.Held)).Set(row => row.Name, value.Name).Written).ToArray())).Written,
            FlagsScalarWireForm flags => new ContractRecord<FlagsScalarWireForm>()
                .Set(wire => wire.Kind, flags.Kind)
                .Set(wire => wire.Values, new ListExpression(flags.Values.Select(value => new ContractRecord<FlagValue>()
                    .Set(row => row.Held, new LitExpression(value.Held)).Set(row => row.Name, value.Name).Set(row => row.Number, value.Number).Written).ToArray())).Written,
            ListWireForm list => new ContractRecord<ListWireForm>().Set(wire => wire.Kind, list.Kind)
                .Set(wire => wire.Item, Of(list.Item)).Set(wire => wire.ItemWhenNothing, list.ItemWhenNothing).Written,
            _ => throw new InvalidOperationException("A request wire form has no contract writer."),
        };
    }
}
