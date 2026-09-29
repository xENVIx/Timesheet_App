using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Timesheeter.Data;
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


        public void PostInit(DataClass<T>? data, IFactory factory)
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

            // The grid has already written the edited value into the bound object.
            if (Rows[e.RowIndex].DataBoundItem is not T item) return;

            if (!_data.Update(item))
            {
                MessageBox.Show("Could not save the change (blank or duplicate value?).");
                _data.Reload(item);
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

            base.OnColumnAdded(e);
        }

        private void AddLookupColumns(IFactory factory)
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










    }
}
