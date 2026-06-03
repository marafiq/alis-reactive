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
        private readonly BrowserObjects _components;
        private readonly BehaviorGraph _behaviors;
        private readonly List<ValidationJob> _validationJobs = new List<ValidationJob>();

        internal PlanBuildContext(
            PlanIdentity identity,
            RegisteredInputComponents registrations)
        {
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _objectContracts = new BrowserObjectContracts();
            _components = new BrowserObjects(_objectContracts, registrations);
            _behaviors = new BehaviorGraph(_components);
        }

        /// <summary>Snapshots the accumulated draft into the serialized plan document.</summary>
        internal PlanDocument BuildPlan() =>
            new PlanDocument(
                _identity,
                _objectContracts.Snapshot(),
                _components.Snapshot(),
                _behaviors.Snapshot());

        /// <summary>Gets a component already registered in the plan.</summary>
        internal BrowserObject GetComponent(ComponentKey key) => _components.Get(key);

        /// <summary>Replaces a registered component, used when validation enriches a container scope.</summary>
        internal void SetComponent(ComponentKey key, BrowserObject component) => _components.Set(key, component);

        /// <summary>Records that a request declared a validation source, to be resolved during Render().</summary>
        internal void RegisterValidationJob(RequestPlan request, ComponentId container, Type validationSourceType) =>
            _validationJobs.Add(new ValidationJob(request.Url, container, validationSourceType));

        /// <summary>The validation jobs declared during plan construction.</summary>
        internal IReadOnlyList<ValidationJob> ValidationJobs => _validationJobs;

        /// <summary>
        /// Declares a DOM element as a native browser object in the plan.
        /// Returns the component key for use in source references.
        /// </summary>
        internal ComponentKey DeclareElement(string elementId) => _components.DeclareElement(elementId);

        /// <summary>Declares a component referenced by page behavior as a browser object target.</summary>
        internal ComponentKey DeclareObjectTarget(string componentId, string vendor) =>
            _components.DeclareObjectTarget(componentId, vendor);

        /// <summary>Declares a layout-owned app component by its fixed runtime object id.</summary>
        internal ComponentKey DeclareLayoutObject(string componentId, string vendor) =>
            _components.DeclareLayoutObject(componentId, vendor);

        /// <summary>Returns the render-time registration for a component value read.</summary>
        internal ComponentRegistration RequireRegistrationById(
            string componentId,
            RegisteredInputValueRead valueRead) =>
            _components.RequireRegistrationById(componentId, valueRead);

        /// <summary>
        /// Declares a registered input component in the plan with readable value metadata.
        /// </summary>
        internal ComponentKey DeclareInputComponent(InputComponentPlanBinding binding) =>
            _components.DeclareInputComponent(binding);

        /// <summary>Declares a property member on a component's browser object contract.</summary>
        internal void DeclareProperty(ComponentKey componentKey, ObjectPropertyContract contract) =>
            _components.DeclareProperty(componentKey, contract);

        /// <summary>Declares a method member on a component's browser object contract.</summary>
        internal ObjectMethod DeclareMethod(ComponentKey componentKey, ObjectMethodContract contract) =>
            _components.DeclareMethod(componentKey, contract);

        /// <summary>Registers a plugin contract. Throws on duplicate registration.</summary>
        internal void RegisterPlugin(PluginContract contract) =>
            _objectContracts.RegisterPlugin(contract);

        /// <summary>Declares a plugin method in the browser object contracts.</summary>
        internal MethodSignature DeclarePluginMethod(PluginMethodRequirement requirement) =>
            _objectContracts.DeclarePluginMethod(requirement);

        /// <summary>Declares a plugin property in the browser object contracts.</summary>
        internal void DeclarePluginProperty(PluginPropertyRequirement requirement) =>
            _objectContracts.DeclarePluginProperty(requirement);

        /// <summary>
        /// Registers all input components from the ReactivePlan's input component onboarding catalog into the plan.
        /// Called during Render().
        /// </summary>
        internal void RegisterInputComponents() => _components.RegisterInputComponents();

        /// <summary>
        /// Wires a component event to the reactive behavior built by one DSL pipeline callback.
        /// </summary>
        internal void WireComponentEvent(string componentId, string vendor, string eventName, ReactionGraph reaction)
        {
            DeclareObjectTarget(componentId, vendor);
            var trigger = StartsWhen.ComponentEvent(componentId, eventName);
            AddBehavior(Behavior.On(trigger, reaction));
        }

        internal void AddBehavior(Behavior behavior) => _behaviors.Add(behavior);
    }
}
