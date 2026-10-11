using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// An expression of the plan's grammar (grammar.md 1.2). Nine productions; the runtime evaluates
    /// each synchronously. Nodes are immutable and may be shared, so the lowering can repeat a block.
    /// </summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<Expr>))]
    internal abstract class Expr
    {
        private protected Expr() { }
    }

    /// <summary><c>lit(json, shape)</c>: a value written in the plan, already in its shape.</summary>
    internal sealed class LitExpr : Expr
    {
        internal LitExpr(object? value, Shape shape)
        {
            Value = value;
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        public string Kind => "lit";

        [JsonInclude]
        [ContractType("JsonValue")]
        public object? Value { get; }

        public Shape Shape { get; }
    }

    /// <summary><c>obj(id)</c>: an object the plan locates (grammar.md 1.1).</summary>
    internal sealed class ObjExpr : Expr
    {
        internal ObjExpr(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Object id required.", nameof(id));
            Id = id;
        }

        public string Kind => "obj";
        public string Id { get; }
    }

    /// <summary><c>ref(name)</c>: a binding made by a handler, a <c>let</c> or a lambda.</summary>
    internal sealed class RefExpr : Expr
    {
        internal RefExpr(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Binding name required.", nameof(name));
            Name = name;
        }

        public string Kind => "ref";
        public string Name { get; }
    }

    /// <summary><c>get(expr, path, shape)</c>: a member read, converted to its shape.</summary>
    internal sealed class GetExpr : Expr
    {
        internal GetExpr(Expr target, IReadOnlyList<string> path, Shape shape)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        public string Kind => "get";
        public Expr Target { get; }
        public IReadOnlyList<string> Path { get; }
        public Shape Shape { get; }
    }

    /// <summary>
    /// <c>call(expr, path, args, argShapes, shape)</c>: a member call. An empty path calls the target
    /// itself. Each argument converts to its declared shape; an argument without one is passed as it is.
    /// </summary>
    internal sealed class CallExpr : Expr
    {
        internal CallExpr(
            Expr target,
            IReadOnlyList<string> path,
            IReadOnlyList<Expr> args,
            IReadOnlyList<Shape> argShapes,
            Shape shape)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Args = args ?? throw new ArgumentNullException(nameof(args));
            ArgShapes = argShapes ?? throw new ArgumentNullException(nameof(argShapes));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            if (argShapes.Count > args.Count)
                throw new ArgumentException("A call declares more argument shapes than it passes arguments.", nameof(argShapes));
        }

        public string Kind => "call";
        public Expr Target { get; }
        public IReadOnlyList<string> Path { get; }
        public IReadOnlyList<Expr> Args { get; }
        public IReadOnlyList<Shape> ArgShapes { get; }
        public Shape Shape { get; }
    }

    /// <summary><c>fn(params, body)</c>: a lambda.</summary>
    internal sealed class FnExpr : Expr
    {
        internal FnExpr(IReadOnlyList<string> parameters, Expr body)
        {
            Params = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public string Kind => "fn";
        public IReadOnlyList<string> Params { get; }
        public Expr Body { get; }
    }

    /// <summary><c>record(entries)</c>: JSON-shaped data built from named fields and spreads, in order.</summary>
    internal sealed class RecordExpr : Expr
    {
        internal RecordExpr(IReadOnlyList<RecordEntry> entries)
        {
            Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        public string Kind => "record";
        public IReadOnlyList<RecordEntry> Entries { get; }
    }

    /// <summary>An entry of a record: a named field or a spread.</summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<RecordEntry>))]
    internal abstract class RecordEntry
    {
        private protected RecordEntry() { }
    }

    /// <summary>A named field of a record.</summary>
    internal sealed class FieldEntry : RecordEntry
    {
        internal FieldEntry(string name, Expr value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Field name required.", nameof(name));
            Name = name;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Kind => "field";
        public string Name { get; }
        public Expr Value { get; }
    }

    /// <summary>The fields of another record, copied in.</summary>
    internal sealed class SpreadEntry : RecordEntry
    {
        internal SpreadEntry(Expr value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Kind => "spread";
        public Expr Value { get; }
    }

    /// <summary><c>list(items)</c>: an array built from expressions.</summary>
    internal sealed class ListExpr : Expr
    {
        internal ListExpr(IReadOnlyList<Expr> items)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public string Kind => "list";
        public IReadOnlyList<Expr> Items { get; }
    }

    /// <summary><c>op(operator, args)</c>: an operator of the closed table (grammar.md 2.2).</summary>
    internal sealed class OpExpr : Expr
    {
        internal OpExpr(Operator op, IReadOnlyList<Expr> args)
        {
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Args = args ?? throw new ArgumentNullException(nameof(args));
            op.RequireArity(args.Count);
        }

        public string Kind => "op";

        [JsonPropertyName("op")]
        [ContractType("Operator")]
        public string Op => Operator.Token;

        public IReadOnlyList<Expr> Args { get; }

        [JsonIgnore]
        internal Operator Operator { get; }
    }
}
