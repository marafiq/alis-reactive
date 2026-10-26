using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Grammar
{
    /// <summary>Short constructors for grammar nodes, so a step, a value or a condition reads like the sentence it writes.</summary>
    internal static class G
    {
        internal static readonly IReadOnlyList<string> NoPath = Array.Empty<string>();
        internal static readonly IReadOnlyList<Statement> NoStatements = Array.Empty<Statement>();

        internal static LitExpr Lit(object? value) => new LitExpr(value);

        /// <summary>A literal read in a shape: converted when it is read, unless it is already a value of that shape.</summary>
        internal static Expr Lit(object? value, Shape shape) => AlreadyIn(value, shape) ? new LitExpr(value) : As(new LitExpr(value), shape);

        internal static LitExpr Null => new LitExpr(null);
        internal static LitExpr True => new LitExpr(true);
        internal static LitExpr False => new LitExpr(false);

        internal static ObjExpr Obj(string id) => new ObjExpr(id);
        internal static RefExpr Ref(string name) => new RefExpr(name);

        /// <summary>A member read, converted to the member's shape.</summary>
        internal static Expr Get(Expr target, IReadOnlyList<string> path, Shape shape) => As(new GetExpr(target, path), shape);

        internal static Expr Get(Expr target, params string[] path) => new GetExpr(target, path);

        /// <summary>
        /// The value converted to a shape; a shape that converts nothing adds nothing, and a list read as a list
        /// of anything converts once, under the list shape it is then written as.
        /// </summary>
        internal static Expr As(Expr value, Shape shape)
        {
            if (IsUnconverted(shape)) return value;
            if (value is AsExpr inner && shape.Kind == "array" && inner.Shape.TryGetArrayItemShape(out var item) && IsUnconverted(item))
                return new AsExpr(inner.Expr, shape);
            return new AsExpr(value, shape);
        }

        /// <summary>A call whose arguments convert to their declared shapes, one per argument, and whose result converts to a shape.</summary>
        internal static Expr Call(Expr target, IReadOnlyList<string> path, IReadOnlyList<Expr> args, IReadOnlyList<Shape> argShapes, Shape shape)
        {
            if (argShapes.Count != args.Count)
                throw new ArgumentException("A call declares one shape per argument.", nameof(argShapes));
            return As(new CallExpr(target, path, args.Select((arg, index) => As(arg, argShapes[index])).ToList()), shape);
        }

        /// <summary>A call whose arguments pass as they are (a lambda, a literal, a record the plan wrote) and whose result converts to a shape.</summary>
        internal static Expr Call(Expr target, IReadOnlyList<string> path, IReadOnlyList<Expr> args, Shape shape) =>
            As(new CallExpr(target, path, args), shape);

        internal static Expr Call(Expr target, string member, params Expr[] args) => new CallExpr(target, new[] { member }, args);

        /// <summary>Applies a lambda to arguments: binds each value once, as a <c>let</c> inside an expression.</summary>
        internal static Expr Apply(FnExpr fn, params Expr[] args) => new CallExpr(fn, NoPath, args);

        internal static FnExpr Fn(Expr body, params string[] parameters) => new FnExpr(parameters, body);

        internal static Expr Op(Operator op, params Expr[] args) => new OpExpr(op, args);

        internal static Expr And(IReadOnlyList<Expr> terms) =>
            terms.Count == 1 ? terms[0] : new OpExpr(Operator.And, terms);

        internal static Expr Or(IReadOnlyList<Expr> terms) =>
            terms.Count == 1 ? terms[0] : new OpExpr(Operator.Or, terms);

        internal static Expr And(params Expr[] terms) => And((IReadOnlyList<Expr>)terms);
        internal static Expr Or(params Expr[] terms) => Or((IReadOnlyList<Expr>)terms);
        internal static Expr Not(Expr term) => new OpExpr(Operator.Not, new[] { term });
        internal static Expr Eq(Expr left, Expr right) => new OpExpr(Operator.Eq, new[] { left, right });

        internal static Expr Record(IEnumerable<RecordField> fields) => new RecordExpr(fields.ToList());

        internal static Expr Record(params (string Name, Expr Value)[] fields) =>
            new RecordExpr(fields.Select(f => new RecordField(f.Name, f.Value)).ToList());

        internal static Expr List(IEnumerable<Expr> items) => new ListExpr(items.ToList());

        /// <summary>Writes a member, with the value converted to the member's shape.</summary>
        internal static Statement Set(Expr target, IReadOnlyList<string> path, Shape shape, Expr value) =>
            new SetStatement(target, path, As(value, shape));

        internal static Statement Do(Expr expr, bool awaits = false) => new DoStatement(expr, awaits);
        internal static Statement Let(string name, Expr expr, bool awaits = false) => new LetStatement(name, expr, awaits);

        internal static Statement If(Expr when, IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise) =>
            new IfStatement(new[] { new Arm(when, then) }, otherwise);

        internal static Statement If(IReadOnlyList<Arm> arms, IReadOnlyList<Statement> otherwise) =>
            new IfStatement(arms, otherwise);

        internal static Statement Try(IReadOnlyList<Statement> body, IReadOnlyList<Statement> cleanup) =>
            new TryStatement(body, cleanup);

        internal static Statement All(IReadOnlyList<IReadOnlyList<Statement>> blocks) => new AllStatement(blocks);

        internal static bool IsUnconverted(Shape shape) => shape.Kind == "none" || shape.Kind == "any";

        // A value the conversion would hand back as it is: a missing value, or text, a boolean or a number
        // under its own scalar shape, nullable or not (grammar.md 2.1).
        private static bool AlreadyIn(object? value, Shape shape)
        {
            if (value == null) return true;
            var kind = shape.TryGetNullableInnerShape(out var inner) ? inner.Kind : shape.Kind;
            switch (kind)
            {
                case "string": return value is string;
                case "boolean": return value is bool;
                case "number":
                    return value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint
                        || value is long || value is ulong || value is float || value is double || value is decimal;
                default: return false;
            }
        }
    }
}
