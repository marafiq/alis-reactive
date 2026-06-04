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
        private readonly BrowserObjects _browserObjects;
        private readonly BehaviorGraph _behaviors;
        private readonly List<ValidationJob> _validationJobs = new List<ValidationJob>();

        internal PlanBuildContext(
            PlanIdentity identity,
            RegisteredInputComponents registrations)
        {
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _objectContracts = new BrowserObjectContracts();
            _browserObjects = new BrowserObjects(_objectContracts, registrations);
            _behaviors = new BehaviorGraph(_browserObjects);
        }

        /// <summary>Snapshots the accumulated draft into the serialized plan document.</summary>
        internal PlanDocument BuildPlan() =>
            new PlanDocument(
                _identity,
                _objectContracts.Snapshot(),
                _browserObjects.Snapshot(),
                _behaviors.Snapshot());

        /// <summary>Gets a browser object already registered in the plan.</summary>
        internal BrowserObject GetBrowserObject(ComponentKey key) => _browserObjects.Get(key);

        /// <summary>Replaces a registered browser object, used when validation enriches a container scope.</summary>
        internal void SetBrowserObject(ComponentKey key, BrowserObject browserObject) =>
            _browserObjects.Set(key, browserObject);

        /// <summary>Records that a request declared a validation source, to be resolved during Render().</summary>
        internal void RegisterValidationJob(RequestPlan request, ComponentId container, Type validationSourceType) =>
            _validationJobs.Add(new ValidationJob(request.Url, container, validationSourceType));

        /// <summary>The validation jobs declared during plan construction.</summary>
        internal IReadOnlyList<ValidationJob> ValidationJobs => _validationJobs;

        /// <summary>
        /// Declares a DOM element as a native object target in the plan.
        /// Returns the component key for use in source references.
        /// </summary>
        internal ComponentKey DeclareElement(string elementId) => _browserObjects.DeclareElement(elementId);

        /// <summary>Declares a component referenced by page behavior as a plan object target.</summary>
        internal ComponentKey DeclareObjectTarget(string componentId, string vendor) =>
            _browserObjects.DeclareObjectTarget(componentId, vendor);

        /// <summary>Declares a layout-owned app component by its fixed runtime object id.</summary>
        internal ComponentKey DeclareLayoutObject(string componentId, string vendor) =>
            _browserObjects.DeclareLayoutObject(componentId, vendor);

        /// <summary>Returns the render-time registration for a component value read.</summary>
        internal ComponentRegistration RequireRegistrationById(
            string componentId,
            RegisteredInputValueRead valueRead) =>
            _browserObjects.RequireRegistrationById(componentId, valueRead);

        /// <summary>
        /// Declares a registered input component in the plan with readable value metadata.
        /// </summary>
        internal ComponentKey DeclareInputComponent(InputComponentPlanBinding binding) =>
            _browserObjects.DeclareInputComponent(binding);

        /// <summary>Declares a property member on a component contract in the plan.</summary>
        internal void DeclareProperty(ComponentKey componentKey, ObjectPropertyContract contract) =>
            _browserObjects.DeclareProperty(componentKey, contract);

        /// <summary>Declares a method member on a component contract in the plan.</summary>
        internal ObjectMethod DeclareMethod(ComponentKey componentKey, ObjectMethodContract contract) =>
            _browserObjects.DeclareMethod(componentKey, contract);

        /// <summary>Registers a plugin contract. Throws on duplicate registration.</summary>
        internal void RegisterPlugin(PluginContract contract) =>
            _objectContracts.RegisterPlugin(contract);

        /// <summary>Declares a plugin method in the Reactive Plan contract registry.</summary>
        internal MethodSignature DeclarePluginMethod(PluginMethodRequirement requirement) =>
            _objectContracts.DeclarePluginMethod(requirement);

        /// <summary>Declares a plugin property in the Reactive Plan contract registry.</summary>
        internal void DeclarePluginProperty(PluginPropertyRequirement requirement) =>
            _objectContracts.DeclarePluginProperty(requirement);

        /// <summary>
        /// Registers all input components from the ReactivePlan's input component onboarding catalog into the plan.
        /// Called during Render().
        /// </summary>
        internal void RegisterInputComponents() => _browserObjects.RegisterInputComponents();

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
