using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds a void plugin method call with optional arguments.
    /// Call <see cref="Fire"/> to emit the CallReaction into the pipeline.
    /// </summary>
    public sealed class PluginCallBuilder<TModel> where TModel : class
    {
        private readonly PluginOperationId _operation;
        private readonly IReactionEmitter _emitter;
        private readonly PluginArguments _args;

        internal PluginCallBuilder(
            PluginOperationId operation,
            IReactionEmitter emitter,
            MethodArgumentContract arguments)
        {
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            _emitter = emitter ?? throw new ArgumentNullException(nameof(emitter));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            _args = new PluginArguments(operation, arguments);
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
            AddArg(PluginInvocationArgument.FromResponse(body, path));
            return this;
        }

        /// <summary>Adds an event arg expression as an argument.</summary>
        public PluginCallBuilder<TModel> Arg<TArgs, TProp>(
            TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            AddArg(PluginInvocationArgument.FromEvent(path));
            return this;
        }

        /// <summary>Adds a typed source as an argument.</summary>
        public PluginCallBuilder<TModel> Arg<TArg>(TypedSource<TArg> source)
        {
            AddArg(PluginInvocationArgument.FromSource(source));
            return this;
        }

        /// <summary>Adds a string literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(string value)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds an int literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(int value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a bool literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(bool value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a long literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(long value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a decimal literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(decimal value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a double literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(double value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a DateTime literal argument formatted for browser date comparison.</summary>
        public PluginCallBuilder<TModel> Arg(DateTime value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Adds a literal argument whose plan shape is derived from <typeparamref name="TValue"/>.</summary>
        public PluginCallBuilder<TModel> ArgValue<TValue>(TValue value)
        {
            AddArg(PluginInvocationArgument.Literal(value));
            return this;
        }

        /// <summary>Emits the CallReaction into the pipeline. Terminal method.</summary>
        public void Fire()
        {
            _emitter.AddStep(Reaction.Call(
                PluginSource.Of(_operation.PluginNameValue), _operation.PlanMethodNameValue,
                _args.Complete()));
        }

        private void AddArg(PluginInvocationArgument argument)
        {
            _args.Add(argument);
        }
    }
}
