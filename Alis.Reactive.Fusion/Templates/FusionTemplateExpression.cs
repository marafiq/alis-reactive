using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace Alis.Reactive.Fusion.Templates
{
    /// <summary>
    /// Converts C# expressions to Syncfusion template syntax.
    /// </summary>
    /// <remarks>
    /// Examples include <c>m =&gt; m.PropertyName</c> becoming <c>${propertyName}</c>
    /// and <c>m =&gt; m.Status == "Active"</c> becoming <c>status === 'Active'</c>.
    /// </remarks>
    public static class FusionTemplateExpression
    {
        /// <summary>
        /// Converts a property expression to a Syncfusion binding token.
        /// </summary>
        /// <returns>A binding token such as <c>${propertyName}</c>.</returns>
        public static string ToBinding<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            var propertyPath = GetPropertyPath(expression.Body);
            return $"${{{propertyPath}}}";
        }

        /// <summary>
        /// Converts a property expression to a Syncfusion template property path.
        /// </summary>
        /// <returns>A property path without the surrounding binding token.</returns>
        public static string ToPropertyPath<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            return GetPropertyPath(expression.Body);
        }

        /// <summary>
        /// Converts a boolean predicate expression to Syncfusion condition syntax.
        /// </summary>
        public static string ToCondition<TModel>(Expression<Func<TModel, bool>> predicate)
        {
            return ConvertToCondition(predicate.Body);
        }

        private static string GetPropertyPath(Expression expression)
        {
            return expression switch
            {
                MemberExpression member => GetMemberPath(member),
                UnaryExpression unary when unary.Operand is MemberExpression member => GetMemberPath(member),
                _ => throw new ArgumentException($"Expression type {expression.NodeType} is not supported")
            };
        }

        private static string GetMemberPath(MemberExpression member)
        {
            var parts = new List<string>();
            var current = member;

            while (current != null)
            {
                // Template data uses the same camelCase names as generated JSON.
                var name = current.Member.Name;
                if (name.Length > 0)
                    name = char.ToLowerInvariant(name[0]) + name.Substring(1);
                parts.Insert(0, name);
                current = current.Expression as MemberExpression;
            }

            return string.Join(".", parts);
        }

        private static string ConvertToCondition(Expression expression)
        {
            return expression switch
            {
                BinaryExpression binary => ConvertBinaryExpression(binary),
                MemberExpression member => GetMemberPath(member),
                UnaryExpression unary when unary.NodeType == ExpressionType.Not => $"!{ConvertToCondition(unary.Operand)}",
                ConstantExpression constant => ConvertConstant(constant.Value),
                _ => throw new ArgumentException($"Expression type {expression.NodeType} is not supported in conditions")
            };
        }

        private static string ConvertBinaryExpression(BinaryExpression binary)
        {
            var left = ConvertOperand(binary.Left);
            var right = ConvertOperand(binary.Right);
            var op = ConvertOperator(binary.NodeType);

            return $"{left} {op} {right}";
        }

        private static string ConvertOperand(Expression expression)
        {
            return expression switch
            {
                MemberExpression memberAccess when memberAccess.Expression is ConstantExpression => EvaluateAndConvert(memberAccess),
                MemberExpression member => GetMemberPath(member),
                ConstantExpression constant => ConvertConstant(constant.Value),
                UnaryExpression unary when unary.Operand is ConstantExpression constant => ConvertConstant(constant.Value),
                BinaryExpression binary => $"({ConvertBinaryExpression(binary)})",
                _ => throw new ArgumentException($"Operand type {expression.NodeType} is not supported")
            };
        }

        private static string EvaluateAndConvert(Expression expression)
        {
            var lambda = Expression.Lambda(expression);
            var compiled = lambda.Compile();
            var value = compiled.DynamicInvoke();
            return ConvertConstant(value);
        }

        private static string ConvertConstant(object? value)
        {
            if (value == null) return "null";

            return value switch
            {
                string s => $"'{s}'",
                bool b => b ? "true" : "false",
                IFormattable number => number.ToString(null, CultureInfo.InvariantCulture),
                _ => $"'{value}'"
            };
        }

        private static string ConvertOperator(ExpressionType nodeType)
        {
            return nodeType switch
            {
                ExpressionType.Equal => "===",
                ExpressionType.NotEqual => "!==",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.AndAlso => "&&",
                ExpressionType.OrElse => "||",
                ExpressionType.Add => "+",
                ExpressionType.Subtract => "-",
                ExpressionType.Multiply => "*",
                ExpressionType.Divide => "/",
                _ => throw new ArgumentException($"Operator {nodeType} is not supported")
            };
        }
    }
}
