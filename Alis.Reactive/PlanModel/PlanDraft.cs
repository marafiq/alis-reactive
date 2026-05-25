using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class JsTypeCatalog
    {
        private readonly Dictionary<string, JsType> _types = new Dictionary<string, JsType>();
        private readonly HashSet<string> _registeredPlugins = new HashSet<string>();

        internal IReadOnlyDictionary<string, JsType> Snapshot() =>
            new Dictionary<string, JsType>(_types);

        internal bool Contains(TypeKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return _types.ContainsKey(key.Value);
        }

        internal JsType Require(TypeKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!_types.TryGetValue(key.Value, out var jsType))
                throw new InvalidOperationException($"JS type '{key.Value}' is not registered in this plan.");
            return jsType;
        }

        internal void AddOrReplace(TypeKey key, JsType jsType)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _types[key.Value] = jsType ?? throw new ArgumentNullException(nameof(jsType));
        }

        internal void EnsureInputValueContract(TypeKey key, InputValueContract contract)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            EnsureEmpty(key);
            contract.Enrich(Require(key));
        }

        internal void EnsureEmpty(TypeKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!_types.ContainsKey(key.Value))
                _types[key.Value] = new JsType();
        }

        internal void EnsureProperty(TypeKey typeKey, JsPropertyContract contract)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            Require(typeKey).Declare(contract);
        }

        internal JsMethod EnsureMethod(TypeKey typeKey, JsMethodContract contract)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            return Require(typeKey).Declare(contract);
        }

        internal void EnsureEvent(TypeKey typeKey, JsEventContract contract)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            Require(typeKey).Declare(contract);
        }

        internal void RegisterPlugin(PluginContract contract)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            var name = contract.Name;
            if (!_registeredPlugins.Add(name.Value))
                throw new InvalidOperationException($"Plugin '{name.Value}' is already registered.");

            AddOrReplace(contract.TypeKey, contract.ToJsType());
        }

        internal MethodSignature EnsurePluginMethod(PluginMethodRequirement requirement)
        {
            if (requirement == null) throw new ArgumentNullException(nameof(requirement));

            var typeKey = TypeKey.Plugin(requirement.PluginName);

            if (!Contains(typeKey))
                throw new InvalidOperationException(
                    $"Plugin '{requirement.PluginName.Value}' is not registered. " +
                    $"Call plan.RegisterPlugin(\"{requirement.PluginName.Value}\", ...) first.");

            var method = EnsureMethod(
                typeKey,
                requirement.ToJsMethodContract());

            return method.Signature;
        }

        internal void EnsurePluginProperty(PluginPropertyRequirement requirement)
        {
            if (requirement == null) throw new ArgumentNullException(nameof(requirement));

            var typeKey = TypeKey.Plugin(requirement.PluginName);

            if (!Contains(typeKey))
                throw new InvalidOperationException(
                    $"Plugin '{requirement.PluginName.Value}' is not registered. " +
                    $"Call plan.RegisterPlugin(\"{requirement.PluginName.Value}\", ...) first.");

            EnsureProperty(typeKey, requirement.ToJsPropertyContract());
        }
    }

    internal sealed class ComponentCatalog
    {
        private readonly JsTypeCatalog _types;
        private readonly Dictionary<string, Component> _components = new Dictionary<string, Component>();
        private readonly ComponentRegistrationCatalog _registrations;

        internal ComponentCatalog(
            JsTypeCatalog types,
            ComponentRegistrationCatalog registrations)
        {
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
        }

        internal IReadOnlyDictionary<string, Component> Snapshot() =>
            new Dictionary<string, Component>(_components);

        internal IReadOnlyDictionary<string, ComponentRegistration> RegisteredInputs =>
            _registrations.Snapshot();

        internal bool Contains(ComponentKey componentKey)
        {
            if (componentKey == null) throw new ArgumentNullException(nameof(componentKey));
            return _components.ContainsKey(componentKey.Value);
        }

        internal Component Get(ComponentKey componentKey)
        {
            if (componentKey == null) throw new ArgumentNullException(nameof(componentKey));
            return _components[componentKey.Value];
        }

        internal void Set(ComponentKey componentKey, Component component)
        {
            if (componentKey == null) throw new ArgumentNullException(nameof(componentKey));
            if (component == null) throw new ArgumentNullException(nameof(component));

            EnsureComponentEntryKeyMatchesId(componentKey.Value, component);
            _components[componentKey.Value] = component;
        }

        internal ComponentKey EnsureElement(string elementId)
        {
            var id = ComponentId.Of(elementId);
            if (_components.ContainsKey(id.Value))
                return ComponentKey.Of(id.Value);

            var typeKey = TypeKey.NativeElement(id);
            _types.EnsureEmpty(typeKey);
            _components[id.Value] = Component.Element(id.Value, ComponentVendor.Native.Value, typeKey.Value);
            return ComponentKey.Of(id.Value);
        }

        internal ComponentKey EnsureComponent(
            string componentId,
            string vendor,
            ComponentContributionIntent contribution)
        {
            var id = ComponentId.Of(componentId);
            var componentVendor = ComponentVendor.From(vendor);

            if (_components.TryGetValue(id.Value, out var existing))
            {
                ValidateVendor(existing, id, componentVendor);
                EnrichExistingComponent(existing, id);
                return ComponentKey.Of(id.Value);
            }

            var typeKey = TypeKey.Component(componentVendor, id);
            var registration = FindRegistration(id);
            registration.EnsureType(_types, typeKey);
            _components[id.Value] = registration.CreateComponent(id, componentVendor, typeKey, contribution);

            return ComponentKey.Of(id.Value);
        }

        internal ComponentKey EnsureInputComponent(InputComponentPlanBinding input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var id = input.ComponentId;
            var componentVendor = input.Vendor;
            var valueContract = input.ValueContract;
            var binding = input.ComponentBinding;

            if (_components.TryGetValue(id.Value, out var existing))
            {
                ValidateVendor(existing, id, componentVendor);
                _types.EnsureInputValueContract(TypeKey.Of(existing.Type), valueContract);
                _components[id.Value] = existing.WithBindingIfAbsent(binding);
                return ComponentKey.Of(id.Value);
            }

            var typeKey = input.TypeKey;
            _types.EnsureInputValueContract(typeKey, valueContract);
            _components[id.Value] = input.CreateComponent();

            return ComponentKey.Of(id.Value);
        }

        internal void EnsureProperty(ComponentKey componentKey, JsPropertyContract contract)
        {
            var component = Get(componentKey);
            _types.EnsureProperty(TypeKey.Of(component.Type), contract);
        }

        internal JsMethod EnsureMethod(ComponentKey componentKey, JsMethodContract contract)
        {
            var component = Get(componentKey);
            return _types.EnsureMethod(TypeKey.Of(component.Type), contract);
        }

        internal void EnsureEvent(ComponentKey componentKey, JsEventContract contract)
        {
            var component = Get(componentKey);
            _types.EnsureEvent(TypeKey.Of(component.Type), contract);
        }

        internal void RegisterInputComponents()
        {
            foreach (var kvp in _registrations.Entries)
            {
                var reg = kvp.Value;
                EnsureInputComponent(reg.PlanBinding);
            }
        }

        internal ComponentRegistrationMatch FindRegistrationById(string componentId) =>
            FindRegistration(ComponentId.Of(componentId));

        private ComponentRegistrationMatch FindRegistration(ComponentId componentId)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            return _registrations.FindForComponent(componentId);
        }

        private static void ValidateVendor(Component existing, ComponentId componentId, ComponentVendor vendor)
        {
            if (existing.Vendor != vendor.Value)
                throw new InvalidOperationException(
                    $"Component '{componentId}' registered as vendor '{existing.Vendor}' " +
                    $"but re-referenced as '{vendor.Value}'. A component cannot change vendor.");
        }

        private void EnrichExistingComponent(Component existing, ComponentId componentId)
        {
            var registration = FindRegistration(componentId);
            var enriched = registration.EnrichExistingComponent(
                existing,
                _types.Require(TypeKey.Of(existing.Type)));
            _components[componentId.Value] = enriched;
        }

        private static void EnsureComponentEntryKeyMatchesId(string componentKey, Component component)
        {
            if (component.Id != componentKey)
                throw new InvalidOperationException(
                    $"Plan component key '{componentKey}' cannot store component '{component.Id}'. " +
                    "Component ids are deterministic runtime join keys; store the component under its own id.");
        }
    }

    internal abstract class ComponentRegistrationMatch
    {
        private protected ComponentRegistrationMatch() { }

        internal static ComponentRegistrationMatch Found(ComponentRegistration registration) =>
            new FoundComponentRegistration(registration);

        internal static ComponentRegistrationMatch Missing() =>
            new MissingComponentRegistration();

        internal abstract Shape RequireShape(ComponentRegistrationRequirement requirement);

        internal abstract InputValueContract RequireValueContract(ComponentRegistrationRequirement requirement);

        internal abstract void EnsureType(JsTypeCatalog types, TypeKey typeKey);

        internal abstract Component CreateComponent(
            ComponentId componentId,
            ComponentVendor vendor,
            TypeKey typeKey,
            ComponentContributionIntent contribution);

        internal abstract Component EnrichExistingComponent(Component existing, JsType jsType);
    }

    internal sealed class FoundComponentRegistration : ComponentRegistrationMatch
    {
        private readonly ComponentRegistration _registration;

        internal FoundComponentRegistration(ComponentRegistration registration)
        {
            _registration = registration ?? throw new ArgumentNullException(nameof(registration));
        }

        internal override Shape RequireShape(ComponentRegistrationRequirement requirement)
        {
            if (requirement == null) throw new ArgumentNullException(nameof(requirement));
            return _registration.Shape;
        }

        internal override InputValueContract RequireValueContract(ComponentRegistrationRequirement requirement)
        {
            if (requirement == null) throw new ArgumentNullException(nameof(requirement));
            return _registration.RequireValueContract(requirement.ValueMember);
        }

        internal override void EnsureType(JsTypeCatalog types, TypeKey typeKey)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            if (typeKey == null) throw new ArgumentNullException(nameof(typeKey));

            types.EnsureInputValueContract(typeKey, _registration.ValueContract);
        }

        internal override Component CreateComponent(
            ComponentId componentId,
            ComponentVendor vendor,
            TypeKey typeKey,
            ComponentContributionIntent contribution)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (vendor == null) throw new ArgumentNullException(nameof(vendor));
            if (typeKey == null) throw new ArgumentNullException(nameof(typeKey));
            if (contribution == null) throw new ArgumentNullException(nameof(contribution));

            return Component.Input(
                componentId.Value,
                vendor.Value,
                typeKey.Value,
                _registration.ValueContract.BindingFor(_registration.RegisteredBindingPath));
        }

        internal override Component EnrichExistingComponent(Component existing, JsType jsType)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            if (jsType == null) throw new ArgumentNullException(nameof(jsType));

            _registration.ValueContract.Enrich(jsType);
            return existing.WithBindingIfAbsent(
                _registration.ValueContract.BindingFor(_registration.RegisteredBindingPath));
        }
    }

    internal sealed class MissingComponentRegistration : ComponentRegistrationMatch
    {
        internal override Shape RequireShape(ComponentRegistrationRequirement requirement)
        {
            if (requirement == null) throw new ArgumentNullException(nameof(requirement));
            throw requirement.MissingRegistrationException();
        }

        internal override InputValueContract RequireValueContract(ComponentRegistrationRequirement requirement)
        {
            if (requirement == null) throw new ArgumentNullException(nameof(requirement));
            throw requirement.MissingRegistrationException();
        }

        internal override void EnsureType(JsTypeCatalog types, TypeKey typeKey)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            if (typeKey == null) throw new ArgumentNullException(nameof(typeKey));

            types.EnsureEmpty(typeKey);
        }

        internal override Component CreateComponent(
            ComponentId componentId,
            ComponentVendor vendor,
            TypeKey typeKey,
            ComponentContributionIntent contribution)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (vendor == null) throw new ArgumentNullException(nameof(vendor));
            if (typeKey == null) throw new ArgumentNullException(nameof(typeKey));
            if (contribution == null) throw new ArgumentNullException(nameof(contribution));

            if (contribution == ComponentContributionIntent.ObjectTarget)
                return Component.Element(
                    componentId.Value,
                    vendor.Value,
                    typeKey.Value);

            if (contribution == ComponentContributionIntent.LayoutObject)
                return Component.LayoutObject(
                    componentId.Value,
                    vendor.Value,
                    typeKey.Value);

            throw new InvalidOperationException(
                $"Component '{componentId.Value}' cannot be created as '{contribution.Kind}' without a render-time registration.");
        }

        internal override Component EnrichExistingComponent(Component existing, JsType jsType)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            if (jsType == null) throw new ArgumentNullException(nameof(jsType));
            return existing;
        }
    }

    internal sealed class ComponentRegistrationRequirement
    {
        private readonly ComponentId _componentId;
        private readonly string _componentName;
        private readonly string _registrationExample;
        private readonly string _usage;
        private readonly MemberName _valueMember;

        private ComponentRegistrationRequirement(
            ComponentId componentId,
            MemberName valueMember,
            string componentName,
            string registrationExample,
            string usage)
        {
            _componentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
            _valueMember = valueMember ?? throw new ArgumentNullException(nameof(valueMember));
            _componentName = componentName ?? throw new ArgumentNullException(nameof(componentName));
            _registrationExample = registrationExample ?? throw new ArgumentNullException(nameof(registrationExample));
            _usage = usage ?? throw new ArgumentNullException(nameof(usage));
        }

        internal static ComponentRegistrationRequirement ForFusionInPlaceEditorValueRead(string componentId) =>
            new ComponentRegistrationRequirement(
                ComponentId.Of(componentId),
                MemberName.Of("value"),
                "FusionInPlaceEditor",
                "Render the editor with Html.InputField(plan, m => m.X).FusionInPlaceEditor(...)",
                "reading .Value() in a pipeline");

        internal static ComponentRegistrationRequirement ForGatherValueRead(string componentId, string valueMember) =>
            new ComponentRegistrationRequirement(
                ComponentId.Of(componentId),
                MemberName.Of(valueMember),
                "Component",
                "Render it with a registered input helper or use a typed component source",
                "gathering '" + valueMember + "'");

        internal MemberName ValueMember => _valueMember;

        internal InvalidOperationException MissingRegistrationException() =>
            new InvalidOperationException(
                $"{_componentName} '{_componentId.Value}' is not registered. " +
                $"{_registrationExample} before {_usage}; " +
                "the registered shape drives the typed read.");
    }

    internal sealed class BehaviorGraph
    {
        private readonly ComponentCatalog _components;
        private readonly List<Behavior> _behaviors = new List<Behavior>();

        internal BehaviorGraph(ComponentCatalog components)
        {
            _components = components ?? throw new ArgumentNullException(nameof(components));
        }

        internal IReadOnlyList<Behavior> Behaviors => _behaviors;

        internal IReadOnlyList<Behavior> Snapshot() => new List<Behavior>(_behaviors);

        internal void Add(Behavior behavior)
        {
            if (behavior == null) throw new ArgumentNullException(nameof(behavior));
            RegisterEventMetadataForTrigger(behavior.StartsWhen);
            _behaviors.Add(behavior);
        }

        private void RegisterEventMetadataForTrigger(StartsWhen trigger)
        {
            if (trigger is ComponentEventTrigger componentEvent)
            {
                var componentIsAlreadyInPlan = _components.Contains(componentEvent.ComponentKey);
                if (!componentIsAlreadyInPlan)
                    return;

                _components.EnsureEvent(
                    componentEvent.ComponentKey,
                    JsEventContract.ForComponentEvent(componentEvent.EventName));
            }
        }
    }

    internal sealed class ValidationWorkQueue
    {
        private readonly List<ValidationJob> _jobs = new List<ValidationJob>();

        internal IReadOnlyList<ValidationJob> Jobs => _jobs;

        internal void Enqueue(Request request, ComponentId container, Type validationSourceType)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (validationSourceType == null) throw new ArgumentNullException(nameof(validationSourceType));

            _jobs.Add(new ValidationJob(request.Url, container, validationSourceType));
        }
    }
}
