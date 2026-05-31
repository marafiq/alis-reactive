using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Collects typed arguments for a plugin read or call. Every <c>Arg</c> lowers
    /// to a <see cref="ValueExpression"/> over the shared value spine; the
    /// accumulation and contract checking are declared once here. The read face
    /// (<see cref="PluginMemberBuilder{TReturn, TModel}"/>) and the call face
    /// (<see cref="PluginCallBuilder{TModel}"/>) forward their <c>Arg</c> calls to
    /// this collector.
    /// </summary>
    internal sealed class PluginArgumentCollector
    {
        private readonly PluginArguments _args;

        internal PluginArgumentCollector(PluginOperationId operation, MethodArgumentContract arguments)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            _args = new PluginArguments(operation, arguments);
        }

        internal void Add(PluginInvocationArgument argument) => _args.Add(argument);

        internal List<ValueExpression> Complete() => _args.Complete();
    }

    /// <summary>
    /// Builds a plugin read (property-less function read) with optional arguments.
    /// Implicitly converts to <see cref="TypedPluginSource{TReturn}"/> — the source
    /// IS the builder, so there is no explicit Build() call.
    /// </summary>
    public sealed class PluginMemberBuilder<TReturn, TModel> where TModel : class
    {
        private readonly PluginOperationId _operation;
        private readonly PluginArgumentCollector _args;

        internal PluginMemberBuilder(PluginOperationId operation, MethodArgumentContract arguments)
        {
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            _args = new PluginArgumentCollector(operation, arguments);
        }

        internal PluginMemberBuilder(PluginFunction<TReturn> function)
            : this(PluginOperationId.Of(function), function.ArgumentContract)
        {
        }

        /// <summary>Adds a response body expression as an argument (carries success/error scope).</summary>
        public PluginMemberBuilder<TReturn, TModel> Arg<TResponse, TProp>(
            ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            _args.Add(PluginInvocationArgument.FromResponse(body, path));
            return this;
        }

        /// <summary>Adds an event arg expression as an argument.</summary>
        public PluginMemberBuilder<TReturn, TModel> Arg<TArgs, TProp>(
            TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            _args.Add(PluginInvocationArgument.FromEvent(path));
            return this;
        }

        /// <summary>Adds a typed source as an argument (component read, URL param, another plugin read).</summary>
        public PluginMemberBuilder<TReturn, TModel> Arg<TArg>(TypedSource<TArg> source)
        {
            _args.Add(PluginInvocationArgument.FromSource(source));
            return this;
        }

        /// <summary>Adds a string literal argument.</summary>
        public PluginMemberBuilder<TReturn, TModel> Arg(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds an int literal argument.</summary>
        public PluginMemberBuilder<TReturn, TModel> Arg(int value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a bool literal argument.</summary>
        public PluginMemberBuilder<TReturn, TModel> Arg(bool value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a long literal argument.</summary>
        public PluginMemberBuilder<TReturn, TModel> Arg(long value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a decimal literal argument.</summary>
        public PluginMemberBuilder<TReturn, TModel> Arg(decimal value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a double literal argument.</summary>
        public PluginMemberBuilder<TReturn, TModel> Arg(double value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a DateTime literal argument formatted for browser date comparison.</summary>
        public PluginMemberBuilder<TReturn, TModel> Arg(DateTime value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a literal argument whose plan shape is derived from <typeparamref name="TValue"/>.</summary>
        public PluginMemberBuilder<TReturn, TModel> ArgValue<TValue>(TValue value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Read terminal: implicit conversion to TypedPluginSource — the source IS the builder.</summary>
        public static implicit operator TypedPluginSource<TReturn>(PluginMemberBuilder<TReturn, TModel> b) =>
            new TypedPluginSource<TReturn>(
                b._operation,
                b._args.Complete());
    }

    /// <summary>
    /// Builds a void plugin command call with optional arguments. Shares the same
    /// <c>Arg</c> surface as <see cref="PluginMemberBuilder{TReturn, TModel}"/>;
    /// call <see cref="Fire"/> to emit the CallReaction into the pipeline.
    /// </summary>
    public sealed class PluginCallBuilder<TModel> where TModel : class
    {
        private readonly PluginOperationId _operation;
        private readonly IReactionEmitter _emitter;
        private readonly PluginArgumentCollector _args;

        internal PluginCallBuilder(
            PluginOperationId operation,
            IReactionEmitter emitter,
            MethodArgumentContract arguments)
        {
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            _emitter = emitter ?? throw new ArgumentNullException(nameof(emitter));
            _args = new PluginArgumentCollector(operation, arguments);
        }

        internal PluginCallBuilder(PluginCommand command, IReactionEmitter emitter)
            : this(PluginOperationId.Of(command), emitter, command.ArgumentContract)
        {
        }

        /// <summary>Adds a response body expression as an argument.</summary>
        public PluginCallBuilder<TModel> Arg<TResponse, TProp>(
            ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            _args.Add(PluginInvocationArgument.FromResponse(body, path));
            return this;
        }

        /// <summary>Adds an event arg expression as an argument.</summary>
        public PluginCallBuilder<TModel> Arg<TArgs, TProp>(
            TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            _args.Add(PluginInvocationArgument.FromEvent(path));
            return this;
        }

        /// <summary>Adds a typed source as an argument.</summary>
        public PluginCallBuilder<TModel> Arg<TArg>(TypedSource<TArg> source)
        {
            _args.Add(PluginInvocationArgument.FromSource(source));
            return this;
        }

        /// <summary>Adds a string literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds an int literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(int value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a bool literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(bool value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a long literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(long value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a decimal literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(decimal value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a double literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(double value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a DateTime literal argument formatted for browser date comparison.</summary>
        public PluginCallBuilder<TModel> Arg(DateTime value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a literal argument whose plan shape is derived from <typeparamref name="TValue"/>.</summary>
        public PluginCallBuilder<TModel> ArgValue<TValue>(TValue value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Emits the CallReaction into the pipeline. Terminal method.</summary>
        public void Fire()
        {
            _emitter.AddStep(ReactionGraph.Call(
                PluginSource.Of(_operation.PluginNameValue), _operation.PlanMethodNameValue,
                _args.Complete()));
        }
    }
}
