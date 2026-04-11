using System;
using System.Collections.Generic;
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
        private readonly string _pluginName;
        private readonly string _member;
        private readonly List<ValueProducer> _args = new List<ValueProducer>();

        internal PluginReadBuilder(string pluginName, string member)
        {
            _pluginName = pluginName;
            _member = member;
        }

        /// <summary>Adds a response body expression as an argument (carries success/error scope).</summary>
        public PluginReadBuilder<TReturn, TModel> Arg<TResponse, TProp>(
            ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            var responsePath = ExpressionPathHelper.ToResponsePath(path);
            _args.Add(ValueProducer.Read(body.Scope, responsePath, shape: Shape.FromClrType(typeof(TProp))));
            return this;
        }

        /// <summary>Adds an event arg expression as an argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg<TArgs, TProp>(
            TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            _args.Add(ValueProducer.Read(PayloadSource.Event(), eventPath, shape: Shape.FromClrType(typeof(TProp))));
            return this;
        }

        /// <summary>Adds a typed source as an argument (component read, URL param, another plugin read).</summary>
        public PluginReadBuilder<TReturn, TModel> Arg<TArg>(TypedSource<TArg> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _args.Add(source.ToValueProducer());
            return this;
        }

        /// <summary>Adds a string literal argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg(string value)
        { _args.Add(ValueProducer.Literal(value)); return this; }

        /// <summary>Adds an int literal argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg(int value)
        { _args.Add(ValueProducer.Literal(value)); return this; }

        /// <summary>Adds a bool literal argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg(bool value)
        { _args.Add(ValueProducer.Literal(value)); return this; }

        /// <summary>Adds a long literal argument.</summary>
        public PluginReadBuilder<TReturn, TModel> Arg(long value)
        { _args.Add(ValueProducer.Literal(value)); return this; }

        /// <summary>Implicit conversion to TypedPluginSource — no Build() needed.</summary>
        public static implicit operator TypedPluginSource<TReturn>(PluginReadBuilder<TReturn, TModel> b) =>
            new TypedPluginSource<TReturn>(b._pluginName, b._member, b._args);
    }
}
