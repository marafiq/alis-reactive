using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds DOM mutations on a target element: text, HTML, CSS classes, and visibility.
    /// </summary>
    /// <remarks>
    /// Obtained via <c>p.Element("elementId")</c>. Most methods return the parent
    /// <see cref="PipelineBuilder{TModel}"/> for continued chaining.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public class ElementBuilder<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel> _pipeline;
        private readonly string _elementId;
        private readonly string _componentKey;

        internal ElementBuilder(PipelineBuilder<TModel> pipeline, string elementId)
        {
            _pipeline = pipeline;
            _elementId = elementId;
            _componentKey = pipeline.Context.EnsureElement(elementId);
        }

        /// <summary>Adds a CSS class to the element.</summary>
        /// <param name="className">The CSS class name.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> AddClass(string className)
        {
            _pipeline.Context.EnsureMethod(_componentKey, "classAdd", "classList.add");
            _pipeline.Steps.Add(Reaction.Call(
                ComponentSource.Of(_componentKey), "classAdd",
                new System.Collections.Generic.List<ValueProducer> { ValueProducer.Literal(className) }));
            return _pipeline;
        }

        /// <summary>Removes a CSS class from the element.</summary>
        /// <param name="className">The CSS class name.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> RemoveClass(string className)
        {
            _pipeline.Context.EnsureMethod(_componentKey, "classRemove", "classList.remove");
            _pipeline.Steps.Add(Reaction.Call(
                ComponentSource.Of(_componentKey), "classRemove",
                new System.Collections.Generic.List<ValueProducer> { ValueProducer.Literal(className) }));
            return _pipeline;
        }

        /// <summary>Toggles a CSS class on the element.</summary>
        /// <param name="className">The CSS class name.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> ToggleClass(string className)
        {
            _pipeline.Context.EnsureMethod(_componentKey, "classToggle", "classList.toggle");
            _pipeline.Steps.Add(Reaction.Call(
                ComponentSource.Of(_componentKey), "classToggle",
                new System.Collections.Generic.List<ValueProducer> { ValueProducer.Literal(className) }));
            return _pipeline;
        }

        /// <summary>Sets the text content of the element to a literal string.</summary>
        /// <param name="text">The text to display.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> SetText(string text)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "text", "textContent", Shape.String, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "text", ValueProducer.Literal(text)));
            return _pipeline;
        }

        /// <summary>Sets the text content from an event payload property.</summary>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <param name="source">The event payload instance.</param>
        /// <param name="path">Expression selecting the property to display.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> SetText<TSource>(TSource source, Expression<Func<TSource, object>> path)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "text", "textContent", Shape.String, "write");
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "text",
                ValueProducer.Read(PayloadSource.Event(), eventPath)));
            return _pipeline;
        }

        /// <summary>Sets the text content from an HTTP response body property.</summary>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="source">The response body instance from <c>OnSuccess</c> or <c>OnError</c>.</param>
        /// <param name="path">Expression selecting the property to display.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> SetText<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object>> path)
            where TResponse : class
        {
            _pipeline.Context.EnsureProperty(_componentKey, "text", "textContent", Shape.String, "write");
            var responsePath = ExpressionPathHelper.ToResponsePath(path);
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "text",
                ValueProducer.Read(source.Scope, responsePath)));
            return _pipeline;
        }

        /// <summary>Sets the text content from a typed source (component, plugin, or URL value).</summary>
        /// <typeparam name="TProp">The source value type.</typeparam>
        /// <param name="source">The typed source providing the value.</param>
        /// <returns>This element builder for chaining additional element mutations.</returns>
        public ElementBuilder<TModel> SetText<TProp>(TypedSource<TProp> source)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "text", "textContent", Shape.String, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "text",
                source.ToValueProducer()));
            return this;
        }

        /// <summary>Sets the inner HTML of the element to a literal string.</summary>
        /// <param name="html">The HTML content.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> SetHtml(string html)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "html", "innerHTML", Shape.String, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "html", ValueProducer.Literal(html)));
            return _pipeline;
        }

        /// <summary>Sets the inner HTML from an event payload property.</summary>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <param name="source">The event payload instance.</param>
        /// <param name="path">Expression selecting the property containing HTML.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> SetHtml<TSource>(TSource source, Expression<Func<TSource, object>> path)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "html", "innerHTML", Shape.String, "write");
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "html",
                ValueProducer.Read(PayloadSource.Event(), eventPath)));
            return _pipeline;
        }

        /// <summary>Sets the inner HTML from a typed source.</summary>
        /// <typeparam name="TProp">The source value type.</typeparam>
        /// <param name="source">The typed source providing the HTML content.</param>
        /// <returns>This element builder for chaining additional element mutations.</returns>
        public ElementBuilder<TModel> SetHtml<TProp>(TypedSource<TProp> source)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "html", "innerHTML", Shape.String, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "html",
                source.ToValueProducer()));
            return this;
        }

        /// <summary>Shows the element by removing the hidden attribute.</summary>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> Show()
        {
            _pipeline.Context.EnsureProperty(_componentKey, "hidden", "hidden", Shape.Boolean, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "hidden", ValueProducer.Literal(false)));
            return _pipeline;
        }

        /// <summary>Hides the element by setting the hidden attribute.</summary>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> Hide()
        {
            _pipeline.Context.EnsureProperty(_componentKey, "hidden", "hidden", Shape.Boolean, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "hidden", ValueProducer.Literal(true)));
            return _pipeline;
        }
    }
}
