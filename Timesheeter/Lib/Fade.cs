using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Timesheeter.Lib
{
    /// <summary>
    /// Fades a control in or out. WinForms controls have no opacity, so this blends the control's
    /// colours between its parent's background and their themed values.
    /// </summary>
    public static class Fade
    {
        private const int DurationMs = 750;
        private const int FrameMs = 15;

        // The running fade per control, so a new fade can stop the previous one.
        private static readonly ConditionalWeakTable<Control, Running> _running = new();

        private sealed class Running
        {
            public required System.Windows.Forms.Timer Timer { get; init; }
            public required Colors Themed { get; init; }
            public bool FadingOut { get; init; }

            // The cursor to put back after a fade-out, which shows the default arrow meanwhile.
            public Cursor? Cursor { get; init; }
        }

        /// <summary>The colours to fade to or from (Theme.Apply styles hidden controls too).</summary>
        /// <remarks>
        /// A flat button paints MouseOverBackColor / MouseDownBackColor instead of BackColor while the
        /// mouse is over it, e.g. a Cancel button fading out right after it was clicked, so those fade too.
        /// </remarks>
        private readonly record struct Colors(Color Back, Color Fore, Color? Border, Color? MouseOver, Color? MouseDown);

        /// <summary>
        /// True while <paramref name="control"/> is fading out. It's already on its way out, so click
        /// handlers should ignore it (e.g. <c>if (Fade.IsFadingOut(button)) return;</c>).
        /// </summary>
        public static bool IsFadingOut(Control control) =>
            _running.TryGetValue(control, out var running) && running.FadingOut;

        /// <summary>Makes <paramref name="control"/> visible and fades it in from its parent's background.</summary>
        public static void In(Control control)
        {
            // Fading in while still fading out: carry on from the colours it had before.
            var themed = Stop(control) ?? Capture(control);

            Apply(control, themed, 0);
            control.Visible = true;

            Run(control, themed, fadingOut: false, t => t, () => Apply(control, themed, 1));
        }

        /// <summary>Fades <paramref name="control"/> into its parent's background, then hides it.</summary>
        public static void Out(Control control)
        {
            var themed = Stop(control) ?? Capture(control);

            if (!control.Visible)
            {
                Apply(control, themed, 1);
                return;
            }

            // No hand cursor on the way out: it's no longer something to click.
            Run(control, themed, fadingOut: true, t => 1 - t, () =>
            {
                control.Visible = false;
                // Hidden with its normal colours, ready to be shown (or faded in) again.
                Apply(control, themed, 1);
            });
        }

        private static Colors Capture(Control control)
        {
            var flat = (control as ButtonBase)?.FlatAppearance;
            return new(control.BackColor, control.ForeColor, flat?.BorderColor, flat?.MouseOverBackColor, flat?.MouseDownBackColor);
        }

        /// <summary>Sets the colours <paramref name="amount"/> (0-1) of the way from the parent's background to <paramref name="themed"/>.</summary>
        private static void Apply(Control control, Colors themed, double amount)
        {
            Color from = control.Parent?.BackColor ?? themed.Back;

            control.BackColor = Theme.Blend(from, themed.Back, amount);
            control.ForeColor = Theme.Blend(from, themed.Fore, amount);

            if (control is ButtonBase button)
            {
                // Empty means "use the default", which can't be blended; leave those as they are.
                Color? Blended(Color? themedColor) => themedColor is Color c && !c.IsEmpty && amount < 1
                    ? Theme.Blend(from, c, amount)
                    : themedColor;

                var flat = button.FlatAppearance;
                if (Blended(themed.Border) is Color border) flat.BorderColor = border;
                if (Blended(themed.MouseOver) is Color mouseOver) flat.MouseOverBackColor = mouseOver;
                if (Blended(themed.MouseDown) is Color mouseDown) flat.MouseDownBackColor = mouseDown;
            }
        }

        /// <param name="amountAt">Maps eased progress (0-1) to how far the colours are from the background.</param>
        private static void Run(Control control, Colors themed, bool fadingOut, Func<double, double> amountAt, Action finish)
        {
            Cursor? cursor = null;
            if (fadingOut)
            {
                cursor = control.Cursor;
                control.Cursor = Cursors.Default;
            }

            var timer = new System.Windows.Forms.Timer { Interval = FrameMs };
            long started = Environment.TickCount64;

            timer.Tick += (s, e) =>
            {
                if (control.IsDisposed)
                {
                    Stop(control);
                    return;
                }

                double t = Math.Min(1.0, (Environment.TickCount64 - started) / (double)DurationMs);
                if (t >= 1.0)
                {
                    Stop(control);
                    finish();
                    return;
                }

                // Ease out: quick at first, settling gently at the end.
                Apply(control, themed, amountAt(1 - Math.Pow(1 - t, 3)));
            };

            _running.AddOrUpdate(control, new Running { Timer = timer, Themed = themed, FadingOut = fadingOut, Cursor = cursor });

            // The theme's hover aura (painted by the parent) is left out while fading out.
            if (fadingOut) control.Parent?.Invalidate();
            timer.Start();
        }

        /// <summary>Stops a running fade, returning the themed colours it was fading between.</summary>
        private static Colors? Stop(Control control)
        {
            if (!_running.TryGetValue(control, out var running)) return null;

            running.Timer.Stop();
            running.Timer.Dispose();
            _running.Remove(control);
            if (running.Cursor != null && !control.IsDisposed) control.Cursor = running.Cursor;
            return running.Themed;
        }
    }
}
