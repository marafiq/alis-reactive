using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Manages JsType and Component registration during plan construction.
    /// Replaces the old PlanAuthoringContext triple-indirection (contracts/objects/bindings).
    /// Each component gets its own JsType. Members are added on-demand as builders reference them.
    /// </summary>
    internal sealed class PlanBuildContext
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
        /// Ensures a DOM element is registered as a component with a JsType.
        /// Returns the component key for use in Source references.
        /// </summary>
        internal string EnsureElement(string elementId)
        {
            var key = elementId;
            if (!_plan.Components.ContainsKey(key))
            {
                var typeKey = "native.element." + elementId;
                if (!_plan.Types.ContainsKey(typeKey))
                    _plan.Types[typeKey] = new JsType();

                _plan.Components[key] = Component.Create(elementId, "native", typeKey);
            }
            return key;
        }

        /// <summary>
        /// Ensures a registered input component exists in the plan with its JsType.
        /// Returns the component key.
        /// </summary>
        internal string EnsureInputComponent(string componentId, string vendor, string readExpr, Shape shape)
        {
            var key = componentId;
            if (!_plan.Components.ContainsKey(key))
            {
                var typeKey = vendor + "." + componentId;
                var jsType = new JsType()
                    .WithProperty(readExpr, Path.Parse(readExpr), shape, "read")
                    .WithDefaultValue(readExpr, shape);

                _plan.Types[typeKey] = jsType;
                _plan.Components[key] = Component.Create(componentId, vendor, typeKey);
            }
            return key;
        }

        /// <summary>
        /// Ensures a property member exists on a component's JsType.
        /// Used by ElementBuilder when setting text/html/hidden.
        /// </summary>
        internal void EnsureProperty(string componentKey, string memberName, string pathExpr, Shape shape, string access)
        {
            var component = _plan.Components[componentKey];
            var jsType = _plan.Types[component.Type];
            jsType.WithProperty(memberName, Path.Parse(pathExpr), shape, access);
        }

        /// <summary>
        /// Ensures a method member exists on a component's JsType.
        /// Used by ElementBuilder for classList.add/remove/toggle, setAttribute, removeAttribute.
        /// </summary>
        internal void EnsureMethod(string componentKey, string memberName, string pathExpr, List<Shape> args = null)
        {
            var component = _plan.Components[componentKey];
            var jsType = _plan.Types[component.Type];
            jsType.WithMethod(memberName, Path.Parse(pathExpr), args);
        }

        /// <summary>
        /// Ensures an event exists on a component's JsType.
        /// </summary>
        internal void EnsureEvent(string componentKey, string eventName, string channel, string payloadType = null)
        {
            var component = _plan.Components[componentKey];
            var jsType = _plan.Types[component.Type];
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
                var shape = Shape.FromClrType(null); // Will be enriched from CoerceAs
                // Map CoerceAs string to Shape
                switch (reg.CoerceAs)
                {
                    case "string": shape = Shape.String; break;
                    case "number": shape = Shape.Number; break;
                    case "boolean": shape = Shape.Boolean; break;
                    case "date": shape = Shape.Date; break;
                    case "raw": shape = Shape.Raw; break;
                    case "array": shape = Shape.ArrayOf(Shape.Any); break;
                    default: shape = Shape.Any; break;
                }

                EnsureInputComponent(reg.ComponentId, reg.Vendor, reg.ReadExpr, shape);
            }
        }

        internal void AddBehavior(Behavior behavior)
        {
            _plan.Behaviors.Add(behavior);
        }
    }
}
