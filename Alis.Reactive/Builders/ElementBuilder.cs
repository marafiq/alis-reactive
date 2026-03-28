using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds mutations on a DOM element — CSS classes, text content, HTML content, and visibility.
    /// </summary>
    /// <remarks>
    /// Created by <see cref="PipelineBuilder{TModel}.Element(string)"/>. Each mutation method
    /// adds a command to the pipeline and returns either this builder (for chaining more mutations
    /// on the same element) or the pipeline builder (for continuing with other commands).
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public class ElementBuilder<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel> _pipeline;
        private readonly string _elementId;

        /// <summary>
        /// NEVER make public. Constructed exclusively by <see cref="PipelineBuilder{TModel}.Element(string)"/>.
        /// Public constructors would let devs create builders detached from a pipeline.
        /// </summary>
        internal ElementBuilder(PipelineBuilder<TModel> pipeline, string elementId)
        {
            _pipeline = pipeline;
            _elementId = elementId;
        }

        /// <summary>
        /// Adds a CSS class to the element.
        /// </summary>
        /// <param name="className">The CSS class name to add.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> AddClass(string className)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, new CallMutation("add", chain: "classList", args: new MethodArg[] { new LiteralArg(className) })));
            return _pipeline;
        }

        /// <summary>
        /// Removes a CSS class from the element.
        /// </summary>
        /// <param name="className">The CSS class name to remove.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> RemoveClass(string className)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, new CallMutation("remove", chain: "classList", args: new MethodArg[] { new LiteralArg(className) })));
            return _pipeline;
        }

        /// <summary>
        /// Toggles a CSS class on the element — adds it if absent, removes it if present.
        /// </summary>
        /// <param name="className">The CSS class name to toggle.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> ToggleClass(string className)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, new CallMutation("toggle", chain: "classList", args: new MethodArg[] { new LiteralArg(className) })));
            return _pipeline;
        }

        /// <summary>
        /// Sets the element's text content to a static string.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> SetText(string text)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, new SetPropMutation("textContent"), text));
            return _pipeline;
        }

        /// <summary>
        /// Sets the element text from an event payload property resolved at runtime.
        /// The source instance is used only for generic type inference — its value is ignored.
        /// </summary>
        public PipelineBuilder<TModel> SetText<TSource>(TSource source, Expression<Func<TSource, object?>> path)
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, new SetPropMutation("textContent"), source: new EventSource(sourcePath)));
            return _pipeline;
        }

        /// <summary>
        /// Sets the element text from an HTTP response body property.
        /// Same phantom pattern as event payload overload — compiler resolves via ResponseBody&lt;T&gt;.
        /// </summary>
        public PipelineBuilder<TModel> SetText<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, new SetPropMutation("textContent"), source: new EventSource(sourcePath)));
            return _pipeline;
        }

        /// <summary>
        /// Sets the element text from a BindSource (event or component).
        /// </summary>
        public ElementBuilder<TModel> SetText(BindSource source)
        {
            _pipeline.Commands.Add(new MutateElementCommand(
                _elementId, new SetPropMutation("textContent"), source: source));
            return this;
        }

        /// <summary>
        /// Sets the element text from a typed source (type-safe for conditions).
        /// </summary>
        public ElementBuilder<TModel> SetText<TProp>(TypedSource<TProp> source)
        {
            _pipeline.Commands.Add(new MutateElementCommand(
                _elementId, new SetPropMutation("textContent"), source: source.ToBindSource()));
            return this;
        }

        /// <summary>
        /// Sets the element's inner HTML to a static string.
        /// </summary>
        /// <param name="html">The HTML markup to inject.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> SetHtml(string html)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, new SetPropMutation("innerHTML"), html));
            return _pipeline;
        }

        /// <summary>
        /// Sets the element HTML from an event payload property resolved at runtime.
        /// </summary>
        public PipelineBuilder<TModel> SetHtml<TSource>(TSource source, Expression<Func<TSource, object?>> path)
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, new SetPropMutation("innerHTML"), source: new EventSource(sourcePath)));
            return _pipeline;
        }

        /// <summary>
        /// Sets the element HTML from a BindSource (event or component).
        /// </summary>
        public ElementBuilder<TModel> SetHtml(BindSource source)
        {
            _pipeline.Commands.Add(new MutateElementCommand(
                _elementId, new SetPropMutation("innerHTML"), source: source));
            return this;
        }

        /// <summary>
        /// Sets the element HTML from a typed source (type-safe for conditions).
        /// </summary>
        public ElementBuilder<TModel> SetHtml<TProp>(TypedSource<TProp> source)
        {
            _pipeline.Commands.Add(new MutateElementCommand(
                _elementId, new SetPropMutation("innerHTML"), source: source.ToBindSource()));
            return this;
        }

        /// <summary>
        /// Shows the element by removing the <c>hidden</c> attribute.
        /// </summary>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> Show()
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, new CallMutation("removeAttribute", args: new MethodArg[] { new LiteralArg("hidden") })));
            return _pipeline;
        }

        /// <summary>
        /// Hides the element by setting the <c>hidden</c> attribute.
        /// </summary>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> Hide()
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, new CallMutation("setAttribute", args: new MethodArg[] { new LiteralArg("hidden"), new LiteralArg("") })));
            return _pipeline;
        }

        /// <summary>
        /// Attaches a per-action guard to the LAST command added to the pipeline.
        /// The guard is evaluated at runtime — if false, the command is skipped.
        /// </summary>
        public ElementBuilder<TModel> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path,
            Func<ConditionSourceBuilder<TModel, TProp>, GuardBuilder<TModel>> configure)
        {
            var source = new EventArgSource<TPayload, TProp>(path);
            var csb = new ConditionSourceBuilder<TModel, TProp>(source);
            var gb = configure(csb);
            if (_pipeline.Commands.Count > 0)
            {
                var idx = _pipeline.Commands.Count - 1;
                _pipeline.Commands[idx] = _pipeline.Commands[idx].WithGuard(gb.Guard);
            }
            return this;
        }
    }
}
