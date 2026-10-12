using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;
using Timesheeter.Lib;

namespace Timesheeter.Elements
{
    /// <summary>
    /// The small round "i" at the bottom of the navigation bar that opens the About dialog.
    /// Drawn by hand rather than a Button, so the nav bar's button styling and resizing leave it alone.
    /// </summary>
    public class AboutButton : Control
    {
        private bool _hovered;
        private bool _pressed;
        private readonly ToolTip _toolTip = new();

        public AboutButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);

            Size = new Size(28, 28);
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleName = "About Timesheeter";
            AccessibleRole = AccessibleRole.PushButton;
            _toolTip.SetToolTip(this, "About Timesheeter");

            if (!Theme.IsDesignTime(this)) Theme.Changed += OnThemeChanged;
        }

        private void OnThemeChanged(object? sender, EventArgs e) => Invalidate();

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (!Theme.IsDesignTime(this)) Forms.FrmAbout.ShowAbout(FindForm());
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode is Keys.Enter or Keys.Space) OnClick(EventArgs.Empty);
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color background = Parent?.BackColor ?? BackColor;
            g.Clear(background);

            var p = Theme.Current;
            bool lit = _hovered || Focused;
            Color ink = lit ? p.Accent : p.NavigationText;

            float stroke = Math.Max(1.5f, DeviceDpi / 64f);
            float inset = stroke / 2 + 1;
            var circle = new RectangleF(inset, inset, Width - inset * 2 - 1, Height - inset * 2 - 1);

            if (lit)
            {
                using var fill = new SolidBrush(_pressed ? p.NavigationActive : p.NavigationHover);
                g.FillEllipse(fill, circle);
            }

            using (var pen = new Pen(ink, stroke))
            {
                g.DrawEllipse(pen, circle);
            }

            // The "i": a dot over a short bar, centered.
            float cx = Width / 2f;
            float unit = Height / 28f;
            using var brush = new SolidBrush(ink);
            float dot = 3.2f * unit;
            g.FillEllipse(brush, cx - dot / 2, 7.5f * unit, dot, dot);
            float barWidth = 2.6f * unit;
            g.FillRectangle(brush, cx - barWidth / 2, 12.5f * unit, barWidth, 8.5f * unit);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Theme.Changed -= OnThemeChanged;
                _toolTip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
