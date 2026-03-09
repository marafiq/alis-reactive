using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
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
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "el.classList.add(val)", className));
            return _pipeline;
        }

        public PipelineBuilder<TModel> RemoveClass(string className)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "el.classList.remove(val)", className));
            return _pipeline;
        }

        public PipelineBuilder<TModel> ToggleClass(string className)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "el.classList.toggle(val)", className));
            return _pipeline;
        }

        public PipelineBuilder<TModel> SetText(string text)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "el.textContent = val", text));
            return _pipeline;
        }

        /// <summary>
        /// Sets the element text from an event payload property resolved at runtime.
        /// The source instance is used only for generic type inference — its value is ignored.
        /// </summary>
        public PipelineBuilder<TModel> SetText<TSource>(TSource source, Expression<Func<TSource, object?>> path)
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "el.textContent = val", source: sourcePath));
            return _pipeline;
        }

        public PipelineBuilder<TModel> SetHtml(string html)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "el.innerHTML = val", html));
            return _pipeline;
        }

        /// <summary>
        /// Sets the element HTML from an event payload property resolved at runtime.
        /// </summary>
        public PipelineBuilder<TModel> SetHtml<TSource>(TSource source, Expression<Func<TSource, object?>> path)
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "el.innerHTML = val", source: sourcePath));
            return _pipeline;
        }

        public PipelineBuilder<TModel> Show()
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "el.removeAttribute('hidden')"));
            return _pipeline;
        }

        public PipelineBuilder<TModel> Hide()
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "el.setAttribute('hidden','')"));
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
                _pipeline.Commands[_pipeline.Commands.Count - 1].When = gb.Guard;
            return this;
        }
    }
}
