using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds a plugin method read with optional arguments.
    /// Implicitly converts to <see cref="TypedPluginSource{TProp}"/> — no explicit Build() call needed.
    /// </summary>
    public sealed class PluginReadBuilder<TReturn, TModel> where TModel : class
    {
        private readonly PluginOperationId _operation;
        private readonly PluginArguments _args;

        internal PluginReadBuilder(PluginOperationId operation, MethodArgumentContract arguments)
        {
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            _args = new PluginArguments(operation, arguments);
        }

        internal PluginReadBuilder(PluginFunction<TReturn> function)
            : this(PluginOperationId.Of(function), function.ArgumentContract)
        {
        }

        /// <summary>Adds a response body expression as an argument (carries success/error scope).</summary>
        public PluginReadBuilder<TReturn, TModel> Arg<TResponse, TProp>(
            ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            AddArg(PluginInvocationArgument.FromResponse(body, path));
            return this;
        }

        /// <summary>Adds an event arg expression as an argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg<TArgs, TProp>(
            TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            AddArg(PluginInvocationArgument.FromEvent(path));
            return this;
        }

        /// <summary>Adds a typed source as an argument (component read, URL param, another plugin read).</summary>
        public PluginReadBuilder<TReturn, TModel> Arg<TArg>(TypedSource<TArg> source)
        {
            AddArg(PluginInvocationArgument.FromSource(source));
            return this;
        }

        /// <summary>Adds a string literal argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg(string value)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds an int literal argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg(int value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a bool literal argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg(bool value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a long literal argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg(long value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a decimal literal argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg(decimal value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a double literal argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg(double value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a DateTime literal argument formatted for browser date comparison.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg(DateTime value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a literal argument whose plan shape is derived from <typeparamref name="TValue"/>.</summary>
        public PluginReadBuilder<TReturn, TModel> ArgValue<TValue>(TValue value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Implicit conversion to TypedPluginSource — no Build() needed.</summary>
        public static implicit operator TypedPluginSource<TReturn>(PluginReadBuilder<TReturn, TModel> b) =>
            new TypedPluginSource<TReturn>(
                b._operation,
                b._args.Complete());

        private void AddArg(PluginInvocationArgument argument)
        {
            _args.Add(argument);
        }
    }
}
