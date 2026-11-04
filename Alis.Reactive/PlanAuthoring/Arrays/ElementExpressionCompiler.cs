using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Alis.Reactive.Grammar;
using Alis.Reactive.Members;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.PlanAuthoring.Arrays
{
    internal static class ElementExpressionCompiler
    {
        private static readonly IReadOnlyDictionary<MethodInfo, string> PureMethods = new Dictionary<MethodInfo, string>
        {
            [MethodOf<string>(text => text.Trim())] = "trim",
            [MethodOf<string>(text => text.TrimStart())] = "trimStart",
            [MethodOf<string>(text => text.TrimEnd())] = "trimEnd",
            [MethodOf<string>(text => text.ToUpper())] = "toUpperCase",
            [MethodOf<string>(text => text.ToUpperInvariant())] = "toUpperCase",
            [MethodOf<string>(text => text.ToLower())] = "toLowerCase",
            [MethodOf<string>(text => text.ToLowerInvariant())] = "toLowerCase",
        };

        private static readonly IReadOnlyDictionary<MemberInfo, Func<ValueExpression, ValueExpression>> ValueMembers =
            new Dictionary<MemberInfo, Func<ValueExpression, ValueExpression>>
            {
                [MemberOf<DateTime>(date => date.Year)] = date => DateCall(date, "getFullYear"),
                [MemberOf<DateTime>(date => date.Month)] = date =>
                    ValueExpression.Operation(Operator.Add, Shape.Number, DateCall(date, "getMonth"), ValueExpression.Literal(1)),
                [MemberOf<DateTime>(date => date.Day)] = date => DateCall(date, "getDate"),
                [MemberOf<DateTime>(date => date.Hour)] = date => DateCall(date, "getHours"),
                [MemberOf<DateTime>(date => date.Minute)] = date => DateCall(date, "getMinutes"),
                [MemberOf<DateTime>(date => date.Second)] = date => DateCall(date, "getSeconds"),
                [MemberOf<string>(text => text.Length)] = text => ValueExpression.Get(Target.Returned(text), new[] { "length" }, Shape.Number),
            };

        private static readonly MemberInfo DayOfWeekMember = MemberOf<DateTime>(date => date.DayOfWeek);

        private static readonly MethodInfo IsNullOrEmpty = StaticMethodOf(() => string.IsNullOrEmpty(""));

        private static readonly MethodInfo IsMatch = StaticMethodOf(() => Regex.IsMatch("", ""));

        private const string Tests = "String.Contains, String.StartsWith, String.EndsWith, String.IsNullOrEmpty, Enumerable.Contains and Regex.IsMatch";

        private static string? PureMethodName(MethodInfo method)
            => PureMethods.TryGetValue(method, out var name) ? name : method.GetCustomAttribute<PureMethodAttribute>()?.JavaScriptName;

        private static IEnumerable<string> PureMethodNames(Type? objectType)
            => PureMethods.Keys
                .Concat(objectType?.GetMethods().Where(method => method.GetCustomAttribute<PureMethodAttribute>() != null) ?? Enumerable.Empty<MethodInfo>())
                .Select(NameOf)
                .Distinct();

        private static MethodInfo MethodOf<T>(Expression<Func<T, object?>> call)
            => ((MethodCallExpression)Expressions.StripConvert(call.Body)).Method;

        private static MethodInfo StaticMethodOf(Expression<Func<object?>> call)
            => ((MethodCallExpression)Expressions.StripConvert(call.Body)).Method;

        private static MemberInfo MemberOf<T>(Expression<Func<T, object?>> read)
            => ((MemberExpression)Expressions.StripConvert(read.Body)).Member;

        internal static Condition CompilePredicate(LambdaExpression predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return CompileCondition(predicate.Body, predicate.Parameters[0]);
        }

        internal static ValueExpression CompileProjection(LambdaExpression projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));

            return CompileValue(projection.Body, projection.Parameters[0]);
        }

        private static Condition CompileCondition(Expression expression, ParameterExpression element)
        {
            var body = Unconverted(expression);

            if (body is BinaryExpression binary)
            {
                switch (binary.NodeType)
                {
                    case ExpressionType.AndAlso:
                        return Condition.All(CompileCondition(binary.Left, element), CompileCondition(binary.Right, element));
                    case ExpressionType.OrElse:
                        return Condition.Any(CompileCondition(binary.Left, element), CompileCondition(binary.Right, element));
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                        return CompileComparison(binary, element);
                }
            }

            if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Not)
                return Condition.Not(CompileCondition(unary.Operand, element));

            if (body is MethodCallExpression call && TryTest(call, element, out var test))
                return test;

            if (body.Type == typeof(bool))
                return Condition.Unary(ValueTest.Truthy, CompileValue(body, element), Shape.Boolean);

            throw new InvalidOperationException(
                "An array predicate is a test of the item: a comparison (== != > >= < <=), && || !, a text test " +
                "(Contains, StartsWith, EndsWith, string.IsNullOrEmpty, Regex.IsMatch), Contains over fixed values, " +
                "or a boolean value. Got a " + CSharpTypeName.Of(body.Type) + ": " + body);
        }

        private static bool TryTest(MethodCallExpression call, ParameterExpression element, out Condition test)
        {
            test = Condition.Always;
            if (!Expressions.Reads(call, element)) return false;

            if (call.Object != null && TryTextTest(call, out var textTest))
            {
                test = Condition.Text(textTest, CompileValue(call.Object, element), FixedText(call.Arguments[0], element), Shape.String);
                return true;
            }

            if (call.Method == IsNullOrEmpty)
            {
                test = Condition.Unary(ValueTest.IsEmpty, CompileValue(call.Arguments[0], element), Shape.String);
                return true;
            }

            if (call.Method == IsMatch)
            {
                test = Condition.Text(TextTest.Matches, CompileValue(call.Arguments[0], element), FixedText(call.Arguments[1], element), Shape.String);
                return true;
            }

            if (Expressions.TryContains(call, out var source, out var candidate) && !Expressions.Reads(source, element))
            {
                var item = Unconverted(candidate);
                var shape = ClrShape.Of(item.Type);
                var values = Expressions.Evaluate(source) as IEnumerable
                    ?? throw new InvalidOperationException("Contains needs the values to look in. Got: " + source);
                var literals = values.Cast<object?>().Select(value => ValueExpression.LiteralFor(value, shape)).ToList();
                test = Condition.Membership(CompileValue(item, element), literals, negated: false, shape);
                return true;
            }

            return false;
        }

        private static Condition CompileComparison(BinaryExpression binary, ParameterExpression element)
        {
            var left = Unconverted(binary.Left);
            var right = Unconverted(binary.Right);

            Expression memberOperand;
            if (Expressions.Reads(left, element))
            {
                memberOperand = left;
            }
            else if (Expressions.Reads(right, element))
            {
                memberOperand = right;
            }
            else
            {
                throw new InvalidOperationException(
                "Array predicate comparison must reference the element on at least one side; " +
                "comparing two constants is a developer mistake. Got: " + binary);
            }

            if (!ComparisonFor(binary.NodeType).IsEquality) BrowserOrder.Require(memberOperand.Type);

            if (IsDayOfWeek(left) || IsDayOfWeek(right))
                return Condition.Ordered(ComparisonFor(binary.NodeType), DayNumber(left, element), DayNumber(right, element), Shape.Number);

            var shape = ClrShape.Of(memberOperand.Type);

            return Condition.Ordered(
                ComparisonFor(binary.NodeType),
                CompileOperand(left, element, memberOperand.Type),
                CompileOperand(right, element, memberOperand.Type),
                shape);
        }

        private static ValueExpression CompileOperand(Expression operand, ParameterExpression element, Type memberType)
        {
            var enumType = EnumMember.TypeOf(memberType);
            if (enumType == null || Expressions.Reads(operand, element)) return CompileValue(operand, element);

            var value = Expressions.Evaluate(operand);
            var member = value == null ? null : EnumMember.Of(enumType, value);
            return ValueExpression.LiteralFor(member, ClrShape.Of(memberType));
        }

        private static ValueExpression DayNumber(Expression operand, ParameterExpression element)
        {
            if (IsDayOfWeek(operand)) return DateCall(CompileValue(((MemberExpression)operand).Expression!, element), "getDay");
            if (Expressions.Reads(operand, element)) return CompileValue(operand, element);

            var day = Expressions.Evaluate(operand);
            return day == null ? ValueExpression.Null() : ValueExpression.LiteralFor(Convert.ToInt32(day, CultureInfo.InvariantCulture), Shape.Number);
        }

        private static ValueExpression CompileValue(Expression expression, ParameterExpression element)
        {
            var node = Unconverted(expression);

            if (!Expressions.Reads(node, element))
            {
                var value = Expressions.Evaluate(node);
                return ValueExpression.LiteralFor(value, ClrShape.OfValue(value));
            }

            if (TryDataPath(node, element, out var path))
            {
                var shape = ClrShape.Of(node.Type);
                return path.IsNone
                    ? ValueExpression.Get(Target.Item, shape)
                    : ValueExpression.Get(Target.Item, path.Text, shape);
            }

            switch (node)
            {
                case MemberExpression member when TryMember(member, element, out var read):
                    return read;
                case MethodCallExpression call:
                    return CompileCall(call, element);
                case BinaryExpression { NodeType: ExpressionType.Coalesce, Conversion: null } coalesce:
                    return ValueExpression.Operation(
                        Operator.Coalesce, ClrShape.Of(coalesce.Type), CompileValue(coalesce.Left, element), CompileValue(coalesce.Right, element));
                case ConditionalExpression conditional:
                    return ValueExpression.Operation(
                        Operator.Cond, ClrShape.Of(conditional.Type),
                        new HoldsExpression(CompileCondition(conditional.Test, element)),
                        CompileValue(conditional.IfTrue, element),
                        CompileValue(conditional.IfFalse, element));
            }

            if (node.Type == typeof(bool) && IsTest(node.NodeType))
                return new HoldsExpression(CompileCondition(node, element));

            throw new InvalidOperationException(
                "An array lambda reads the item (x => x), its members (x => x.Field, x => x.A.B), and the members, " +
                "methods and operators of the member table (grammar.md 2.3); a computed object and arithmetic have " +
                "no form the plan writes. Project the value on the server. Got: " + node);
        }

        private static bool TryDataPath(Expression node, ParameterExpression element, out Path path)
        {
            var link = node;
            while (link is MemberExpression { Expression: { } owner } member)
            {
                if (Expressions.IsValueMember(member.Member))
                {
                    path = Path.None;
                    return false;
                }

                link = Expressions.StripConvert(owner);
            }

            return ExpressionPathHelper.TryJsonPath(node, element, out path);
        }

        private static bool TryMember(MemberExpression member, ParameterExpression element, out ValueExpression read)
        {
            read = null!;
            if (!Expressions.IsValueMember(member.Member)) return false;

            var owner = member.Expression!;
            if (Nullable.GetUnderlyingType(owner.Type) != null)
            {
                if (member.Member.Name == nameof(Nullable<int>.Value))
                {
                    read = CompileValue(owner, element);
                    return true;
                }

                if (member.Member.Name == nameof(Nullable<int>.HasValue))
                {
                    read = new HoldsExpression(Condition.Unary(ValueTest.NotNull, CompileValue(owner, element), ClrShape.Of(owner.Type)));
                    return true;
                }
            }

            if (ValueMembers.TryGetValue(member.Member, out var javaScriptForm))
            {
                read = javaScriptForm(CompileValue(owner, element));
                return true;
            }

            if (IsDayOfWeek(member))
            {
                throw new InvalidOperationException(
                    "'DateTime.DayOfWeek' is read in the browser as getDay(), the day's number, so a lambda reads it only " +
                    "to compare it with a DayOfWeek (x => x.Admitted.DayOfWeek == DayOfWeek.Saturday). Project it on " +
                    "the server. Got: " + member);
            }

            throw new InvalidOperationException(
                $"'{NameOf(member.Member)}' is a member of a value .NET defines that has no JavaScript form; a lambda may read " +
                string.Join(", ", ValueMembers.Keys.Select(NameOf)) +
                ", DateTime.DayOfWeek (compared with a DayOfWeek), Nullable.Value and Nullable.HasValue. " +
                "Project the value on the server. Got: " + member);
        }

        private static ValueExpression CompileCall(MethodCallExpression call, ParameterExpression element)
        {
            if (call.Object != null && PureMethodName(call.Method) is string methodName)
            {
                var methodArgs = call.Arguments.Select(argument => CompileValue(argument, element)).ToList();
                var unconverted = methodArgs.Select(_ => Shape.Any).ToList();
                return ValueExpression.Call(
                    Target.Returned(CompileValue(call.Object, element)), new[] { methodName }, methodArgs, unconverted, ClrShape.Of(call.Type));
            }

            if (TryTest(call, element, out var test))
                return new HoldsExpression(test);

            throw new InvalidOperationException(
                $"'{NameOf(call.Method)}' has no pure JavaScript form the plan can call on an item; " +
                "the lambda may call " + string.Join(", ", PureMethodNames(call.Object?.Type)) +
                ", and test with " + Tests + ". Project the value on the server, or write a reaction for a call that " +
                "changes the item. Got: " + call);
        }

        private static Expression Unconverted(Expression expression)
        {
            var node = expression;
            while (node is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } conversion)
            {
                if (Narrows(conversion.Operand.Type, conversion.Type))
                {
                    throw new InvalidOperationException(
                        $"A cast from {CSharpTypeName.Of(conversion.Operand.Type)} to {CSharpTypeName.Of(conversion.Type)} changes the value as C# computes it " +
                        "(a whole number drops the fraction, a narrower number wraps), and a browser's number does not. " +
                        "Project the value on the server. Got: " + expression);
                }

                node = conversion.Operand;
            }

            return node;
        }

        private static bool Narrows(Type from, Type to)
        {
            var source = Nullable.GetUnderlyingType(from) ?? from;
            var target = Nullable.GetUnderlyingType(to) ?? to;
            if (!WholeNumbers.TryGetValue(target, out var whole)) return false;
            if (source == typeof(double) || source == typeof(float) || source == typeof(decimal)) return true;
            if (!WholeNumbers.TryGetValue(source, out var cast)) return false;

            var holdsEvery = cast.Signed == whole.Signed ? whole.Bytes >= cast.Bytes : !cast.Signed && whole.Bytes > cast.Bytes;
            return !holdsEvery;
        }

        private static readonly IReadOnlyDictionary<Type, (int Bytes, bool Signed)> WholeNumbers = new Dictionary<Type, (int, bool)>
        {
            [typeof(sbyte)] = (1, true),
            [typeof(byte)] = (1, false),
            [typeof(short)] = (2, true),
            [typeof(ushort)] = (2, false),
            [typeof(int)] = (4, true),
            [typeof(uint)] = (4, false),
            [typeof(long)] = (8, true),
            [typeof(ulong)] = (8, false),
        };

        private static bool IsDayOfWeek(Expression expression)
            => expression is MemberExpression member && member.Member.Equals(DayOfWeekMember);

        private static bool IsTest(ExpressionType nodeType)
            => nodeType is ExpressionType.AndAlso or ExpressionType.OrElse or ExpressionType.Not
                or ExpressionType.Equal or ExpressionType.NotEqual
                or ExpressionType.GreaterThan or ExpressionType.GreaterThanOrEqual
                or ExpressionType.LessThan or ExpressionType.LessThanOrEqual;

        private static ValueExpression DateCall(ValueExpression date, string method)
            => ValueExpression.Call(Target.Returned(date), new[] { method }, Array.Empty<ValueExpression>(), Array.Empty<Shape>(), Shape.Number);

        private static string NameOf(MemberInfo member) => CSharpTypeName.Of(member.DeclaringType!) + "." + member.Name;

        private static Comparison ComparisonFor(ExpressionType nodeType)
            => nodeType switch
            {
                ExpressionType.Equal => Comparison.Equal,
                ExpressionType.NotEqual => Comparison.NotEqual,
                ExpressionType.GreaterThan => Comparison.GreaterThan,
                ExpressionType.GreaterThanOrEqual => Comparison.GreaterThanOrEqual,
                ExpressionType.LessThan => Comparison.LessThan,
                ExpressionType.LessThanOrEqual => Comparison.LessThanOrEqual,
                _ => throw new InvalidOperationException("Unsupported comparison node: " + nodeType),
            };

        private static bool TryTextTest(MethodCallExpression call, out TextTest test)
        {
            if (call.Object?.Type == typeof(string) && call.Arguments.Count == 1)
            {
                switch (call.Method.Name)
                {
                    case "Contains": test = TextTest.Contains; return true;
                    case "StartsWith": test = TextTest.StartsWith; return true;
                    case "EndsWith": test = TextTest.EndsWith; return true;
                }
            }

            test = TextTest.Contains;
            return false;
        }

        private static string FixedText(Expression expression, ParameterExpression element)
        {
            var argument = Unconverted(expression);
            if (Expressions.Reads(argument, element))
            {
                throw new InvalidOperationException(
                    "A text test's text (Contains, StartsWith, EndsWith, Regex.IsMatch's pattern) must be a constant or a " +
                    "captured value, not a read of the item: the plan writes it as a literal. Got: " + argument);
            }

            return Expressions.Evaluate(argument) as string
                ?? throw new InvalidOperationException("String operation arguments must be text. Got: " + argument);
        }
    }
}
