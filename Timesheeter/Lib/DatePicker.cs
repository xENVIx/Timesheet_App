using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Timesheeter.Lib
{
    /// <summary>
    /// A <see cref="DateTimePicker"/> for picking a calendar date only. Use <see cref="Date"/>
    /// instead of <see cref="DateTimePicker.Value"/> so callers never deal with a time part.
    /// </summary>
    public class DatePicker : DateTimePicker
    {
        public DatePicker() : base()
        {
            Format = DateTimePickerFormat.Short;

            // Don't set Value here: the designer would then save today's date/time into
            // InitializeComponent. DateTimePicker already starts at the current date and time.
        }

        /// <summary>The selected date.</summary>
        // Hidden from the designer so it never writes a fixed date into InitializeComponent.
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateOnly Date
        {
            get { return DateOnly.FromDateTime(Value); }
            set { Value = value.ToDateTime(TimeOnly.MinValue); }
        }
    }
}
