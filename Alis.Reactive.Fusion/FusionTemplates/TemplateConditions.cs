using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Alis.Reactive.Fusion.FusionTemplates;

internal static class TemplateConditions
{
    private static readonly IReadOnlyDictionary<ExpressionType, string> Compared = new Dictionary<ExpressionType, string>
    {
        [ExpressionType.Equal] = "===",
        [ExpressionType.NotEqual] = "!==",
        [ExpressionType.LessThan] = "<",
        [ExpressionType.LessThanOrEqual] = "<=",
        [ExpressionType.GreaterThan] = ">",
        [ExpressionType.GreaterThanOrEqual] = ">=",
    };

    private static readonly IReadOnlyDictionary<ExpressionType, ExpressionType> Mirrored = new Dictionary<ExpressionType, ExpressionType>
    {
        [ExpressionType.Equal] = ExpressionType.Equal,
        [ExpressionType.NotEqual] = ExpressionType.NotEqual,
        [ExpressionType.LessThan] = ExpressionType.GreaterThan,
        [ExpressionType.LessThanOrEqual] = ExpressionType.GreaterThanOrEqual,
        [ExpressionType.GreaterThan] = ExpressionType.LessThan,
        [ExpressionType.GreaterThanOrEqual] = ExpressionType.LessThanOrEqual,
    };

    internal static string Written<TModel>(Expression<Func<TModel, bool>> condition, TemplateContext context)
        => new Writer<TModel>(condition.Parameters[0], context).Term(condition.Body);

    private sealed class Writer<TModel>(ParameterExpression item, TemplateContext context)
    {
        internal string Term(Expression node) => node switch
        {
            BinaryExpression { NodeType: ExpressionType.AndAlso } both => Side(both.Left, ExpressionType.OrElse) + "&&" + Side(both.Right, ExpressionType.OrElse),
            BinaryExpression { NodeType: ExpressionType.OrElse } either => Term(either.Left) + "||" + Term(either.Right),
            UnaryExpression { NodeType: ExpressionType.Not } not => "!(" + Term(not.Operand) + ")",
            BinaryExpression compared when Compared.ContainsKey(compared.NodeType) => Comparison(compared),
            ConstantExpression { Value: bool constant } => constant ? "true" : "false",
            _ => Guarded([Operand(node)], Operand(node).Written),
        };

        private string Side(Expression side, ExpressionType grouped) => side.NodeType == grouped ? "(" + Term(side) + ")" : Term(side);

        private string Comparison(BinaryExpression compared)
        {
            var left = Operand(compared.Left);
            var right = Operand(compared.Right);
            var constantFirst = left.IsConstant && !right.IsConstant;
            var (first, second, kind) = constantFirst ? (right, left, Mirrored[compared.NodeType]) : (left, right, compared.NodeType);
            first.RefuseAsAConstant(second);
            second.RefuseAsAConstant(first);
            return Guarded([first, second], first.Written + Compared[kind] + second.Written);
        }

        private static string Guarded(IReadOnlyList<Operand> operands, string term)
        {
            var holders = operands.SelectMany(operand => operand.Holders).Distinct().ToList();
            return holders.Count == 0 ? term : string.Join("&&", holders) + "&&" + term;
        }

        private Operand Operand(Expression node)
        {
            var read = Unconverted(node);
            if (read is ConstantExpression constant) return new Constant(constant.Value);
            if (read is BinaryExpression arithmetic && Arithmetic.TryGetValue(arithmetic.NodeType, out var operation)) return Product(arithmetic, operation);
            if (IsItemsRead(read)) return Read((MemberExpression)read);
            if (IsViewsValue(read)) throw new NotImplementedException($"A template condition reads the view's value '{read}', and reading a view's value without a compiled expression is not built for templates; compare with a literal, or with a member the server sets on the item.");
            throw new InvalidOperationException($"A template condition holds '{read}', which Syncfusion's template engine cannot read; compare members of the item with literals, joined by &&, || and !.");
        }

        private static readonly IReadOnlyDictionary<ExpressionType, string> Arithmetic = new Dictionary<ExpressionType, string>
        {
            [ExpressionType.Add] = "adds",
            [ExpressionType.AddChecked] = "adds",
            [ExpressionType.Subtract] = "subtracts",
            [ExpressionType.SubtractChecked] = "subtracts",
            [ExpressionType.Multiply] = "*",
            [ExpressionType.MultiplyChecked] = "*",
            [ExpressionType.Divide] = "/",
        };

