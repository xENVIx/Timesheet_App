using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;

using Timesheeter.Lib;

namespace Timesheeter.Elements
{
    public class DataGridViewData<T> : DataGridView where T : class
    {

        //protected DataClass<T>? _data;

        protected DataClass<T>? _data;

        public DataGridViewData() : base()
        {
            base.AllowUserToAddRows = false;
        }


        public void PostInit(DataClass<T>? data, IDataFactory factory)
        {

            
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            _data = data;
            CellEndEdit += DataGridViewData_CellEndEdit;

            // Lookup columns must exist before DataSource is set: auto-generation keeps a
            // column whose DataPropertyName matches a property instead of generating its own.
            AddLookupColumns(factory);

            this.DataSource = data.All;


        }

        private void DataGridViewData_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _data == null) return;

            // Unbound columns (e.g. the time entries page's delete checkboxes) aren't data to save.
            if (string.IsNullOrEmpty(Columns[e.ColumnIndex].DataPropertyName)) return;

            // The grid has already written the edited value into the bound object.
            if (Rows[e.RowIndex].DataBoundItem is not T item) return;

            // Moving rows inside CellEndEdit makes the grid throw a reentrancy error, so the
            // reload / re-sort runs just after the edit has finished.
            if (!_data.Update(item))
            {
                MessageBox.Show("Could not save the change (blank or duplicate value?).");
                BeginInvoke(() => _data.Reload(item));
            }
            else
            {
                // The edit may have changed where the row sorts to, or whether it matches the filter.
                BeginInvoke(() => _data.All.Reposition(item));
            }
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            // Runs for auto-generated columns too, including when the grid rebinds.
            var prop = typeof(T).GetProperty(e.Column.DataPropertyName);
            if (prop?.GetCustomAttribute<GridHiddenAttribute>() != null)
            {
                e.Column.Visible = false;
            }

            // Show decimals to at most 2 places (e.g. hours 1.3333 as 1.33) without rounding the data.
            var type = prop == null ? null : Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (type == typeof(double) || type == typeof(float) || type == typeof(decimal))
            {
                e.Column.DefaultCellStyle.Format = "0.##";
            }

            base.OnColumnAdded(e);
        }

        private void AddLookupColumns(IDataFactory factory)
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                var lookup = prop.GetCustomAttribute<GridLookupAttribute>();
                if (lookup == null || Columns.Contains(prop.Name)) continue;

                var source = factory.GetData(lookup.DataType) as IDataClass;
                if (source == null)
                {
                    throw new InvalidOperationException(
                        $"GridLookup on {typeof(T).Name}.{prop.Name}: the factory has no data class of type {lookup.DataType.Name}.");
                }

                Columns.Add(new DataGridViewComboBoxColumn()
                {
                    Name = prop.Name,
                    DataPropertyName = prop.Name,
                    HeaderText = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? prop.Name,
                    DataSource = source.All,
                    ValueMember = lookup.ValueMember,
                    DisplayMember = lookup.DisplayMember,
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing,
                });
            }
        }

        #region Row animation

        // Total time to take the rows away, and to bring them back.
        private const int RemoveMs = 1000;
        private const int AddMs = 1000;
        private const int AnimationFrameMs = 15;

        private enum RowAnimation { None, Removing, Adding }

        private RowAnimation _animation = RowAnimation.None;
        private System.Windows.Forms.Timer? _animationTimer;
        private Action? _pendingChange;

        // Rows from this index down are painted blank while animating ("removed" / not yet added).
        private int _shownBelow = int.MaxValue;
        private int _animationFirst;
        private int _animationStart;
        private double _msPerRow;
        private long _animationStarted;

        /// <summary>
        /// Changes what the grid shows with an animation: the displayed rows disappear one by one
        /// from the bottom up (over 1 second), then <paramref name="change"/> runs (e.g. refilling
        /// the bound list), then the new rows appear one by one from the top down (over 1 second).
        /// </summary>
        /// <remarks>
        /// The rows aren't really removed: bound rows can't be, without changing the data. Rows not yet
        /// shown are just painted as empty grid. Changes made while a removal is running wait and run
        /// together at its end; a change made while rows are being added takes the shown rows away again.
        /// When the grid isn't on screen the change just runs.
        /// </remarks>
        public void AnimateChange(Action change)
        {
            if (!IsHandleCreated || !Visible || Theme.IsDesignTime(this))
            {
                FinishAnimation();
                change();
                return;
            }

            if (_animation == RowAnimation.Removing)
            {
                _pendingChange += change;
                return;
            }

            _pendingChange = change;

            // Remove the rows currently shown: all displayed rows, or only those already added.
            int first = Math.Max(0, FirstDisplayedScrollingRowIndex);
            int displayed = DisplayedRowCount(includePartialRow: true);
            int shownEnd = Math.Min(first + displayed, _animation == RowAnimation.Adding ? _shownBelow : int.MaxValue);

            StartPhase(RowAnimation.Removing, first, shownEnd, displayed, RemoveMs);
        }

        private void StartPhase(RowAnimation phase, int first, int start, int displayed, int totalMs)
        {
            _animation = phase;
            _animationFirst = first;
            _animationStart = start;
            _shownBelow = start;
            _msPerRow = totalMs / (double)Math.Max(1, displayed);
            _animationStarted = Environment.TickCount64;

            if (_animationTimer == null)
            {
                _animationTimer = new System.Windows.Forms.Timer { Interval = AnimationFrameMs };
                _animationTimer.Tick += (s, e) => StepAnimation();
            }
            _animationTimer.Start();
            StepAnimation();
        }

        private void StepAnimation()
        {
            long elapsed = Environment.TickCount64 - _animationStarted;
            int steps = (int)(elapsed / _msPerRow);

            if (_animation == RowAnimation.Removing)
            {
                // The bottom row goes at once, then one more every step.
                _shownBelow = Math.Max(_animationFirst, _animationStart - 1 - steps);
                Invalidate();

                // One step after the top row went: swap the data, then bring the new rows in.
                if (_animationStart - steps <= _animationFirst)
                {
                    var change = _pendingChange;
                    _pendingChange = null;
                    change?.Invoke();

                    int first = Math.Max(0, FirstDisplayedScrollingRowIndex);
                    int displayed = Rows.Count == 0 ? 0 : DisplayedRowCount(includePartialRow: true);
                    if (displayed == 0)
                    {
                        FinishAnimation();
                        return;
                    }

                    StartPhase(RowAnimation.Adding, first, first, displayed, AddMs);
                }
            }
            else if (_animation == RowAnimation.Adding)
            {
                // The top row comes in at once, then one more every step.
                _shownBelow = _animationFirst + steps + 1;
                Invalidate();

                if (elapsed >= AddMs) FinishAnimation();
            }
        }

        private void FinishAnimation()
        {
            _animationTimer?.Stop();

            var change = _pendingChange;
            _pendingChange = null;
            change?.Invoke();

            _animation = RowAnimation.None;
            _shownBelow = int.MaxValue;
            Invalidate();
        }

        protected override void OnRowPrePaint(DataGridViewRowPrePaintEventArgs e)
        {
            if (_animation != RowAnimation.None && e.RowIndex >= _shownBelow)
            {
                using var brush = new SolidBrush(BackgroundColor);
                e.Graphics.FillRectangle(brush, e.RowBounds);
                e.Handled = true;
                return;
            }

            base.OnRowPrePaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _animationTimer?.Dispose();
            base.Dispose(disposing);
        }

        #endregion










    }
}
