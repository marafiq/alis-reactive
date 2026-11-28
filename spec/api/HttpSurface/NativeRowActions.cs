using System;
using System.IO;
using System.Text.Encodings.Web;
using Alis.Reactive.Components;
using Alis.Reactive.Elements;
using Alis.Reactive.Hosting;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Values;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Native;

public sealed class RowActivated<TRow> : JsObject
{
    public ReadOnly<TRow> Row => ReadOnly<TRow>();

    public Element Element => LiveObject<Element>();
}

public sealed class RowAction<TModel, TRow>
{
    private RowAction()
    {
    }

    public RowControl Render(TRow row, string text) => throw new NotSupportedException();
}

public sealed class RowControl : IHtmlContent
{
    private RowControl()
    {
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder) => throw new NotSupportedException();
}

public static class NativeRowActions
{
    public static RowAction<TModel, TRow> NativeActionButton<TModel, TRow>(this Rows<TModel, TRow> rows, Action<RowActivated<TRow>, Pipeline<TModel>> action)
        => throw new NotSupportedException();

    public static RowAction<TModel, TRow> NativeActionLink<TModel, TRow>(this Rows<TModel, TRow> rows, Action<RowActivated<TRow>, Pipeline<TModel>> action)
        => throw new NotSupportedException();
}
