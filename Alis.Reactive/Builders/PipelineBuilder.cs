using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds the sequence of commands that execute when a trigger fires: element mutations,
    /// event dispatches, HTTP calls, component interactions, and conditional logic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Received as the <c>p</c> parameter inside trigger callbacks:
    /// <c>t.DomReady(p =&gt; { p.Element("id").AddClass("x"); p.Dispatch("ready"); })</c>.
    /// </para>
    /// <para>
    /// Commands execute in declaration order. Conditions (<c>When</c>/<c>Then</c>/<c>Else</c>)
    /// and HTTP calls (<c>Get</c>/<c>Post</c>) create branching points that produce
    /// separate workflow segments.
    /// </para>
    /// </remarks>
    /// <typeparam name="TModel">The view model type, providing compile-time expression paths.</typeparam>
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        private enum PipelineMode { Sequential, Http, Parallel, Conditional }

        private readonly PlanAuthoringContext _authoring;
        private readonly WorkflowScope _scope;

        internal List<PlanAction> Actions { get; } = new List<PlanAction>();
        internal List<BranchCase>? ConditionalBranches { get; private set; }
        private HttpRequestBuilder<TModel>? _httpBuilder;
        private ParallelBuilder<TModel>? _parallelBuilder;
        private PipelineMode _mode = PipelineMode.Sequential;

        /// <summary>
        /// Completed workflow segments. When a new When() is called after a previous
        /// When().Then().Else() block, the current segment (commands + branches) is
        /// flushed here so both conditionals produce independent workflows.
        /// </summary>
        private List<PlanAction>? _segments;

        internal PipelineBuilder(PlanAuthoringContext authoring, WorkflowScope scope)
        {
            _authoring = authoring;
            _scope = scope;
        }

        internal PlanAuthoringContext Authoring => _authoring;
        internal WorkflowScope Scope => _scope;

        internal void AddAction(PlanAction action)
        {
            Actions.Add(action);
        }

        internal void SetElementProperty(string elementId, string propertyPath, object? value, string? coerceAs = null)
        {
            AddAction(_authoring.CreateSetActionForElement(
                elementId,
                vendor: null,
                memberPath: propertyPath,
                literal: value,
                coerceAs: coerceAs));
        }

        internal void SetElementProperty(string elementId, string propertyPath, ValueExpr valueExpr, string? valueCoerceAs = null, string? coerceAs = null)
        {
            AddAction(_authoring.CreateSetActionForElement(
                elementId,
                vendor: null,
                propertyPath,
                valueExpr: valueExpr,
                valueCoerceAs: valueCoerceAs,
                coerceAs: coerceAs));
        }

        internal void SetElementPropertyFromPath(string elementId, string propertyPath, string valueMemberPath, string? coerceAs = null)
        {
            SetElementProperty(elementId, propertyPath, _authoring.CreateContextValue(valueMemberPath), coerceAs: coerceAs);
        }

        internal void CallElementMember(string elementId, string memberPath, params object?[] args)
        {
            AddAction(_authoring.CreateCallActionForElement(
                elementId,
                vendor: null,
                memberPath,
                BuildLiteralArguments(args),
                BuildLiteralArgumentShapes(args)));
        }

        internal void CallElementMemberFromPath(string elementId, string memberPath, string valueMemberPath, string? valueCoerceAs = null)
        {
            AddAction(_authoring.CreateCallActionForElement(
                elementId,
                vendor: null,
                memberPath: memberPath,
                args: new List<ValueExpr> { _authoring.CreateContextValue(valueMemberPath) },
                argShapes: new List<ValueShape> { PlanAuthoringContext.ResolveAssignedShape(valueCoerceAs, literal: null, coerceAs: null) }));
        }

        internal void SetComponentProperty(string componentId, string vendor, string propertyPath, object? value, string? coerceAs = null)
        {
            AddAction(_authoring.CreateSetActionForElement(
                componentId,
                vendor,
                memberPath: propertyPath,
                literal: value,
                coerceAs: coerceAs));
        }

        internal void SetComponentProperty(string componentId, string vendor, string propertyPath, ValueExpr valueExpr, string? valueCoerceAs = null, string? coerceAs = null)
        {
            AddAction(_authoring.CreateSetActionForElement(
                componentId,
                vendor,
                propertyPath,
                valueExpr: valueExpr,
                valueCoerceAs: valueCoerceAs,
                coerceAs: coerceAs));
        }

        internal void SetComponentPropertyFromPath(string componentId, string vendor, string propertyPath, string valueMemberPath, string? coerceAs = null)
        {
            SetComponentProperty(componentId, vendor, propertyPath, _authoring.CreateContextValue(valueMemberPath), coerceAs: coerceAs);
        }

        internal void CallComponentMember(string componentId, string vendor, string memberPath, params object?[] args)
        {
            AddAction(_authoring.CreateCallActionForElement(
                componentId,
                vendor,
                memberPath,
                BuildLiteralArguments(args),
                BuildLiteralArgumentShapes(args)));
        }

        internal void CallComponentMemberFromPath(string componentId, string vendor, string memberPath, string valueMemberPath, string? valueCoerceAs = null)
        {
            AddAction(_authoring.CreateCallActionForElement(
                componentId,
                vendor,
                memberPath: memberPath,
                args: new List<ValueExpr> { _authoring.CreateContextValue(valueMemberPath) },
                argShapes: new List<ValueShape> { PlanAuthoringContext.ResolveAssignedShape(valueCoerceAs, literal: null, coerceAs: null) }));
        }

        internal void SetEventProperty(string propertyPath, object? value, string? coerceAs = null)
        {
            AddAction(_authoring.CreateSetActionForEvent(_scope, propertyPath, literal: value, coerceAs: coerceAs));
        }

        internal void SetEventProperty(string propertyPath, ValueExpr valueExpr, string? valueCoerceAs = null, string? coerceAs = null)
        {
            AddAction(_authoring.CreateSetActionForEvent(
                _scope,
                propertyPath,
                valueExpr: valueExpr,
                valueCoerceAs: valueCoerceAs,
                coerceAs: coerceAs));
        }

        internal void CallEventMember(string memberPath, params object?[] args)
        {
            AddAction(_authoring.CreateCallActionForEvent(
                _scope,
                memberPath,
                BuildLiteralArguments(args),
                BuildLiteralArgumentShapes(args)));
        }

        internal void CallEventMemberFromPath(string memberPath, string valueMemberPath, string? valueCoerceAs = null)
        {
            AddAction(_authoring.CreateCallActionForEvent(
                _scope,
                memberPath: memberPath,
                args: new List<ValueExpr> { _authoring.CreateContextValue(valueMemberPath) },
                argShapes: new List<ValueShape> { PlanAuthoringContext.ResolveAssignedShape(valueCoerceAs, literal: null, coerceAs: null) }));
        }

        /// <summary>
        /// Fires a custom event in the browser that other triggers can listen for.
        /// </summary>
        /// <param name="eventName">The event name (e.g. <c>"order-submitted"</c>).</param>
        /// <returns>This builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            Actions.Add(new DispatchAction(eventName));
            return this;
        }

        /// <summary>
        /// Fires a custom event with a payload object in the browser.
        /// </summary>
        /// <typeparam name="TPayload">The payload type, serialized as the event's detail data.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <param name="payload">The data to attach to the event.</param>
        /// <returns>This builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            var action = new DispatchAction(eventName)
            {
                Detail = new LiteralValueExpr(payload)
            };

            Actions.Add(action);
            return this;
        }

        /// <summary>
        /// Targets a DOM element by its ID for mutations (CSS classes, text, visibility).
        /// </summary>
        /// <remarks>
        /// Use <c>Element()</c> for non-input display elements. For input components bound to
        /// a model property, use <see cref="Component{TComponent}(Expression{Func{TModel, object}})"/> instead.
        /// </remarks>
        /// <param name="elementId">The HTML element ID.</param>
        /// <returns>An element builder for chaining mutations like <c>AddClass</c>, <c>SetText</c>, <c>Show</c>.</returns>
        public ElementBuilder<TModel> Element(string elementId)
        {
            return new ElementBuilder<TModel>(this, elementId);
        }

        // ── Component<T>() — 3 overloads ──

        /// <summary>
        /// Targets a component by model expression (input components bound to a model property).
        /// </summary>
        /// <remarks>
        /// Available component types include:
        /// <b>Native</b> — <c>NativeTextBox</c>, <c>NativeCheckBox</c>, <c>NativeHiddenField</c>,
        /// <c>NativeSelect</c>, <c>NativeButton</c>, <c>NativeRadioButton</c>.
        /// <b>Fusion</b> — <c>FusionDropDownList</c>, <c>FusionNumericTextBox</c>,
        /// <c>FusionDatePicker</c>, <c>FusionTimePicker</c>, <c>FusionSwitch</c>,
        /// <c>FusionAutoComplete</c>, <c>FusionColorPicker</c>, <c>FusionInputMask</c>,
        /// <c>FusionMultiSelect</c>, <c>FusionRichTextEditor</c>, <c>FusionFileUpload</c>,
        /// <c>FusionMultiColumnComboBox</c>, <c>FusionDateTimePicker</c>,
        /// <c>FusionDateRangePicker</c>.
        /// </remarks>
        /// <typeparam name="TComponent">The component type (implements <see cref="IComponent"/> with <c>new()</c>).</typeparam>
        /// <param name="expr">The model property expression (e.g. <c>m =&gt; m.Address.City</c>).</param>
        /// <returns>A component reference for chaining mutations like <c>SetValue</c> or <c>Focus</c>.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(
            Expression<Func<TModel, object?>> expr)
            where TComponent : IComponent, new()
        {
            var elementId = IdGenerator.For<TModel>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>
        /// Targets a component from a different model (cross-plan component reference).
        /// </summary>
        /// <remarks>
        /// Uses <see cref="IdGenerator.For{TModel}(Expression{Func{TModel, object}})"/> with
        /// <typeparamref name="TOtherModel"/> to produce the correct element ID.
        /// Example: <c>p.Component&lt;NativeHiddenField, Step2Model&gt;(m =&gt; m.Diagnosis).SetValue(...)</c>.
        /// </remarks>
        /// <typeparam name="TComponent">The component type.</typeparam>
        /// <typeparam name="TOtherModel">The other view's model type.</typeparam>
        /// <param name="expr">The model property expression on the other model.</param>
        /// <returns>A component reference for chaining mutations.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent, TOtherModel>(
            Expression<Func<TOtherModel, object?>> expr)
            where TComponent : IComponent, new()
            where TOtherModel : class
        {
            var elementId = IdGenerator.For<TOtherModel>(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>Targets a component by its string ID (non-input components).</summary>
        /// <typeparam name="TComponent">The component type.</typeparam>
        /// <param name="refId">The HTML element ID of the component.</param>
        /// <returns>A component reference for chaining mutations.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
            where TComponent : IComponent, new()
        {
            return new ComponentRef<TComponent, TModel>(refId, this);
        }

        /// <summary>Targets an app-level component by its default ID (e.g. <c>FusionConfirm</c>).</summary>
        /// <typeparam name="TComponent">The app-level component type.</typeparam>
        /// <returns>A component reference for chaining mutations.</returns>
        public ComponentRef<TComponent, TModel> Component<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var comp = new TComponent();
            return new ComponentRef<TComponent, TModel>(comp.DefaultId, this);
        }

        /// <summary>
        /// Displays server-side validation errors returned in the 400 response body
        /// at the correct form fields.
        /// </summary>
        /// <remarks>
        /// Typically used inside a <c>.Response(r =&gt; r.OnError(400, ...))</c> handler.
        /// Pair with <see cref="Requests.HttpRequestBuilder{TModel}.Validate{TValidator}"/>
        /// for client-side validation before the request fires.
        /// </remarks>
        /// <param name="formId">The form element ID to scope error display to.</param>
        /// <returns>This builder for chaining additional commands.</returns>
        public PipelineBuilder<TModel> ValidationErrors(string formId)
        {
            Actions.Add(new ShowValidationErrorsAction(formId));
            return this;
        }

        /// <summary>
        /// Injects the HTTP response body as inner HTML of the target element.
        /// </summary>
        /// <remarks>
        /// Typically used inside a <c>.Response(r =&gt; r.OnSuccess(...))</c> handler for
        /// loading partial views:
        /// <c>p.Get("/url").Response(response: r =&gt; r.OnSuccess(pipeline: s =&gt; s.Into("container")))</c>.
        /// </remarks>
        /// <param name="elementId">The HTML element ID to inject content into.</param>
        /// <returns>This builder for chaining additional commands.</returns>
        /// <seealso cref="ValidationErrors"/>
        public PipelineBuilder<TModel> Into(string elementId)
        {
            Actions.Add(new InjectAction(_authoring.EnsureElementObjectForAction(elementId)));
            return this;
        }

        internal void SetConditionalBranches(List<BranchCase> branches)
        {
            ConditionalBranches = branches;
        }

        private static List<ValueExpr>? BuildLiteralArguments(object?[] args)
        {
            if (args.Length == 0)
                return null;

            var values = new List<ValueExpr>(args.Length);
            foreach (var arg in args)
                values.Add(new LiteralValueExpr(arg));

            return values;
        }

        private static List<ValueShape>? BuildLiteralArgumentShapes(object?[] args)
        {
            if (args.Length == 0)
                return null;

            var shapes = new List<ValueShape>(args.Length);
            foreach (var arg in args)
                shapes.Add(PlanAuthoringContext.InferShape(arg));

            return shapes;
        }

        /// <summary>
        /// Flushes the current segment (accumulated commands + conditional branches)
        /// into _segments, then resets for the next segment. Called by When() when
        /// a previous conditional block already exists.
        /// </summary>
        internal void FlushSegment()
        {
            _segments ??= new List<PlanAction>();

            if (_mode == PipelineMode.Http && _httpBuilder != null)
            {
                var requestAction = new RequestAction(_httpBuilder.BuildRequestPlan());
                if (Actions.Count > 0)
                {
                    var steps = new List<PlanAction>(Actions) { requestAction };
                    _segments.Add(PlanAuthoringContext.SequenceOrSingle(steps));
                }
                else
                {
                    _segments.Add(requestAction);
                }

                Actions.Clear();
                _httpBuilder = null;
            }
            else if (_mode == PipelineMode.Parallel && _parallelBuilder != null)
            {
                var parallelAction = _parallelBuilder.BuildAction();
                if (Actions.Count > 0)
                {
                    var steps = new List<PlanAction>(Actions) { parallelAction };
                    _segments.Add(PlanAuthoringContext.SequenceOrSingle(steps));
                }
                else
                {
                    _segments.Add(parallelAction);
                }

                Actions.Clear();
                _parallelBuilder = null;
            }
            else
            {
                if (Actions.Count > 0)
                {
                    _segments.Add(PlanAuthoringContext.SequenceOrSingle(new List<PlanAction>(Actions)));
                    Actions.Clear();
                }
            }

            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
            {
                _segments.Add(new BranchAction(new List<BranchCase>(ConditionalBranches)));
                ConditionalBranches = null;
            }

            _mode = PipelineMode.Sequential;
        }

        /// <summary>
        /// Returns the single action for this pipeline.
        /// </summary>
        /// <remarks>
        /// Throws if the pipeline produced multiple segments. Callers that
        /// need multi-segment support must use <see cref="BuildActions"/> instead.
        /// </remarks>
        /// <returns>The single action built from the pipeline commands.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the pipeline contains multiple action segments.</exception>
        internal PlanAction BuildAction()
        {
            var actions = BuildActions();
            if (actions.Count > 1)
                throw new InvalidOperationException(
                    $"BuildAction() requires exactly one action segment but found {actions.Count}. " +
                    "Use BuildActions() for pipelines with multiple When() blocks.");

            return actions[0];
        }

        /// <summary>
        /// Builds all action segments from the pipeline. A single When() block produces
        /// one action. Multiple When() blocks produce multiple actions.
        /// Commands between or around conditions produce sequential actions.
        /// </summary>
        /// <returns>All action segments built from the pipeline commands.</returns>
        internal List<PlanAction> BuildActions()
        {
            if (_segments == null || _segments.Count == 0)
            {
                return new List<PlanAction> { BuildSingleAction() };
            }

            FlushSegment();

            return _segments;
        }

        private PlanAction BuildSingleAction()
        {
            return _mode switch
            {
                PipelineMode.Parallel => Actions.Count > 0
                    ? PlanAuthoringContext.SequenceOrSingle(new List<PlanAction>(Actions)
                    {
                        _parallelBuilder!.BuildAction()
                    })
                    : (PlanAction)_parallelBuilder!.BuildAction(),

                PipelineMode.Http => Actions.Count > 0
                    ? PlanAuthoringContext.SequenceOrSingle(new List<PlanAction>(Actions)
                    {
                        new RequestAction(_httpBuilder!.BuildRequestPlan())
                    })
                    : (PlanAction)new RequestAction(_httpBuilder!.BuildRequestPlan()),

                PipelineMode.Conditional => Actions.Count > 0
                    ? PlanAuthoringContext.SequenceOrSingle(new List<PlanAction>(Actions)
                    {
                        new BranchAction(ConditionalBranches!)
                    })
                    : (PlanAction)new BranchAction(ConditionalBranches!),

                _ => PlanAuthoringContext.SequenceOrSingle(Actions),
            };
        }
    }
}
