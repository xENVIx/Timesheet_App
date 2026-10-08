using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Timesheeter.Core.Lib
{
    /// <summary>
    /// A <see cref="BindingList{T}"/> that supports sorting and filtering the way a DataView does,
    /// so <see cref="BindingSource.Sort"/>, <see cref="BindingSource.Filter"/> and DataGridView
    /// column-header sorting work on plain objects.
    /// </summary>
    /// <remarks>
    /// The list's contents (indexer, Count, enumeration) are the <i>visible</i> items: filtered and
    /// sorted. <see cref="Unfiltered"/> holds every item. Adding, removing and replacing items keeps
    /// both in step. Plain objects don't report property changes, so after editing an item's values
    /// call <see cref="Reposition"/> to move it to its new place (or out of the filter).
    ///
    /// Filter syntax: <c>Property op value</c> conditions joined by <c>AND</c>, where op is one of
    /// <c>= &lt;&gt; != &lt; &lt;= &gt; &gt;=</c>, or <c>Property IN (value, value, ...)</c>.
    /// Values are numbers, <c>true</c>/<c>false</c>, or quoted text (<c>'...'</c>, with <c>''</c> for a
    /// quote) converted to the property's type, e.g.
    /// <c>Date &gt;= '2026-09-01' AND ProjectCodeID IN (3, 5)</c>.
    /// Text comparisons ignore case.
    /// </remarks>
    public class BindingListView<T> : BindingList<T>, IBindingListView where T : class
    {
        private readonly List<T> _unfiltered = new List<T>();
        private readonly ListSortDescriptionCollection _noSort = new ListSortDescriptionCollection();

        private ListSortDescriptionCollection _sort;
        private string? _filter;
        private Func<T, bool>? _filterPredicate;

        // True while the list rebuilds its visible items from _unfiltered, so the overrides below
        // don't treat that as the caller adding or removing items.
        private bool _rebuilding;

        public BindingListView() : base()
        {
            _sort = _noSort;
        }

        /// <summary>Every item, ignoring the filter, in the order they were added.</summary>
        public IReadOnlyList<T> Unfiltered => _unfiltered;

        /// <summary>
        /// Sort as a string, like <see cref="BindingSource.Sort"/>: comma-separated property names,
        /// each optionally followed by ASC or DESC, e.g. "Date, TimeStart" or "Name DESC".
        /// Null or empty removes the sort.
        /// </summary>
        public string? Sort
        {
            get => _sort.Count == 0 ? null : FormatSort(_sort);
            set
            {
                if (string.IsNullOrWhiteSpace(value)) RemoveSortCore();
                else ApplySort(ParseSort(value));
            }
        }

        #region Sorting (IBindingList and IBindingListView)

        protected override bool SupportsSortingCore => true;
        protected override bool IsSortedCore => _sort.Count > 0;
        protected override PropertyDescriptor? SortPropertyCore => _sort.Count > 0 ? _sort[0]!.PropertyDescriptor : null;
        protected override ListSortDirection SortDirectionCore => _sort.Count > 0 ? _sort[0]!.SortDirection : ListSortDirection.Ascending;

        // Called for single-column sorts, e.g. clicking a DataGridView column header.
        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            ApplySort(new ListSortDescriptionCollection([new ListSortDescription(prop, direction)]));
        }

        protected override void RemoveSortCore()
        {
            _sort = _noSort;
            Rebuild();
        }

        public bool SupportsAdvancedSorting => true;

        public ListSortDescriptionCollection SortDescriptions => _sort;

        public void ApplySort(ListSortDescriptionCollection sorts)
        {
            _sort = sorts ?? _noSort;
            Rebuild();
        }

        #endregion

        #region Filtering (IBindingListView)

        public bool SupportsFiltering => true;

        public string? Filter
        {
            get => _filter;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    RemoveFilter();
                    return;
                }

                // Parse first so an invalid filter throws without changing the current one.
                _filterPredicate = FilterParser.Parse(value);
                _filter = value;
                Rebuild();
            }
        }

        public void RemoveFilter()
        {
            _filter = null;
            _filterPredicate = null;
            Rebuild();
        }

        #endregion

        /// <summary>
        /// Call after an item's values changed: moves it to where the current sort and filter put it
        /// (a new position, out of the list if it no longer matches the filter, or back in if it now
        /// does), and always raises ListChanged so everything bound to the list sees the edit.
        /// </summary>
        public void Reposition(T item)
        {
            if (!_unfiltered.Contains(item)) return;

            int current = IndexOf(item);
            bool visible = Matches(item);

            if (current >= 0)
            {
                if (visible && IsInPlace(current))
                {
                    // Plain objects don't report property changes themselves, so announce it here;
                    // otherwise e.g. a renamed project code never reaches the timesheet.
                    ResetItem(current);
                    return;
                }

                _rebuilding = true;
                try { base.RemoveItem(current); }
                finally { _rebuilding = false; }
            }

            if (visible)
            {
                _rebuilding = true;
                try { base.InsertItem(VisibleInsertIndex(item), item); }
                finally { _rebuilding = false; }
            }
        }

        /// <summary>Re-applies the sort and filter to every item.</summary>
        public void Refresh()
        {
            Rebuild();
        }

        #region Keeping the visible items and _unfiltered in step

        protected override void InsertItem(int index, T item)
        {
            if (_rebuilding)
            {
                base.InsertItem(index, item);
                return;
            }

            // Keep _unfiltered in the order items were added, relative to the visible item at index.
            int unfilteredIndex = index < Count ? _unfiltered.IndexOf(this[index]) : _unfiltered.Count;
            _unfiltered.Insert(unfilteredIndex, item);

            if (!Matches(item)) return;

            // With a sort, the item goes where the sort puts it rather than where it was inserted.
            base.InsertItem(IsSortedCore ? VisibleInsertIndex(item) : Math.Min(index, Count), item);
        }

        protected override void RemoveItem(int index)
        {
            if (!_rebuilding) _unfiltered.Remove(this[index]);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            if (_rebuilding)
            {
                base.SetItem(index, item);
                return;
            }

            int unfilteredIndex = _unfiltered.IndexOf(this[index]);
            if (unfilteredIndex >= 0) _unfiltered[unfilteredIndex] = item;
            else _unfiltered.Add(item);

            base.SetItem(index, item);
            Reposition(item);
        }

        protected override void ClearItems()
        {
            if (!_rebuilding) _unfiltered.Clear();
            base.ClearItems();
        }

        private void Rebuild()
        {
            IEnumerable<T> items = _unfiltered;
            if (_filterPredicate != null) items = items.Where(_filterPredicate);
            if (_sort.Count > 0) items = items.Order(Comparer<T>.Create(CompareBySort)); // stable

            var visible = items.ToList();

            bool raise = RaiseListChangedEvents;
            RaiseListChangedEvents = false;
            _rebuilding = true;
            try
            {
                base.ClearItems();
                foreach (var item in visible) base.InsertItem(Count, item);
            }
            finally
            {
                _rebuilding = false;
                RaiseListChangedEvents = raise;
            }

            if (raise) ResetBindings();
        }

        private bool Matches(T item) => _filterPredicate == null || _filterPredicate(item);

        private bool IsInPlace(int index)
        {
            if (_sort.Count == 0) return true;
            if (index > 0 && CompareBySort(this[index - 1], this[index]) > 0) return false;
            if (index < Count - 1 && CompareBySort(this[index], this[index + 1]) > 0) return false;
            return true;
        }

        // After any equal items, so items that sort the same keep the order they arrived in.
        private int VisibleInsertIndex(T item)
        {
            if (_sort.Count == 0) return Count;

            for (int i = 0; i < Count; i++)
            {
                if (CompareBySort(item, this[i]) < 0) return i;
            }
            return Count;
        }

        #endregion

        #region Comparing

        private int CompareBySort(T x, T y)
        {
            foreach (ListSortDescription sort in _sort)
            {
                var prop = sort.PropertyDescriptor!;
                int result = CompareValues(prop.GetValue(x), prop.GetValue(y));
                if (result != 0) return sort.SortDirection == ListSortDirection.Ascending ? result : -result;
            }
            return 0;
        }

        /// <summary>Nulls first; text ignoring case; otherwise the values' own ordering.</summary>
        internal static int CompareValues(object? x, object? y)
        {
            if (x == null) return y == null ? 0 : -1;
            if (y == null) return 1;
            if (x is string sx && y is string sy) return string.Compare(sx, sy, StringComparison.OrdinalIgnoreCase);
            if (x is IComparable cx && x.GetType() == y.GetType()) return cx.CompareTo(y);
            return string.Compare(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Sort string

        private static ListSortDescriptionCollection ParseSort(string sort)
        {
            var props = TypeDescriptor.GetProperties(typeof(T));
            var descriptions = new List<ListSortDescription>();

            foreach (var part in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var words = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var name = words[0].Trim('[', ']');
                var prop = props.Find(name, ignoreCase: true)
                    ?? throw new ArgumentException($"Sort: {typeof(T).Name} has no property '{name}'.");

                var direction = ListSortDirection.Ascending;
                if (words.Length == 2 && words[1].Equals("DESC", StringComparison.OrdinalIgnoreCase)) direction = ListSortDirection.Descending;
                else if (words.Length != 1 && !(words.Length == 2 && words[1].Equals("ASC", StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException($"Sort: '{part}' should be a property name optionally followed by ASC or DESC.");

                descriptions.Add(new ListSortDescription(prop, direction));
            }

            return new ListSortDescriptionCollection(descriptions.ToArray());
        }

        private static string FormatSort(ListSortDescriptionCollection sorts)
        {
            var parts = new List<string>();
            foreach (ListSortDescription sort in sorts)
            {
                parts.Add(sort.SortDirection == ListSortDirection.Descending
                    ? $"{sort.PropertyDescriptor!.Name} DESC"
                    : sort.PropertyDescriptor!.Name);
            }
            return string.Join(", ", parts);
        }

        #endregion

        #region Filter parsing

        /// <summary>Turns a filter string into a predicate. See the class remarks for the syntax.</summary>
        private sealed class FilterParser
        {
            private readonly string _text;
            private readonly PropertyDescriptorCollection _props = TypeDescriptor.GetProperties(typeof(T));
            private int _pos;

            private FilterParser(string text) { _text = text; }

            public static Func<T, bool> Parse(string text)
            {
                var parser = new FilterParser(text);
                var conditions = new List<Func<T, bool>> { parser.ParseCondition() };

                while (parser.TryKeyword("AND")) conditions.Add(parser.ParseCondition());

                parser.SkipSpaces();
                if (parser._pos < text.Length) throw parser.Error("expected AND or the end of the filter");

                return item => conditions.All(condition => condition(item));
            }

            private Func<T, bool> ParseCondition()
            {
                var prop = ParseProperty();

                if (TryKeyword("IN"))
                {
                    Expect('(');
                    var values = new List<object?> { ParseValue(prop) };
                    while (TryChar(',')) values.Add(ParseValue(prop));
                    Expect(')');

                    return item =>
                    {
                        var actual = prop.GetValue(item);
                        return values.Any(value => CompareValues(actual, value) == 0);
                    };
                }

                var op = ParseOperator();
                var expected = ParseValue(prop);

                return item =>
                {
                    int c = CompareValues(prop.GetValue(item), expected);
                    return op switch
                    {
                        "=" => c == 0,
                        "<>" or "!=" => c != 0,
                        "<" => c < 0,
                        "<=" => c <= 0,
                        ">" => c > 0,
                        ">=" => c >= 0,
                        _ => false,
                    };
                };
            }

            private PropertyDescriptor ParseProperty()
            {
                SkipSpaces();
                string name;

                if (TryChar('['))
                {
                    int end = _text.IndexOf(']', _pos);
                    if (end < 0) throw Error("missing ']'");
                    name = _text[_pos..end];
                    _pos = end + 1;
                }
                else
                {
                    int start = _pos;
                    while (_pos < _text.Length && (char.IsLetterOrDigit(_text[_pos]) || _text[_pos] == '_')) _pos++;
                    if (_pos == start) throw Error("expected a property name");
                    name = _text[start.._pos];
                }

                return _props.Find(name, ignoreCase: true)
                    ?? throw new ArgumentException($"Filter: {typeof(T).Name} has no property '{name}'.");
            }

            private string ParseOperator()
            {
                SkipSpaces();
                foreach (var op in new[] { "<=", ">=", "<>", "!=", "=", "<", ">" })
                {
                    if (string.CompareOrdinal(_text, _pos, op, 0, op.Length) == 0)
                    {
                        _pos += op.Length;
                        return op;
                    }
                }
                throw Error("expected an operator (=, <>, !=, <, <=, >, >=) or IN");
            }

            /// <summary>Reads a value and converts it to the property's type.</summary>
            private object? ParseValue(PropertyDescriptor prop)
            {
                SkipSpaces();
                string raw;

                if (TryChar('\''))
                {
                    var sb = new StringBuilder();
                    while (true)
                    {
                        if (_pos >= _text.Length) throw Error("missing closing quote");
                        char c = _text[_pos++];
                        if (c != '\'') { sb.Append(c); continue; }
                        if (_pos < _text.Length && _text[_pos] == '\'') { sb.Append('\''); _pos++; continue; }
                        break;
                    }
                    raw = sb.ToString();
                }
                else
                {
                    int start = _pos;
                    while (_pos < _text.Length && !char.IsWhiteSpace(_text[_pos]) && _text[_pos] != ',' && _text[_pos] != ')') _pos++;
                    if (_pos == start) throw Error("expected a value");
                    raw = _text[start.._pos];
                }

                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (type == typeof(string)) return raw;

                try
                {
                    return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(raw);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Filter: '{raw}' is not a valid {type.Name} for {prop.Name}.", ex);
                }
            }

            private bool TryKeyword(string keyword)
            {
                SkipSpaces();
                int end = _pos + keyword.Length;
                if (end > _text.Length || string.Compare(_text, _pos, keyword, 0, keyword.Length, StringComparison.OrdinalIgnoreCase) != 0)
                    return false;
                // Must be a whole word: "INDEX" is not "IN".
                if (end < _text.Length && (char.IsLetterOrDigit(_text[end]) || _text[end] == '_')) return false;

                _pos = end;
                return true;
            }

            private bool TryChar(char c)
            {
                SkipSpaces();
                if (_pos < _text.Length && _text[_pos] == c)
                {
                    _pos++;
                    return true;
                }
                return false;
            }

            private void Expect(char c)
            {
                if (!TryChar(c)) throw Error($"expected '{c}'");
            }

            private void SkipSpaces()
            {
                while (_pos < _text.Length && char.IsWhiteSpace(_text[_pos])) _pos++;
            }

            private ArgumentException Error(string message) =>
                new ArgumentException($"Filter: {message} at position {_pos} in \"{_text}\".");
        }

        #endregion
    }
}
