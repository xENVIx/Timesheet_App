using System;
using System.Collections.Generic;
using System.Text;
using ScottPlot;
using Timesheeter.Lib;
using ProjectHours = Timesheeter.Elements.TimesheetDataGridView.ProjectHours;

namespace Timesheeter.Elements
{
    /// <summary>
    /// Pie chart of where the week's hours went, one slice per project code, fed by
    /// <see cref="TimesheetDataGridView.WeekTotalsChanged"/> via <see cref="ShowTotals"/>.
    /// </summary>
    public class TimesheetChart : ScottPlot.WinForms.FormsPlot
    {
        // Categorical palettes in a fixed order, one per theme brightness. Checked with the dataviz
        // palette validator against the sidebar backgrounds (white / #1f1f23): neighbouring colours stay
        // distinguishable with colour blindness. Some light-mode colours are pale against white, so
        // every slice is also named with its hours in the legend.
        private static readonly string[] LightSeries =
            ["#2a78d6", "#eb6834", "#1baf7a", "#eda100", "#e87ba4", "#008300", "#4a3aa7", "#e34948"];
        private static readonly string[] DarkSeries =
            ["#3987e5", "#d95926", "#199e70", "#c98500", "#d55181", "#008300", "#9085e9", "#e66767"];

        // More codes than colours: the smallest fold into one grey "Other" slice rather than reusing colours.
        private const string OtherColor = "#898781";
        private const int MaxSlices = 8;

        private IReadOnlyList<ProjectHours> _totals = [];

        // Created once: every ShowLegend(Edge) call adds another legend panel, which Plot.Clear()
        // doesn't remove, so redrawing per week stacked legends until the pie was squeezed out.
        private readonly ScottPlot.Panels.LegendPanel _legendPanel;

        public TimesheetChart() : base()
        {
            // A summary to read, not a plot to pan or zoom.
            UserInputProcessor.Disable();

            _legendPanel = Plot.ShowLegend(Edge.Bottom);

            Theme.Changed += (s, e) => Redraw();
            Redraw();
        }

        /// <summary>Shows the given project totals (in the timesheet's row order).</summary>
        public void ShowTotals(IReadOnlyList<ProjectHours> totals)
        {
            _totals = totals;
            Redraw();
        }

        // Deferred a tick: rendering synchronously (e.g. from the constructor, before the control
        // has gone through its first real paint) left a stray ghost of the legend's last row behind
        // that only a later rebuild (e.g. switching weeks) would clear. Posting the render after the
        // current message runs gives WinForms time to finish laying the control out first.
        private void Redraw()
        {
            if (IsHandleCreated) BeginInvoke(RedrawCore);
            else RedrawCore();
        }

        private void RedrawCore()
        {
            var palette = Theme.Current;
            var surface = ScottPlot.Color.FromColor(palette.Sidebar);
            var text = ScottPlot.Color.FromColor(palette.Text);

            Plot.Clear();
            Plot.FigureBackground.Color = surface;
            Plot.DataBackground.Color = surface;
            Plot.Axes.Frameless();
            Plot.HideGrid();

            var slices = BuildSlices(palette.IsDark ? DarkSeries : LightSeries);

            _legendPanel.IsVisible = slices.Count > 0;

            if (slices.Count == 0)
            {
                Plot.Title("No hours this week");
                Plot.Title(true);
                Plot.Axes.Title.Label.ForeColor = ScottPlot.Color.FromColor(palette.MutedText);
                Plot.Axes.Title.Label.FontSize = 13;
                Plot.Axes.Title.Label.FontName = "Segoe UI";
            }
            else
            {
                Plot.Title(false);

                var pie = Plot.Add.Pie(slices);
                pie.ExplodeFraction = 0;
                // A thin gap in the background colour between slices keeps neighbours apart.
                pie.LineColor = surface;
                pie.LineWidth = 2;
                pie.Padding = 0.02;

                // Legend below the pie, blending into the sidebar (no box or shadow), kept compact
                // so the pie gets most of the height.
                Plot.Legend.FontName = "Segoe UI";
                Plot.Legend.FontColor = text;
                Plot.Legend.FontSize = 12;
                Plot.Legend.BackgroundColor = surface;
                Plot.Legend.OutlineWidth = 0;
                Plot.Legend.ShadowColor = Colors.Transparent;
                Plot.Legend.Padding = new PixelPadding(4);
                Plot.Legend.InterItemPadding = new PixelPadding(2);

                // Fit the view to the pie: after an empty week the axes keep a default range,
                // which drew the next pie smaller.
                Plot.Axes.AutoScale();
            }

            Refresh();
        }

        private List<PieSlice> BuildSlices(string[] series)
        {
            var worked = _totals.Where(t => t.Hours > 0).ToList();
            double weekTotal = worked.Sum(t => t.Hours);
            if (weekTotal <= 0) return [];

            // Keep the largest codes; colours then follow the timesheet's row order, so a code keeps
            // its colour when another code's hours change.
            var shown = worked;
            if (worked.Count > MaxSlices)
            {
                var largest = worked.OrderByDescending(t => t.Hours).Take(MaxSlices - 1).ToHashSet();
                shown = worked.Where(largest.Contains).ToList();
            }
            double otherHours = weekTotal - shown.Sum(t => t.Hours);

            var slices = shown
                .Select((t, i) => Slice(t.ProjectCode, t.Hours, weekTotal, series[i]))
                .ToList();

            if (otherHours > 0) slices.Add(Slice("Other", otherHours, weekTotal, OtherColor));

            return slices;
        }

        private static PieSlice Slice(string name, double hours, double weekTotal, string color) => new PieSlice()
        {
            Value = hours,
            FillColor = ScottPlot.Color.FromHex(color),
            LegendText = $"{name}  {hours:0.##} h ({hours / weekTotal:0%})",
        };
    }
}
