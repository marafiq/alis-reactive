using System;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Builders
{
    public class ElementBuilder<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel> _pipeline;
        private readonly string _elementId;

        internal ElementBuilder(PipelineBuilder<TModel> pipeline, string elementId)
        {
            _pipeline = pipeline;
            _elementId = elementId;
        }

        public PipelineBuilder<TModel> AddClass(string className)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "add-class", className));
            return _pipeline;
        }

        public PipelineBuilder<TModel> RemoveClass(string className)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "remove-class", className));
            return _pipeline;
        }

        public PipelineBuilder<TModel> ToggleClass(string className)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "toggle-class", className));
            return _pipeline;
        }

        public PipelineBuilder<TModel> SetText(string text)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "set-text", text));
            return _pipeline;
        }

        /// <summary>
        /// Sets the element text from an event payload property resolved at runtime.
        /// The source instance is used only for generic type inference — its value is ignored.
        /// </summary>
        public PipelineBuilder<TModel> SetText<TSource>(TSource source, Expression<Func<TSource, object?>> path)
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "set-text", source: sourcePath));
            return _pipeline;
        }

        public PipelineBuilder<TModel> SetHtml(string html)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "set-html", html));
            return _pipeline;
        }

        /// <summary>
        /// Sets the element HTML from an event payload property resolved at runtime.
        /// </summary>
        public PipelineBuilder<TModel> SetHtml<TSource>(TSource source, Expression<Func<TSource, object?>> path)
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "set-html", source: sourcePath));
            return _pipeline;
        }

        public PipelineBuilder<TModel> Show()
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "show"));
            return _pipeline;
        }

        public PipelineBuilder<TModel> Hide()
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "hide"));
            return _pipeline;
        }
    }
}
