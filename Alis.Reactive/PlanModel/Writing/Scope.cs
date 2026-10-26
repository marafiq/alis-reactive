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
        private readonly ResultFrame? _results;

        private Scope(PlanWriter plan, string eventParam, ResponseFrame? responses, string? item, ResultFrame? results)
        {
            _plan = plan;
            EventParam = eventParam;
            _responses = responses;
            _item = item;
            _results = results;
        }

        internal static Scope ForHandler(PlanWriter plan, string eventParam) => new Scope(plan, eventParam, null, null, null);

        /// <summary>The scope of a block: it binds the method results its statements use, and sees its parent's.</summary>
        internal Scope ForBlock() => new Scope(_plan, EventParam, _responses, _item, new ResultFrame(_results));

        internal ObjectTable Objects => _plan.Objects;

        internal string PlanId => _plan.PlanId;

        internal string EventParam { get; }

        internal string Item =>
            _item ?? throw new InvalidOperationException("An array element is read outside an array operation.");

        internal string Fresh(string stem) => _plan.Fresh(stem);

        internal Scope WithResponse(ResponseHandle handle, string binding) =>
            new Scope(_plan, EventParam, new ResponseFrame(handle, binding, _responses), _item, _results);

        internal Scope WithItem(string name) => new Scope(_plan, EventParam, _responses, name, _results);

        /// <summary>
        /// What a method returned, bound once per block by a <c>let</c> hoisted before the statement that
        /// first reads it; a later read in the block, or in a block inside it, is the name (grammar.md 3.1).
        /// </summary>
        internal Expr Result(object result, string stem, bool awaits, string member, Func<Expr> call)
        {
            for (var frame = _results; frame != null; frame = frame.Outer)
                if (frame.TryGet(result, out var bound)) return G.Ref(bound);

            if (_results == null)
                throw new InvalidOperationException(
                    $"Plan '{PlanId}' calls '{member}' outside a reaction. Call a method inside a trigger, a component's " +
                    ".Reactive, Then, Else, OnSuccess, OnError, WhileLoading, Finally or OnAllSettled callback.");

            var name = Fresh(stem);
            _plan.Hoist(G.Let(name, call(), awaits), member);
            _results.Add(result, name);
            return G.Ref(name);
        }

        /// <summary>The binding of the response the handle names; the innermost one for <see cref="ResponseHandle.Innermost"/>.</summary>
        internal string Response(ResponseHandle handle)
        {
            if (handle == ResponseHandle.Innermost && _responses != null) return _responses.Binding;
            for (var frame = _responses; frame != null; frame = frame.Outer)
                if (frame.Handle == handle) return frame.Binding;
            throw new InvalidOperationException(
                $"Plan '{PlanId}' reads a response outside the routes of the request that receives it. Read a " +
                "response inside OnSuccess, OnError, Chained or Finally of its request.");
        }

        /// <summary>
        /// Writes one statement's worth of steps; the let that binds a method result the statement reads,
        /// awaited when the method returns a promise, lands just before them.
        /// </summary>
        internal IReadOnlyList<Statement> WithHoisted(Func<IReadOnlyList<Statement>> write) => _plan.WithHoisted(write);

        /// <summary>Writes a value where no statement runs before it is read: inside an array operation's lambda.</summary>
        internal T WithoutHoisting<T>(Func<T> write) => _plan.WithoutHoisting(write);


        private sealed class ResultFrame
        {
            private readonly Dictionary<object, string> _bound = new Dictionary<object, string>();

            internal ResultFrame(ResultFrame? outer)
            {
                Outer = outer;
            }

            internal ResultFrame? Outer { get; }

            internal bool TryGet(object result, out string name) => _bound.TryGetValue(result, out name!);

            internal void Add(object result, string name) => _bound.Add(result, name);
        }

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

    /// <summary>The plan being written: its objects, its fresh names and the statements hoisted before the one being written.</summary>
    internal sealed class PlanWriter
    {
        private int _names;

        // The statements hoisted before the statement being written, such as the let that binds a method's
        // result; null where no statement can run before a value is read, as in an array operation.
        private List<Statement>? _hoisted;

        internal PlanWriter(string planId, ObjectTable objects)
        {
            PlanId = planId;
            Objects = objects;
        }

        internal string PlanId { get; }

        internal ObjectTable Objects { get; }

        internal string Fresh(string stem) => stem + (++_names).ToString(CultureInfo.InvariantCulture);

        internal IReadOnlyList<Statement> WithHoisted(Func<IReadOnlyList<Statement>> write)
        {
            var outer = _hoisted;
            var hoisted = new List<Statement>();
            _hoisted = hoisted;
            try
            {
                var statements = write();
                return hoisted.Count == 0 ? statements : hoisted.Concat(statements).ToList();
            }
            finally
            {
                _hoisted = outer;
            }
        }

        internal T WithoutHoisting<T>(Func<T> write)
        {
            var outer = _hoisted;
            _hoisted = null;
            try
            {
                return write();
            }
            finally
            {
                _hoisted = outer;
            }
        }

        internal void Hoist(Statement statement, string member)
        {
            if (_hoisted == null)
                throw new InvalidOperationException(
                    $"Plan '{PlanId}' calls '{member}' inside an array operation or a validation rule, where no " +
                    "statement runs before the value is read. Call it in a reaction step instead.");

            _hoisted.Add(statement);
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
            if (!_elementVendors.TryGetValue(id, out var vendor))
                throw new InvalidOperationException($"Element '{id}' is used by a step before the plan declared it.");
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

        // A module object is declared once, with its data where it has any: the plan writes the model's
        // inputs and each form's rules before any step names them.
        private string Module(string id, string name, string key, PluginData data)
        {
            if (!_objects.ContainsKey(id))
                Add(id, new PluginLocator(name, key, data));
            else if (!(data is NoPluginData))
                throw new InvalidOperationException($"'{id}' is declared twice with data.");
            return id;
        }

        private void Add(string id, ObjectLocator locator)
        {
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
