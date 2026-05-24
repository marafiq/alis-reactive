using System;
using System.Collections.Generic;
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
        private readonly ComponentKey _componentKey;

        internal ElementBuilder(PipelineBuilder<TModel> pipeline, string elementId)
        {
            _pipeline = pipeline;
            _componentKey = pipeline.Context.EnsureElement(elementId);
        }

        /// <summary>Adds a CSS class to the element.</summary>
        /// <param name="className">The CSS class name.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> AddClass(string className)
        {
            return Call(BrowserElementMembers.AddClass, ValueProducer.Literal(className));
        }

        /// <summary>Removes a CSS class from the element.</summary>
        /// <param name="className">The CSS class name.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> RemoveClass(string className)
        {
            return Call(BrowserElementMembers.RemoveClass, ValueProducer.Literal(className));
        }

        /// <summary>Toggles a CSS class on the element.</summary>
        /// <param name="className">The CSS class name.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> ToggleClass(string className)
        {
            return Call(BrowserElementMembers.ToggleClass, ValueProducer.Literal(className));
        }

        /// <summary>Sets the text content of the element to a literal string.</summary>
        /// <param name="text">The text to display.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> SetText(string text)
        {
            return Set(BrowserElementMembers.Text, ValueProducer.Literal(text));
        }

        /// <summary>Sets the text content from an event payload property.</summary>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <param name="source">The event payload instance.</param>
        /// <param name="path">Expression selecting the property to display.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> SetText<TSource>(TSource source, Expression<Func<TSource, object>> path)
        {
            var eventPath = ExpressionPathHelper.ToEventPath<TSource, object>(path);
            return Set(BrowserElementMembers.Text, ValueProducer.ReadPayload(PayloadSource.Event(), eventPath));
        }

        /// <summary>Sets the text content from an HTTP response body property.</summary>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="source">The response body instance from <c>OnSuccess</c> or <c>OnError</c>.</param>
        /// <param name="path">Expression selecting the property to display.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> SetText<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object>> path)
            where TResponse : class
        {
            var responsePath = ExpressionPathHelper.ToResponsePath<TResponse, object>(path);
            return Set(BrowserElementMembers.Text, ValueProducer.ReadPayload(source.Scope, responsePath));
        }

        /// <summary>Sets the text content from a typed source (component, plugin, or URL value).</summary>
        /// <typeparam name="TProp">The source value type.</typeparam>
        /// <param name="source">The typed source providing the value.</param>
        /// <returns>This element builder for chaining additional element mutations.</returns>
        public ElementBuilder<TModel> SetText<TProp>(TypedSource<TProp> source)
        {
            Set(BrowserElementMembers.Text, source.ToValueProducer());
            return this;
        }

        /// <summary>Sets the inner HTML of the element to a literal string.</summary>
        /// <param name="html">The HTML content.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> SetHtml(string html)
        {
            return Set(BrowserElementMembers.Html, ValueProducer.Literal(html));
        }

        /// <summary>Sets the inner HTML from an event payload property.</summary>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <param name="source">The event payload instance.</param>
        /// <param name="path">Expression selecting the property containing HTML.</param>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> SetHtml<TSource>(TSource source, Expression<Func<TSource, object>> path)
        {
            var eventPath = ExpressionPathHelper.ToEventPath<TSource, object>(path);
            return Set(BrowserElementMembers.Html, ValueProducer.ReadPayload(PayloadSource.Event(), eventPath));
        }

        /// <summary>Sets the inner HTML from a typed source.</summary>
        /// <typeparam name="TProp">The source value type.</typeparam>
        /// <param name="source">The typed source providing the HTML content.</param>
        /// <returns>This element builder for chaining additional element mutations.</returns>
        public ElementBuilder<TModel> SetHtml<TProp>(TypedSource<TProp> source)
        {
            Set(BrowserElementMembers.Html, source.ToValueProducer());
            return this;
        }

        /// <summary>Shows the element by removing the hidden attribute.</summary>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> Show()
        {
            return Set(BrowserElementMembers.Hidden, ValueProducer.Literal(false));
        }

        /// <summary>Hides the element by setting the hidden attribute.</summary>
        /// <returns>The pipeline builder for chaining.</returns>
        public PipelineBuilder<TModel> Hide()
        {
            return Set(BrowserElementMembers.Hidden, ValueProducer.Literal(true));
        }

        private PipelineBuilder<TModel> Set<TValue>(ComponentProperty<TValue> property, ValueProducer value)
        {
            _pipeline.Context.EnsureProperty(
                _componentKey,
                property.ContractFor(MemberAccess.Write));
            _pipeline.AddStep(Reaction.Set(
                ComponentSource.Of(_componentKey), property.Member, value));
            return _pipeline;
        }

        private PipelineBuilder<TModel> Call(ComponentMethod method, ValueProducer arg)
        {
            _pipeline.Context.EnsureMethod(
                _componentKey,
                method.ContractReturning(Shape.None));
            _pipeline.AddStep(Reaction.Call(
                ComponentSource.Of(_componentKey),
                method.Member,
                new List<ValueProducer> { arg }));
            return _pipeline;
        }
    }
}
