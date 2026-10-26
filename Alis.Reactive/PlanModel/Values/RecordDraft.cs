using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// A record assembled from values at paths, in the order they were assigned: a name makes a field, an
    /// index makes a list item, an index skipped a null item. Each path is assigned once; a value under a
    /// path that holds one, or a path under a value, is a conflict the developer resolves.
    /// </summary>
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
            if (path.ReadsEachItem)
                throw new ArgumentException($"'{path}' names every item of a list; a record assigns one path.", nameof(path));
            Assign(path.Segments, 0, value, path.ToString());
        }

        /// <summary>The record, or the list a root of positions alone makes.</summary>
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

            // A record with both names and positions names its positions by their digits.
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
