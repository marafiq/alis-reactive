using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// The plan a view renders (grammar.md 1.1): the vendors its components come from, the objects it
    /// locates, and the handlers it subscribes. The document names no feature.
    /// </summary>
    internal sealed class GrammarPlan
    {
        internal GrammarPlan(
            string planId,
            IReadOnlyDictionary<string, Vendor> vendors,
            IReadOnlyDictionary<string, ObjectLocator> objects,
            IReadOnlyList<Handler> handlers)
        {
            if (string.IsNullOrEmpty(planId)) throw new ArgumentException("Plan id required.", nameof(planId));
            PlanId = planId;
            Vendors = vendors ?? throw new ArgumentNullException(nameof(vendors));
            Objects = objects ?? throw new ArgumentNullException(nameof(objects));
            Handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        }

        public int Version => 5;

        public string PlanId { get; }
        public IReadOnlyDictionary<string, Vendor> Vendors { get; }
        public IReadOnlyDictionary<string, ObjectLocator> Objects { get; }
        public IReadOnlyList<Handler> Handlers { get; }
    }

    /// <summary>Where a vendor's objects live on an element, how they take a listener, and what a listener receives.</summary>
    internal sealed class Vendor
    {
        /// <summary>What a vendor's listener receives, as the plan names it: one name per declared kind.</summary>
        internal static string PayloadOf(VendorEventPayload payload) => payload switch
        {
            VendorEventPayload.Argument => "argument",
            VendorEventPayload.DomEvent => "element",
            _ => throw new ArgumentOutOfRangeException(nameof(payload), payload, "A vendor payload the plan has no name for."),
        };

        internal Vendor(IReadOnlyList<string> objectPath, VendorListening listen, string payload)
        {
            ObjectPath = objectPath ?? throw new ArgumentNullException(nameof(objectPath));
            Listen = listen ?? throw new ArgumentNullException(nameof(listen));
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }

        public IReadOnlyList<string> ObjectPath { get; }
        public VendorListening Listen { get; }

        [ContractType("VendorPayload")]
        public string Payload { get; }

        internal bool SameAs(Vendor other) =>
            string.Join(".", ObjectPath) == string.Join(".", other.ObjectPath)
            && Listen.SameAs(other.Listen)
            && Payload == other.Payload;
    }

    /// <summary>
    /// How a vendor's objects take a listener (grammar.md 1.1): through a pair of methods, or by
    /// assignment to the property the event names. These two are the whole set; the method pair
    /// carries its names and the assignment has nothing to carry, so a vendor cannot name half of each.
    /// </summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<VendorListening>))]
    internal abstract class VendorListening
    {
        private protected VendorListening() { }

        internal abstract bool SameAs(VendorListening other);
    }

    /// <summary><c>method(subscribe, unsubscribe)</c>: <c>object.on(event, fn)</c> adds a listener and <c>object.off(event, fn)</c> removes it.</summary>
    internal sealed class MethodListening : VendorListening
    {
        internal MethodListening(string subscribe, string unsubscribe)
        {
            if (string.IsNullOrEmpty(subscribe)) throw new ArgumentException("Subscribe method required.", nameof(subscribe));
            if (string.IsNullOrEmpty(unsubscribe)) throw new ArgumentException("Unsubscribe method required.", nameof(unsubscribe));
            Subscribe = subscribe;
            Unsubscribe = unsubscribe;
        }

        public string Kind => "method";
        public string Subscribe { get; }
        public string Unsubscribe { get; }

        internal override bool SameAs(VendorListening other) =>
            other is MethodListening method && method.Subscribe == Subscribe && method.Unsubscribe == Unsubscribe;
    }

    /// <summary>
    /// <c>assignment</c>: <c>object.onEvent = fn</c> adds the listener after the function the property
    /// already holds, and what the property held is put back when the handler's owner unloads.
    /// </summary>
    internal sealed class AssignmentListening : VendorListening
    {
        internal AssignmentListening() { }

        public string Kind => "assignment";

        internal override bool SameAs(VendorListening other) => other is AssignmentListening;
    }

    /// <summary>How the runtime finds an object: on an element, or made by a registered plugin.</summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<ObjectLocator>))]
    internal abstract class ObjectLocator
    {
        private protected ObjectLocator() { }
    }

    /// <summary><c>element(id, vendor)</c>: the vendor's object on the element with this id.</summary>
    internal sealed class ElementLocator : ObjectLocator
    {
        internal ElementLocator(string id, string vendor)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Element id required.", nameof(id));
            if (string.IsNullOrEmpty(vendor)) throw new ArgumentException("Vendor required.", nameof(vendor));
            Id = id;
            Vendor = vendor;
        }

        public string Kind => "element";
        public string Id { get; }
        public string Vendor { get; }
    }

    /// <summary><c>plugin(name, key, data)</c>: an object of the plugin a bundle registered under the name.</summary>
    internal sealed class PluginLocator : ObjectLocator
    {
        internal PluginLocator(string name, string key, PluginData data)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Plugin name required.", nameof(name));
            Name = name;
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public string Kind => "plugin";
        public string Name { get; }
        public string Key { get; }
        public PluginData Data { get; }
    }

    /// <summary>What a plan contributes to a plugin, under its owner.</summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<PluginData>))]
    internal abstract class PluginData
    {
        private protected PluginData() { }

        internal static PluginData None { get; } = new NoPluginData();
    }

    /// <summary>A plugin object that takes no data from the plan.</summary>
    internal sealed class NoPluginData : PluginData
    {
        public string Kind => "none";
    }

    /// <summary>
    /// <c>alis.hub</c> data: it carries nothing but itself. A plan that names a hub contributes it, so the plan
    /// holds the hub's connection for as long as its owner lives (grammar.md 6.7, B1).
    /// </summary>
    internal sealed class HubPluginData : PluginData
    {
        internal HubPluginData() { }

        public string Kind => "hub";
    }

    /// <summary><c>alis.model</c> data: the inputs this plan rendered, by element id (grammar.md 6.3).</summary>
    internal sealed class ModelPluginData : PluginData
    {
        internal ModelPluginData(IReadOnlyDictionary<string, InputBinding> inputs)
        {
            Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
        }

        public string Kind => "model";
        public IReadOnlyDictionary<string, InputBinding> Inputs { get; }
    }

    /// <summary>
    /// An input bound to the model: its MVC name, its JSON body path, how its value reads, and the
    /// event its component raises when a value is committed.
    /// </summary>
    internal sealed class InputBinding
    {
        internal InputBinding(string name, IReadOnlyList<object> body, Expr value, Shape shape, string committed)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Binding name required.", nameof(name));
            if (string.IsNullOrEmpty(committed)) throw new ArgumentException("Commit event required.", nameof(committed));
            Name = name;
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            Committed = committed;
            List = shape.DescribesArray;
        }

        public string Name { get; }

        [ContractType("(string | number)[]")]
        public IReadOnlyList<object> Body { get; }

        public Expr Value { get; }
        public Shape Shape { get; }

        /// <summary>The component's event that commits a value; the model re-raises it as <c>alis:commit</c>.</summary>
        public string Committed { get; }

        /// <summary>Whether the input binds a list, so a request repeats its key per item and sends nothing when it is missing.</summary>
        public bool List { get; }
    }

    /// <summary><c>alis.form</c> data: the fields a validation container checks (grammar.md 6.4).</summary>
    internal sealed class FormPluginData : PluginData
    {
        internal FormPluginData(string summary, IReadOnlyList<FormField> fields)
        {
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        public string Kind => "form";

        /// <summary>The element id of the form's validation summary, <c>{formId}_validation_summary</c>, which the server renders.</summary>
        public string Summary { get; }

        public IReadOnlyList<FormField> Fields { get; }
    }

    /// <summary>A field of a form: its element, the name the server reports errors under, its value and tests.</summary>
    internal sealed class FormField
    {
        internal FormField(string id, string name, Expr value, IReadOnlyList<FormTest> tests)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Field id required.", nameof(id));
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Tests = tests ?? throw new ArgumentNullException(nameof(tests));
        }

        public string Id { get; }
        public string Name { get; }
        public Expr Value { get; }
        public IReadOnlyList<FormTest> Tests { get; }
    }

    /// <summary>
    /// A test of a field: the check its value must pass while the condition holds. The rule names
    /// the declaration it came from, for traces; the check alone decides.
    /// </summary>
    internal sealed class FormTest
    {
        internal FormTest(string rule, FnExpr check, Expr when, string message)
        {
            if (string.IsNullOrEmpty(rule)) throw new ArgumentException("Rule name required.", nameof(rule));
            Rule = rule;
            Check = check ?? throw new ArgumentNullException(nameof(check));
            When = when ?? throw new ArgumentNullException(nameof(when));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string Rule { get; }
        public FnExpr Check { get; }
        public Expr When { get; }
        public string Message { get; }
    }

    /// <summary>
    /// <c>on(object, path, event) { do }</c>: a handler of an event of the object, or of the object it holds at
    /// <c>path</c>. The payload is bound as <c>e</c> (<see cref="Payload"/>) for the handler's block.
    /// </summary>
    internal sealed class Handler
    {
        /// <summary>The name a handler's block reads its payload by.</summary>
        internal const string Payload = "e";

        internal Handler(string on, IReadOnlyList<string> path, string eventName, IReadOnlyList<Statement> body)
        {
            if (string.IsNullOrEmpty(on)) throw new ArgumentException("Object id required.", nameof(on));
            if (string.IsNullOrEmpty(eventName)) throw new ArgumentException("Event required.", nameof(eventName));
            On = on;
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Event = eventName;
            Do = body ?? throw new ArgumentNullException(nameof(body));
        }

        public string On { get; }
        public IReadOnlyList<string> Path { get; }
        public string Event { get; }
        public IReadOnlyList<Statement> Do { get; }
    }
}
