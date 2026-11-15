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
        if (!MemberPath.TryOf(path, options, out var members)) throw NoMemberPath(path);
        members.RefuseAReadPastItsOwnConverter(path);
        var form = members.EndsInItsOwnForm ? holdersForm.OwnForm : holdersForm;
        return new Computed<TResult>(new DataExpression(members.DataSteps(), holder), form);
    }

    private static InvalidOperationException NoMemberPath(LambdaExpression path)
        => new($"'{path}' names no member path, so the browser has no member to read; read a member and the members past it, such as record => record.Detail.Name.");
}
