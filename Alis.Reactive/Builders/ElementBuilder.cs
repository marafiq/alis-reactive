using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds mutations on a DOM element: CSS classes, text content, HTML content, and visibility.
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
            _pipeline.CallElementMember(_elementId, "classList.add", className);
            return _pipeline;
        }

        /// <summary>
        /// Removes a CSS class from the element.
        /// </summary>
        /// <param name="className">The CSS class name to remove.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> RemoveClass(string className)
        {
            _pipeline.CallElementMember(_elementId, "classList.remove", className);
            return _pipeline;
        }

        /// <summary>
        /// Toggles a CSS class on the element. Adds it if absent, removes it if present.
        /// </summary>
        /// <param name="className">The CSS class name to toggle.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> ToggleClass(string className)
        {
            _pipeline.CallElementMember(_elementId, "classList.toggle", className);
            return _pipeline;
        }

        /// <summary>
        /// Sets the element's text content to a static string.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> SetText(string text)
        {
            _pipeline.SetElementProperty(_elementId, "textContent", text);
            return _pipeline;
        }

        /// <summary>
        /// Sets the element text from an event payload property resolved in the browser.
        /// </summary>
        /// <remarks>
        /// The <paramref name="payload"/> instance is used only for compile-time type inference;
        /// its property values are never read.
        /// </remarks>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <param name="payload">The payload instance providing compile-time type inference.</param>
        /// <param name="path">The property-access expression into the payload.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> SetText<TSource>(TSource payload, Expression<Func<TSource, object?>> path)
        {
            var valueMemberPath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.SetElementPropertyFromPath(_elementId, "textContent", valueMemberPath);
            return _pipeline;
        }

        /// <summary>
        /// Sets the element text from an HTTP response body property.
        /// </summary>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="response">The response body instance providing compile-time type inference.</param>
        /// <param name="path">The property-access expression into the response body.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> SetText<TResponse>(ResponseBody<TResponse> response, Expression<Func<TResponse, object?>> path)
            where TResponse : class
        {
            var valueMemberPath = ExpressionPathHelper.ToResponsePath(path);
            _pipeline.SetElementPropertyFromPath(_elementId, "textContent", valueMemberPath);
            return _pipeline;
        }

        /// <summary>
        /// Sets the element text from a typed value reference (type-safe for conditions).
        /// </summary>
        /// <remarks>
        /// Use with a component's <c>Value()</c> method to display its current value:
        /// <code>
        /// var comp = p.Component&lt;NativeTextBox&gt;(m => m.Name);
        /// p.Element("name-echo").SetText(comp.Value());
        /// </code>
        /// </remarks>
        /// <typeparam name="TProp">The value type.</typeparam>
        /// <param name="value">The typed value reference to resolve.</param>
        /// <returns>This element builder for chaining additional mutations.</returns>
        public ElementBuilder<TModel> SetText<TProp>(ValueExpression<TProp> value)
        {
            _pipeline.SetElementProperty(
                _elementId,
                "textContent",
                value.ToValueExpr(_pipeline.Authoring),
                value.CoercionType);
            return this;
        }

        /// <summary>
        /// Sets the element's inner HTML to a static string.
        /// </summary>
        /// <param name="html">The HTML markup to inject.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> SetHtml(string html)
        {
            _pipeline.SetElementProperty(_elementId, "innerHTML", html);
            return _pipeline;
        }

        /// <summary>
        /// Sets the element HTML from an event payload property resolved in the browser.
        /// </summary>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <param name="payload">The payload instance providing compile-time type inference.</param>
        /// <param name="path">The property-access expression into the payload.</param>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> SetHtml<TSource>(TSource payload, Expression<Func<TSource, object?>> path)
        {
            var valueMemberPath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.SetElementPropertyFromPath(_elementId, "innerHTML", valueMemberPath);
            return _pipeline;
        }

        /// <summary>
        /// Sets the element HTML from a typed value reference (type-safe for conditions).
        /// </summary>
        /// <remarks>
        /// Use with a component's <c>Value()</c> method to display its current value as HTML:
        /// <code>
        /// var comp = p.Component&lt;NativeTextBox&gt;(m => m.Name);
        /// p.Element("name-html").SetHtml(comp.Value());
        /// </code>
        /// </remarks>
        /// <typeparam name="TProp">The value type.</typeparam>
        /// <param name="value">The typed value reference to resolve.</param>
        /// <returns>This element builder for chaining additional mutations.</returns>
        public ElementBuilder<TModel> SetHtml<TProp>(ValueExpression<TProp> value)
        {
            _pipeline.SetElementProperty(
                _elementId,
                "innerHTML",
                value.ToValueExpr(_pipeline.Authoring),
                value.CoercionType);
            return this;
        }

        /// <summary>
        /// Shows the element by removing the <c>hidden</c> attribute.
        /// </summary>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> Show()
        {
            _pipeline.SetElementProperty(_elementId, "hidden", false, coerceAs: "boolean");
            return _pipeline;
        }

        /// <summary>
        /// Hides the element by setting the <c>hidden</c> attribute.
        /// </summary>
        /// <returns>The pipeline builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> Hide()
        {
            _pipeline.SetElementProperty(_elementId, "hidden", true, coerceAs: "boolean");
            return _pipeline;
        }
    }
}
