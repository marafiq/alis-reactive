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

        public PipelineBuilder<TModel> SetHtml(string html)
        {
            _pipeline.Commands.Add(new MutateElementCommand(_elementId, "set-html", html));
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
