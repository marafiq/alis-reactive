using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Alis.Reactive.PlanModel
{
    internal sealed class BrowserObjects
    {
        private readonly BrowserObjectContracts _objectContracts;
        private readonly Dictionary<string, BrowserObject> _objects = new Dictionary<string, BrowserObject>();
        private readonly RegisteredInputComponents _registrations;

        internal BrowserObjects(
            BrowserObjectContracts objectContracts,
            RegisteredInputComponents registrations)
        {
            _objectContracts = objectContracts;
            _registrations = registrations;
        }

        internal IReadOnlyDictionary<string, BrowserObject> Snapshot() =>
            new Dictionary<string, BrowserObject>(_objects);

        internal BrowserObject Get(ComponentKey componentKey) =>
            _objects[componentKey.Value];

        internal void Set(ComponentKey componentKey, BrowserObject browserObject) =>
            _objects[componentKey.Value] = browserObject;

        internal ComponentKey DeclareElement(string elementId)
        {
            var id = ComponentId.Of(elementId);
            if (_objects.ContainsKey(id.Value))
                return ComponentKey.Of(id.Value);

            var typeKey = BrowserObjectId.NativeElement(id);
            _objectContracts.DeclareObject(typeKey);
            _objects[id.Value] = BrowserObject.Element(id, ComponentVendor.Native, typeKey);
            return ComponentKey.Of(id.Value);
        }

        internal ComponentKey DeclareObjectTarget(string componentId, string vendor) =>
            DeclareComponentObject(componentId, vendor, BrowserObject.Element);

        internal ComponentKey DeclareLayoutObject(string componentId, string vendor) =>
            DeclareComponentObject(componentId, vendor, BrowserObject.LayoutObject);

        private ComponentKey DeclareComponentObject(
            string componentId,
            string vendor,
            Func<ComponentId, ComponentVendor, BrowserObjectId, BrowserObject> createUnregisteredComponent)
        {
            var id = ComponentId.Of(componentId);
            var componentVendor = ComponentVendor.From(vendor);

            if (_objects.TryGetValue(id.Value, out var existing))
            {
                RequireSameVendor(existing, id, componentVendor);
                EnrichExistingComponent(existing, id);
                return ComponentKey.Of(id.Value);
            }

            var typeKey = BrowserObjectId.ComponentObject(componentVendor, id);
            if (TryFindRegistration(id, out var registration))
            {
                registration.DeclareValueContract(_objectContracts, typeKey);
                _objects[id.Value] = registration.CreateComponent(id, componentVendor, typeKey);
            }
            else
            {
                _objectContracts.DeclareObject(typeKey);
                _objects[id.Value] = createUnregisteredComponent(id, componentVendor, typeKey);
            }

            return ComponentKey.Of(id.Value);
        }

        internal ComponentKey DeclareInputComponent(InputComponentPlanBinding input)
        {
            var id = input.ComponentId;
            var componentVendor = input.Vendor;
            var valueContract = input.ValueContract;
            var binding = input.InputBinding;

            if (_objects.TryGetValue(id.Value, out var existing))
            {
                RequireSameVendor(existing, id, componentVendor);
                _objectContracts.DeclareInputValueContract(BrowserObjectId.Of(existing.Type), valueContract);
                _objects[id.Value] = existing.WithBindingIfAbsent(binding);
                return ComponentKey.Of(id.Value);
            }

            var typeKey = input.BrowserObjectId;
            _objectContracts.DeclareInputValueContract(typeKey, valueContract);
            _objects[id.Value] = input.CreateComponent();

            return ComponentKey.Of(id.Value);
        }

        internal void DeclareProperty(ComponentKey componentKey, ObjectPropertyContract contract)
        {
            var browserObject = Get(componentKey);
            _objectContracts.DeclareProperty(BrowserObjectId.Of(browserObject.Type), contract);
        }

        internal ObjectMethod DeclareMethod(ComponentKey componentKey, ObjectMethodContract contract)
        {
            var browserObject = Get(componentKey);
            return _objectContracts.DeclareMethod(BrowserObjectId.Of(browserObject.Type), contract);
        }

        internal void DeclareEvent(ComponentKey componentKey, ObjectEventContract contract)
        {
            var browserObject = Get(componentKey);
            _objectContracts.DeclareEvent(BrowserObjectId.Of(browserObject.Type), contract);
        }

        internal void RegisterInputComponents()
        {
            foreach (var registrationEntry in _registrations.Entries)
                DeclareInputComponent(registrationEntry.Value.PlanBinding);
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

        private static void RequireSameVendor(BrowserObject existing, ComponentId componentId, ComponentVendor vendor)
        {
            if (existing.Vendor != vendor.Value)
                throw new InvalidOperationException(
                    $"Component '{componentId.Value}' registered as vendor '{existing.Vendor}' " +
                    $"but re-referenced as '{vendor.Value}'. A component cannot change vendor.");
        }

        private void EnrichExistingComponent(BrowserObject existing, ComponentId componentId)
        {
            if (!TryFindRegistration(componentId, out var registration))
                return;

            _objects[componentId.Value] = registration.AddBindingTo(
                existing,
                _objectContracts.Require(BrowserObjectId.Of(existing.Type)));
        }

    }
}
