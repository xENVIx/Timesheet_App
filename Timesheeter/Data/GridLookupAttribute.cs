using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheeter.Data
{
    /// <summary>
    /// Shows a foreign key column in a <see cref="Elements.DataGridViewData{T}"/> as the matching
    /// row's display value from another data class, e.g. a CustomerID shown as the customer's Name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class GridLookupAttribute : Attribute
    {
        public GridLookupAttribute(Type dataType, string displayMember, string valueMember = "ID")
        {
            DataType = dataType;
            DisplayMember = displayMember;
            ValueMember = valueMember;
        }

        /// <summary>The data class holding the lookup rows (e.g. typeof(Customers)), fetched from the factory.</summary>
        public Type DataType { get; }

        /// <summary>Property of the lookup rows shown in the grid (e.g. "Name").</summary>
        public string DisplayMember { get; }

        /// <summary>Property of the lookup rows matched against this column's value (default "ID").</summary>
        public string ValueMember { get; }
    }
}
