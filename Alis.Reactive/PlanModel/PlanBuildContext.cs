using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Accumulates JsType, Component, and Behavior registrations during plan construction,
    /// then snapshots them into an immutable <see cref="PlanModel.Plan"/> via <see cref="BuildPlan"/>.
    /// Each component gets its own JsType. Members are added on-demand as builders reference them.
    /// </summary>
    public sealed class PlanBuildContext
    {
        private readonly string _planId;
        private readonly string? _partId;
        private readonly Dictionary<string, JsType> _types = new Dictionary<string, JsType>();
        private readonly Dictionary<string, Component> _components = new Dictionary<string, Component>();
        private readonly List<Behavior> _behaviors = new List<Behavior>();
        private readonly Dictionary<string, ComponentRegistration> _registrations;
        private readonly HashSet<string> _registeredPlugins = new HashSet<string>();
        private readonly List<(Request Request, Type ValidatorType)> _validationJobs =
            new List<(Request, Type)>();

        internal PlanBuildContext(
            string planId, string? partId, Dictionary<string, ComponentRegistration> registrations)
        {
            _planId = planId;
            _partId = partId;
            _registrations = registrations;
        }

        /// <summary>Snapshots the accumulated state into an immutable plan document.</summary>
        internal Plan BuildPlan() =>
            new Plan(
                _planId,
                _partId,
                new Dictionary<string, JsType>(_types),
                new Dictionary<string, Component>(_components),
                new List<Behavior>(_behaviors));

        /// <summary>Behaviors collected so far — a read-only view for mid-build inspection.</summary>
        internal IReadOnlyList<Behavior> Behaviors => _behaviors;

        /// <summary>Gets a component already registered in the plan.</summary>
        internal Component GetComponent(string key) => _components[key];

        /// <summary>Replaces a registered component (used when enriching it with a container scope).</summary>
        internal void SetComponent(string key, Component component) => _components[key] = component;

        /// <summary>Records that a request must run validation before it is sent, with its validator type.</summary>
        internal void RegisterValidationJob(Request request, Type validatorType) =>
            _validationJobs.Add((request, validatorType));

        /// <summary>Requests that declared a validator at build time, each paired with its validator type.</summary>
        internal IReadOnlyList<(Request Request, Type ValidatorType)> ValidationJobs => _validationJobs;

        /// <summary>
        /// Ensures a DOM element is registered as a native component with a JsType.
        /// Only used by <see cref="Builders.ElementBuilder{TModel}"/> for plain DOM elements.
        /// Returns the component key for use in Source references.
        /// </summary>
        internal string EnsureElement(string elementId)
        {
            if (_components.ContainsKey(elementId))
                return elementId;

            var typeKey = "native.element." + elementId;
            if (!_types.ContainsKey(typeKey))
                _types[typeKey] = new JsType();

            _components[elementId] = Component.Create(elementId, "native", typeKey);
            return elementId;
        }

        /// <summary>
        /// Ensures a component is registered with the correct vendor.
        /// Used by <see cref="ComponentRef{TComponent,TModel}"/> which knows
        /// its vendor from the <see cref="IComponent"/> instance.
        /// </summary>
        internal string EnsureComponent(string componentId, string vendor)
        {
            if (_components.TryGetValue(componentId, out var existing))
            {
                ValidateVendor(existing, componentId, vendor);
                EnrichExistingComponent(existing, componentId);
                return componentId;
            }

            var typeKey = vendor + "." + componentId;
            var reg = FindRegistration(componentId);

            if (reg != null)
            {
                _types[typeKey] = CreateEnrichedType(reg.ValueMember, reg.Shape);
            }
            else if (!_types.ContainsKey(typeKey))
            {
                _types[typeKey] = new JsType();
            }

            _components[componentId] = Component.Create(
                componentId, vendor, typeKey, reg?.BindingPath, reg?.ValueMember);
            return componentId;
        }

        private static void ValidateVendor(Component existing, string componentId, string vendor)
        {
            if (existing.Vendor != vendor)
                throw new InvalidOperationException(
                    $"Component '{componentId}' registered as vendor '{existing.Vendor}' " +
                    $"but re-referenced as '{vendor}'. A component cannot change vendor.");
        }

        private void EnrichExistingComponent(Component existing, string componentId)
        {
            var reg = FindRegistration(componentId);
            if (reg == null) return;

            var jsType = _types[existing.Type];
            _components[componentId] = existing.WithBindingIfAbsent(reg.BindingPath, reg.ValueMember);
            EnrichTypeIfNeeded(jsType, reg.ValueMember, reg.Shape);
        }

        /// <summary>
        /// Searches the registration map for a registration whose ComponentId matches.
        /// The map is keyed by binding path, but componentId is the generated HTML id.
        /// </summary>
        internal bool TryFindRegistrationById(string componentId, out ComponentRegistration? registration)
        {
            registration = FindRegistration(componentId);
            return registration != null;
        }

        private ComponentRegistration? FindRegistration(string componentId)
        {
            if (_registrations.TryGetValue(componentId, out var reg))
                return reg;

            // _registrations is a live map the caller keeps populating after this context
            // is constructed, so resolve by ComponentId against its current contents.
            foreach (var kvp in _registrations)
            {
                if (kvp.Value.ComponentId == componentId)
                    return kvp.Value;
            }

            return null;
        }

        /// <summary>
        /// Ensures a registered input component exists in the plan with its JsType.
        /// Returns the component key.
        /// </summary>
        internal string EnsureInputComponent(string componentId, string vendor, string valueMember, Shape shape, string? bindingPath = null)
        {
            if (_components.TryGetValue(componentId, out var existing))
            {
                EnrichTypeIfNeeded(_types[existing.Type], valueMember, shape);
                _components[componentId] = existing.WithBindingIfAbsent(bindingPath, valueMember);
                return componentId;
            }

            var typeKey = vendor + "." + componentId;
            _types[typeKey] = CreateEnrichedType(valueMember, shape);

            _components[componentId] = Component.Create(
                componentId, vendor, typeKey, bindingPath, valueMember);
            return componentId;
        }

        /// <summary>
        /// Ensures a property member exists on a component's JsType.
        /// Used by ElementBuilder when setting text/html/hidden.
        /// </summary>
        internal void EnsureProperty(string componentKey, string memberName, string pathExpr, Shape shape, string access)
        {
            var jsType = GetJsType(componentKey);
            jsType.WithProperty(memberName, Path.Parse(pathExpr), shape, access);
        }

        /// <summary>
        /// Ensures a method member exists on a component's JsType.
        /// Used by ElementBuilder for classList.add/remove/toggle, setAttribute, removeAttribute.
        /// </summary>
        internal void EnsureMethod(string componentKey, string memberName, string pathExpr, List<Shape>? args = null, Shape? returns = null)
        {
            var jsType = GetJsType(componentKey);
            jsType.WithMethod(memberName, Path.Parse(pathExpr), args, returns);
        }

        /// <summary>
        /// Ensures an event exists on a component's JsType.
        /// </summary>
        internal void EnsureEvent(string componentKey, string eventName, string channel, string? payloadType = null)
        {
            var jsType = GetJsType(componentKey);
            jsType.WithEvent(eventName, channel, payloadType);
        }

        /// <summary>Registers a plugin's JsType. Throws on duplicate registration.</summary>
        internal void RegisterPlugin(string pluginName, Action<Builders.PluginTypeBuilder> configure)
        {
            if (!_registeredPlugins.Add(pluginName))
                throw new InvalidOperationException($"Plugin '{pluginName}' is already registered.");
            var jsType = new JsType();
            _types["plugin." + pluginName] = jsType;
            configure(new Builders.PluginTypeBuilder(jsType));
        }

        /// <summary>Ensures a plugin method exists in the plan's JsType registry.
        /// Auto-creates the JsType on first use. Methods only.</summary>
        internal void EnsurePluginMethod(string pluginName, string member, Shape? returns = null)
        {
            if (string.IsNullOrWhiteSpace(pluginName))
                throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            if (string.IsNullOrWhiteSpace(member))
                throw new System.ArgumentException("Method name required.", nameof(member));
            var typeKey = "plugin." + pluginName;
            if (!_types.ContainsKey(typeKey))
                throw new System.InvalidOperationException(
                    $"Plugin '{pluginName}' is not registered. Call plan.RegisterPlugin(\"{pluginName}\", ...) first.");
            _types[typeKey].WithMethod(member, Path.Parse(member), returns: returns);
        }

        private JsType GetJsType(string componentKey)
        {
            var component = _components[componentKey];
            return _types[component.Type];
        }

        private static JsType CreateEnrichedType(string valueMember, Shape shape)
        {
            var jsType = new JsType();
            EnrichTypeIfNeeded(jsType, valueMember, shape);
            return jsType;
        }

        private static void EnrichTypeIfNeeded(JsType jsType, string valueMember, Shape shape)
        {
            var valuePath = Path.Parse(valueMember);
            jsType.WithProperty(valueMember, valuePath, shape, "read");
            // Always register "value" as an alias so parent validators can read any
            // input component uniformly via member:"value" — even checkbox ("checked")
            // or switch ("checked"). The alias points to the same path with the same shape.
            if (valueMember != "value")
                jsType.WithProperty("value", valuePath, shape, "read");
        }

        /// <summary>
        /// Registers all input components from the ReactivePlan's ComponentsMap into the plan.
        /// Called during Render().
        /// </summary>
        internal void RegisterInputComponents()
        {
            foreach (var kvp in _registrations)
            {
                var reg = kvp.Value;
                EnsureInputComponent(reg.ComponentId, reg.Vendor, reg.ValueMember, reg.Shape, kvp.Key);
            }
        }

        /// <summary>
        /// Returns all registered input components for IncludeAll expansion at build time.
        /// </summary>
        internal IReadOnlyDictionary<string, ComponentRegistration> GetRegisteredComponents() => _registrations;

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

        internal void AddBehavior(Behavior behavior)
        {
            RegisterEventMetadataForTrigger(behavior.StartsWhen);
            _behaviors.Add(behavior);
        }

        /// <summary>
        /// A component-event trigger needs its <see cref="JsEvent"/> in the plan so the
        /// runtime carries the event metadata. Covers all 26 Reactive extensions.
        /// </summary>
        private void RegisterEventMetadataForTrigger(StartsWhen trigger)
        {
            if (trigger is ComponentEventTrigger cet
                && _components.ContainsKey(cet.Component))
            {
                EnsureEvent(cet.Component, cet.Event, cet.Event);
            }
        }
    }
}
