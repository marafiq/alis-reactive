using System;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Expression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.ValuesAndSources;

internal static class DataReads
{
    internal static TypedSource<TResult> Past<TResult>(Expression holder, HeldForm holdersForm, LambdaExpression path, JsonSerializerOptions options)
    {
        RefuseAReadPastAHolderItsConverterWrites(holdersForm, path);
        if (!MemberPath.TryOf(path, options, out var members)) throw MemberPath.NoMemberPath(path);
        var read = path.Body.ToString();
        members.RefuseAReadPastItsOwnConverter(read);
        var form = members.Member.HoldsItsOwnForm ? holdersForm.WrittenBy(members.Member, read) : holdersForm;
        return new Computed<TResult>(new DataExpression(members.DataSteps(), holder), form);
    }

    private static void RefuseAReadPastAHolderItsConverterWrites(HeldForm holdersForm, LambdaExpression path)
    {
        if (holdersForm.Own is not { Member.HasItsOwnConverter: true } || path.Body is not MemberExpression) return;
        var holder = path.Parameters[0].Name;
        throw new InvalidOperationException(
            $"'{path.Body}' reads past '{holder}', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read '{holder}' itself, or read past it on the server.");
    }
}
