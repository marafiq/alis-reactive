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
    private readonly Action _checkPrefix;
    private bool _closed;

    internal RequestBlock(TextWriter writer, PlanContent plan, string id, string prefix, IReadOnlyList<Live> live, Action checkPrefix)
    {
        _writer = writer;
        _checkPrefix = checkPrefix;
        var summary = id + "_summary";
        var underNoPrefix = prefix.Length == 0;
        PathStep[] steps = underNoPrefix ? [] : [.. BoundInput.PrefixSteps(prefix, plan.Json.App)];
        ModelPlugin.Contains(plan, id, new FieldPrefix(prefix, steps), summary, live);
        writer.Write($"<fieldset id=\"{WebUtility.HtmlEncode(id)}\">");
        writer.Write($"<div id=\"{WebUtility.HtmlEncode(summary)}\" class=\"validation-summary-valid\" hidden><ul></ul></div>");
    }

    internal void Close()
    {
        if (_closed) return;
        _checkPrefix();
        _writer.Write("</fieldset>");
        _closed = true;
    }
}
