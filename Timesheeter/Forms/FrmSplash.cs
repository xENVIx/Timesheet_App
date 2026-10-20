using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;

namespace Timesheeter.Forms
{
    /// <summary>
    /// The splash screen shown while the app starts: opened before the database and data load,
    /// faded out once the main window is on screen (<see cref="CloseWhenShown"/>).
    /// </summary>
    /// <remarks>
    /// The saved theme isn't loaded yet when this opens, so it has fixed colors of its own: dark,
    /// with the icon's cyan and pink.
    /// </remarks>
    public class FrmSplash : Form
    {
        private const int FadeOutMs = 300;

        private static readonly Color Background = Color.FromArgb(24, 24, 27);   // zinc 900
        private static readonly Color Border = Color.FromArgb(63, 63, 70);       // zinc 700
        private static readonly Color TitleColor = Color.FromArgb(244, 244, 245); // zinc 100
        private static readonly Color SubtitleColor = Color.FromArgb(161, 161, 170); // zinc 400
        private static readonly Color Cyan = Color.FromArgb(103, 232, 249);
        private static readonly Color Pink = Color.FromArgb(249, 168, 212);

        private readonly Bitmap? _logo;
        private readonly Font _titleFont = new("Segoe UI Semibold", 22f);
        private readonly Font _subtitleFont = new("Segoe UI", 10f);

        /// <summary>Opens the splash screen and paints it straight away (before any message loop runs).</summary>
        public static FrmSplash ShowNow()
        {
            var splash = new FrmSplash();
            splash.Show();
            splash.Update();
            return splash;
        }

        public FrmSplash()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = true; // so the app shows up in the taskbar while it loads
            Text = "Timesheeter";
            BackColor = Background;
            DoubleBuffered = true;
            ClientSize = new Size(LogicalToDeviceUnits(420), LogicalToDeviceUnits(260));

            using var iconStream = typeof(FrmSplash).Assembly.GetManifestResourceStream("Timesheeter.ico");
            if (iconStream != null)
            {
                Icon = new Icon(iconStream);
                iconStream.Position = 0;
                using var large = new Icon(iconStream, new Size(256, 256)); // the largest size in the .ico
                _logo = large.ToBitmap();
            }
        }

        /// <summary>Fades the splash out and closes it once <paramref name="main"/> is on screen.</summary>
        public void CloseWhenShown(Form main)
        {
            main.Shown += (s, e) =>
            {
                main.Activate();
                FadeOutAndClose();
            };
        }

        private void FadeOutAndClose()
        {
            var timer = new System.Windows.Forms.Timer { Interval = 15 };
            long started = Environment.TickCount64;
            timer.Tick += (s, e) =>
            {
                double t = (Environment.TickCount64 - started) / (double)FadeOutMs;
                if (t >= 1)
                {
                    timer.Stop();
                    timer.Dispose();
                    Close();
                    return;
                }
                Opacity = 1 - t;
            };
            timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int width = ClientSize.Width;
            int height = ClientSize.Height;

            // Logo, centered near the top.
            int logoSize = LogicalToDeviceUnits(96);
            int logoTop = LogicalToDeviceUnits(36);
            if (_logo != null)
            {
                g.DrawImage(_logo, new Rectangle((width - logoSize) / 2, logoTop, logoSize, logoSize));
            }

            // Name and status under it.
            int titleTop = logoTop + logoSize + LogicalToDeviceUnits(12);
            var titleSize = TextRenderer.MeasureText(g, "Timesheeter", _titleFont);
            TextRenderer.DrawText(g, "Timesheeter", _titleFont,
                new Rectangle(0, titleTop, width, titleSize.Height), TitleColor, TextFormatFlags.HorizontalCenter);

            TextRenderer.DrawText(g, "Loading…", _subtitleFont,
                new Rectangle(0, titleTop + titleSize.Height, width, LogicalToDeviceUnits(24)), SubtitleColor,
                TextFormatFlags.HorizontalCenter);

            // A cyan-to-pink strip along the bottom, the icon's two colors.
            int stripHeight = LogicalToDeviceUnits(4);
            var strip = new Rectangle(0, height - stripHeight, width, stripHeight);
            using (var brush = new LinearGradientBrush(strip, Cyan, Pink, LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, strip);
            }

            using var border = new Pen(Border);
            g.DrawRectangle(border, 0, 0, width - 1, height - 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logo?.Dispose();
                _titleFont.Dispose();
                _subtitleFont.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
