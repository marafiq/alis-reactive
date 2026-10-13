using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Where a step or a value is written: the plan's objects and fresh names, and the names a callback
    /// binds (its event parameter, the responses of the requests it sits in, an array item).
    /// </summary>
    internal sealed class Scope
    {
        private readonly PlanWriter _plan;
        private readonly string? _item;
        private readonly ResponseFrame? _responses;

        private Scope(PlanWriter plan, string eventParam, ResponseFrame? responses, string? item)
        {
            _plan = plan;
            EventParam = eventParam;
            _responses = responses;
            _item = item;
        }

        internal static Scope ForHandler(PlanWriter plan, string eventParam) => new Scope(plan, eventParam, null, null);

        internal ObjectTable Objects => _plan.Objects;

        internal string PlanId => _plan.PlanId;

        internal string EventParam { get; }

        internal string Item =>
            _item ?? throw new InvalidOperationException("An array element is read outside an array operation.");

        internal string Fresh(string stem) => _plan.Fresh(stem);

        internal Scope WithResponse(ResponseHandle handle, string binding) =>
            new Scope(_plan, EventParam, new ResponseFrame(handle, binding, _responses), _item);

        internal Scope WithItem(string name) => new Scope(_plan, EventParam, _responses, name);

        /// <summary>The binding of the response the handle names; the innermost one for <see cref="ResponseHandle.Innermost"/>.</summary>
        internal string? ResponseOf(ResponseHandle handle)
        {
            if (handle == ResponseHandle.Innermost) return _responses?.Binding;
            for (var frame = _responses; frame != null; frame = frame.Outer)
                if (frame.Handle == handle) return frame.Binding;
            return null;
        }

        /// <summary>
        /// Writes one statement's worth of steps; a value it reads that returns a promise settles in a
        /// <c>let</c> just before them.
        /// </summary>
        internal IReadOnlyList<Statement> WithAwaitedReads(Func<IReadOnlyList<Statement>> write) => _plan.WithAwaitedReads(write);

        /// <summary>Writes a value where nothing waits for a promise: inside an array operation's lambda.</summary>
        internal T WithoutAwaiting<T>(Func<T> write) => _plan.WithoutAwaiting(write);

        internal Expr Awaited(Expr call, string member) => _plan.Awaited(call, member);

        private sealed class ResponseFrame
        {
            internal ResponseFrame(ResponseHandle handle, string binding, ResponseFrame? outer)
            {
                Handle = handle;
                Binding = binding;
                Outer = outer;
            }

            internal ResponseHandle Handle { get; }

            internal string Binding { get; }

            internal ResponseFrame? Outer { get; }
        }
    }

    /// <summary>
    /// A request's response, named by the routes that read it. A body read inside a nested route still
    /// reads the response of the request it names; <see cref="Innermost"/> reads the nearest one.
    /// </summary>
    internal sealed class ResponseHandle
    {
        internal static ResponseHandle Innermost { get; } = new ResponseHandle();
    }

    /// <summary>The plan being written: its objects, its fresh names and the awaited reads of the statement being written.</summary>
    internal sealed class PlanWriter
    {
        private int _names;

        // The awaited reads of the statement being written; null where nothing can wait for one.
        private List<Statement>? _awaitedReads;

        internal PlanWriter(string planId, ObjectTable objects)
        {
            PlanId = planId;
            Objects = objects;
        }

        internal string PlanId { get; }

        internal ObjectTable Objects { get; }

        internal string Fresh(string stem) => stem + (++_names).ToString(CultureInfo.InvariantCulture);

        internal IReadOnlyList<Statement> WithAwaitedReads(Func<IReadOnlyList<Statement>> write)
        {
            var outer = _awaitedReads;
            var awaited = new List<Statement>();
            _awaitedReads = awaited;
            try
            {
                var statements = write();
                return awaited.Count == 0 ? statements : awaited.Concat(statements).ToList();
            }
            finally
            {
                _awaitedReads = outer;
            }
        }

        internal T WithoutAwaiting<T>(Func<T> write)
        {
            var outer = _awaitedReads;
            _awaitedReads = null;
            try
            {
                return write();
            }
            finally
            {
                _awaitedReads = outer;
            }
        }

        internal Expr Awaited(Expr call, string member)
        {
            if (_awaitedReads == null)
                throw new InvalidOperationException(
                    $"Plan '{PlanId}' reads '{member}', which returns a promise, inside an array operation or a " +
                    "validation rule, where nothing waits for it. Read it in a reaction step instead.");

            var settled = Fresh("r");
            _awaitedReads.Add(G.Let(settled, call, awaits: true));
            return G.Ref(settled);
        }
    }

    /// <summary>
    /// The objects a plan names, in the order it first names them: elements by id and vendor, and the
    /// modules a bundle registered (the document, HTTP, forms, slots, streams, hubs, plugins).
    /// </summary>
    internal sealed class ObjectTable
    {
        internal const string DocumentId = "$document";
        internal const string HttpId = "$http";
        internal const string ModelId = "$model";
        internal const string ConfirmId = "$confirm";

        private readonly string _planId;
        private readonly Dictionary<string, ObjectLocator> _objects = new Dictionary<string, ObjectLocator>(StringComparer.Ordinal);
        private readonly List<string> _order = new List<string>();
        private readonly Dictionary<string, ComponentVendor> _elementVendors = new Dictionary<string, ComponentVendor>(StringComparer.Ordinal);
        private readonly Dictionary<string, Vendor> _vendors = new Dictionary<string, Vendor>(StringComparer.Ordinal);

        internal ObjectTable(string planId, IEnumerable<DeclaredObject> declared)
        {
            _planId = planId;
            foreach (var element in declared)
            {
                _elementVendors[element.Id] = element.Vendor;
                Element(element.Id);
            }
        }

        internal string Element(string id)
        {
            if (_objects.ContainsKey(id)) return id;
            var vendor = _elementVendors.TryGetValue(id, out var declared) ? declared : ComponentVendor.Native;
            AddVendor(vendor.Name, vendor.ToGrammar());
            Add(id, new ElementLocator(id, vendor.Name));
            return id;
        }

        internal string Document() => Module(DocumentId, "alis.document", "", PluginData.None);

        internal string Http() => Module(HttpId, "alis.http", "", PluginData.None);

        internal string Confirm() => Module(ConfirmId, "alis.confirm", "", PluginData.None);

        internal string Model(PluginData data) => Module(ModelId, "alis.model", _planId, data);

        internal string Model() => Module(ModelId, "alis.model", _planId, PluginData.None);

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
    }
}
