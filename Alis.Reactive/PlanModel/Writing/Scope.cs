using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel.Writing
{
    internal delegate IReadOnlyList<Statement> BranchWriter(Scope scope);

    internal sealed class Scope
    {
        private readonly PlanWriter _plan;
        private readonly string? _item;
        private readonly ResponseFrame? _responses;
        private readonly ResultFrame _results;

        private Scope(PlanWriter plan, string eventParam, ResponseFrame? responses, string? item, ResultFrame results)
        {
            _plan = plan;
            EventParam = eventParam;
            _responses = responses;
            _item = item;
            _results = results;
        }

        internal static Scope ForHandler(PlanWriter plan, string eventParam) => new Scope(plan, eventParam, null, null, new ResultFrame(null));

        internal Scope ForBlock() => new Scope(_plan, EventParam, _responses, _item, new ResultFrame(_results));

        internal PlanObjects Objects => _plan.Objects;

        internal string PlanId => _plan.PlanId;

        internal string EventParam { get; }

        internal string Item
            => _item ?? throw new InvalidOperationException("An array element is read outside an array operation.");

        internal string Fresh(string stem) => _plan.Fresh(stem);

        internal Scope WithResponse(ResponseHandle handle, string binding)
            => new Scope(_plan, EventParam, new ResponseFrame(handle, binding, _responses), _item, _results);

        internal bool HasResponse => _responses != null;

        internal Scope WithItem(string name) => new Scope(_plan, EventParam, _responses, name, _results);

        internal Expr Result(object result, string stem, bool awaits, Func<Expr> call)
        {
            for (var frame = _results; frame != null; frame = frame.Outer)
                if (frame.TryGet(result, out var bound)) return G.Ref(bound);

            var name = Fresh(stem);
            _plan.Hoist(G.Let(name, call(), awaits));
            _results.Add(result, name);
            _plan.Written(result);
            return G.Ref(name);
        }

        internal void DeferOwnLine(object result, int index, Func<IReadOnlyList<Statement>> write)
            => _results.Defer(result, index, write);

        internal void WriteDeferred(List<Statement> block)
        {
            foreach (var (result, index, write) in _results.Deferred)
            {
                if (_plan.WasWritten(result)) continue;
                block.InsertRange(index, write());
            }
        }

        internal string Response(ResponseHandle handle)
        {
            if (handle == ResponseHandle.Innermost && _responses != null) return _responses.Binding;
            for (var frame = _responses; frame != null; frame = frame.Outer)
                if (frame.Handle == handle) return frame.Binding;
            throw new InvalidOperationException(
                $"Plan '{PlanId}' reads a response outside the routes of the request that receives it. Read a " +
                "response inside OnSuccess, OnError or Chained of its request; Finally runs whether or not one came.");
        }

        internal IReadOnlyList<Statement> WithHoisted(Func<IReadOnlyList<Statement>> write) => _plan.WithHoisted(write);

        private sealed class ResultFrame
        {
            private readonly Dictionary<object, string> _bound = new Dictionary<object, string>();
            private readonly List<(object Result, int Index, Func<IReadOnlyList<Statement>> Write)> _deferred =
                new List<(object, int, Func<IReadOnlyList<Statement>>)>();

            internal ResultFrame(ResultFrame? outer)
            {
                Outer = outer;
            }

            internal ResultFrame? Outer { get; }

            internal bool TryGet(object result, out string name) => _bound.TryGetValue(result, out name!);

            internal void Add(object result, string name) => _bound.Add(result, name);

            internal void Defer(object result, int index, Func<IReadOnlyList<Statement>> write) => _deferred.Add((result, index, write));

            internal IEnumerable<(object Result, int Index, Func<IReadOnlyList<Statement>> Write)> Deferred
                => _deferred.OrderByDescending(deferred => deferred.Index);
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

    internal sealed class ResponseHandle
    {
        internal static ResponseHandle Innermost { get; } = new ResponseHandle();
    }

    internal sealed class PlanWriter
    {
        private int _names;

        private readonly HashSet<object> _written = new HashSet<object>();

        private List<Statement>? _hoisted;

        internal PlanWriter(string planId, PlanObjects objects)
        {
            PlanId = planId;
            Objects = objects;
        }

        internal string PlanId { get; }

        internal PlanObjects Objects { get; }

        internal string Fresh(string stem) => stem + (++_names).ToString(CultureInfo.InvariantCulture);

        internal void Written(object result) => _written.Add(result);

        internal bool WasWritten(object result) => _written.Contains(result);

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

        internal void Hoist(Statement statement) => _hoisted!.Add(statement);
    }

    internal sealed class PlanObjects
    {
        internal const string DocumentId = "$document";
        internal const string HttpId = "$http";
        internal const string ModelId = "$model";
        internal const string ConfirmId = "$confirm";

        private readonly Dictionary<string, ObjectLocator> _objects = new Dictionary<string, ObjectLocator>(StringComparer.Ordinal);
        private readonly List<string> _order = new List<string>();
        private readonly Dictionary<string, ComponentVendor> _elementVendors = new Dictionary<string, ComponentVendor>(StringComparer.Ordinal);
        private readonly Dictionary<string, Vendor> _vendors = new Dictionary<string, Vendor>(StringComparer.Ordinal);
        private IReadOnlyDictionary<string, InputBinding> _renderedInputs = new Dictionary<string, InputBinding>(StringComparer.Ordinal);

        internal PlanObjects(IEnumerable<DeclaredObject> declared)
        {
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
            Add(id, new ElementObjectLocator(id, vendor.Name));
            return id;
        }

        internal string Document() => PluginObject(DocumentId, "alis.document", "", PluginData.None);

        internal string Http() => PluginObject(HttpId, "alis.http", "", PluginData.None);

        internal string Confirm() => PluginObject(ConfirmId, "alis.confirm", "", PluginData.None);

        internal string Model(ModelPluginData data)
        {
            _renderedInputs = data.Inputs;
            return PluginObject(ModelId, "alis.model", "", data);
        }

        internal bool TryFindOnlyInputItRendered(string bindingName, out string inputId)
        {
            inputId = "";
            var rendered = _renderedInputs.Where(input => input.Value.Name == bindingName).Select(input => input.Key).ToList();
            if (rendered.Count != 1) return false;
            inputId = rendered[0];
            return true;
        }

        internal string Model() => PluginObject(ModelId, "alis.model", "", PluginData.None);

        internal string Inputs(string containerId) => PluginObject("$inputs:" + containerId, "alis.model", containerId, PluginData.None);

        internal string Form(string containerId, FormPluginData data) => PluginObject("$form:" + containerId, "alis.form", containerId, data);

        internal string Form(string containerId) => PluginObject("$form:" + containerId, "alis.form", containerId, PluginData.None);

        internal string Slot(string containerId) => PluginObject("$slot:" + containerId, "alis.slot", containerId, PluginData.None);

        internal string Stream(string url) => PluginObject("$stream:" + url, "alis.stream", url, PluginData.None);

        internal string Hub(string url) => PluginObject("$hub:" + url, "alis.hub", url, new HubPluginData());

        internal string Plugin(string name) => PluginObject("$plugin:" + name, name, "", PluginData.None);

        internal IReadOnlyDictionary<string, ObjectLocator> Snapshot()
        {
            var snapshot = new Dictionary<string, ObjectLocator>(StringComparer.Ordinal);
            foreach (var id in _order) snapshot.Add(id, _objects[id]);
            return snapshot;
        }

        internal IReadOnlyDictionary<string, Vendor> Vendors()
            => _vendors.OrderBy(v => v.Key, StringComparer.Ordinal).ToDictionary(v => v.Key, v => v.Value, StringComparer.Ordinal);

        private string PluginObject(string id, string name, string key, PluginData data)
        {
            if (!_objects.ContainsKey(id))
                Add(id, new PluginObjectLocator(name, key, data));
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
