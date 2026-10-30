using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// An expression of the plan's grammar (grammar.md 1.2). Ten productions; the runtime evaluates
    /// each synchronously. Nodes are immutable and may be shared, so a step can write a block twice.
    /// </summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<Expr>))]
    internal abstract class Expr
    {
        private protected Expr() { }
    }

    /// <summary><c>lit(json)</c>: a value written in the plan, as it is.</summary>
    internal sealed class LitExpr : Expr
    {
        internal LitExpr(object? value)
        {
            Value = value;
        }

        public string Kind => "lit";

        [JsonInclude]
        [ContractType("JsonValue")]
        public object? Value { get; }
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

    /// <summary><c>get(expr, path)</c>: a member read.</summary>
    internal sealed class GetExpr : Expr
    {
        internal GetExpr(Expr target, IReadOnlyList<string> path)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public string Kind => "get";
        public Expr Target { get; }
        public IReadOnlyList<string> Path { get; }
    }

    /// <summary>
    /// <c>as(expr, shape)</c>: the value converted to a shape (grammar.md 2.1), the one place a value
    /// changes type. A shape that converts nothing is never written.
    /// </summary>
    internal sealed class AsExpr : Expr
    {
        internal AsExpr(Expr expr, Shape shape)
        {
            Expr = expr ?? throw new ArgumentNullException(nameof(expr));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            if (G.IsUnconverted(shape)) throw new ArgumentException("A conversion needs a shape that converts.", nameof(shape));
        }

        public string Kind => "as";
        public Expr Expr { get; }
        public Shape Shape { get; }
    }

    /// <summary><c>call(expr, path, args)</c>: a member call. An empty path calls the target itself.</summary>
    internal sealed class CallExpr : Expr
    {
        internal CallExpr(Expr target, IReadOnlyList<string> path, IReadOnlyList<Expr> args)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public string Kind => "call";
        public Expr Target { get; }
        public IReadOnlyList<string> Path { get; }
        public IReadOnlyList<Expr> Args { get; }
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

    /// <summary><c>record(field*)</c>: JSON-shaped data built from named fields, in order.</summary>
    internal sealed class RecordExpr : Expr
    {
        internal RecordExpr(IReadOnlyList<RecordField> fields)
        {
            Fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        public string Kind => "record";
        public IReadOnlyList<RecordField> Fields { get; }
    }

    /// <summary>A field of a record.</summary>
    internal sealed class RecordField
    {
        internal RecordField(string name, Expr value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Field name required.", nameof(name));
            Name = name;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Name { get; }
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
