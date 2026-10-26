using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The plan a view is building: the elements it names, the handlers its triggers start, the forms
    /// its requests validate and the callback a step lands in. <see cref="Write"/> writes the grammar
    /// the runtime runs.
    /// </summary>
    internal sealed class PlanBuildContext
    {
        private const string EventParam = "e";

        private readonly string _planId;
        private readonly RenderedInputs _inputs;
        private readonly List<DeclaredObject> _declared = new List<DeclaredObject>();
        private readonly Dictionary<string, DeclaredObject> _declaredById = new Dictionary<string, DeclaredObject>(StringComparer.Ordinal);
        private readonly List<(Func<ObjectTable, string> On, IReadOnlyList<string> Path, string Event, Block Body)> _handlers =
            new List<(Func<ObjectTable, string>, IReadOnlyList<string>, string, Block)>();
        private readonly HashSet<string> _plugins = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<(RequestStep Request, FormValidation Validation)> _validatedRequests =
            new List<(RequestStep, FormValidation)>();
        private readonly Dictionary<string, FormChecks> _forms = new Dictionary<string, FormChecks>(StringComparer.Ordinal);

        // The callbacks being built, innermost last: a step lands in the innermost one.
        private readonly Stack<Block> _blocks = new Stack<Block>();

        internal PlanBuildContext(string planId, RenderedInputs inputs)
        {
            _planId = planId ?? throw new ArgumentNullException(nameof(planId));
            _inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
        }

        // ── callbacks ────────────────────────────────────────────────────────────

        /// <summary>Builds a callback: every step written while it runs lands in the returned block.</summary>
        internal Block Build(Action callback)
        {
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
        internal void On(Func<ObjectTable, string> on, IReadOnlyList<string> path, string eventName, Block body) =>
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
        /// validated again keeps its place with the new validation.
        /// </summary>
        internal void ValidateBefore(RequestStep request, FormValidation validation)
        {
            var index = _validatedRequests.FindIndex(validated => validated.Request == request);
            if (index < 0) _validatedRequests.Add((request, validation));
            else _validatedRequests[index] = (request, validation);
        }

        internal IReadOnlyList<(RequestStep Request, FormValidation Validation)> ValidatedRequests => _validatedRequests;

        /// <summary>The inputs a request's validator checks on the form, merged into the form's checks.</summary>
        internal void Check(string formId, IEnumerable<CheckedField> fields)
        {
            DeclareElement(formId);
            if (!_forms.TryGetValue(formId, out var form))
                _forms.Add(formId, form = new FormChecks());
            form.Merge(fields);
        }

        // ── writing ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Writes the plan: every element by id and vendor, the inputs <c>alis.model</c> binds, each
        /// validated form's rules as <c>alis.form</c> data, then each handler.
        /// </summary>
        internal GrammarPlan Write()
        {
            var objects = new ObjectTable(_planId, _declared);
            var plan = new PlanWriter(_planId, objects);

            var inputs = ModelInputs(objects);
            if (inputs.Count > 0) objects.Model(new ModelPluginData(inputs));

            var forms = new FormWriter(Scope.ForHandler(plan, "$form"));
            foreach (var declared in _declared)
                if (_forms.TryGetValue(declared.Id, out var form))
                    objects.Form(declared.Id, forms.Form(form.Fields));

            var handlers = _handlers
                .Select(handler =>
                {
                    var body = handler.Body.Write(Scope.ForHandler(plan, EventParam));
                    return new Handler(handler.On(objects), handler.Path, handler.Event, EventParam, body);
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
        private IReadOnlyDictionary<string, InputBinding> ModelInputs(ObjectTable objects)
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
