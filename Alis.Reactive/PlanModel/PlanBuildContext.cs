using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Construction boundary used by the public DSL builders.
    /// It delegates domain decisions to the plan authoring state and only exposes the
    /// narrow mutation verbs the DSL needs while authoring a plan.
    /// </summary>
    public sealed class PlanBuildContext
    {
        private readonly PlanIdentity _identity;
        private readonly BrowserObjectContracts _objectContracts;
        private readonly ComponentObjects _components;
        private readonly BehaviorGraph _behaviors;
        private readonly List<ValidationJob> _validationJobs = new List<ValidationJob>();

        internal PlanBuildContext(
            PlanIdentity identity,
            RegisteredInputComponents registrations)
        {
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _objectContracts = new BrowserObjectContracts();
            _components = new ComponentObjects(_objectContracts, registrations);
            _behaviors = new BehaviorGraph(_components);
        }

        /// <summary>Snapshots the accumulated draft into the serialized plan document.</summary>
        internal PlanDocument BuildPlan() =>
            new PlanDocument(
                _identity,
                _objectContracts.Snapshot(),
                _components.Snapshot(),
                _behaviors.Snapshot());

        /// <summary>Behaviors collected so far — a read-only view for mid-build inspection.</summary>
        internal IReadOnlyList<Behavior> Behaviors => _behaviors.Behaviors;

        /// <summary>Gets a component already registered in the plan.</summary>
        internal ComponentObject GetComponent(ComponentKey key) => _components.Get(key);

        /// <summary>Replaces a registered component, used when validation enriches a container scope.</summary>
        internal void SetComponent(ComponentKey key, ComponentObject component) => _components.Set(key, component);

        /// <summary>Records that a request declared a validation source, to be resolved during Render().</summary>
        internal void RegisterValidationJob(RequestPlan request, ComponentId container, Type validationSourceType) =>
            _validationJobs.Add(new ValidationJob(request.Url, container, validationSourceType));

        /// <summary>The validation jobs declared during plan construction.</summary>
        internal IReadOnlyList<ValidationJob> ValidationJobs => _validationJobs;

        /// <summary>
        /// Ensures a DOM element is registered as a native component with a BrowserObjectContract.
        /// Returns the component key for use in source references.
        /// </summary>
        internal ComponentKey EnsureElement(string elementId) => _components.EnsureElement(elementId);

        /// <summary>
        /// Ensures a component is registered with its vendor, type metadata, and any known input binding.
        /// </summary>
        internal ComponentKey EnsureComponent(string componentId, string vendor) =>
            EnsureComponent(componentId, vendor, ComponentRole.ObjectTarget);

        /// <summary>
        /// Ensures a component is registered with the browser-object role that caused the reference.
        /// </summary>
        internal ComponentKey EnsureComponent(
            string componentId,
            string vendor,
            ComponentRole role) =>
            _components.EnsureComponent(componentId, vendor, role);

        /// <summary>Returns the render-time registration for a component value read.</summary>
        internal ComponentRegistration RequireRegistrationById(
            string componentId,
            RegisteredInputValueRead valueRead) =>
            _components.RequireRegistrationById(componentId, valueRead);

        /// <summary>
        /// Ensures a registered input component exists in the plan with readable value metadata.
        /// </summary>
        internal ComponentKey EnsureInputComponent(InputComponentPlanBinding binding) =>
            _components.EnsureInputComponent(binding);

        /// <summary>Ensures a property member exists on a component's BrowserObjectContract.</summary>
        internal void EnsureProperty(ComponentKey componentKey, ObjectPropertyContract contract) =>
            _components.EnsureProperty(componentKey, contract);

        /// <summary>Ensures a method member exists on a component's BrowserObjectContract.</summary>
        internal ObjectMethod EnsureMethod(ComponentKey componentKey, ObjectMethodContract contract) =>
            _components.EnsureMethod(componentKey, contract);

        /// <summary>Registers a plugin contract. Throws on duplicate registration.</summary>
        internal void RegisterPlugin(PluginContract contract) =>
            _objectContracts.RegisterPlugin(contract);

        /// <summary>Ensures a plugin method exists in the browser object contracts.</summary>
        internal MethodSignature EnsurePluginMethod(PluginMethodRequirement requirement) =>
            _objectContracts.EnsurePluginMethod(requirement);

        /// <summary>Ensures a plugin property exists in the browser object contracts.</summary>
        internal void EnsurePluginProperty(PluginPropertyRequirement requirement) =>
            _objectContracts.EnsurePluginProperty(requirement);

        /// <summary>
        /// Registers all input components from the ReactivePlan's input component onboarding catalog into the plan.
        /// Called during Render().
        /// </summary>
        internal void RegisterInputComponents() => _components.RegisterInputComponents();

        /// <summary>Returns all registered input components for IncludeAll expansion at build time.</summary>
        internal IReadOnlyDictionary<string, ComponentRegistration> GetRegisteredComponents() =>
            _components.RegisteredInputs;

        /// <summary>
        /// Wires a component event to the reactive behavior built by one DSL pipeline callback.
        /// </summary>
        internal void WireComponentEvent(string componentId, string vendor, string eventName, ReactionGraph reaction)
        {
            EnsureComponent(componentId, vendor);
            var trigger = StartsWhen.ComponentEvent(componentId, eventName);
            AddBehavior(Behavior.On(trigger, reaction));
        }

        internal void AddBehavior(Behavior behavior) => _behaviors.Add(behavior);
    }
}
