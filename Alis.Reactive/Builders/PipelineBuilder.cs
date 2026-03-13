using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Builders
{
    public class PipelineBuilder<TModel> where TModel : class
    {
        private enum PipelineMode { Sequential, Http, Parallel, Conditional }

        internal List<Command> Commands { get; } = new List<Command>();
        internal List<Branch>? ConditionalBranches { get; private set; }
        private HttpRequestBuilder<TModel>? _httpBuilder;
        private ParallelBuilder<TModel>? _parallelBuilder;
        private PipelineMode _mode = PipelineMode.Sequential;

        /// <summary>
        /// Adds a command to the pipeline. Used by vendor-specific projects
        /// (Fusion, Native) to emit their own command descriptors.
        /// </summary>
        public void AddCommand(Command command)
        {
            Commands.Add(command);
        }

        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            Commands.Add(new DispatchCommand(eventName));
            return this;
        }

        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            Commands.Add(new DispatchCommand(eventName, payload));
            return this;
        }

        public ElementBuilder<TModel> Element(string elementId)
        {
            return new ElementBuilder<TModel>(this, elementId);
        }

        // ── Component<T>() — 3 overloads ──

        /// <summary>
        /// Resolve component by model expression (input components bound to model).
        /// Target ID uses underscore separator matching Html.IdFor() convention:
        /// m => m.Address.City → target "Address_City".
        /// </summary>
        public ComponentRef<TComponent, TModel> Component<TComponent>(
            Expression<Func<TModel, object?>> expr)
            where TComponent : IComponent, new()
        {
            var elementId = IdGenerator.For<TModel>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>Resolve component by string ref (non-input components by ID).</summary>
        public ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
            where TComponent : IComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(refId, this);
        }

        /// <summary>Resolve app-level component by its default ID (e.g., FusionConfirm).</summary>
        public ComponentRef<TComponent, TModel> Component<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var comp = new TComponent();
            return new ComponentRef<TComponent, TModel>(comp.DefaultId, this);
        }

        // ── HTTP Request Methods ──

        /// <summary>Starts a GET request to the given URL.</summary>
        public HttpRequestBuilder<TModel> Get(string url)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>();
            _httpBuilder.SetVerb("GET").SetUrl(url);
            return _httpBuilder;
        }

        /// <summary>Starts a POST request to the given URL.</summary>
        public HttpRequestBuilder<TModel> Post(string url)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>();
            _httpBuilder.SetVerb("POST").SetUrl(url);
            return _httpBuilder;
        }

        /// <summary>Starts a POST request with a gather configuration.</summary>
        public HttpRequestBuilder<TModel> Post(string url, Action<GatherBuilder<TModel>> gather)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>();
            _httpBuilder.SetVerb("POST").SetUrl(url);
            _httpBuilder.Gather(gather);
            return _httpBuilder;
        }

        /// <summary>Starts a PUT request with a gather configuration.</summary>
        public HttpRequestBuilder<TModel> Put(string url, Action<GatherBuilder<TModel>> gather)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>();
            _httpBuilder.SetVerb("PUT").SetUrl(url);
            _httpBuilder.Gather(gather);
            return _httpBuilder;
        }

        /// <summary>Starts a DELETE request to the given URL.</summary>
        public HttpRequestBuilder<TModel> Delete(string url)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>();
            _httpBuilder.SetVerb("DELETE").SetUrl(url);
            return _httpBuilder;
        }

        /// <summary>Starts parallel HTTP requests that fire concurrently.</summary>
        public ParallelBuilder<TModel> Parallel(params Action<HttpRequestBuilder<TModel>>[] branches)
        {
            SetMode(PipelineMode.Parallel);
            _parallelBuilder = new ParallelBuilder<TModel>();
            foreach (var branch in branches)
            {
                _parallelBuilder.AddBranch(branch);
            }
            return _parallelBuilder;
        }

        private void SetMode(PipelineMode mode)
        {
            if (_mode != PipelineMode.Sequential && _mode != mode)
                throw new InvalidOperationException(
                    $"Cannot switch to {mode} — pipeline is already in {_mode} mode.");
            _mode = mode;
        }

        /// <summary>
        /// Starts a conditional branch on an event payload property.
        /// TProp is inferred from the expression — operators on the returned builder
        /// demand TProp operands (e.g. int property → Gte(int), string → Eq(string)).
        /// </summary>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            SetMode(PipelineMode.Conditional);

            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Starts a conditional branch on a component property.
        /// TProp is inferred from the TypedSource — operators on the returned builder
        /// demand TProp operands (e.g. decimal property → Gt(0m)).
        /// Usage: p.When(comp.ReadProperty&lt;decimal&gt;("value")).Gt(0m).Then(...)
        /// </summary>
        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            SetMode(PipelineMode.Conditional);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Starts a Confirm guard — an async halting condition that pauses the pipeline
        /// and shows a dialog to the user.
        /// </summary>
        public GuardBuilder<TModel> Confirm(string message)
        {
            SetMode(PipelineMode.Conditional);

            return new GuardBuilder<TModel>(new ConfirmGuard(message), this);
        }

        /// <summary>
        /// Adds a validation-errors command — displays server-side validation errors
        /// returned in the 400 response body at the correct form fields.
        /// </summary>
        public PipelineBuilder<TModel> ValidationErrors(string formId)
        {
            Commands.Add(new ValidationErrorsCommand(formId));
            return this;
        }

        /// <summary>
        /// Injects the HTTP response body as innerHTML of the target element.
        /// Used for loading partial views: p.Get("/url").Response(r => r.OnSuccess(s => s.Into("container")))
        /// </summary>
        public PipelineBuilder<TModel> Into(string elementId)
        {
            Commands.Add(new IntoCommand(elementId));
            return this;
        }

        internal void SetConditionalBranches(List<Branch> branches)
        {
            ConditionalBranches = branches;
        }

        public Reaction BuildReaction()
        {
            return _mode switch
            {
                PipelineMode.Parallel => _parallelBuilder!.BuildReaction(
                    Commands.Count > 0 ? Commands : null),
                PipelineMode.Http => new HttpReaction(
                    Commands.Count > 0 ? Commands : null,
                    _httpBuilder!.BuildRequestDescriptor()),
                PipelineMode.Conditional => new ConditionalReaction(
                    ConditionalBranches!.ToArray()),
                _ => new SequentialReaction(Commands),
            };
        }
    }
}
