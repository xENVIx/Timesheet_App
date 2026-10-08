using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheeter.Core.Data
{
    /// <summary>
    /// Hides a property's column in a <see cref="Elements.DataGridViewData{T}"/>.
    /// Use this instead of [Browsable(false)], which also hides the property from data binding,
    /// so it can no longer be a ValueMember (e.g. a GridLookup or combo box on "ID").
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class GridHiddenAttribute : Attribute
    {
    }
}
