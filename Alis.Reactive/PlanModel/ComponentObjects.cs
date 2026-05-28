using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Alis.Reactive.PlanModel
{
    internal sealed class ComponentObjects
    {
        private readonly BrowserObjectContracts _objectContracts;
        private readonly Dictionary<string, ComponentObject> _components = new Dictionary<string, ComponentObject>();
        private readonly RegisteredInputComponents _registrations;

        internal ComponentObjects(
            BrowserObjectContracts objectContracts,
            RegisteredInputComponents registrations)
        {
            _objectContracts = objectContracts;
            _registrations = registrations;
        }

        internal IReadOnlyDictionary<string, ComponentObject> Snapshot() =>
            new Dictionary<string, ComponentObject>(_components);

        internal IReadOnlyDictionary<string, ComponentRegistration> RegisteredInputs =>
            _registrations.Snapshot();

        internal ComponentObject Get(ComponentKey componentKey) =>
            _components[componentKey.Value];

        internal void Set(ComponentKey componentKey, ComponentObject component) =>
            _components[componentKey.Value] = component;

        internal ComponentKey EnsureElement(string elementId)
        {
            var id = ComponentId.Of(elementId);
            if (_components.ContainsKey(id.Value))
                return ComponentKey.Of(id.Value);

            var typeKey = TypeKey.NativeElement(id);
            _objectContracts.EnsureEmpty(typeKey);
            _components[id.Value] = ComponentObject.Element(id.Value, ComponentVendor.Native.Value, typeKey.Value);
            return ComponentKey.Of(id.Value);
        }

        internal ComponentKey EnsureObjectTarget(string componentId, string vendor) =>
            EnsureComponentObject(componentId, vendor, ComponentObject.Element);

        internal ComponentKey EnsureLayoutObject(string componentId, string vendor) =>
            EnsureComponentObject(componentId, vendor, ComponentObject.LayoutObject);

        private ComponentKey EnsureComponentObject(
            string componentId,
            string vendor,
            Func<string, string, string, ComponentObject> createUnregisteredComponent)
        {
            var id = ComponentId.Of(componentId);
            var componentVendor = ComponentVendor.From(vendor);

            if (_components.TryGetValue(id.Value, out var existing))
            {
                EnsureSameVendor(existing, id, componentVendor);
                EnrichExistingComponent(existing, id);
                return ComponentKey.Of(id.Value);
            }

            var typeKey = TypeKey.ComponentObject(componentVendor, id);
            if (TryFindRegistration(id, out var registration))
            {
                registration.DeclareValueContract(_objectContracts, typeKey);
                _components[id.Value] = registration.CreateComponent(id, componentVendor, typeKey);
            }
            else
            {
                _objectContracts.EnsureEmpty(typeKey);
                _components[id.Value] = createUnregisteredComponent(id.Value, componentVendor.Value, typeKey.Value);
            }

            return ComponentKey.Of(id.Value);
        }

        internal ComponentKey EnsureInputComponent(InputComponentPlanBinding input)
        {
            var id = input.ComponentId;
            var componentVendor = input.Vendor;
            var valueContract = input.ValueContract;
            var binding = input.InputBinding;

            if (_components.TryGetValue(id.Value, out var existing))
            {
                EnsureSameVendor(existing, id, componentVendor);
                _objectContracts.EnsureInputValueContract(TypeKey.Of(existing.Type), valueContract);
                _components[id.Value] = existing.WithBindingIfAbsent(binding);
                return ComponentKey.Of(id.Value);
            }

            var typeKey = input.TypeKey;
            _objectContracts.EnsureInputValueContract(typeKey, valueContract);
            _components[id.Value] = input.CreateComponent();

            return ComponentKey.Of(id.Value);
        }

        internal void EnsureProperty(ComponentKey componentKey, ObjectPropertyContract contract)
        {
            var component = Get(componentKey);
            _objectContracts.EnsureProperty(TypeKey.Of(component.Type), contract);
        }

        internal ObjectMethod EnsureMethod(ComponentKey componentKey, ObjectMethodContract contract)
        {
            var component = Get(componentKey);
            return _objectContracts.EnsureMethod(TypeKey.Of(component.Type), contract);
        }

        internal void EnsureEvent(ComponentKey componentKey, ObjectEventContract contract)
        {
            var component = Get(componentKey);
            _objectContracts.EnsureEvent(TypeKey.Of(component.Type), contract);
        }

        internal void RegisterInputComponents()
        {
            foreach (var kvp in _registrations.Entries)
                EnsureInputComponent(kvp.Value.PlanBinding);
        }

        internal ComponentRegistration RequireRegistrationById(
            string componentId,
            RegisteredInputValueRead valueRead) =>
            RequireRegistration(ComponentId.Of(componentId), valueRead);

        private bool TryFindRegistration(
            ComponentId componentId,
            [NotNullWhen(true)] out ComponentRegistration? registration) =>
            _registrations.TryFindForComponent(componentId, out registration);

        private ComponentRegistration RequireRegistration(
            ComponentId componentId,
            RegisteredInputValueRead valueRead)
        {
            if (TryFindRegistration(componentId, out var registration)) return registration;

            throw valueRead.MissingRegistrationException();
        }

        private static void EnsureSameVendor(ComponentObject existing, ComponentId componentId, ComponentVendor vendor)
        {
            if (existing.Vendor != vendor.Value)
                throw new InvalidOperationException(
                    $"Component '{componentId.Value}' registered as vendor '{existing.Vendor}' " +
                    $"but re-referenced as '{vendor.Value}'. A component cannot change vendor.");
        }

        private void EnrichExistingComponent(ComponentObject existing, ComponentId componentId)
        {
            if (!TryFindRegistration(componentId, out var registration))
                return;

            _components[componentId.Value] = registration.AddBindingTo(
                existing,
                _objectContracts.Require(TypeKey.Of(existing.Type)));
        }

    }
}
