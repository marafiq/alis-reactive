using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.PlanModel.Values
{
    internal sealed class RecordDraft
    {
        private readonly List<string> _order = new List<string>();
        private readonly Dictionary<string, RecordDraft> _fields = new Dictionary<string, RecordDraft>(StringComparer.Ordinal);
        private readonly SortedDictionary<int, RecordDraft> _items = new SortedDictionary<int, RecordDraft>();
        private ValueExpression? _value;

        internal bool IsEmpty => _value == null && _fields.Count == 0 && _items.Count == 0;

        internal void Assign(Path path, ValueExpression value)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (value == null) throw new ArgumentNullException(nameof(value));
            Assign(path.Segments, 0, value, path.ToString());
        }

        internal ValueExpression ToValueExpression()
        {
            if (_value != null) return _value;
            if (_items.Count > 0 && _fields.Count == 0)
            {
                var count = _items.Keys.Last() + 1;
                var items = new List<ValueExpression>(count);
                for (var index = 0; index < count; index++)
                    items.Add(_items.TryGetValue(index, out var item) ? item.ToValueExpression() : ValueExpression.Null());
                return ValueExpression.List(items, Shape.ArrayOf(Shape.Any));
            }

            var fields = _order.Select(name => (name, _fields[name].ToValueExpression()))
                .Concat(_items.Select(item => (item.Key.ToString(CultureInfo.InvariantCulture), item.Value.ToValueExpression())))
                .ToList();
            return ValueExpression.Record(fields);
        }

        private void Assign(IReadOnlyList<PathSegment> segments, int at, ValueExpression value, string path)
        {
            if (at == segments.Count)
            {
                if (!IsEmpty)
                    throw new InvalidOperationException($"'{path}' is assigned twice, or over members of its own; assign each path once.");
                _value = value;
                return;
            }

            if (_value != null)
                throw new InvalidOperationException($"'{path}' is assigned under a value; assign the value or its members, not both.");
            var segment = segments[at];
            var child = segment.IsIndex ? ItemAt(segment.Index) : FieldNamed(segment.ToPathText());
            child.Assign(segments, at + 1, value, path);
        }

        private RecordDraft FieldNamed(string name)
        {
            if (_fields.TryGetValue(name, out var field)) return field;
            field = new RecordDraft();
            _fields.Add(name, field);
            _order.Add(name);
            return field;
        }

        private RecordDraft ItemAt(int index)
        {
            if (_items.TryGetValue(index, out var item)) return item;
            item = new RecordDraft();
            _items.Add(index, item);
            return item;
        }
    }
}
