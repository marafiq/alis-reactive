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
            var key = elementId;
            if (!_plan.MutableComponents.ContainsKey(key))
            {
                var typeKey = "native.element." + elementId;
                if (!_plan.MutableTypes.ContainsKey(typeKey))
                    _plan.MutableTypes[typeKey] = new JsType();

                _plan.MutableComponents[key] = Component.Create(elementId, "native", typeKey);
            }
            return key;
        }

        /// <summary>
        /// Ensures a component is registered with the correct vendor.
        /// Used by <see cref="ComponentRef{TComponent,TModel}"/> which knows
        /// its vendor from the <see cref="IComponent"/> instance.
        /// </summary>
        internal string EnsureComponent(string componentId, string vendor)
        {
            var key = componentId;
            if (!_plan.MutableComponents.ContainsKey(key))
            {
                var typeKey = vendor + "." + componentId;

                // When the component is a registered input component, create an enriched
                // JsType with valueMember and defaultValue so that Read(ComponentSource, "value")
                // can resolve the member even when EnsureInputComponent runs later.
                if (_components.TryGetValue(key, out var reg)
                    || TryFindRegistrationById(componentId, out reg))
                {
                    var jsType = new JsType()
                        .WithProperty(reg.ValueMember, Path.Parse(reg.ValueMember), reg.Shape, "read")
                        .WithDefaultValue(reg.ValueMember, reg.Shape);
                    _plan.MutableTypes[typeKey] = jsType;
                }
                else if (!_plan.MutableTypes.ContainsKey(typeKey))
                {
                    _plan.MutableTypes[typeKey] = new JsType();
                }

                _plan.MutableComponents[key] = Component.Create(componentId, vendor, typeKey);
            }
            else
            {
                // Component already registered — validate vendor consistency
                var existing = _plan.MutableComponents[key];
                if (existing.Vendor != vendor)
                    throw new InvalidOperationException(
                        $"Component '{componentId}' registered as vendor '{existing.Vendor}' " +
                        $"but re-referenced as '{vendor}'. A component cannot change vendor.");

                ComponentRegistration reg;
                if (!_components.TryGetValue(key, out reg))
                    TryFindRegistrationById(componentId, out reg);

                if (reg != null)
                {
                    var jsType = _plan.MutableTypes[existing.Type];
                    if (jsType.DefaultValue == null)
                    {
                        jsType.WithProperty(reg.ValueMember, Path.Parse(reg.ValueMember), reg.Shape, "read");
                        jsType.WithDefaultValue(reg.ValueMember, reg.Shape);
                    }
                }
            }
            return key;
        }

        /// <summary>
        /// Searches the components map for a registration whose ComponentId matches.
        /// The map is keyed by binding path (property name), but the componentId is
        /// the generated HTML id which differs from the binding path.
        /// </summary>
        private bool TryFindRegistrationById(string componentId, out ComponentRegistration registration)
        {
            foreach (var kvp in _components)
            {
                if (kvp.Value.ComponentId == componentId)
                {
                    registration = kvp.Value;
                    return true;
                }
            }
            registration = null;
            return false;
        }

        /// <summary>
        /// Ensures a registered input component exists in the plan with its JsType.
        /// Returns the component key.
        /// </summary>
        internal string EnsureInputComponent(string componentId, string vendor, string valueMember, Shape shape)
        {
            var key = componentId;
            if (_plan.MutableComponents.ContainsKey(key))
            {
                // Component already registered — enrich its JsType with defaultValue if missing
                var existing = _plan.MutableComponents[key];
                var jsType = _plan.MutableTypes[existing.Type];
                if (jsType.DefaultValue == null)
                {
                    jsType.WithProperty(valueMember, Path.Parse(valueMember), shape, "read");
                    jsType.WithDefaultValue(valueMember, shape);
                }
            }
            else
            {
                var typeKey = vendor + "." + componentId;
                var jsType = new JsType()
                    .WithProperty(valueMember, Path.Parse(valueMember), shape, "read")
                    .WithDefaultValue(valueMember, shape);

                _plan.MutableTypes[typeKey] = jsType;
                _plan.MutableComponents[key] = Component.Create(componentId, vendor, typeKey);
            }
            return key;
        }

        /// <summary>
        /// Ensures a property member exists on a component's JsType.
        /// Used by ElementBuilder when setting text/html/hidden.
        /// </summary>
        internal void EnsureProperty(string componentKey, string memberName, string pathExpr, Shape shape, string access)
        {
            var component = _plan.MutableComponents[componentKey];
            var jsType = _plan.MutableTypes[component.Type];
            jsType.WithProperty(memberName, Path.Parse(pathExpr), shape, access);
        }

        /// <summary>
        /// Ensures a method member exists on a component's JsType.
        /// Used by ElementBuilder for classList.add/remove/toggle, setAttribute, removeAttribute.
        /// </summary>
        internal void EnsureMethod(string componentKey, string memberName, string pathExpr, List<Shape> args = null)
        {
            var component = _plan.MutableComponents[componentKey];
            var jsType = _plan.MutableTypes[component.Type];
            jsType.WithMethod(memberName, Path.Parse(pathExpr), args);
        }

        /// <summary>
        /// Ensures an event exists on a component's JsType.
        /// </summary>
        internal void EnsureEvent(string componentKey, string eventName, string channel, string payloadType = null)
        {
            var component = _plan.MutableComponents[componentKey];
            var jsType = _plan.MutableTypes[component.Type];
            jsType.WithEvent(eventName, channel, payloadType);
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
                EnsureInputComponent(reg.ComponentId, reg.Vendor, reg.ValueMember, reg.Shape);
            }
        }

        /// <summary>
        /// Returns all registered input components for IncludeAll expansion at build time.
        /// </summary>
        internal IReadOnlyDictionary<string, ComponentRegistration> GetRegisteredComponents() => _components;

        internal void AddBehavior(Behavior behavior)
        {
            _plan.MutableBehaviors.Add(behavior);
        }
    }
}
