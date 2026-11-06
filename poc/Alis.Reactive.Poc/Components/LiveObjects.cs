using System;
using System.Collections.Generic;
using Alis.Reactive.Lists;
using Alis.Reactive.Plan;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Components;

public sealed class LiveObjects<TItem>
    where TItem : JsObject, new()
{
    private readonly Expression _list;
    private readonly PlanBuild _build;

    internal LiveObjects(Expression list, PlanBuild build)
    {
        _list = list;
        _build = build;
    }

    public TItem At(int position)
    {
        if (position < 0) throw new ArgumentOutOfRangeException(nameof(position), position, "An item's position is zero or more.");
        return JsObject.Bound<TItem>(new Binding(new GetExpression(_list, [PathStep.Index(position)], ReadPresence.Required), _build));
    }

    public TypedSource<int> Count() => new DataRead<int>(ListForms.Length(_list), _build);

    public LiveObjects<TItem> Where(Func<TItem, TypedSource<bool>> keep)
        => new(ListForms.Filter(_list, ItemRead(keep)), _build);

    public TypedSource<IReadOnlyList<TResult>> Select<TResult>(Func<TItem, TypedSource<TResult>> field)
        => new DataRead<IReadOnlyList<TResult>>(ListForms.Map(_list, ItemRead(field)), _build);

    private FnExpression ItemRead<TResult>(Func<TItem, TypedSource<TResult>> read)
    {
        var name = _build.ItemName();
        var item = JsObject.Bound<TItem>(new Binding(new RefExpression(name), _build));
        var source = _build.ReadingItems(() => read(item))
            ?? throw new ArgumentException("A list's lambda reads a member of its item, such as bed => bed.Resident.", nameof(read));
        return new FnExpression([name], source.Expression);
    }
}
