using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    internal sealed class PluginInvocationArgument
    {
        private PluginInvocationArgument(ValueExpression value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal ValueExpression Value { get; }

        internal Shape Shape => Value.OutputShape;

        internal static PluginInvocationArgument From(ValueExpression value) =>
            new PluginInvocationArgument(value);

        internal static PluginInvocationArgument FromResponse<TResponse, TProp>(
            ResponseBody<TResponse> body,
            Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (path == null) throw new ArgumentNullException(nameof(path));

            var responsePath = ExpressionPathHelper.ToResponsePath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            return From(ValueExpression.ReadPayload(body.Scope, responsePath, shape));
        }

        internal static PluginInvocationArgument FromEvent<TPayload, TProp>(
            Expression<Func<TPayload, TProp>> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var payloadPath = ExpressionPathHelper.ToEventPath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            return From(ValueExpression.ReadPayload(PayloadSource.Event(), payloadPath, shape));
        }

        internal static PluginInvocationArgument FromSource<TArg>(TypedSource<TArg> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return From(source.ToValueExpression());
        }

        internal static PluginInvocationArgument Literal<TValue>(TValue value)
        {
            var shape = Shape.FromClrType(typeof(TValue));
            return From(LiteralExpressionFor(value, shape));
        }

        private static ValueExpression LiteralExpressionFor<TValue>(TValue value, Shape shape)
        {
            if (value is DateTime dateTime)
                return ValueExpression.Literal(dateTime);

            return ValueExpression.LiteralRaw(value, shape);
        }
    }

    internal sealed class PluginArguments
    {
        private readonly PluginOperationId _operation;
        private readonly MethodArgumentContract _contract;
        private readonly List<ValueExpression> _values = new List<ValueExpression>();

        internal PluginArguments(PluginOperationId operation, MethodArgumentContract contract)
        {
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            _contract = contract ?? throw new ArgumentNullException(nameof(contract));
        }

        internal void Add(PluginInvocationArgument argument)
        {
            if (argument == null) throw new ArgumentNullException(nameof(argument));

            _contract.AcceptInvocationArgument(_operation.Label, _values.Count, argument.Shape);
            _values.Add(argument.Value);
        }

        internal List<ValueExpression> Complete()
        {
            _contract.AcceptInvocationComplete(_operation.Label, _values.Count);
            return new List<ValueExpression>(_values);
        }
    }
}
