using System;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Execution;

internal sealed class ResultBinding
{
    internal const string NameStem = "result";

    private readonly HandlerResults _handler;

    internal ResultBinding(string name, HandlerResults handler)
    {
        Reference = new RefExpression(name);
        _handler = handler;
    }

    internal RefExpression Reference { get; }

    internal bool FromElseIf { get; set; }

    internal RefExpression Read()
    {
        if (_handler.Closed) throw OutsideBlock();
        return Reference;
    }

    internal static InvalidOperationException OutsideBlock()
        => new("A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it.");

    internal static InvalidOperationException AfterWhen()
        => new("A method's result called in an ElseIf's condition is read after its When, and the call runs only when the ElseIf is reached: call the method before the When to read its result after it.");
}
