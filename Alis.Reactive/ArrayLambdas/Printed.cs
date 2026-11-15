using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.ArrayLambdas;

internal static class Printed
{
    private const string Closure = "value(";
    private const int Choice = 0;
    private const int Unary = 8;
    private const int Primary = 9;

    private static readonly Dictionary<ExpressionType, (string Text, int Binds)> Operators = new()
    {
        [ExpressionType.Coalesce] = ("??", 1),
        [ExpressionType.OrElse] = ("||", 2),
        [ExpressionType.AndAlso] = ("&&", 3),
        [ExpressionType.Equal] = ("==", 4),
        [ExpressionType.NotEqual] = ("!=", 4),
        [ExpressionType.LessThan] = ("<", 5),
        [ExpressionType.LessThanOrEqual] = ("<=", 5),
        [ExpressionType.GreaterThan] = (">", 5),
        [ExpressionType.GreaterThanOrEqual] = (">=", 5),
        [ExpressionType.Add] = ("+", 6),
        [ExpressionType.AddChecked] = ("+", 6),
        [ExpressionType.Subtract] = ("-", 6),
        [ExpressionType.SubtractChecked] = ("-", 6),
        [ExpressionType.Multiply] = ("*", 7),
        [ExpressionType.MultiplyChecked] = ("*", 7),
        [ExpressionType.Divide] = ("/", 7),
        [ExpressionType.Modulo] = ("%", 7),
    };

    private static readonly Dictionary<Type, string> Keywords = new()
    {
        [typeof(bool)] = "bool",
        [typeof(bool?)] = "bool?",
        [typeof(char)] = "char",
        [typeof(char?)] = "char?",
        [typeof(sbyte)] = "sbyte",
        [typeof(sbyte?)] = "sbyte?",
        [typeof(byte)] = "byte",
        [typeof(byte?)] = "byte?",
        [typeof(short)] = "short",
        [typeof(short?)] = "short?",
        [typeof(ushort)] = "ushort",
        [typeof(ushort?)] = "ushort?",
        [typeof(int)] = "int",
        [typeof(int?)] = "int?",
        [typeof(uint)] = "uint",
        [typeof(uint?)] = "uint?",
        [typeof(long)] = "long",
        [typeof(long?)] = "long?",
        [typeof(ulong)] = "ulong",
        [typeof(ulong?)] = "ulong?",
        [typeof(float)] = "float",
        [typeof(float?)] = "float?",
        [typeof(double)] = "double",
        [typeof(double?)] = "double?",
        [typeof(decimal)] = "decimal",
        [typeof(decimal?)] = "decimal?",
    };

    internal static string Of(Expression node) => Printing(node, Choice);

    private static string Printing(Expression node, int outer)
    {
        var (text, binds) = Composed(node);
        return binds < outer ? "(" + text + ")" : text;
    }

    private static (string Text, int Binds) Composed(Expression node) => node switch
    {
        BinaryExpression binary when Operators.TryGetValue(binary.NodeType, out var op) => (Binary(binary, op.Text, op.Binds), op.Binds),
        UnaryExpression { NodeType: ExpressionType.Not } negation when negation.Type == typeof(bool) || negation.Type == typeof(bool?) => ("!" + Printing(negation.Operand, Unary), Unary),
        UnaryExpression { NodeType: ExpressionType.Negate or ExpressionType.NegateChecked } negation => ("-" + Printing(negation.Operand, Unary), Unary),
        UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } conversion => Converted(conversion),
        ConditionalExpression choice => (Printing(choice.Test, Choice + 1) + " ? " + Printing(choice.IfTrue, Choice) + " : " + Printing(choice.IfFalse, Choice), Choice),
        ConstantExpression constant => (Constant(constant), Primary),
        _ => (WithoutClosures(node.ToString()), Primary),
    };

    private static string Binary(BinaryExpression binary, string op, int binds)
    {
        var groupsToTheRight = binary.NodeType == ExpressionType.Coalesce;
        var left = Printing(binary.Left, groupsToTheRight ? binds + 1 : binds);
        var right = Printing(binary.Right, groupsToTheRight ? binds : binds + 1);
        return left + " " + op + " " + right;
    }

    private static (string Text, int Binds) Converted(UnaryExpression conversion)
    {
        var takesTheValue = conversion is { IsLifted: true, IsLiftedToNull: false };
        var writtenAsACast = conversion.Method is null && (takesTheValue || ComparedTypes.Narrows(conversion.Operand.Type, conversion.Type));
        if (!writtenAsACast) return Composed(conversion.Operand);
        if (!Keywords.TryGetValue(conversion.Type, out var keyword)) return (WithoutClosures(conversion.ToString()), Primary);
        return ("(" + keyword + ")" + Printing(conversion.Operand, Unary), Unary);
    }

    private static string Constant(ConstantExpression constant) => constant.Value switch
    {
        null => "null",
        bool truth => truth ? "true" : "false",
        string text => "\"" + text + "\"",
        char letter => "'" + letter + "'",
        sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal => ((IFormattable)constant.Value).ToString(null, CultureInfo.InvariantCulture),
        _ => WithoutClosures(constant.ToString()),
    };

    private static string WithoutClosures(string text)
    {
        var start = text.IndexOf(Closure, StringComparison.Ordinal);
        while (start >= 0)
        {
            var end = text.IndexOf(").", start, StringComparison.Ordinal);
            if (end < 0) return text;
            text = text.Remove(start, end + 2 - start);
            start = text.IndexOf(Closure, start, StringComparison.Ordinal);
        }

        return text;
    }
}
