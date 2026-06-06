using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds DOM updates on a target element: text, HTML, CSS classes, and visibility.
    /// </summary>
    /// <remarks>
    /// Obtained via <c>p.Element("elementId")</c>. Literal and payload overloads return
    /// the parent <see cref="PipelineBuilder{TModel}"/>; typed-source overloads keep
    /// the element builder active for additional element updates.
    /// </remarks>
    /// <typeparam name="TModel">The view model used to author typed expression paths.</typeparam>
    public class ElementBuilder<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel> _pipeline;
        private readonly ComponentKey _elementKey;

        internal ElementBuilder(PipelineBuilder<TModel> pipeline, string elementId)
        {
            _pipeline = pipeline;
            _elementKey = pipeline.Context.DeclareElement(elementId);
        }

        /// <summary>Adds a CSS class to the element.</summary>
        /// <param name="className">CSS class token to add.</param>
        public PipelineBuilder<TModel> AddClass(string className)
        {
            return Call(BrowserElementMembers.AddClass, ValueExpression.Literal(className));
        }

        /// <summary>Removes a CSS class from the element.</summary>
        /// <param name="className">CSS class token to remove.</param>
        public PipelineBuilder<TModel> RemoveClass(string className)
        {
            return Call(BrowserElementMembers.RemoveClass, ValueExpression.Literal(className));
        }

        /// <summary>Toggles a CSS class on the element.</summary>
        /// <param name="className">CSS class token to toggle.</param>
        public PipelineBuilder<TModel> ToggleClass(string className)
        {
            return Call(BrowserElementMembers.ToggleClass, ValueExpression.Literal(className));
        }

        /// <summary>Sets the text content of the element to a literal string.</summary>
        /// <param name="text">Literal text content serialized into the Reactive Plan.</param>
        public PipelineBuilder<TModel> SetText(string text)
        {
            return Set(BrowserElementMembers.Text, ValueExpression.Literal(text));
        }

        /// <summary>Sets the text content from an event payload property.</summary>
        /// <typeparam name="TPayload">The event payload contract supplied by the trigger callback.</typeparam>
        /// <param name="source">The typed event payload marker supplied by the trigger callback.</param>
        /// <param name="path">Expression selecting the event payload property to display.</param>
        public PipelineBuilder<TModel> SetText<TPayload>(TPayload source, Expression<Func<TPayload, object>> path)
        {
            var payloadPath = ExpressionPathHelper.ToEventPath<TPayload, object>(path);
            return Set(BrowserElementMembers.Text, ValueExpression.ReadPayload(PayloadSource.Event(), payloadPath));
        }

        /// <summary>Sets the text content from an HTTP response body property.</summary>
        /// <typeparam name="TResponse">The response body contract for the active response route.</typeparam>
        /// <param name="source">The typed response body scope from <c>OnSuccess</c> or <c>OnError</c>.</param>
        /// <param name="path">Expression selecting the response body property to display.</param>
        public PipelineBuilder<TModel> SetText<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object>> path)
            where TResponse : class
        {
            var responsePath = ExpressionPathHelper.ToResponsePath<TResponse, object>(path);
            return Set(BrowserElementMembers.Text, ValueExpression.ReadPayload(source.Scope, responsePath));
        }

        /// <summary>Sets the text content from a typed source (component, plugin, or URL value).</summary>
        /// <typeparam name="TProp">The CLR type carried by the typed value source.</typeparam>
        /// <param name="source">The typed value source evaluated when the reaction executes.</param>
        /// <returns>This element builder for chaining additional element updates.</returns>
        public ElementBuilder<TModel> SetText<TProp>(TypedSource<TProp> source)
        {
            Set(BrowserElementMembers.Text, source.ToValueExpression());
            return this;
        }

        /// <summary>Sets the inner HTML of the element to a literal string.</summary>
        /// <param name="html">Literal HTML content serialized into the Reactive Plan.</param>
        public PipelineBuilder<TModel> SetHtml(string html)
        {
            return Set(BrowserElementMembers.Html, ValueExpression.Literal(html));
        }

        /// <summary>Sets the inner HTML from an event payload property.</summary>
        /// <typeparam name="TPayload">The event payload contract supplied by the trigger callback.</typeparam>
        /// <param name="source">The typed event payload marker supplied by the trigger callback.</param>
        /// <param name="path">Expression selecting the event payload property containing HTML.</param>
        public PipelineBuilder<TModel> SetHtml<TPayload>(TPayload source, Expression<Func<TPayload, object>> path)
        {
            var payloadPath = ExpressionPathHelper.ToEventPath<TPayload, object>(path);
            return Set(BrowserElementMembers.Html, ValueExpression.ReadPayload(PayloadSource.Event(), payloadPath));
        }

        /// <summary>Sets the inner HTML from a typed source.</summary>
        /// <typeparam name="TProp">The CLR type carried by the typed value source.</typeparam>
        /// <param name="source">The typed value source evaluated when the reaction executes.</param>
        /// <returns>This element builder for chaining additional element updates.</returns>
        public ElementBuilder<TModel> SetHtml<TProp>(TypedSource<TProp> source)
        {
            Set(BrowserElementMembers.Html, source.ToValueExpression());
            return this;
        }

        /// <summary>Shows the element by removing the hidden attribute.</summary>
        public PipelineBuilder<TModel> Show()
        {
            return Set(BrowserElementMembers.Hidden, ValueExpression.Literal(false));
        }

        /// <summary>Hides the element by setting the hidden attribute.</summary>
        public PipelineBuilder<TModel> Hide()
        {
            return Set(BrowserElementMembers.Hidden, ValueExpression.Literal(true));
        }

        private PipelineBuilder<TModel> Set<TValue>(ComponentProperty<TValue> property, ValueExpression value)
        {
            _pipeline.Context.DeclareProperty(
                _elementKey,
                property.ContractFor(MemberAccess.Write));
            _pipeline.AddStep(ReactionGraph.Set(
                ComponentSource.Of(_elementKey), property.Member, value));
            return _pipeline;
        }

        private PipelineBuilder<TModel> Call(ComponentMethod method, ValueExpression arg)
        {
            _pipeline.Context.DeclareMethod(
                _elementKey,
                method.ContractReturning(Shape.None));
            _pipeline.AddStep(ReactionGraph.Call(
                ComponentSource.Of(_elementKey),
                method.Member,
                new List<ValueExpression> { arg }));
            return _pipeline;
        }
    }
}
