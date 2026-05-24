using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    internal sealed class PluginInvocationArgument
    {
        private PluginInvocationArgument(ValueProducer value, Shape shape)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal ValueProducer Value { get; }

        internal Shape Shape { get; }

        internal static PluginInvocationArgument From(ValueProducer value, Shape shape) =>
            new PluginInvocationArgument(value, shape);

        internal static PluginInvocationArgument FromResponse<TResponse, TProp>(
            ResponseBody<TResponse> body,
            Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (path == null) throw new ArgumentNullException(nameof(path));

            var responsePath = ExpressionPathHelper.ToResponsePath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            return From(ValueProducer.ReadPayload(body.Scope, responsePath, shape), shape);
        }

        internal static PluginInvocationArgument FromEvent<TArgs, TProp>(
            Expression<Func<TArgs, TProp>> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var eventPath = ExpressionPathHelper.ToEventPath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            return From(ValueProducer.ReadPayload(PayloadSource.Event(), eventPath, shape), shape);
        }

        internal static PluginInvocationArgument FromSource<TArg>(TypedSource<TArg> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return From(source.ToValueProducer(), source.Shape);
        }

        internal static PluginInvocationArgument Literal<TValue>(TValue value)
        {
            var shape = Shape.FromClrType(typeof(TValue));
            return From(LiteralProducerFor(value, shape), shape);
        }

        private static ValueProducer LiteralProducerFor<TValue>(TValue value, Shape shape)
        {
            if (value is DateTime dateTime)
                return ValueProducer.Literal(dateTime);

            return ValueProducer.LiteralRaw(value, shape);
        }
    }

    internal sealed class PluginArguments
    {
        private readonly PluginOperationId _operation;
        private readonly MethodArgumentContract _contract;
        private readonly List<ValueProducer> _values = new List<ValueProducer>();

        internal PluginArguments(PluginOperationId operation, MethodArgumentContract contract)
        {
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            _contract = contract ?? throw new ArgumentNullException(nameof(contract));
        }

        internal void Add(PluginInvocationArgument argument)
        {
            if (argument == null) throw new ArgumentNullException(nameof(argument));

            _contract.ValidateInvocationArgument(_operation.Label, _values.Count, argument.Shape);
            _values.Add(argument.Value);
        }

        internal List<ValueProducer> Complete()
        {
            _contract.ValidateInvocationComplete(_operation.Label, _values.Count);
            return new List<ValueProducer>(_values);
        }
    }
}
