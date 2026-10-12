using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Grammar.Lowering
{
    /// <summary>
    /// What a lowering pass knows about the plan it lowers: its declared objects and their member
    /// contracts, the objects the grammar plan locates, and fresh binding names.
    /// </summary>
    internal sealed class LoweringContext
    {
        private int _names;

        internal LoweringContext(PlanModel.PlanDocument document)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Objects = new ObjectTable(document);
        }

        internal PlanModel.PlanDocument Document { get; }
        internal ObjectTable Objects { get; }
        internal string PlanId => Document.PlanId;

        /// <summary>A binding name no other binding of this plan uses.</summary>
        internal string Fresh(string stem) => stem + (++_names).ToString(System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Reads a member path from a target. A <c>[]</c> segment stays in the path, so
        /// <c>filesData[].rawFile</c> reads <c>get(t, ["filesData", "[]", "rawFile"])</c>: each file's
        /// <c>rawFile</c>, checked as the object it is read from is.
        /// </summary>
        internal Expr Read(Expr target, Path path, Shape shape) =>
            G.Get(target, path.Segments.Select(segment => segment.ToPathText()).ToList(), shape);

        internal bool IsDeclared(string componentKey) => Document.Components.ContainsKey(componentKey);

        internal BrowserObjectContract ContractOf(string componentKey)
        {
            if (!Document.Components.TryGetValue(componentKey, out var component))
                throw new InvalidOperationException(
                    $"'{componentKey}' is used by plan '{PlanId}' but this plan does not declare it.");
            return Document.Types[component.Type];
        }

        internal BrowserObjectContract PluginContractOf(string pluginName)
        {
            if (!Document.Types.TryGetValue("plugin." + pluginName, out var contract))
                throw new InvalidOperationException(
                    $"Plugin '{pluginName}' is used by plan '{PlanId}' but is not registered in it.");
            return contract;
        }

        internal ObjectProperty Property(BrowserObjectContract contract, string member, string owner)
        {
            if (contract.Properties.TryGetValue(member, out var property)) return property;
            throw new InvalidOperationException($"'{owner}' declares no property '{member}' in plan '{PlanId}'.");
        }

        internal ObjectMethod Method(BrowserObjectContract contract, string member, string owner)
        {
            if (contract.Methods.TryGetValue(member, out var method)) return method;
            throw new InvalidOperationException($"'{owner}' declares no method '{member}' in plan '{PlanId}'.");
        }

        /// <summary>The element id of the plan's validation summary.</summary>
        internal string SummaryId =>
            PlanId.Replace('.', '_').Replace('+', '_') + "_validation_summary";
    }

    /// <summary>
    /// The objects a grammar plan locates, by the id its expressions use. Elements keep their own id;
    /// a module's object is named <c>$name</c> or <c>$name:key</c>, which no element id can be.
    /// </summary>
    internal sealed class ObjectTable
    {
        internal const string DocumentId = "$document";
        internal const string HttpId = "$http";
        internal const string ModelId = "$model";
        internal const string ConfirmId = "$confirm";

        private readonly PlanModel.PlanDocument _document;
        private readonly Dictionary<string, ObjectLocator> _objects = new Dictionary<string, ObjectLocator>(StringComparer.Ordinal);
        private readonly List<string> _order = new List<string>();
        private readonly Dictionary<string, Vendor> _vendors = new Dictionary<string, Vendor>(StringComparer.Ordinal);

        internal ObjectTable(PlanModel.PlanDocument document)
        {
            _document = document;
            foreach (var component in document.Components.Values)
                Element(component.Id);
        }

        internal string Element(string id)
        {
            if (_objects.ContainsKey(id)) return id;
            var vendor = _document.Components.TryGetValue(id, out var component)
                ? component.ComponentVendor
                : ComponentVendor.Native;
            AddVendor(vendor.Name, GrammarVendor(vendor.Contract));
            Add(id, new ElementLocator(id, vendor.Name));
            return id;
        }

        /// <summary>An element whose handler receives the DOM event itself, for a link's click.</summary>
        internal string DomElement(string id)
        {
            AddVendor("dom", Vendor.Dom);
            Add(id, new ElementLocator(id, "dom"));
            return id;
        }

        internal string Document() => Module(DocumentId, "alis.document", "", PluginData.None);
        internal string Http() => Module(HttpId, "alis.http", "", PluginData.None);
        internal string Confirm() => Module(ConfirmId, "alis.confirm", "", PluginData.None);
        internal string Model(PluginData data) => Module(ModelId, "alis.model", _document.PlanId, data);
        internal string Model() => Module(ModelId, "alis.model", _document.PlanId, PluginData.None);
        internal string Form(string containerId, PluginData data) => Module("$form:" + containerId, "alis.form", containerId, data);
        internal string Form(string containerId) => Module("$form:" + containerId, "alis.form", containerId, PluginData.None);
        internal string Slot(string containerId) => Module("$slot:" + containerId, "alis.slot", containerId, PluginData.None);
        internal string Stream(string url) => Module("$stream:" + url, "alis.stream", url, PluginData.None);
        internal string Hub(string url) => Module("$hub:" + url, "alis.hub", url, PluginData.None);
        internal string Plugin(string name) => Module("$plugin:" + name, name, "", PluginData.None);

        internal IReadOnlyDictionary<string, ObjectLocator> Snapshot()
        {
            var snapshot = new Dictionary<string, ObjectLocator>(StringComparer.Ordinal);
            foreach (var id in _order) snapshot.Add(id, _objects[id]);
            return snapshot;
        }

        internal IReadOnlyDictionary<string, Vendor> Vendors() =>
            _vendors.OrderBy(v => v.Key, StringComparer.Ordinal).ToDictionary(v => v.Key, v => v.Value, StringComparer.Ordinal);

        // A module object is declared once; data given later replaces "no data", so a form whose
        // rules this plan carries keeps them however the plan first referred to it.
        private string Module(string id, string name, string key, PluginData data)
        {
            if (_objects.TryGetValue(id, out var existing))
            {
                var keepsExistingData = data is NoPluginData || !(((PluginLocator)existing).Data is NoPluginData);
                if (!keepsExistingData) _objects[id] = new PluginLocator(name, key, data);
                return id;
            }

            Add(id, new PluginLocator(name, key, data));
            return id;
        }

        private void Add(string id, ObjectLocator locator)
        {
            if (_objects.ContainsKey(id)) return;
            _objects.Add(id, locator);
            _order.Add(id);
        }

        private void AddVendor(string name, Vendor vendor)
        {
            if (_vendors.TryGetValue(name, out var declared))
            {
                if (!declared.SameAs(vendor))
                    throw new InvalidOperationException($"Two different vendors are named '{name}'. Give each vendor its own name.");
                return;
            }

            _vendors.Add(name, vendor);
        }

        private static Vendor GrammarVendor(VendorContract contract) =>
            new Vendor(
                G.PathOf(contract.ObjectPath),
                contract.Subscribe,
                contract.Unsubscribe,
                contract.Payload == "argument" ? Vendor.ArgumentPayload : Vendor.ElementPayload);
    }

    /// <summary>
    /// The bindings visible where a node is lowered: the handler's parameter, the response of the
    /// request whose route encloses it, and the element of the enclosing array lambda. An inner
    /// binding hides an outer one, as a C# lambda parameter or a nested route's response does.
    /// </summary>
    internal sealed class LoweringScope
    {
        private readonly string? _element;

        private LoweringScope(string eventParam, string? response, string? element)
        {
            EventParam = eventParam;
            Response = response;
            _element = element;
        }

        internal static LoweringScope ForHandler(string eventParam) => new LoweringScope(eventParam, null, null);

        internal string EventParam { get; }

        /// <summary>The binding of the innermost enclosing response, or none outside every route.</summary>
        internal string? Response { get; }

        internal string Element =>
            _element ?? throw new InvalidOperationException("An array element is read outside an array operation.");

        internal LoweringScope WithResponse(string name) => new LoweringScope(EventParam, name, _element);

        internal LoweringScope WithElement(string name) => new LoweringScope(EventParam, Response, name);
    }
}
