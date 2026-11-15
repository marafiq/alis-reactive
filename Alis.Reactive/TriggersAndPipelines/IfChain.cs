using System;
using System.Collections.Generic;
using Alis.Reactive.Execution;
using Alis.Reactive.Schema;

namespace Alis.Reactive.TriggersAndPipelines;

internal sealed record Arm(Expression When, IReadOnlyList<Statement> Then, IReadOnlyList<Statement> Results);

internal sealed class IfChain
{
    private readonly BlockWriter _writer;
    private readonly List<Arm> _arms;
    private readonly BlockWriter.Place _place;
    private IReadOnlyList<Statement> _otherwise = [];
    private bool _ended;

    private IfChain(BlockWriter writer, Arm first)
    {
        _writer = writer;
        _arms = [first];
        _place = writer.Placed(Folded());
    }

    internal static IfChain Opened(BlockWriter writer, Arm first) => new(writer, first);

    internal IReadOnlyList<Statement> TakeResults(Expression condition)
    {
        RefuseAfterItsElse();
        return _writer.TakeResults(_place, condition);
    }

    internal IfChain Continued(Arm arm)
    {
        RefuseAfterItsElse();
        _arms.Add(arm);
        Rewrite();
        return this;
    }

    internal void Ended(IReadOnlyList<Statement> otherwise)
    {
        _otherwise = otherwise;
        _ended = true;
        Rewrite();
    }

    internal void RefuseAfterItsElse()
    {
        if (!_ended) return;
        throw new InvalidOperationException("An Else ends its When: write each ElseIf before it, and every step of the remaining case in that one Else.");
    }

    private void Rewrite()
    {
        if (_writer.Replaced(_place, Folded())) return;
        throw BlockWriter.BrokenSentence();
    }

    private IfStatement Folded()
    {
        var otherwise = _otherwise;
        for (var arm = _arms.Count - 1; arm > 0; arm--)
            otherwise = [.. _arms[arm].Results, new IfStatement(_arms[arm].When, _arms[arm].Then, otherwise)];
        return new IfStatement(_arms[0].When, _arms[0].Then, otherwise);
    }
}
