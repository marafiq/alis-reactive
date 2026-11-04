using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
    [JsonConverter(typeof(PlanNodeDiscriminator<Expr>))]
    internal abstract class Expr
    {
        private protected Expr() { }
    }

    internal sealed class LitExpr : Expr
    {
        internal LitExpr(Scalar value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Kind => "lit";
        public Scalar Value { get; }
    }

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

    internal sealed class RecordExpr : Expr
    {
        internal RecordExpr(IReadOnlyList<RecordField> fields)
        {
            Fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        public string Kind => "record";
        public IReadOnlyList<RecordField> Fields { get; }
    }

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

    internal sealed class ListExpr : Expr
    {
        internal ListExpr(IReadOnlyList<Expr> items)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public string Kind => "list";
        public IReadOnlyList<Expr> Items { get; }
    }

    internal sealed class OpExpr : Expr
    {
        internal OpExpr(Operator op, IReadOnlyList<Expr> args)
        {
            Op = op;
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public string Kind => "op";

        [JsonPropertyName("op")]
        public Operator Op { get; }

        public IReadOnlyList<Expr> Args { get; }
    }
}
