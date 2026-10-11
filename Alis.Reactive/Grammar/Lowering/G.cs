using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Grammar.Lowering
{
    /// <summary>Short constructors for grammar nodes, so a lowering reads like the sentence it writes.</summary>
    internal static class G
    {
        internal static readonly IReadOnlyList<string> NoPath = Array.Empty<string>();
        internal static readonly IReadOnlyList<Expr> NoArgs = Array.Empty<Expr>();
        internal static readonly IReadOnlyList<Shape> NoShapes = Array.Empty<Shape>();
        internal static readonly IReadOnlyList<Statement> NoStatements = Array.Empty<Statement>();

        internal static LitExpr Lit(object? value) => new LitExpr(value, Shape.None);
        internal static LitExpr Lit(object? value, Shape shape) => new LitExpr(value, shape);
        internal static LitExpr Null => new LitExpr(null, Shape.None);
        internal static LitExpr True => new LitExpr(true, Shape.None);
        internal static LitExpr False => new LitExpr(false, Shape.None);

        internal static ObjExpr Obj(string id) => new ObjExpr(id);
        internal static RefExpr Ref(string name) => new RefExpr(name);

        internal static Expr Get(Expr target, IReadOnlyList<string> path, Shape shape) =>
            new GetExpr(target, path, shape);

        internal static Expr Get(Expr target, params string[] path) => new GetExpr(target, path, Shape.None);

        /// <summary>The value converted to a shape: a read with no path.</summary>
        internal static Expr As(Expr value, Shape shape) =>
            IsUnconverted(shape) ? value : new GetExpr(value, NoPath, shape);

        internal static Expr Call(Expr target, IReadOnlyList<string> path, IReadOnlyList<Expr> args, IReadOnlyList<Shape> argShapes, Shape shape) =>
            new CallExpr(target, path, args, argShapes, shape);

        internal static Expr Call(Expr target, string member, params Expr[] args) =>
            new CallExpr(target, new[] { member }, args, NoShapes, Shape.None);

        internal static Expr CallTyped(Expr target, string member, Shape shape, params Expr[] args) =>
            new CallExpr(target, new[] { member }, args, NoShapes, shape);

        /// <summary>Applies a lambda to arguments: binds each value once, as a <c>let</c> inside an expression.</summary>
        internal static Expr Apply(FnExpr fn, params Expr[] args) =>
            new CallExpr(fn, NoPath, args, NoShapes, Shape.None);

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

        internal static Expr Record(IEnumerable<RecordEntry> entries) => new RecordExpr(entries.ToList());

        internal static Expr Record(params (string Name, Expr Value)[] fields) =>
            new RecordExpr(fields.Select(f => (RecordEntry)new FieldEntry(f.Name, f.Value)).ToList());

        internal static Expr List(IEnumerable<Expr> items) => new ListExpr(items.ToList());

        internal static Statement Set(Expr target, IReadOnlyList<string> path, Shape shape, Expr value) =>
            new SetStatement(target, path, shape, value);

        internal static Statement Do(Expr expr, bool awaits = false) => new DoStatement(expr, awaits);
        internal static Statement Let(string name, Expr expr, bool awaits = false) => new LetStatement(name, expr, awaits);

        internal static Statement If(Expr when, IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise) =>
            new IfStatement(new[] { new Arm(when, then) }, otherwise);

        internal static Statement If(IReadOnlyList<Arm> arms, IReadOnlyList<Statement> otherwise) =>
            new IfStatement(arms, otherwise);

        internal static Statement Try(IReadOnlyList<Statement> body, IReadOnlyList<Statement> cleanup) =>
            new TryStatement(body, cleanup);

        internal static Statement All(IReadOnlyList<IReadOnlyList<Statement>> blocks) => new AllStatement(blocks);

        internal static IReadOnlyList<string> PathOf(Path path) =>
            path.Segments.Select(segment => segment.ToPathText()).ToList();

        internal static bool IsUnconverted(Shape shape) => shape.Kind == "none" || shape.Kind == "any";
    }
}
