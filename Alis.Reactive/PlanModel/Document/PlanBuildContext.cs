using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Construction boundary used by the public DSL builders.
    /// It delegates domain decisions to the plan authoring state and only exposes the
    /// narrow authoring operations the DSL needs while building a Reactive Plan.
    /// </summary>
    internal sealed class PlanBuildContext
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

        internal PlanDocument BuildPlan() =>
            new PlanDocument(
                _identity,
                _objectContracts.Snapshot(),
                _browserObjects.Snapshot(),
                _behaviors.Snapshot());

        internal BrowserObject GetBrowserObject(ComponentKey key) => _browserObjects.Get(key);

        internal void SetBrowserObject(ComponentKey key, BrowserObject browserObject) =>
            _browserObjects.Set(key, browserObject);

        internal void RegisterValidationJob(RequestPlan request, ComponentId container, Type validationSourceType) =>
            _validationJobs.Add(new ValidationJob(request.Url, container, validationSourceType));

        internal IReadOnlyList<ValidationJob> ValidationJobs => _validationJobs;

        internal ComponentKey DeclareElement(string elementId) => _browserObjects.DeclareElement(elementId);

        internal ComponentKey DeclareObjectTarget(string componentId, string vendor) =>
            _browserObjects.DeclareObjectTarget(componentId, vendor);

        internal ComponentKey DeclareLayoutObject(string componentId, string vendor) =>
            _browserObjects.DeclareLayoutObject(componentId, vendor);

        internal ComponentRegistration RequireRegistrationById(
            string componentId,
            RegisteredInputValueRead valueRead) =>
            _browserObjects.RequireRegistrationById(componentId, valueRead);

        internal ComponentKey DeclareInputComponent(InputComponentPlanBinding binding) =>
            _browserObjects.DeclareInputComponent(binding);

        internal void DeclareProperty(ComponentKey componentKey, ObjectPropertyContract contract) =>
            _browserObjects.DeclareProperty(componentKey, contract);

        internal ObjectMethod DeclareMethod(ComponentKey componentKey, ObjectMethodContract contract) =>
            _browserObjects.DeclareMethod(componentKey, contract);

        internal void RegisterPlugin(PluginContract contract) =>
            _objectContracts.RegisterPlugin(contract);

        internal MethodSignature DeclarePluginMethod(PluginMethodRequirement requirement) =>
            _objectContracts.DeclarePluginMethod(requirement);

        internal void DeclarePluginProperty(PluginPropertyRequirement requirement) =>
            _objectContracts.DeclarePluginProperty(requirement);

        internal void RegisterInputComponents() => _browserObjects.RegisterInputComponents();

        internal void WireComponentEvent(string componentId, string vendor, string eventName, ReactionGraph reaction)
        {
            DeclareObjectTarget(componentId, vendor);
            var trigger = StartsWhen.ComponentEvent(componentId, eventName);
            AddBehavior(Behavior.On(trigger, reaction));
        }

        internal void AddBehavior(Behavior behavior) => _behaviors.Add(behavior);
    }
}
