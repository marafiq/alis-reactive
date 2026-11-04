using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.Inputs;
using Alis.Reactive.Members;
using Alis.Reactive.PlanModel.Reactions;
using Alis.Reactive.PlanModel.Requests;
using Alis.Reactive.PlanModel.Validation;
using Alis.Reactive.PlanModel.Writing;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.PlanModel.Document
{
    internal sealed class PlanBuildContext
    {
        private readonly string _planId;
        private readonly RenderedInputs _inputs;
        private readonly ReactiveClassNames _classNames;
        private readonly List<DeclaredObject> _declared = new List<DeclaredObject>();
        private readonly Dictionary<string, DeclaredObject> _declaredById = new Dictionary<string, DeclaredObject>(StringComparer.Ordinal);
        private readonly List<(Func<PlanObjects, string> On, IReadOnlyList<string> Path, string Event, Block Body)> _handlers =
            new List<(Func<PlanObjects, string>, IReadOnlyList<string>, string, Block)>();
        private readonly HashSet<string> _plugins = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<(RequestStep Request, FormValidation Validation)> _validatedRequests =
            new List<(RequestStep, FormValidation)>();
        private readonly Dictionary<string, IReadOnlyList<CheckedField>> _forms =
            new Dictionary<string, IReadOnlyList<CheckedField>>(StringComparer.Ordinal);
        private readonly HashSet<string> _slots = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _contentSetById = new Dictionary<string, string>(StringComparer.Ordinal);

        private readonly Stack<Block> _blocks = new Stack<Block>();

        internal PlanBuildContext(string planId, RenderedInputs inputs, string fieldPrefix, string view, ReactiveClassNames classNames)
        {
            _planId = planId ?? throw new ArgumentNullException(nameof(planId));
            _inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
            FieldPrefix = fieldPrefix ?? throw new ArgumentNullException(nameof(fieldPrefix));
            View = view ?? throw new ArgumentNullException(nameof(view));
            _classNames = classNames ?? throw new ArgumentNullException(nameof(classNames));
        }

        internal string FieldPrefix { get; }

        internal string View { get; }

        internal bool Rendered { get; private set; }

        internal void Seal()
        {
            if (Rendered)
            {
                throw new InvalidOperationException(
                    $"The plan of {View} is rendered twice, so every handler it holds would run twice: render it once, " +
                    "after the last behavior written to it.");
            }

            Rendered = true;
        }

        internal void AddInput(RenderedInput input)
        {
            RefuseOnceRendered($"Input '{input.BindingPath}'", "the input");
            _inputs.Add(input);
        }

        private void RefuseOnceRendered(string written, string what)
        {
            if (!Rendered) return;
            throw new InvalidOperationException(
                $"{written} is written after the plan of {View} rendered, so the plan the page runs does not hold it. " +
                "Razor runs a @section after the view's body: render the plan in a section the layout renders after the " +
                $"others, such as @section Scripts {{ @Html.RenderPlan(plan) }}, or write {what} in the body.");
        }

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

        internal Block ActiveBlock
        {
            get
            {
                if (_blocks.Count == 0)
                {
                    throw new InvalidOperationException(
                        "A reaction step was written outside any callback. Write it inside a trigger, a component's " +
                        ".Reactive, Then, Else, OnSuccess, OnError, WhileLoading, Finally or OnAllSettled callback.");
                }

                return _blocks.Peek();
            }
        }

        internal void AddStep(Step step) => ActiveBlock.Add(step);

        internal void On(Func<PlanObjects, string> on, IReadOnlyList<string> path, string eventName, Block body)
            => _handlers.Add((on, path, eventName, body));

        internal void DeclareElement(string elementId)
        {
            if (string.IsNullOrWhiteSpace(elementId)) throw new ArgumentException("Element ID required.", nameof(elementId));
            if (!_declaredById.ContainsKey(elementId)) Add(new DeclaredObject(elementId, ComponentVendor.Native));
        }

        internal void DeclareComponent(string componentId, ComponentVendor vendor)
        {
            if (string.IsNullOrWhiteSpace(componentId)) throw new ArgumentException("Component ID required.", nameof(componentId));
            if (_declaredById.TryGetValue(componentId, out var existing))
            {
                if (existing.Vendor.Name != vendor.Name)
                {
                    throw new InvalidOperationException(
                        $"Element '{componentId}' is named as a component of vendor '{existing.Vendor.Name}' and as one of vendor " +
                        $"'{vendor.Name}', but an element holds one component: name it by the declaration of the component its view renders.");
                }

                return;
            }

            Add(new DeclaredObject(componentId, vendor));
        }

        internal void LoadsOrUnloads(string slotId)
        {
            DeclareElement(slotId);
            _slots.Add(slotId);
        }

        internal void SetsContentOf(string elementId, string content)
        {
            if (!_contentSetById.ContainsKey(elementId)) _contentSetById.Add(elementId, content);
        }

        internal IReadOnlyCollection<string> Slots => _slots;

        internal IReadOnlyDictionary<string, string> ContentSetById => _contentSetById;

        internal void RegisterPlugin(string name)
        {
            if (!_plugins.Add(name))
            {
                throw new InvalidOperationException(
                    $"Plugin '{name}' is registered twice on the plan of {View}: register it once, and use the plugin " +
                    "RegisterPlugin returned wherever the view needs it.");
            }
        }

        private void Add(DeclaredObject declared)
        {
            _declared.Add(declared);
            _declaredById.Add(declared.Id, declared);
        }

        internal void ValidateBefore(RequestStep request, FormValidation validation)
        {
            foreach (var (other, otherValidation) in _validatedRequests)
            {
                if (other == request || otherValidation.Form != validation.Form) continue;
                if (otherValidation.Validator != validation.Validator)
                {
                    throw new InvalidOperationException(
                        $"Form '{validation.Form}' is validated by {CSharpTypeName.Of(otherValidation.Validator)} and by " +
                        $"{CSharpTypeName.Of(validation.Validator)}, but a form takes one validator. Give each validator a " +
                        "form of its own, or write one validator whose rules differ by When conditions.");
                }

                if (otherValidation.RuleSet != validation.RuleSet)
                {
                    throw new InvalidOperationException(
                        $"Form '{validation.Form}' is validated by {CSharpTypeName.Of(validation.Validator)} under {otherValidation.DescribedRuleSet} " +
                        $"and under {validation.DescribedRuleSet}, but a form takes one validator and one rule set. Give each a form of its own.");
                }
            }

            var index = _validatedRequests.FindIndex(validated => validated.Request == request);
            if (index < 0) _validatedRequests.Add((request, validation));
            else _validatedRequests[index] = (request, validation);
        }

        internal IReadOnlyList<(RequestStep Request, FormValidation Validation)> ValidatedRequests => _validatedRequests;

        internal void ShowErrorsOn(string formId)
        {
            DeclareElement(formId);
            if (!_forms.ContainsKey(formId)) _forms.Add(formId, Array.Empty<CheckedField>());
        }

        internal IReadOnlyList<string> Forms
            => _validatedRequests
                .Select(validated => validated.Validation.Form)
                .Concat(_forms.Keys)
                .Distinct(StringComparer.Ordinal)
                .ToList();

        internal void Check(string formId, IReadOnlyList<CheckedField> fields)
        {
            DeclareElement(formId);
            _forms[formId] = fields;
        }

        internal PlanDocument Write()
        {
            var objects = new PlanObjects(_declared);
            var plan = new PlanWriter(_planId, objects);

            var inputs = ModelInputs(objects);
            if (inputs.Count > 0) objects.Model(new ModelPluginData(inputs));

            var forms = new FormWriter(Scope.ForHandler(plan, "$form"));
            foreach (var declared in _declared)
            {
                if (_forms.TryGetValue(declared.Id, out var fields))
                    objects.Form(declared.Id, forms.Form(declared.Id, fields));
            }

            var handlers = _handlers
                .Select(handler =>
                {
                    var body = handler.Body.Write(Scope.ForHandler(plan, Handler.Payload));
                    return new Handler(handler.On(objects), handler.Path, handler.Event, body);
                })
                .ToList();

            return new PlanDocument(_planId, objects.Vendors(), objects.Snapshot(), handlers);
        }

        internal void DeclareRenderedInputs()
        {
            foreach (var input in _inputs.Entries)
                DeclareComponent(input.Value.ComponentId, input.Value.ComponentVendor);
        }

        private IReadOnlyDictionary<string, InputBinding> ModelInputs(PlanObjects objects)
        {
            var inputs = new Dictionary<string, InputBinding>(StringComparer.Ordinal);
            foreach (var declared in _declared)
            {
                if (!_inputs.TryFindForComponent(ComponentId.Of(declared.Id), out var rendered)) continue;

                var value = G.Get(G.Obj(objects.Element(declared.Id)), rendered.ValuePath.Text, rendered.Shape, PathMembers.Required);
                inputs.Add(
                    declared.Id,
                    new InputBinding(
                        rendered.BindingPath, BodyOf(rendered.Binding.Path), value, rendered.Shape, rendered.CommittedBy.Value, _classNames.InvalidInput));
            }

            return inputs;
        }

        private static IReadOnlyList<BodyStep> BodyOf(Path path) => path.Segments
            .Select(segment => segment.IsEachItem
                ? throw new InvalidOperationException($"'{path}' reads each item of a list; a body names members and positions.")
                : segment.IsIndex ? BodyStep.At(segment.Index) : BodyStep.Member(segment.ToPathText()))
            .ToList();
    }
}
