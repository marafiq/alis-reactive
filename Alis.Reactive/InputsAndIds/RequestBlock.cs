using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;

namespace Alis.Reactive.InputsAndIds;

internal sealed class RequestBlock
{
    private readonly TextWriter _writer;
    private readonly string _element;
    private readonly Action _checkPrefix;
    private bool _closed;

    internal RequestBlock(TextWriter writer, PlanContent plan, string id, ContainerKind kind, IReadOnlyList<Live> live, Action checkPrefix)
    {
        _writer = writer;
        _element = kind == ContainerKind.Form ? "form" : "fieldset";
        _checkPrefix = checkPrefix;
        var summary = id + "_summary";
        ModelPlugin.Contains(plan, id, kind, summary, live);
        var method = kind == ContainerKind.Form ? " method=\"post\"" : "";
        writer.Write($"<{_element} id=\"{WebUtility.HtmlEncode(id)}\"{method}>");
        writer.Write($"<div id=\"{WebUtility.HtmlEncode(summary)}\" class=\"validation-summary-valid\" hidden><ul></ul></div>");
    }

    internal void Close()
    {
        if (_closed) return;
        _checkPrefix();
        _writer.Write($"</{_element}>");
        _closed = true;
    }
}
