using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Centralizes plugin argument lowering so value reads and commands enforce
    /// the same argument contract.
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
    /// Collects arguments for a value-returning plan-registered plugin method or root function.
    /// Use the builder where a <see cref="TypedPluginSource{TReturn}"/> is expected.
    /// </summary>
    /// <typeparam name="TReturn">The plugin method return type exposed to downstream value expressions.</typeparam>
    /// <typeparam name="TModel">The model type for the pipeline that owns the plugin read.</typeparam>
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

        /// <summary>Adds a value from an HTTP response body as a plugin argument.</summary>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <typeparam name="TProp">The response value type passed to the plugin.</typeparam>
        /// <param name="body">The success or error response body scope.</param>
        /// <param name="path">The response property path to pass.</param>
        public PluginMemberBuilder<TReturn, TModel> Arg<TResponse, TProp>(
            ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            _args.Add(PluginInvocationArgument.FromResponse(body, path));
            return this;
        }

        /// <summary>Adds a value from the current event payload as a plugin argument.</summary>
        /// <typeparam name="TPayload">The event payload type.</typeparam>
        /// <typeparam name="TProp">The event value type passed to the plugin.</typeparam>
        /// <param name="args">A typed event payload marker used to infer <typeparamref name="TPayload"/>.</param>
        /// <param name="path">The event payload path to pass.</param>
        public PluginMemberBuilder<TReturn, TModel> Arg<TPayload, TProp>(
            TPayload args, Expression<Func<TPayload, TProp>> path)
        {
            _args.Add(PluginInvocationArgument.FromEvent(path));
            return this;
        }

        /// <summary>Adds another typed value source as a plugin argument.</summary>
        /// <typeparam name="TArg">The source value type.</typeparam>
        /// <param name="source">The component, URL, response, or plugin value source to pass.</param>
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

        /// <summary>Adds a <see cref="DateTime"/> literal argument formatted for runtime date comparison.</summary>
        public PluginMemberBuilder<TReturn, TModel> Arg(DateTime value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a literal argument whose plan shape is derived from <typeparamref name="TValue"/>.</summary>
        /// <typeparam name="TValue">The literal value type.</typeparam>
        public PluginMemberBuilder<TReturn, TModel> ArgValue<TValue>(TValue value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Creates the typed value source represented by the configured plugin method call.</summary>
        /// <param name="b">The configured plugin member builder.</param>
        /// <returns>A typed plugin value source that captures the configured arguments.</returns>
        public static implicit operator TypedPluginSource<TReturn>(PluginMemberBuilder<TReturn, TModel> b) =>
            new TypedPluginSource<TReturn>(
                b._operation,
                b._args.Complete());
    }

    /// <summary>
    /// Collects arguments for a Reactive Plan plugin command. Call <see cref="Fire"/>
    /// to append the command to the current pipeline.
    /// </summary>
    /// <typeparam name="TModel">The model type for the pipeline that owns the plugin command.</typeparam>
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

        /// <summary>Adds a value from an HTTP response body as a plugin argument.</summary>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <typeparam name="TProp">The response value type passed to the plugin command.</typeparam>
        /// <param name="body">The success or error response body scope.</param>
        /// <param name="path">The response property path to pass.</param>
        public PluginCallBuilder<TModel> Arg<TResponse, TProp>(
            ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            _args.Add(PluginInvocationArgument.FromResponse(body, path));
            return this;
        }

        /// <summary>Adds a value from the current event payload as a plugin argument.</summary>
        /// <typeparam name="TPayload">The event payload type.</typeparam>
        /// <typeparam name="TProp">The event value type passed to the plugin command.</typeparam>
        /// <param name="args">A typed event payload marker used to infer <typeparamref name="TPayload"/>.</param>
        /// <param name="path">The event payload path to pass.</param>
        public PluginCallBuilder<TModel> Arg<TPayload, TProp>(
            TPayload args, Expression<Func<TPayload, TProp>> path)
        {
            _args.Add(PluginInvocationArgument.FromEvent(path));
            return this;
        }

        /// <summary>Adds a typed value source as a plugin argument.</summary>
        /// <typeparam name="TArg">The source value type.</typeparam>
        /// <param name="source">The component, URL, response, or plugin value source to pass.</param>
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

        /// <summary>Adds a <see cref="DateTime"/> literal argument formatted for runtime date comparison.</summary>
        public PluginCallBuilder<TModel> Arg(DateTime value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a literal argument whose plan shape is derived from <typeparamref name="TValue"/>.</summary>
        /// <typeparam name="TValue">The literal value type.</typeparam>
        public PluginCallBuilder<TModel> ArgValue<TValue>(TValue value)
        {
            _args.Add(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Appends the configured plugin command to the current pipeline.</summary>
        public void Fire()
        {
            _emitter.AddStep(ReactionGraph.Call(
                PluginSource.Of(_operation.PluginNameValue), _operation.PlanMethodNameValue,
                _args.Complete()));
        }
    }
}
