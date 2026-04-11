using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Manages JsType and Component registration during plan construction.
    /// Replaces the old PlanAuthoringContext triple-indirection (contracts/objects/bindings).
    /// Each component gets its own JsType. Members are added on-demand as builders reference them.
    /// </summary>
    public sealed class PlanBuildContext
    {
        private readonly Plan _plan;
        private readonly Dictionary<string, ComponentRegistration> _components;

        internal PlanBuildContext(Plan plan, Dictionary<string, ComponentRegistration> components)
        {
            _plan = plan;
            _components = components;
        }

        internal Plan Plan => _plan;

        /// <summary>
        /// Ensures a DOM element is registered as a native component with a JsType.
        /// Only used by <see cref="Builders.ElementBuilder{TModel}"/> for plain DOM elements.
        /// Returns the component key for use in Source references.
        /// </summary>
        internal string EnsureElement(string elementId)
        {
            if (_plan.MutableComponents.ContainsKey(elementId))
                return elementId;

            var typeKey = "native.element." + elementId;
            if (!_plan.MutableTypes.ContainsKey(typeKey))
                _plan.MutableTypes[typeKey] = new JsType();

            _plan.MutableComponents[elementId] = Component.Create(elementId, "native", typeKey);
            return elementId;
        }

        /// <summary>
        /// Ensures a component is registered with the correct vendor.
        /// Used by <see cref="ComponentRef{TComponent,TModel}"/> which knows
        /// its vendor from the <see cref="IComponent"/> instance.
        /// </summary>
        internal string EnsureComponent(string componentId, string vendor)
        {
            var key = componentId;

            if (_plan.MutableComponents.TryGetValue(key, out var existing))
            {
                ValidateVendor(existing, componentId, vendor);
                EnrichExistingComponent(existing, key, componentId);
                return key;
            }

            var typeKey = vendor + "." + componentId;
            var reg = FindRegistration(key, componentId);

            if (reg != null)
            {
                _plan.MutableTypes[typeKey] = CreateEnrichedType(reg.ValueMember, reg.Shape);
            }
            else if (!_plan.MutableTypes.ContainsKey(typeKey))
            {
                _plan.MutableTypes[typeKey] = new JsType();
            }

            var comp = Component.Create(componentId, vendor, typeKey);
            if (reg != null)
            {
                comp.BindingPath = reg.BindingPath;
                comp.ValueMember = reg.ValueMember;
            }
            _plan.MutableComponents[key] = comp;
            return key;
        }

        private static void ValidateVendor(Component existing, string componentId, string vendor)
        {
            if (existing.Vendor != vendor)
                throw new InvalidOperationException(
                    $"Component '{componentId}' registered as vendor '{existing.Vendor}' " +
                    $"but re-referenced as '{vendor}'. A component cannot change vendor.");
        }

        private void EnrichExistingComponent(Component existing, string key, string componentId)
        {
            var reg = FindRegistration(key, componentId);
            if (reg == null) return;

            if (existing.BindingPath == null)
                existing.BindingPath = reg.BindingPath;
            if (existing.ValueMember == null)
                existing.ValueMember = reg.ValueMember;

            var jsType = _plan.MutableTypes[existing.Type];
            EnrichTypeIfNeeded(jsType, reg.ValueMember, reg.Shape);
        }

        /// <summary>
        /// Searches the components map for a registration whose ComponentId matches.
        /// The map is keyed by binding path, but componentId is the generated HTML id.
        /// </summary>
        internal bool TryFindRegistrationById(string componentId, out ComponentRegistration? registration)
        {
            registration = FindRegistration(componentId, componentId);
            return registration != null;
        }

        private ComponentRegistration? FindRegistration(string key, string componentId)
        {
            if (_components.TryGetValue(key, out var reg))
                return reg;

            foreach (var kvp in _components)
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
            var key = componentId;

            if (_plan.MutableComponents.TryGetValue(key, out var existing))
            {
                if (bindingPath != null && existing.BindingPath == null)
                    existing.BindingPath = bindingPath;
                if (existing.ValueMember == null)
                    existing.ValueMember = valueMember;

                EnrichTypeIfNeeded(_plan.MutableTypes[existing.Type], valueMember, shape);
                return key;
            }

            var typeKey = vendor + "." + componentId;
            _plan.MutableTypes[typeKey] = CreateEnrichedType(valueMember, shape);

            var comp = Component.Create(componentId, vendor, typeKey);
            comp.BindingPath = bindingPath;
            comp.ValueMember = valueMember;
            _plan.MutableComponents[key] = comp;
            return key;
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

        private readonly HashSet<string> _registeredPlugins = new HashSet<string>();

        /// <summary>Registers a plugin's JsType. Throws on duplicate registration.</summary>
        internal void RegisterPlugin(string pluginName, Action<Builders.PluginTypeBuilder> configure)
        {
            if (!_registeredPlugins.Add(pluginName))
                throw new InvalidOperationException($"Plugin '{pluginName}' is already registered.");
            var typeKey = "plugin." + pluginName;
            _plan.MutableTypes[typeKey] = new JsType();
            configure(new Builders.PluginTypeBuilder(_plan, typeKey));
        }

        /// <summary>Ensures a plugin method exists in the plan's JsType registry.
        /// Auto-creates the JsType on first use. Methods only.</summary>
        internal void EnsurePluginMethod(string pluginName, string member, Shape returns = null)
        {
            if (string.IsNullOrWhiteSpace(pluginName))
                throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
            if (string.IsNullOrWhiteSpace(member))
                throw new System.ArgumentException("Method name required.", nameof(member));
            var typeKey = "plugin." + pluginName;
            if (!_plan.MutableTypes.ContainsKey(typeKey))
                _plan.MutableTypes[typeKey] = new JsType();
            _plan.MutableTypes[typeKey].WithMethod(member, Path.Parse(member), returns: returns);
        }

        private JsType GetJsType(string componentKey)
        {
            var component = _plan.MutableComponents[componentKey];
            return _plan.MutableTypes[component.Type];
        }

        private static JsType CreateEnrichedType(string valueMember, Shape shape)
        {
            var jsType = new JsType()
                .WithProperty(valueMember, Path.Parse(valueMember), shape, "read");
            // Always register "value" as an alias so parent validators can read any
            // input component uniformly via member:"value" — even checkbox ("checked")
            // or switch ("checked"). The alias points to the same path with the same shape.
            if (valueMember != "value")
                jsType.WithProperty("value", Path.Parse(valueMember), shape, "read");
            return jsType;
        }

        private static void EnrichTypeIfNeeded(JsType jsType, string valueMember, Shape shape)
        {
            jsType.WithProperty(valueMember, Path.Parse(valueMember), shape, "read");
            if (valueMember != "value")
                jsType.WithProperty("value", Path.Parse(valueMember), shape, "read");
        }

        /// <summary>
        /// Registers all input components from the ReactivePlan's ComponentsMap into the Plan.
        /// Called during Render().
        /// </summary>
        internal void RegisterInputComponents()
        {
            foreach (var kvp in _components)
            {
                var reg = kvp.Value;
                EnsureInputComponent(reg.ComponentId, reg.Vendor, reg.ValueMember, reg.Shape, kvp.Key);
            }
        }

        /// <summary>
        /// Returns all registered input components for IncludeAll expansion at build time.
        /// </summary>
        internal IReadOnlyDictionary<string, ComponentRegistration> GetRegisteredComponents() => _components;

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
            // Auto-register JsEvent for component-event triggers so the plan
            // carries event metadata. Covers all 26 Reactive extensions.
            if (behavior.StartsWhen is ComponentEventTrigger cet
                && _plan.MutableComponents.ContainsKey(cet.Component))
            {
                EnsureEvent(cet.Component, cet.Event, cet.Event);
            }

            _plan.MutableBehaviors.Add(behavior);
        }
    }
}
