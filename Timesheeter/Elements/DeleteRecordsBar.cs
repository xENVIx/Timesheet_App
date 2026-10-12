using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Timesheeter.Core.Data;
using Timesheeter.Lib;

namespace Timesheeter.Elements
{
    /// <summary>
    /// The "Delete Records" bar under a <see cref="DataGridViewData{T}"/>: the first click slides in a
    /// column of checkboxes and fades in Cancel; the second (now "Confirm") deletes the checked records
    /// after asking. Deleting is <see cref="DataClass{T}.Delete"/>, so records stay in the database.
    /// Call <see cref="PostInit"/> after the grid's PostInit.
    /// </summary>
    public class DeleteRecordsBar : UserControl
    {
        private const string DeleteText = "Delete Records";
        private const string ConfirmText = "Confirm";

        private readonly Button _btnDelete;
        private readonly Button _btnCancel;

        private DataGridView? _grid;
        private DataGridViewCheckBoxColumn? _column;
        private bool _deleteMode;

        // Set by PostInit, which knows the grid's record type.
        private Action? _slideIn;
        private Action? _slideOut;
        private Func<bool>? _deleteChecked;

        public DeleteRecordsBar()
        {
            Height = 29;

            _btnDelete = new Button
            {
                Name = "_btnDelete",
                Text = DeleteText,
                Location = new Point(6, 3),
                Size = new Size(109, 23),
                TabIndex = 0,
            };
            _btnDelete.Click += (s, e) => DeleteClicked();

            _btnCancel = new Button
            {
                Name = "_btnCancel",
                Text = "Cancel",
                Location = new Point(121, 3),
                Size = new Size(109, 23),
                TabIndex = 1,
                Visible = false,
            };
            _btnCancel.Click += (s, e) => CancelClicked();

            Controls.Add(_btnDelete);
            Controls.Add(_btnCancel);
        }

        /// <param name="grid">The grid to delete from (already PostInit).</param>
        /// <param name="data">The grid's data, which does the deleting.</param>
        /// <param name="singular">What one record is called in the confirmation, e.g. "customer".</param>
        /// <param name="plural">What several are called, e.g. "customers".</param>
        /// <param name="inUseWarning">
        /// Optional: given the records about to be deleted, returns a line for the confirmation saying
        /// what still uses them, or null when nothing does.
        /// </param>
        public void PostInit<T>(DataGridViewData<T> grid, DataClass<T> data, string singular, string plural,
            Func<IReadOnlyList<T>, string?>? inUseWarning = null) where T : class
        {
            _grid = grid;
            _column = new DataGridViewCheckBoxColumn
            {
                Name = "_colDelete",
                HeaderText = "Select Item(s) for Deletion",
                Visible = false,
            };
            grid.Columns.Add(_column);

            var column = _column;
            _slideIn = () => grid.SlideColumnIn(column);
            _slideOut = () => grid.SlideColumnOut(column);
            _deleteChecked = () => DeleteChecked(grid, data, singular, plural, inUseWarning);
        }

        private void DeleteClicked()
        {
            if (_grid == null || _column == null) return;

            if (_deleteMode)
            {
                if (_deleteChecked?.Invoke() != true) return;
                ExitDeleteMode();
            }
            else
            {
                _deleteMode = true;
                _btnDelete.Text = ConfirmText;

                // The grid generates its data columns when it's first shown, which is after this
                // column was added, so they end up after it. Move it to the end each time.
                _column.DisplayIndex = _grid.Columns.Count - 1;
                _slideIn?.Invoke();
                Fade.In(_btnCancel);
            }
        }

        private void CancelClicked()
        {
            // Clicked again while it's fading out: it's already cancelled.
            if (Fade.IsFadingOut(_btnCancel)) return;
            ExitDeleteMode();
        }

        /// <summary>Back to normal: column slides away, Cancel fades out, nothing left checked.</summary>
        private void ExitDeleteMode()
        {
            if (_grid == null || _column == null) return;

            _deleteMode = false;
            _btnDelete.Text = DeleteText;
            _slideOut?.Invoke();
            Fade.Out(_btnCancel);

            _grid.CancelEdit();
            foreach (DataGridViewRow row in _grid.Rows) row.Cells[_column.Index].Value = null;
        }

        /// <returns>True when records were deleted, false to stay in delete mode.</returns>
        private bool DeleteChecked<T>(DataGridViewData<T> grid, DataClass<T> data, string singular, string plural,
            Func<IReadOnlyList<T>, string?>? inUseWarning) where T : class
        {
            if (_column == null) return false;

            // A checkbox that was just clicked is still being edited; commit it first.
            grid.EndEdit();

            var selected = grid.Rows
                .Cast<DataGridViewRow>()
                .Where(row => row.Cells[_column.Index].Value is true)
                .Select(row => row.DataBoundItem)
                .OfType<T>()
                .ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show(this, "At Least One Record Should Be Selected");
                return false;
            }

            string question = $"Delete {selected.Count} {(selected.Count == 1 ? singular : plural)}?";
            string? warning = inUseWarning?.Invoke(selected);
            if (!string.IsNullOrWhiteSpace(warning)) question = $"{warning}{Environment.NewLine}{Environment.NewLine}{question}";

            if (MessageBox.Show(this, question, DeleteText, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return false;
            }

            // Collected first: deleting removes rows from the grid.
            foreach (var item in selected)
            {
                if (!data.Delete(item))
                {
                    MessageBox.Show(this, $"Could not delete one of the {plural}.");
                }
            }

            return true;
        }
    }
}
