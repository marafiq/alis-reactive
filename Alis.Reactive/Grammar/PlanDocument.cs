using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
    internal sealed class PlanDocument
    {
        internal PlanDocument(
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

        public int Version => 6;

        public string PlanId { get; }
        public IReadOnlyDictionary<string, Vendor> Vendors { get; }
        public IReadOnlyDictionary<string, ObjectLocator> Objects { get; }
        public IReadOnlyList<Handler> Handlers { get; }
    }

    internal sealed class Vendor
    {
        internal Vendor(IReadOnlyList<string> objectPath, VendorListening listen, VendorEventPayload payload)
        {
            ObjectPath = objectPath ?? throw new ArgumentNullException(nameof(objectPath));
            Listen = listen ?? throw new ArgumentNullException(nameof(listen));
            Payload = payload;
        }

        public IReadOnlyList<string> ObjectPath { get; }
        public VendorListening Listen { get; }
        public VendorEventPayload Payload { get; }

        internal bool SameAs(Vendor other) =>
            string.Join(".", ObjectPath) == string.Join(".", other.ObjectPath)
            && Listen.SameAs(other.Listen)
            && Payload == other.Payload;
    }

    [JsonConverter(typeof(PlanNodeDiscriminator<VendorListening>))]
    internal abstract class VendorListening
    {
        private protected VendorListening() { }

        internal abstract bool SameAs(VendorListening other);
    }

    internal sealed class MethodVendorListening : VendorListening
    {
        internal MethodVendorListening(string subscribe, string unsubscribe)
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
            other is MethodVendorListening method && method.Subscribe == Subscribe && method.Unsubscribe == Unsubscribe;
    }

    internal sealed class AssignmentVendorListening : VendorListening
    {
        internal AssignmentVendorListening() { }

        public string Kind => "assignment";

        internal override bool SameAs(VendorListening other) => other is AssignmentVendorListening;
    }

    [JsonConverter(typeof(PlanNodeDiscriminator<ObjectLocator>))]
    internal abstract class ObjectLocator
    {
        private protected ObjectLocator() { }
    }

    internal sealed class ElementObjectLocator : ObjectLocator
    {
        internal ElementObjectLocator(string id, string vendor)
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

    internal sealed class PluginObjectLocator : ObjectLocator
    {
        internal PluginObjectLocator(string name, string key, PluginData data)
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

    [JsonConverter(typeof(PlanNodeDiscriminator<PluginData>))]
    internal abstract class PluginData
    {
        private protected PluginData() { }

        internal static PluginData None { get; } = new NonePluginData();
    }

    internal sealed class NonePluginData : PluginData
    {
        public string Kind => "none";
    }

    internal sealed class HubPluginData : PluginData
    {
        internal HubPluginData() { }

        public string Kind => "hub";
    }

    internal sealed class ModelPluginData : PluginData
    {
        internal ModelPluginData(IReadOnlyDictionary<string, InputBinding> inputs)
        {
            Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
        }

        public string Kind => "model";
        public IReadOnlyDictionary<string, InputBinding> Inputs { get; }
    }

    internal sealed class InputBinding
    {
        internal InputBinding(string name, IReadOnlyList<BodyStep> body, Expr value, Shape shape, string committed, string invalidClass)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Binding name required.", nameof(name));
            if (string.IsNullOrEmpty(committed)) throw new ArgumentException("Commit event required.", nameof(committed));
            if (string.IsNullOrEmpty(invalidClass)) throw new ArgumentException("Invalid class required.", nameof(invalidClass));
            Name = name;
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            Committed = committed;
            InvalidClass = invalidClass;
            List = shape.DescribesArray;
        }

        public string Name { get; }

        public IReadOnlyList<BodyStep> Body { get; }

        public Expr Value { get; }
        public Shape Shape { get; }

        public string Committed { get; }

        public string InvalidClass { get; }

        public bool List { get; }
    }

    internal sealed class FormPluginData : PluginData
    {
        internal FormPluginData(string summary, IReadOnlyList<FormField> fields)
        {
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        public string Kind => "form";

        public string Summary { get; }

        public IReadOnlyList<FormField> Fields { get; }
    }

    internal sealed class FormField
    {
        internal FormField(string name, IReadOnlyList<FormTest> tests)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Field name required.", nameof(name));
            Name = name;
            Tests = tests ?? throw new ArgumentNullException(nameof(tests));
        }

        public string Name { get; }
        public IReadOnlyList<FormTest> Tests { get; }
    }

    internal sealed class FormTest
    {
        internal FormTest(FnExpr check, FnExpr when, string message)
        {
            Check = check ?? throw new ArgumentNullException(nameof(check));
            When = when ?? throw new ArgumentNullException(nameof(when));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public FnExpr Check { get; }
        public FnExpr When { get; }
        public string Message { get; }
    }

    internal sealed class Handler
    {
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
