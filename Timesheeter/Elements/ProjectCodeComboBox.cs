using System;
using System.Collections.Generic;
using System.Text;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;

namespace Timesheeter.Elements
{
    /// <summary>
    /// Project code picker showing each code as &lt;CustomerName&gt;-&lt;ProjectCode&gt;-&lt;Location&gt;.
    /// SelectedValue is still the project code's ID.
    /// </summary>
    public class ProjectCodeComboBox : DataComboBox<ProjectCodes.ProjectCode>
    {
        private Customers? _customers;

        public ProjectCodeComboBox() : base()
        {
            base.DisplayMember = "Code";

            // OnFormat below only runs with formatting enabled.
            FormattingEnabled = true;
        }

        internal void PostInit(ProjectCodes? projectCodes, Customers? customers)
        {
            // Set before binding, so the first time the items are drawn they include the customer.
            _customers = customers ?? throw new ArgumentNullException(nameof(customers));

            base.PostInit(projectCodes);

            // The text shown includes the customer's name, so redraw when a customer is renamed.
            _customers.All.ListChanged += (s, e) => RefreshDisplayText();
        }

        private void RefreshDisplayText()
        {
            // RefreshItems re-selects the bound position, which would pick the first code
            // when nothing has been chosen yet; keep an empty selection empty.
            int selectedIndex = SelectedIndex;
            string text = Text;

            RefreshItems();

            if (selectedIndex < 0)
            {
                SelectedIndex = -1;
                Text = text;
            }
        }

        // Called for each item's display text; the underlying value (ID) is unchanged.
        protected override void OnFormat(ListControlConvertEventArgs e)
        {
            if (e.ListItem is ProjectCodes.ProjectCode code)
            {
                string? customerName = _customers?[code.CustomerID]?.Name;

                // Skip empty parts so a code without a location doesn't end in "-".
                e.Value = string.Join("-", new[] { customerName, code.Code, code.Location }
                    .Where(part => !string.IsNullOrWhiteSpace(part)));
            }

            base.OnFormat(e);
        }
    }
}
