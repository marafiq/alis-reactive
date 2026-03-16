using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Triggers
{
    /// <summary>
    /// Fires when a component's JS event fires (e.g. SF "change", DOM "click").
    /// Created by vendor-specific .Reactive() extensions (Fusion, Native).
    ///
    /// Vendor tells the TS runtime HOW to wire:
    ///   fusion → el.ej2_instances[0].addEventListener(jsEvent, ...)
    ///   native → el.addEventListener(jsEvent, ...)
    ///
    /// BindingPath is the dot-notation model property path (e.g. "Address.City")
    /// carried for future HTTP gather support. Optional — present when the
    /// component is model-bound via an expression.
    /// </summary>
    public sealed class ComponentEventTrigger : Trigger
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "component-event";

        public string ComponentId { get; }
        public string JsEvent { get; }
        public string Vendor { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BindingPath { get; }

        /// <summary>
        /// Property path from the vendor-determined root for reading the component's value.
        /// Examples: "value" (NativeDropDown), "checked" (NativeCheckBox).
        /// Null for non-readable components (e.g. NativeButton).
        /// The runtime uses walk(el, readExpr) to extract the event payload — plan-driven,
        /// no hardcoded property lists in the runtime.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ReadExpr { get; }

        public ComponentEventTrigger(string componentId, string jsEvent, string vendor, string? bindingPath = null, string? readExpr = null)
        {
            ComponentId = componentId;
            JsEvent = jsEvent;
            Vendor = vendor;
            BindingPath = bindingPath;
            ReadExpr = readExpr;
        }
    }
}
