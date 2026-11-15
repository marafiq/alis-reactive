using System;
using System.Collections.Generic;
using System.Globalization;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Execution;

internal sealed class BlockWriter
{
    private readonly Stack<List<Statement>> _open = new();
    private readonly Stack<HandlerResults> _handlers = new();
    private readonly Stack<LambdaResults> _lambdas = new();
    private readonly Dictionary<string, int> _bindings = new(StringComparer.Ordinal);
    private int _results;
    private int _liveItems;
    private int _answers;
    private int _answerDepth;
    private int _listens;

    internal int ListenDepth { get; private set; }

    internal BlockWriter(PlanContent content)
    {
        Content = content;
    }

    internal PlanContent Content { get; }

    internal ResultBinding Result(CallExpression call, bool awaits)
    {
        if (_answerDepth > 0 && awaits) throw AsyncInAnswer();
        if (_lambdas.Count > 0 && awaits) throw AsyncInLambda();
        var name = ResultBinding.NameStem + (++_results).ToString(CultureInfo.InvariantCulture);
        if (_lambdas.Count > 0) return _lambdas.Peek().Add(name, call);
        Write(new LetStatement(name, awaits, LetBinds.Line, call));
        return _handlers.Peek().Add(name);
    }

    internal string NextLiveItem() => "liveItem" + (++_liveItems).ToString(CultureInfo.InvariantCulture);

    internal RefExpression NextBinding(string stem)
    {
        var number = _bindings.TryGetValue(stem, out var last) ? last + 1 : 1;
        _bindings[stem] = number;
        return new RefExpression(stem + number.ToString(CultureInfo.InvariantCulture));
    }

    internal (string Payload, string Arguments) NextListen()
    {
        var count = (++_listens).ToString(CultureInfo.InvariantCulture);
        return ("listenPayload" + count, "listenArguments" + count);
    }

    internal IReadOnlyList<Statement> Listening(Action body)
    {
        ListenDepth++;
        try
        {
            return Block(body);
        }
        finally
        {
            ListenDepth--;
        }
    }

    internal string NextAnswer() => "answer" + (++_answers).ToString(CultureInfo.InvariantCulture);

    internal AnswerExpression Answer(string arguments, Func<Expression> body)
    {
        var results = new LambdaResults();
        _lambdas.Push(results);
        _answerDepth++;
        try
        {
            return new AnswerExpression(arguments, results.Complete(body()));
        }
        finally
        {
            results.Closed = true;
            _lambdas.Pop();
            _answerDepth--;
        }
    }

    internal (Expression Body, TValue Value) Lambda<TValue>(Func<(Expression Body, TValue Value)> body)
    {
        var lambda = new LambdaResults();
        _lambdas.Push(lambda);
        try
        {
            var written = body();
            return (lambda.Complete(written.Body), written.Value);
        }
        finally
        {
            lambda.Closed = true;
            _lambdas.Pop();
        }
    }

    internal IReadOnlyList<Statement> Handler(Action body)
    {
        var handler = new HandlerResults();
        _handlers.Push(handler);
        try
        {
            return handler.Complete(Block(body));
        }
        finally
        {
            handler.Closed = true;
            _handlers.Pop();
        }
    }

    internal IReadOnlyList<Statement> TakeResults(Place place, Expression condition)
    {
        if (_open.Count == 0 || !ReferenceEquals(_open.Peek(), place.Block)) throw BrokenSentence();
        var suffix = place.Block.GetRange(place.Index + 1, place.Block.Count - place.Index - 1);
        var needed = new HashSet<RefExpression>();
        foreach (var read in ExpressionReads.In(condition)) needed.Add(read.Reference);
        var handler = _handlers.Peek();
        for (var index = suffix.Count - 1; index >= 0; index--)
        {
            if (suffix[index] is not LetStatement result || !handler.IsResult(result) || !needed.Contains(handler.ReferenceOf(result)))
                throw BrokenSentence();
            foreach (var read in ExpressionReads.In(result.Expr)) needed.Add(read.Reference);
        }

        foreach (var statement in suffix) handler.MovedToElseIf((LetStatement)statement);
        place.Block.RemoveRange(place.Index + 1, suffix.Count);
        return suffix;
    }

    internal static InvalidOperationException BrokenSentence()
        => new("An ElseIf or an Else continues the When it follows, before any other step of its block: write it in the same sentence.");

    internal void Write(Statement statement)
    {
        if (_answerDepth > 0)
        {
            if (statement is DoStatement { Await: true }) throw AsyncInAnswer();
            throw new InvalidOperationException("A step is written inside a callback's answer, which answers its vendor with one value. Write the step in a pipeline.");
        }

        if (_lambdas.Count > 0)
        {
            if (statement is DoStatement { Await: true }) throw AsyncInLambda();
            throw new InvalidOperationException("A step is written inside a list's lambda, which reads each item and writes nothing. Write the step in the pipeline.");
        }

        if (_open.Count == 0)
            throw new InvalidOperationException("A step is written inside a pipeline, the callback its trigger runs: no pipeline is open here.");
        _open.Peek().Add(statement);
    }

    internal Place Placed(Statement statement)
    {
        Write(statement);
        var block = _open.Peek();
        return new Place(block, block.Count - 1);
    }

    internal bool Replaced(Place place, Statement statement)
    {
        var stillItsBlocksLastStep = _open.Count > 0 && ReferenceEquals(_open.Peek(), place.Block) && place.Index == place.Block.Count - 1;
        if (!stillItsBlocksLastStep) return false;
        place.Block[place.Index] = statement;
        return true;
    }

    internal IReadOnlyList<Statement> Block(Action body)
    {
        var block = new List<Statement>();
        _open.Push(block);
        try
        {
            body();
        }
        finally
        {
            _open.Pop();
        }

        return block;
    }

    private static InvalidOperationException AsyncInAnswer()
        => new("A callback answers its vendor at once, so its answer cannot wait for an async method or a request.");

    private static InvalidOperationException AsyncInLambda()
        => new("An async method is called inside a list's lambda, which reads every item in one turn and waits for nothing. Call the method in the pipeline and read its result in the lambda.");

    internal sealed record Place(List<Statement> Block, int Index);
}
