using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The plan a view is building: the elements it names, the handlers its triggers start, the forms
    /// its requests validate or show the server's errors on, and the callback a step lands in.
    /// <see cref="Write"/> writes the grammar the runtime runs.
    /// </summary>
    internal sealed class PlanBuildContext
    {

        private readonly string _planId;
        private readonly RenderedInputs _inputs;
        private readonly List<DeclaredObject> _declared = new List<DeclaredObject>();
        private readonly Dictionary<string, DeclaredObject> _declaredById = new Dictionary<string, DeclaredObject>(StringComparer.Ordinal);
        private readonly List<(Func<PlanObjects, string> On, IReadOnlyList<string> Path, string Event, Block Body)> _handlers =
            new List<(Func<PlanObjects, string>, IReadOnlyList<string>, string, Block)>();
        private readonly HashSet<string> _plugins = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<(RequestStep Request, FormValidation Validation)> _validatedRequests =
            new List<(RequestStep, FormValidation)>();
        // The forms of this plan by container id, with the inputs each places messages on. A form an error
        // route shows the server's errors on joins when the route is written, a form a request validates when
        // the plan renders; every form's fields are bound when the plan renders (Check).
        private readonly Dictionary<string, IReadOnlyList<CheckedField>> _forms =
            new Dictionary<string, IReadOnlyList<CheckedField>>(StringComparer.Ordinal);

        // The callbacks being built, innermost last: a step lands in the innermost one.
        private readonly Stack<Block> _blocks = new Stack<Block>();

        internal PlanBuildContext(string planId, RenderedInputs inputs, string fieldPrefix, string view)
        {
            _planId = planId ?? throw new ArgumentNullException(nameof(planId));
            _inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
            FieldPrefix = fieldPrefix ?? throw new ArgumentNullException(nameof(fieldPrefix));
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        /// <summary>The MVC field prefix the plan's view renders under; empty for a page's own view.</summary>
        internal string FieldPrefix { get; }

        /// <summary>The path of the view that started the plan, which what the plan refuses names.</summary>
        internal string View { get; }

        /// <summary>Whether the plan has rendered: from then on it takes no input and no behavior, and renders no more.</summary>
        internal bool Rendered { get; private set; }

        // ── rendering ────────────────────────────────────────────────────────────

        /// <summary>The plan renders, once.</summary>
        internal void Seal()
        {
            if (Rendered)
                throw new InvalidOperationException(
                    $"The plan of {View} is rendered twice, so every handler it holds would run twice: render it once, " +
                    "after the last behavior written to it.");
            Rendered = true;
        }

        /// <summary>An input the view rendered joins the plan, while the plan has not rendered.</summary>
        internal void AddInput(RenderedInput input)
        {
            RefuseOnceRendered($"Input '{input.BindingPath}'", "the input");
            _inputs.Add(input);
        }

        // Razor runs a view's sections after its body, when the layout renders them: what a section writes to a plan
        // the body rendered never reaches the page.
        private void RefuseOnceRendered(string written, string what)
        {
            if (!Rendered) return;
            throw new InvalidOperationException(
                $"{written} is written after the plan of {View} rendered, so the plan the page runs does not hold it. " +
                "Razor runs a @section after the view's body: render the plan in a section the layout renders after the " +
                $"others, such as @section Scripts {{ @Html.RenderPlan(plan) }}, or write {what} in the body.");
        }

        // ── callbacks ────────────────────────────────────────────────────────────

        /// <summary>Builds a callback: every step written while it runs lands in the returned block.</summary>
        internal Block Build(Action callback)
        {
            RefuseOnceRendered("Behavior", "the behavior");
            var block = new Block();
            _blocks.Push(block);
            try
            {
                callback();
            }
            finally
            {
                _blocks.Pop();
            }

            return block;
        }

        /// <summary>The callback being built, where a step written now lands.</summary>
        internal Block ActiveBlock
        {
            get
            {
                if (_blocks.Count == 0)
                    throw new InvalidOperationException(
                        "A reaction step was written outside any callback. Write it inside a trigger, a component's " +
                        ".Reactive, Then, Else, OnSuccess, OnError, WhileLoading, Finally or OnAllSettled callback.");
                return _blocks.Peek();
            }
        }

        internal void AddStep(Step step) => ActiveBlock.Add(step);

        /// <summary>Runs <paramref name="body"/> each time an object the plan names raises <paramref name="eventName"/>.</summary>
        internal void On(Func<PlanObjects, string> on, IReadOnlyList<string> path, string eventName, Block body) =>
            _handlers.Add((on, path, eventName, body));

        // ── objects ──────────────────────────────────────────────────────────────

        /// <summary>An element the view rendered with this id, found by the browser itself unless a component claims it.</summary>
        internal void DeclareElement(string elementId)
        {
            if (string.IsNullOrWhiteSpace(elementId)) throw new ArgumentException("Element ID required.", nameof(elementId));
            if (!_declaredById.ContainsKey(elementId)) Add(new DeclaredObject(elementId, ComponentVendor.Native));
        }

        /// <summary>A component on the element with this id, from one vendor for the whole plan.</summary>
        internal void DeclareComponent(string componentId, ComponentVendor vendor)
        {
            if (string.IsNullOrWhiteSpace(componentId)) throw new ArgumentException("Component ID required.", nameof(componentId));
            if (_declaredById.TryGetValue(componentId, out var existing))
            {
                if (existing.Vendor.Name != vendor.Name)
                    throw new InvalidOperationException(
                        $"Component '{componentId}' registered as vendor '{existing.Vendor.Name}' " +
                        $"but re-referenced as '{vendor.Name}'. A component cannot change vendor.");
                return;
            }

            Add(new DeclaredObject(componentId, vendor));
        }

        internal void RegisterPlugin(string name)
        {
            if (!_plugins.Add(name))
                throw new InvalidOperationException($"Plugin '{name}' is already registered.");
        }

        private void Add(DeclaredObject declared)
        {
            _declared.Add(declared);
            _declaredById.Add(declared.Id, declared);
        }

        // ── validation ───────────────────────────────────────────────────────────

        /// <summary>
        /// A request that checks its form first; the form's rules are bound when the plan is written. A request
        /// validated again keeps its place with the new validation. A form is validated by one validator, so a
        /// request that names another for a form another request validates is refused.
        /// </summary>
        internal void ValidateBefore(RequestStep request, FormValidation validation)
        {
            foreach (var (other, otherValidation) in _validatedRequests)
            {
                if (other == request || otherValidation.Form != validation.Form || otherValidation.Validator == validation.Validator)
                    continue;
                throw new InvalidOperationException(
                    $"Form '{validation.Form}' is validated by {CSharpTypeName.Of(otherValidation.Validator)} and by " +
                    $"{CSharpTypeName.Of(validation.Validator)}, but a form takes one validator. Give each validator a " +
                    "form of its own, or write one validator whose rules differ by When conditions.");
            }

            var index = _validatedRequests.FindIndex(validated => validated.Request == request);
            if (index < 0) _validatedRequests.Add((request, validation));
            else _validatedRequests[index] = (request, validation);
        }

        internal IReadOnlyList<(RequestStep Request, FormValidation Validation)> ValidatedRequests => _validatedRequests;

        /// <summary>
        /// A form an error route shows the server's errors on is a form of the plan, as a validated form is: the
        /// plan writes its fields, so each message is placed by the binding name the server reports it under.
        /// </summary>
        internal void ShowErrorsOn(string formId)
        {
            DeclareElement(formId);
            if (!_forms.ContainsKey(formId)) _forms.Add(formId, Array.Empty<CheckedField>());
        }

        /// <summary>The forms of this plan: each a request validates, and each an error route shows the server's errors on.</summary>
        internal IReadOnlyList<string> Forms =>
            _validatedRequests
                .Select(validated => validated.Validation.Form)
                .Concat(_forms.Keys)
                .Distinct(StringComparer.Ordinal)
                .ToList();

        /// <summary>
        /// The inputs a form places messages on, with its one validator's checks when a request validates it,
        /// bound again each time the plan renders.
        /// </summary>
        internal void Check(string formId, IReadOnlyList<CheckedField> fields)
        {
            DeclareElement(formId);
            _forms[formId] = fields;
        }

        // ── writing ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Writes the plan: every element by id and vendor, the inputs <c>alis.model</c> binds, each form's
        /// fields and their checks as <c>alis.form</c> data, then each handler.
        /// </summary>
        internal GrammarPlan Write()
        {
            var objects = new PlanObjects(_planId, _declared);
            var plan = new PlanWriter(_planId, objects);

            var inputs = ModelInputs(objects);
            if (inputs.Count > 0) objects.Model(new ModelPluginData(inputs));

            var forms = new FormWriter(Scope.ForHandler(plan, "$form"));
            foreach (var declared in _declared)
                if (_forms.TryGetValue(declared.Id, out var fields))
                    objects.Form(declared.Id, forms.Form(fields));

            var handlers = _handlers
                .Select(handler =>
                {
                    var body = handler.Body.Write(Scope.ForHandler(plan, Handler.Payload));
                    return new Handler(handler.On(objects), handler.Path, handler.Event, body);
                })
                .ToList();

            return new GrammarPlan(_planId, objects.Vendors(), objects.Snapshot(), handlers);
        }

        /// <summary>Every input the view rendered is an element of the plan, after the elements its steps name.</summary>
        internal void DeclareRenderedInputs()
        {
            foreach (var input in _inputs.Entries)
                DeclareComponent(input.Value.ComponentId, input.Value.ComponentVendor);
        }

        // Each input this plan rendered: its MVC name, its JSON body path, its value read in the model's
        // type and the event its component commits a value with.
        private IReadOnlyDictionary<string, InputBinding> ModelInputs(PlanObjects objects)
        {
            var inputs = new Dictionary<string, InputBinding>(StringComparer.Ordinal);
            foreach (var declared in _declared)
            {
                if (!_inputs.TryFindForComponent(ComponentId.Of(declared.Id), out var rendered)) continue;

                var value = G.Get(G.Obj(objects.Element(declared.Id)), rendered.Input.ValuePath.Text, rendered.Shape);
                inputs.Add(
                    declared.Id,
                    new InputBinding(rendered.BindingPath, rendered.Binding.Path.Body, value, rendered.Shape, rendered.Input.CommittedBy.Value));
            }

            return inputs;
        }

    }

    /// <summary>An element the plan names, with the vendor whose object it holds.</summary>
    internal sealed class DeclaredObject
    {
        internal DeclaredObject(string id, ComponentVendor vendor)
        {
            Id = id;
            Vendor = vendor;
        }

        internal string Id { get; }

        internal ComponentVendor Vendor { get; }
    }
}
