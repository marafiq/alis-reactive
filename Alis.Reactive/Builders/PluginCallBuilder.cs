using System;
using System.Collections.Generic;
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
        private readonly string _pluginName;
        private readonly string _method;
        private readonly IReactionEmitter _emitter;
        private readonly List<ValueProducer> _args = new List<ValueProducer>();

        internal PluginCallBuilder(string pluginName, string method, IReactionEmitter emitter)
        {
            _pluginName = pluginName;
            _method = method;
            _emitter = emitter;
        }

        /// <summary>Adds a response body expression as an argument.</summary>
        public PluginCallBuilder<TModel> Arg<TResponse, TProp>(
            ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            var responsePath = ExpressionPathHelper.ToResponsePath(path);
            _args.Add(ValueProducer.Read(body.Scope, responsePath, shape: Shape.FromClrType(typeof(TProp))));
            return this;
        }

        /// <summary>Adds an event arg expression as an argument.</summary>
        public PluginCallBuilder<TModel> Arg<TArgs, TProp>(
            TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            _args.Add(ValueProducer.Read(PayloadSource.Event(), eventPath, shape: Shape.FromClrType(typeof(TProp))));
            return this;
        }

        /// <summary>Adds a typed source as an argument.</summary>
        public PluginCallBuilder<TModel> Arg<TArg>(TypedSource<TArg> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _args.Add(source.ToValueProducer());
            return this;
        }

        /// <summary>Adds a string literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(string value)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));
            _args.Add(ValueProducer.Literal(value)); return this;
        }

        /// <summary>Adds an int literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(int value)
        { _args.Add(ValueProducer.Literal(value)); return this; }

        /// <summary>Adds a bool literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(bool value)
        { _args.Add(ValueProducer.Literal(value)); return this; }

        /// <summary>Adds a long literal argument.</summary>
        public PluginCallBuilder<TModel> Arg(long value)
        { _args.Add(ValueProducer.Literal(value)); return this; }

        /// <summary>Emits the CallReaction into the pipeline. Terminal method.</summary>
        public void Fire()
        {
            _emitter.AddStep(Reaction.Call(
                PluginSource.Of(_pluginName), _method,
                _args.Count > 0 ? _args : null));
        }
    }
}
