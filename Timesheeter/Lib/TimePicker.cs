using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Timesheeter.Lib
{
    /// <summary>
    /// A <see cref="DateTimePicker"/> for picking a time of day only, to the minute. Use
    /// <see cref="Time"/> instead of <see cref="DateTimePicker.Value"/>.
    /// </summary>
    public class TimePicker : DateTimePicker
    {
        public TimePicker() : base()
        {
            Format = DateTimePickerFormat.Custom;
            CustomFormat = "HH:mm";
            ShowUpDown = true;

            // Don't set Value here: the designer would then save today's date/time into
            // InitializeComponent. DateTimePicker already starts at the current date and time.
        }

        /// <summary>The selected time, always to the whole minute (no seconds).</summary>
        // Hidden from the designer so it never writes a fixed time into InitializeComponent.
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TimeOnly Time
        {
            // Value carries the seconds it was created with; drop them so 8:30-9:30 is exactly an hour.
            get { return new TimeOnly(Value.Hour, Value.Minute); }

            // Today as the date part: it's always inside MinDate/MaxDate, unlike DateOnly.MinValue.
            set { Value = DateTime.Today.Add(value.ToTimeSpan()); }
        }
    }
}
