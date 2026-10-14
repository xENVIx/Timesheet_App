using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheeter.Lib;

namespace Timesheeter.Elements
{
    /// <summary>
    /// A panel with a clickable header that slides closed to just the header and open again.
    /// The children stay directly in the panel (no inner panels), so the designer works as with any
    /// Panel: place them below the header. Stack several with Dock = Top so collapsing one moves
    /// the ones below it up.
    /// </summary>
    public class AccordionPanel : Panel
    {
        private const int AnimationMilliseconds = 200;

        private string _title = "Section";
        private int _headerHeight = 32;
        private bool _expanded = true;
        private int _expandedHeight = 150;
        private bool _hovered;

        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 15 };
        private DateTime _animationStart;
        private int _animationFrom;
        private bool _animating;

        // Children hidden while collapsed, so Tab can't reach controls that can't be seen.
        private readonly List<Control> _hiddenChildren = new();

        public AccordionPanel()
        {
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.Selectable, true);
            TabStop = true;
            Padding = new Padding(0, _headerHeight, 0, 0); // docked children start below the header
            Height = _expandedHeight;

            _timer.Tick += Timer_Tick;
            if (!Theme.IsDesignTime(this)) Theme.Changed += OnThemeChanged;
        }

        /// <summary>Raised after the panel has finished opening or closing.</summary>
        public event EventHandler? ExpandedChanged;

        [Category("Appearance"), DefaultValue("Section"), Description("The text in the header.")]
        public string Title
        {
            get => _title;
            set { _title = value ?? string.Empty; Invalidate(HeaderBounds); }
        }

        [Category("Layout"), DefaultValue(32), Description("Height of the clickable header; also the height when collapsed.")]
        public int HeaderHeight
        {
            get => _headerHeight;
            set
            {
                _headerHeight = Math.Max(16, value);
                Padding = new Padding(Padding.Left, _headerHeight, Padding.Right, Padding.Bottom);
                if (!_expanded) Height = _headerHeight;
                Invalidate();
            }
        }

        [Category("Behavior"), DefaultValue(true), Description("Whether the panel is open.")]
        public bool Expanded
        {
            get => _expanded;
            set
            {
                if (_expanded == value) return;
                _expanded = value;

                if (value) ShowChildren();

                // No animation in the designer or before the panel is on screen.
                if (Theme.IsDesignTime(this) || !IsHandleCreated || !Visible)
                {
                    FinishAnimation();
                    return;
                }

                _animationFrom = Height;
                _animationStart = DateTime.UtcNow;
                _animating = true;
                _timer.Start();
            }
        }

        /// <summary>The height when open. Saved by the designer so a panel collapsed there still knows its open size.</summary>
        [Category("Layout"), Description("Height of the panel when open.")]
        public int ExpandedHeight
        {
            get => _expandedHeight;
            set
            {
                _expandedHeight = Math.Max(_headerHeight + 1, value);
                if (_expanded && !_animating) Height = _expandedHeight;
            }
        }

        private Rectangle HeaderBounds => new(0, 0, Width, _headerHeight);

        /// <summary>Opens the panel if closed and closes it if open.</summary>
        public void Toggle() => Expanded = !Expanded;

        #region Animation

        private void Timer_Tick(object? sender, EventArgs e)
        {
            double t = (DateTime.UtcNow - _animationStart).TotalMilliseconds / AnimationMilliseconds;
            if (t >= 1)
            {
                FinishAnimation();
                return;
            }

            double eased = 1 - Math.Pow(1 - t, 3); // ease-out: fast start, gentle stop
            int target = _expanded ? _expandedHeight : _headerHeight;
            Height = (int)Math.Round(_animationFrom + (target - _animationFrom) * eased);
            Invalidate(HeaderBounds); // the chevron turns with the height
        }

        private void FinishAnimation()
        {
            _timer.Stop();
            _animating = false;
            Height = _expanded ? _expandedHeight : _headerHeight;
            if (!_expanded) HideChildren();
            Invalidate();
            ExpandedChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HideChildren()
        {
            if (Theme.IsDesignTime(this)) return;
            foreach (Control child in Controls)
            {
                if (!child.Visible) continue;
                child.Visible = false;
                _hiddenChildren.Add(child);
            }
        }

        private void ShowChildren()
        {
            foreach (var child in _hiddenChildren) child.Visible = true;
            _hiddenChildren.Clear();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            // Resized while open (designer, docking, code): that's the new open size.
            // A collapsed height is never taken as the open size, whatever order the designer sets things in.
            if (_expanded && !_animating && Height > _headerHeight) _expandedHeight = Height;
        }

        #endregion

        #region Mouse and keyboard

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Left && e.Y < _headerHeight)
            {
                Focus();
                Toggle();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool overHeader = e.Y < _headerHeight;
            Cursor = overHeader ? Cursors.Hand : Cursors.Default;
            if (overHeader != _hovered)
            {
                _hovered = overHeader;
                Invalidate(HeaderBounds);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Cursor = Cursors.Default;
            if (_hovered)
            {
                _hovered = false;
                Invalidate(HeaderBounds);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode is Keys.Enter or Keys.Space) Toggle();
        }

        protected override void OnGotFocus(EventArgs e) { Invalidate(HeaderBounds); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(HeaderBounds); base.OnLostFocus(e); }

        #endregion

        #region Painting

        private void OnThemeChanged(object? sender, EventArgs e) => Invalidate();

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var p = Theme.Current;
            var header = HeaderBounds;
            bool lit = _hovered || Focused;

            if (lit)
            {
                using var hover = new SolidBrush(Theme.Blend(BackColor, p.Accent, 0.12));
                g.FillRectangle(hover, header);
            }

            Color ink = lit ? p.Accent : ForeColor;

            // Chevron: points right when closed, down when open, turning as the panel slides.
            float open = _expandedHeight > _headerHeight
                ? Math.Clamp((Height - _headerHeight) / (float)(_expandedHeight - _headerHeight), 0f, 1f)
                : (_expanded ? 1f : 0f);
            float size = _headerHeight / 6f;
            var center = new PointF(8 + size * 1.5f, _headerHeight / 2f);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(ink, Math.Max(1.5f, DeviceDpi / 64f)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                var state = g.Save();
                g.TranslateTransform(center.X, center.Y);
                g.RotateTransform(90f * open);
                g.DrawLines(pen, new[] { new PointF(-size / 2, -size), new PointF(size / 2, 0), new PointF(-size / 2, size) });
                g.Restore(state);
            }
            g.SmoothingMode = SmoothingMode.Default;

            int textLeft = (int)(center.X + size * 1.5f + 4);
            var textBounds = new Rectangle(textLeft, 0, Math.Max(0, Width - textLeft - 8), _headerHeight);
            TextRenderer.DrawText(g, _title, Font, textBounds, ink,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            // A thin line under the header separates it from the contents (and from the next panel when closed).
            using var line = new Pen(p.Border);
            g.DrawLine(line, 0, _headerHeight - 1, Width, _headerHeight - 1);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Theme.Changed -= OnThemeChanged;
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
