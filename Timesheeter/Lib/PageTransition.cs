using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Text;

namespace Timesheeter.Lib
{
    /// <summary>
    /// Switches between pages docked in the same container with a fade: the old page fades out
    /// into the background, then the new page fades in.
    /// </summary>
    /// <remarks>
    /// WinForms controls have no opacity, so this works with pictures: the old page is captured,
    /// shown on an overlay covering it, and the pages are swapped underneath (so the swap's
    /// repainting never flickers on screen). The new page is then captured and the overlay blends
    /// old picture, background and new picture before removing itself.
    /// </remarks>
    public static class PageTransition
    {
        /// <summary>Fade out plus fade in, in milliseconds.</summary>
        private const int DurationMs = 500;
        private const int FrameMs = 15;

        private static readonly ConditionalWeakTable<Control, Overlay> _running = new();

        /// <summary>Hides <paramref name="from"/> and shows <paramref name="to"/>, fading between them.</summary>
        public static void Switch(Control container, Control? from, Control to)
        {
            // A page clicked mid-fade: finish the running fade at once, then start this one.
            if (_running.TryGetValue(container, out var running)) running.Finish();

            if (from == to) return;

            Bitmap? oldPicture = from != null && from.Visible ? Capture(from) : null;

            if (oldPicture == null || Theme.IsDesignTime(container))
            {
                Show(from, to);
                return;
            }

            var overlay = new Overlay(container, oldPicture, from!.BackColor)
            {
                Bounds = from.Bounds,
            };
            Theme.SetRole(overlay, ThemeRole.Ignore); // it paints its own pictures
            container.Controls.Add(overlay);
            overlay.BringToFront();
            overlay.Update(); // on screen before anything moves underneath

            Show(from, to);
            container.PerformLayout();

            overlay.Start(Capture(to));
            _running.AddOrUpdate(container, overlay);
        }

        private static void Show(Control? from, Control to)
        {
            if (from != null)
            {
                from.Visible = false;
                from.Enabled = false;
            }
            to.Enabled = true;
            to.Visible = true;
        }

        private static Bitmap? Capture(Control control)
        {
            if (control.Width <= 0 || control.Height <= 0) return null;

            var bitmap = new Bitmap(control.Width, control.Height);
            control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, control.Size));
            return bitmap;
        }

        private sealed class Overlay : Control
        {
            private readonly Control _container;
            private readonly Bitmap _oldPicture;
            private readonly Color _background;
            private readonly System.Windows.Forms.Timer _timer = new() { Interval = FrameMs };
            private Bitmap? _newPicture;
            private long _started;

            // -1 to 1: below 0 the old picture is fading out, above 0 the new one is fading in.
            private double _progress = -1;

            public Overlay(Control container, Bitmap oldPicture, Color background)
            {
                _container = container;
                _oldPicture = oldPicture;
                _background = background;

                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.UserPaint | ControlStyles.Opaque, true);

                _timer.Tick += (s, e) => Step();
            }

            public void Start(Bitmap? newPicture)
            {
                _newPicture = newPicture;
                _started = Environment.TickCount64;
                _timer.Start();
            }

            private void Step()
            {
                double t = (Environment.TickCount64 - _started) / (double)DurationMs;
                if (t >= 1)
                {
                    Finish();
                    return;
                }

                _progress = t * 2 - 1;
                Invalidate();
            }

            /// <summary>Ends the fade: removes the overlay, leaving the new page showing.</summary>
            public void Finish()
            {
                _timer.Stop();
                _running.Remove(_container);
                _container.Controls.Remove(this);
                Dispose();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.Clear(_background);

                // Each half eases out, so the page clears quickly and the new one settles gently.
                bool fadingIn = _progress >= 0;
                double half = fadingIn ? _progress : -_progress;
                double opacity = fadingIn ? 1 - Math.Pow(1 - half, 3) : Math.Pow(half, 3);

                Bitmap? picture = fadingIn ? _newPicture : _oldPicture;
                if (picture == null || opacity <= 0) return;

                using var attributes = new ImageAttributes();
                attributes.SetColorMatrix(new ColorMatrix { Matrix33 = (float)opacity });
                e.Graphics.DrawImage(picture, new Rectangle(Point.Empty, picture.Size),
                    0, 0, picture.Width, picture.Height, GraphicsUnit.Pixel, attributes);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _timer.Dispose();
                    _oldPicture.Dispose();
                    _newPicture?.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }
}
