using System;
using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.Grammar
{
    internal static class G
    {
        internal static readonly IReadOnlyList<string> NoPath = Array.Empty<string>();
        internal static readonly IReadOnlyList<Statement> NoStatements = Array.Empty<Statement>();

        internal static LitExpr Lit(object? value) => new LitExpr(Scalar.Of(value));

        internal static Expr Lit(Scalar value, Shape shape) => AlreadyIn(value, shape) ? new LitExpr(value) : As(new LitExpr(value), shape);

        internal static LitExpr Null => new LitExpr(Scalar.Null);
        internal static LitExpr True => new LitExpr(Scalar.Of(true));
        internal static LitExpr False => new LitExpr(Scalar.Of(false));

        internal static ObjExpr Obj(string id) => new ObjExpr(id);
        internal static RefExpr Ref(string name) => new RefExpr(name);

        internal static Expr Get(Expr target, IReadOnlyList<string> path, Shape shape, PathMembers members)
            => As(new GetExpr(target, path, members), shape);

        internal static Expr Get(Expr target, PathMembers members, params string[] path) => new GetExpr(target, path, members);

        internal static Expr As(Expr value, Shape shape)
        {
            if (IsUnconverted(shape)) return value;
            if (value is AsExpr inner)
            {
                if (inner.Shape == shape) return value;
                if (shape is ArrayShape && inner.Shape.TryGetArrayItemShape(out var item) && IsUnconverted(item))
                    return new AsExpr(inner.Expr, shape);
            }
            return new AsExpr(value, shape);
        }

        internal static Expr Call(
            Expr target,
            IReadOnlyList<string> path,
            IReadOnlyList<Expr> args,
            IReadOnlyList<Shape> argShapes,
            Shape shape,
            PathMembers members)
        {
            if (argShapes.Count != args.Count)
                throw new ArgumentException("A call declares one shape per argument.", nameof(argShapes));
            return As(new CallExpr(target, path, args.Select((arg, index) => As(arg, argShapes[index])).ToList(), members), shape);
        }

        internal static Expr Call(Expr target, IReadOnlyList<string> path, IReadOnlyList<Expr> args, Shape shape, PathMembers members)
            => As(new CallExpr(target, path, args, members), shape);

        internal static Expr Call(Expr target, PathMembers members, string member, params Expr[] args)
            => new CallExpr(target, new[] { member }, args, members);

        internal static Expr Apply(FnExpr fn, params Expr[] args) => new CallExpr(fn, NoPath, args, PathMembers.Required);

        internal static FnExpr Fn(Expr body, params string[] parameters) => new FnExpr(parameters, body);

        internal static Expr Op(Operator op, params Expr[] args) => new OpExpr(op, args);

        internal static Expr And(IReadOnlyList<Expr> terms)
            => terms.Count == 1 ? terms[0] : new OpExpr(Operator.And, terms);

        internal static Expr Or(IReadOnlyList<Expr> terms)
            => terms.Count == 1 ? terms[0] : new OpExpr(Operator.Or, terms);

        internal static Expr And(params Expr[] terms) => And((IReadOnlyList<Expr>)terms);
        internal static Expr Or(params Expr[] terms) => Or((IReadOnlyList<Expr>)terms);
        internal static Expr Not(Expr term) => new OpExpr(Operator.Not, new[] { term });
        internal static Expr Eq(Expr left, Expr right) => new OpExpr(Operator.Eq, new[] { left, right });

        internal static Expr Record(IEnumerable<RecordField> fields) => new RecordExpr(fields.ToList());

        internal static Expr Record(params (string Name, Expr Value)[] fields)
            => new RecordExpr(fields.Select(f => new RecordField(f.Name, f.Value)).ToList());

        internal static Expr List(IEnumerable<Expr> items) => new ListExpr(items.ToList());

        internal static Statement Set(Expr target, IReadOnlyList<string> path, Shape shape, Expr value, PathMembers members)
            => new SetStatement(target, path, As(value, shape), members);

        internal static Statement Do(Expr expr, bool awaits = false) => new DoStatement(expr, awaits);
        internal static Statement Let(string name, Expr expr, bool awaits = false) => new LetStatement(name, expr, awaits);

        internal static Statement If(Expr when, IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise)
            => new IfStatement(new[] { new Arm(when, then) }, otherwise);

        internal static Statement If(IReadOnlyList<Arm> arms, IReadOnlyList<Statement> otherwise)
            => new IfStatement(arms, otherwise);

        internal static Statement Try(IReadOnlyList<Statement> body, IReadOnlyList<Statement> cleanup)
            => new TryStatement(body, cleanup);

        internal static Statement All(IReadOnlyList<IReadOnlyList<Statement>> blocks) => new AllStatement(blocks);

        internal static bool IsUnconverted(Shape shape) => shape is AnyShape;

        private static bool AlreadyIn(Scalar scalar, Shape shape)
        {
            var value = scalar.Value;
            if (value == null) return true;
            var kind = shape.TryGetNullableInnerShape(out var inner) ? inner.Kind : shape.Kind;
            switch (kind)
            {
                case "string": return value is string;
                case "boolean": return value is bool;
                case "number": return Scalar.IsNumber(value);
                default: return false;
            }
        }
    }
}