        private Operand Product(BinaryExpression arithmetic, string operation)
        {
            var left = Unconverted(arithmetic.Left);
            var right = Unconverted(arithmetic.Right);
            var named = $"{CSharpText(left)} and {CSharpText(right)}";
            if (operation == "adds") throw new InvalidOperationException($"A template condition adds {named}, which Syncfusion's template engine reads as one word; compare with a member the server computes on the item.");
            if (operation == "subtracts") throw new InvalidOperationException($"A template condition subtracts {CSharpText(right)} from {CSharpText(left)}, which Syncfusion's template engine reads as one word; compare with a member the server computes on the item.");
            if (!IsItemsRead(left) || !IsItemsRead(right)) throw new InvalidOperationException($"A template condition computes {named} with '{operation}', which Syncfusion's template engine reads as a number and a member; compare with a member the server computes on the item.");
            var first = Read((MemberExpression)left);
            var second = Read((MemberExpression)right);
            return new Computed(first.Written + operation + second.Written, [.. first.Holders, .. second.Holders]);
        }

        private Read Read(MemberExpression member)
        {
            var path = context.Path(Expression.Lambda<Func<TModel, object>>(Expression.Convert(member, typeof(object)), item));
            var holders = Enumerable.Range(1, path.Count - 1).Select(count => TemplateReads.Path(path.Take(count))).ToList();
            return new Read(TemplateReads.Path(path), holders, CSharpText(member));
        }

        private bool IsItemsRead(Expression node)
        {
            var reached = node;
            while (reached is MemberExpression { Expression: { } holder }) reached = holder;
            return node is MemberExpression && reached == item;
        }

        private static bool IsViewsValue(Expression node) => node is MemberExpression;

        private string CSharpText(Expression node) => node is MemberExpression member && member.Expression is { } holder && holder != item
            ? CSharpText(holder) + "." + member.Member.Name
            : node is MemberExpression own ? own.Member.Name : node.ToString();

        private static Expression Unconverted(Expression node) => node is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } converted ? Unconverted(converted.Operand) : node;
    }

    private abstract class Operand
    {
        internal abstract string Written { get; }

        internal abstract IReadOnlyList<string> Holders { get; }

        internal virtual bool IsConstant => false;

        internal virtual string Named => Written;

        internal virtual void RefuseAsAConstant(Operand compared)
        {
        }
    }

    private sealed class Read(string written, IReadOnlyList<string> holders, string named) : Operand
    {
        internal override string Written => written;

        internal override IReadOnlyList<string> Holders => holders;

        internal override string Named => named;
    }

    private sealed class Computed(string written, IReadOnlyList<string> holders) : Operand
    {
        internal override string Written => written;

        internal override IReadOnlyList<string> Holders => holders;
    }

    private sealed class Constant(object? value) : Operand
    {
        private const string Readable = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 _-.";

        internal override string Written => WrittenAs() ?? throw new InvalidOperationException("A constant is written only once it is read back.");

        internal override IReadOnlyList<string> Holders => [];

        internal override bool IsConstant => true;

        internal override void RefuseAsAConstant(Operand compared)
        {
            if (value is Enum) throw new NotImplementedException($"A template condition compares {compared.Named} with the enum value {value}, and writing an enum as the app's JSON writes it is not built for templates; compare with a member the server sets on the item.");
            if (WrittenAs() is null) throw new InvalidOperationException($"A template condition compares {compared.Named} with {Shown()}, which Syncfusion's template engine cannot read back as written; compare with true, false, a whole number or plain text, or with a boolean member the server sets on the item.");
        }

        private string? WrittenAs() => value switch
        {
            bool flag => flag ? "true" : "false",
            string text when IsPlain(text) => "'" + text + "'",
            sbyte or byte or short or ushort or int or uint or long or ulong => Whole(Convert.ToDecimal(value, CultureInfo.InvariantCulture)),
            float or double => Whole(Convert.ToDouble(value, CultureInfo.InvariantCulture)),
            decimal number => Whole(number),
            _ => null,
        };

        private static string? Whole(double number) => number >= 0 && number == Math.Floor(number) && number <= (double)decimal.MaxValue ? Whole((decimal)number) : null;

        private static string? Whole(decimal number) => number >= 0 && number == decimal.Truncate(number) ? number.ToString("0", CultureInfo.InvariantCulture) : null;

        private static bool IsPlain(string text) => text.All(letter => Readable.IndexOf(letter) >= 0) && !text.Contains("  ");

        private string Shown() => value switch
        {
            null => "null",
            string text => "\"" + text + "\"",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "",
        };
    }
}
