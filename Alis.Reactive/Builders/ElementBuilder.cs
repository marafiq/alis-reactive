using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
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

        public PipelineBuilder<TModel> AddClass(string className)
        {
            _pipeline.Context.EnsureMethod(_componentKey, "classAdd", "classList.add");
            _pipeline.Steps.Add(Reaction.Call(
                ComponentSource.Of(_componentKey), "classAdd",
                new System.Collections.Generic.List<ValueProducer> { ValueProducer.Literal(className) }));
            return _pipeline;
        }

        public PipelineBuilder<TModel> RemoveClass(string className)
        {
            _pipeline.Context.EnsureMethod(_componentKey, "classRemove", "classList.remove");
            _pipeline.Steps.Add(Reaction.Call(
                ComponentSource.Of(_componentKey), "classRemove",
                new System.Collections.Generic.List<ValueProducer> { ValueProducer.Literal(className) }));
            return _pipeline;
        }

        public PipelineBuilder<TModel> ToggleClass(string className)
        {
            _pipeline.Context.EnsureMethod(_componentKey, "classToggle", "classList.toggle");
            _pipeline.Steps.Add(Reaction.Call(
                ComponentSource.Of(_componentKey), "classToggle",
                new System.Collections.Generic.List<ValueProducer> { ValueProducer.Literal(className) }));
            return _pipeline;
        }

        public PipelineBuilder<TModel> SetText(string text)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "text", "textContent", Shape.String, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "text", ValueProducer.Literal(text)));
            return _pipeline;
        }

        public PipelineBuilder<TModel> SetText<TSource>(TSource source, Expression<Func<TSource, object>> path)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "text", "textContent", Shape.String, "write");
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "text",
                ValueProducer.Read(PayloadSource.Event(), eventPath)));
            return _pipeline;
        }

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

        public ElementBuilder<TModel> SetText<TProp>(TypedSource<TProp> source)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "text", "textContent", Shape.String, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "text",
                source.ToValueProducer()));
            return this;
        }

        public PipelineBuilder<TModel> SetHtml(string html)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "html", "innerHTML", Shape.String, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "html", ValueProducer.Literal(html)));
            return _pipeline;
        }

        public PipelineBuilder<TModel> SetHtml<TSource>(TSource source, Expression<Func<TSource, object>> path)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "html", "innerHTML", Shape.String, "write");
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "html",
                ValueProducer.Read(PayloadSource.Event(), eventPath)));
            return _pipeline;
        }

        public ElementBuilder<TModel> SetHtml<TProp>(TypedSource<TProp> source)
        {
            _pipeline.Context.EnsureProperty(_componentKey, "html", "innerHTML", Shape.String, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "html",
                source.ToValueProducer()));
            return this;
        }

        public PipelineBuilder<TModel> Show()
        {
            _pipeline.Context.EnsureProperty(_componentKey, "hidden", "hidden", Shape.Boolean, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "hidden", ValueProducer.Literal(false)));
            return _pipeline;
        }

        public PipelineBuilder<TModel> Hide()
        {
            _pipeline.Context.EnsureProperty(_componentKey, "hidden", "hidden", Shape.Boolean, "write");
            _pipeline.Steps.Add(Reaction.Set(
                ComponentSource.Of(_componentKey), "hidden", ValueProducer.Literal(true)));
            return _pipeline;
        }
    }
}
