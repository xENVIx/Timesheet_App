using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Timesheeter.Lib
{
    /// <summary>
    /// Fades a control in. WinForms controls have no opacity, so this shows the control in its
    /// parent's background colour and blends its colours to their themed values.
    /// </summary>
    public static class Fade
    {
        private const int DurationMs = 250;
        private const int FrameMs = 15;

        // The running fade per control, so a second call or a hide can stop it.
        private static readonly ConditionalWeakTable<Control, System.Windows.Forms.Timer> _running = new();

        /// <summary>Makes <paramref name="control"/> visible and fades it in from its parent's background.</summary>
        public static void In(Control control)
        {
            Stop(control);

            // The themed colours to end on (Theme.Apply styles hidden controls too).
            Color back = control.BackColor;
            Color fore = control.ForeColor;
            Color from = control.Parent?.BackColor ?? back;

            var button = control as ButtonBase;
            Color? border = button?.FlatAppearance.BorderColor;

            void SetColors(double t)
            {
                control.BackColor = Theme.Blend(from, back, t);
                control.ForeColor = Theme.Blend(from, fore, t);
                if (button != null && border is Color b && !b.IsEmpty)
                    button.FlatAppearance.BorderColor = Theme.Blend(from, b, t);
            }

            void Finish()
            {
                Stop(control);
                control.BackColor = back;
                control.ForeColor = fore;
                if (button != null && border is Color b) button.FlatAppearance.BorderColor = b;
            }

            SetColors(0);
            control.Visible = true;

            var timer = new System.Windows.Forms.Timer { Interval = FrameMs };
            var started = Environment.TickCount64;
            timer.Tick += (s, e) =>
            {
                // Hidden part way through (e.g. Cancel clicked): just restore the colours.
                if (!control.Visible || control.IsDisposed)
                {
                    Finish();
                    return;
                }

                double t = Math.Min(1.0, (Environment.TickCount64 - started) / (double)DurationMs);
                if (t >= 1.0)
                {
                    Finish();
                    return;
                }

                // Ease out: quick at first, settling gently at the end.
                SetColors(1 - Math.Pow(1 - t, 3));
            };

            _running.AddOrUpdate(control, timer);
            timer.Start();
        }

        private static void Stop(Control control)
        {
            if (_running.TryGetValue(control, out var timer))
            {
                timer.Stop();
                timer.Dispose();
                _running.Remove(control);
            }
        }
    }
}
