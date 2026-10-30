using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// A statement of the plan's grammar (grammar.md 1.2). Six productions. Only <c>do</c> and
    /// <c>let</c> await, and only when their call is to an async member.
    /// </summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<Statement>))]
    internal abstract class Statement
    {
        private protected Statement() { }
    }

    /// <summary><c>set(target, path, value)</c>: writes a member; the value is converted where it is read (<c>as</c>).</summary>
    internal sealed class SetStatement : Statement
    {
        internal SetStatement(Expr target, IReadOnlyList<string> path, Expr value)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            if (path.Count == 0) throw new ArgumentException("A set needs the member it writes.", nameof(path));
        }

        public string Kind => "set";
        public Expr Target { get; }
        public IReadOnlyList<string> Path { get; }
        public Expr Value { get; }
    }

    /// <summary><c>do(expr, await)</c>: evaluates an expression for its effect.</summary>
    internal sealed class DoStatement : Statement
    {
        internal DoStatement(Expr expr, bool awaits)
        {
            Expr = expr ?? throw new ArgumentNullException(nameof(expr));
            Await = awaits;
        }

        public string Kind => "do";
        public Expr Expr { get; }
        public bool Await { get; }
    }

    /// <summary><c>let(name, expr, await)</c>: binds a value for the rest of the block.</summary>
    internal sealed class LetStatement : Statement
    {
        internal LetStatement(string name, Expr expr, bool awaits)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Binding name required.", nameof(name));
            Name = name;
            Expr = expr ?? throw new ArgumentNullException(nameof(expr));
            Await = awaits;
        }

        public string Kind => "let";
        public string Name { get; }
        public Expr Expr { get; }
        public bool Await { get; }
    }

    /// <summary><c>if(arms, else)</c>: runs the first arm whose condition is truthy, else the else block.</summary>
    internal sealed class IfStatement : Statement
    {
        internal IfStatement(IReadOnlyList<Arm> arms, IReadOnlyList<Statement> otherwise)
        {
            Arms = arms ?? throw new ArgumentNullException(nameof(arms));
            Else = otherwise ?? throw new ArgumentNullException(nameof(otherwise));
            if (arms.Count == 0) throw new ArgumentException("An if needs at least one arm.", nameof(arms));
        }

        public string Kind => "if";
        public IReadOnlyList<Arm> Arms { get; }
        public IReadOnlyList<Statement> Else { get; }
    }

    /// <summary>An arm of an <c>if</c>: a condition and the block it guards.</summary>
    internal sealed class Arm
    {
        internal Arm(Expr when, IReadOnlyList<Statement> then)
        {
            When = when ?? throw new ArgumentNullException(nameof(when));
            Then = then ?? throw new ArgumentNullException(nameof(then));
        }

        public Expr When { get; }
        public IReadOnlyList<Statement> Then { get; }
    }

    /// <summary><c>all(blocks)</c>: starts every block, then waits until all have settled.</summary>
    internal sealed class AllStatement : Statement
    {
        internal AllStatement(IReadOnlyList<IReadOnlyList<Statement>> blocks)
        {
            Blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
        }

        public string Kind => "all";
        public IReadOnlyList<IReadOnlyList<Statement>> Blocks { get; }
    }

    /// <summary><c>try(body, finally)</c>: runs the finally block on every exit from the body.</summary>
    internal sealed class TryStatement : Statement
    {
        internal TryStatement(IReadOnlyList<Statement> body, IReadOnlyList<Statement> cleanup)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Finally = cleanup ?? throw new ArgumentNullException(nameof(cleanup));
        }

        public string Kind => "try";
        public IReadOnlyList<Statement> Body { get; }
        public IReadOnlyList<Statement> Finally { get; }
    }
}
