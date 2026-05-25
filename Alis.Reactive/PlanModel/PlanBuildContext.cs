using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Construction boundary used by the public DSL builders.
    /// It delegates domain decisions to the plan draft aggregates and only exposes the
    /// narrow mutation verbs the DSL needs while authoring a plan.
    /// </summary>
    public sealed class PlanBuildContext
    {
        private readonly PlanIdentity _identity;
        private readonly JsTypeCatalog _types;
        private readonly ComponentCatalog _components;
        private readonly BehaviorGraph _behaviors;
        private readonly ValidationWorkQueue _validationJobs = new ValidationWorkQueue();

        internal PlanBuildContext(
            PlanIdentity identity,
            ComponentRegistrationCatalog registrations)
        {
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _types = new JsTypeCatalog();
            _components = new ComponentCatalog(_types, registrations);
            _behaviors = new BehaviorGraph(_components);
        }

        /// <summary>Snapshots the accumulated draft into the serialized plan document.</summary>
        internal Plan BuildPlan() =>
            new Plan(
                _identity,
                PlanTypes.From(_types.Snapshot()),
                PlanComponents.From(_components.Snapshot()),
                PlanBehaviors.From(_behaviors.Snapshot()));

        /// <summary>Behaviors collected so far — a read-only view for mid-build inspection.</summary>
        internal IReadOnlyList<Behavior> Behaviors => _behaviors.Behaviors;

        /// <summary>Gets a component already registered in the plan.</summary>
        internal Component GetComponent(ComponentKey key) => _components.Get(key);

        /// <summary>Replaces a registered component, used when validation enriches a container scope.</summary>
        internal void SetComponent(ComponentKey key, Component component) => _components.Set(key, component);

        /// <summary>Records that a request declared a validation source, to be resolved during Render().</summary>
        internal void RegisterValidationJob(Request request, ComponentId container, Type validationSourceType) =>
            _validationJobs.Enqueue(request, container, validationSourceType);

        /// <summary>The validation jobs declared during plan construction.</summary>
        internal IReadOnlyList<ValidationJob> ValidationJobs => _validationJobs.Jobs;

        /// <summary>
        /// Ensures a DOM element is registered as a native component with a JsType.
        /// Returns the component key for use in source references.
        /// </summary>
        internal ComponentKey EnsureElement(string elementId) => _components.EnsureElement(elementId);

        /// <summary>
        /// Ensures a component is registered with its vendor, type metadata, and any known input binding.
        /// </summary>
        internal ComponentKey EnsureComponent(string componentId, string vendor) =>
            EnsureComponent(componentId, vendor, ComponentContributionIntent.ObjectTarget);

        /// <summary>
        /// Ensures a component is registered with the contribution intent that caused the reference.
        /// </summary>
        internal ComponentKey EnsureComponent(
            string componentId,
            string vendor,
            ComponentContributionIntent contribution) =>
            _components.EnsureComponent(componentId, vendor, contribution);

        /// <summary>
        /// Searches the registration map for a registration whose ComponentId matches.
        /// </summary>
        internal ComponentRegistrationMatch FindRegistrationById(string componentId) =>
            _components.FindRegistrationById(componentId);

        /// <summary>
        /// Ensures a registered input component exists in the plan with readable value metadata.
        /// </summary>
        internal ComponentKey EnsureInputComponent(InputComponentPlanBinding binding) =>
            _components.EnsureInputComponent(binding);

        /// <summary>Ensures a property member exists on a component's JsType.</summary>
        internal void EnsureProperty(ComponentKey componentKey, JsPropertyContract contract) =>
            _components.EnsureProperty(componentKey, contract);

        /// <summary>Ensures a method member exists on a component's JsType.</summary>
        internal JsMethod EnsureMethod(ComponentKey componentKey, JsMethodContract contract) =>
            _components.EnsureMethod(componentKey, contract);

        /// <summary>Registers a plugin contract. Throws on duplicate registration.</summary>
        internal void RegisterPlugin(PluginContract contract) =>
            _types.RegisterPlugin(contract);

        /// <summary>Ensures a plugin method exists in the plan's JsType registry.</summary>
        internal MethodSignature EnsurePluginMethod(PluginMethodRequirement requirement) =>
            _types.EnsurePluginMethod(requirement);

        /// <summary>Ensures a plugin property exists in the plan's JsType registry.</summary>
        internal void EnsurePluginProperty(PluginPropertyRequirement requirement) =>
            _types.EnsurePluginProperty(requirement);

        /// <summary>
        /// Registers all input components from the ReactivePlan's input component onboarding catalog into the plan.
        /// Called during Render().
        /// </summary>
        internal void RegisterInputComponents() => _components.RegisterInputComponents();

        /// <summary>Returns all registered input components for IncludeAll expansion at build time.</summary>
        internal IReadOnlyDictionary<string, ComponentRegistration> GetRegisteredComponents() =>
            _components.RegisteredInputs;

        /// <summary>
        /// Wires a component event to a set of reactive behaviors.
        /// Ensures the component is registered, creates the trigger, and adds one behavior per reaction.
        /// </summary>
        internal void WireComponentEvent(string componentId, string vendor, string eventName, List<Reaction> reactions)
        {
            EnsureComponent(componentId, vendor);
            var trigger = StartsWhen.ComponentEvent(componentId, eventName);
            foreach (var reaction in reactions)
                AddBehavior(Behavior.On(trigger, reaction));
        }

        internal void AddBehavior(Behavior behavior) => _behaviors.Add(behavior);
    }
}
