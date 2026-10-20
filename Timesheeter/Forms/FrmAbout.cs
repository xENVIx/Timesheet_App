using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Timesheeter.Lib;

namespace Timesheeter.Forms
{
    /// <summary>The About dialog: version, publisher, where the data is kept, and third-party notices.</summary>
    public class FrmAbout : Form
    {
        private const string Publisher = "WeissLab";

        private static readonly string ThirdPartyNotices = string.Join(Environment.NewLine, new[]
        {
            "Timesheeter uses these open source components:",
            "",
            "ClosedXML (MIT License) - Excel timesheet reports",
            "DocumentFormat.OpenXml (MIT License)",
            "ScottPlot (MIT License) - charts",
            "SkiaSharp (MIT License) - chart drawing",
            "Dapper (Apache License 2.0) - database access",
            "Microsoft.Data.Sqlite (MIT License) and SQLite (public domain) - database",
            "Fluent UI System Icons, \"Calendar Clock\" (MIT License, Microsoft) - app icon, recolored",
            ".NET and Windows Forms (MIT License, Microsoft)",
        });

        private const int OpenMs = 300;
        private const int CloseMs = 200;
        private const int FrameMs = 15;

        private readonly Bitmap? _logo;

        // Where the window grows from (the About button, in screen coordinates); null = its own center.
        private Rectangle? _from;
        private readonly System.Windows.Forms.Timer _animation = new() { Interval = FrameMs };
        private bool _closing;      // the shrink is running
        private bool _closeAnimated; // the shrink has finished, so the next close really closes

        /// <summary>
        /// Shows the About dialog over <paramref name="owner"/>. It slides and fades in from
        /// <paramref name="from"/> (screen coordinates, e.g. the About button) to the center of the owner.
        /// </summary>
        public static void ShowAbout(IWin32Window? owner, Rectangle? from = null)
        {
            using var about = new FrmAbout();
            about._from = from;
            Theme.Apply(about);
            about.ShowDialog(owner);
        }

        public FrmAbout()
        {
            Text = "About Timesheeter";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual; // placed by the opening animation
            AutoScaleMode = AutoScaleMode.None;
            Font = new Font("Segoe UI", 9f);
            ClientSize = new Size(S(460), S(430));

            using (var iconStream = typeof(FrmAbout).Assembly.GetManifestResourceStream("Timesheeter.ico"))
            {
                if (iconStream != null)
                {
                    Icon = new Icon(iconStream);
                    iconStream.Position = 0;
                    using var large = new Icon(iconStream, new Size(256, 256));
                    _logo = large.ToBitmap();
                }
            }

            var logo = new PictureBox
            {
                Image = _logo,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(S(20), S(20)),
                Size = new Size(S(64), S(64)),
            };

            var title = new Label
            {
                Text = "Timesheeter",
                Font = new Font("Segoe UI Semibold", 18f),
                AutoSize = true,
                Location = new Point(S(100), S(18)),
            };

            var details = new Label
            {
                Text = $"Version {AppVersion()}{Environment.NewLine}" +
                       $"by {Publisher}{Environment.NewLine}" +
                       $".NET {Environment.Version} ({RuntimeInformation.ProcessArchitecture})",
                AutoSize = true,
                Location = new Point(S(102), S(56)),
            };

            var dataLabel = new Label
            {
                Text = "Your data is kept in:",
                AutoSize = true,
                Location = new Point(S(20), S(112)),
            };

            // Opens the folder in Explorer.
            var dataLink = new LinkLabel
            {
                Text = Factory.DataFolder,
                AutoSize = false,
                AutoEllipsis = true,
                Location = new Point(S(20), S(132)),
                Size = new Size(S(420), S(20)),
            };
            dataLink.LinkClicked += (s, e) => OpenFolder(Factory.DataFolder);

            var noticesLabel = new Label
            {
                Text = "Third-party notices",
                AutoSize = true,
                Location = new Point(S(20), S(166)),
            };

            var notices = new TextBox
            {
                Text = ThirdPartyNotices,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(S(20), S(186)),
                Size = new Size(S(420), S(180)),
                TabStop = false,
            };

            var close = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.OK,
                Location = new Point(S(340), S(382)),
                Size = new Size(S(100), S(30)),
            };
            AcceptButton = close;
            CancelButton = close;

            Controls.AddRange(new Control[] { logo, title, details, dataLabel, dataLink, noticesLabel, notices, close });

            // The theme doesn't style links; use the accent so it reads as clickable in every mode.
            Shown += (s, e) =>
            {
                dataLink.LinkColor = Theme.Current.Accent;
                dataLink.ActiveLinkColor = Theme.Current.AccentHover;
                dataLink.VisitedLinkColor = Theme.Current.Accent;
                notices.Select(0, 0);
            };
        }

        private int S(int logical) => LogicalToDeviceUnits(logical);

        #region Open and close animation

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Where it ends up: the center of the app (or of the screen without one), kept on screen.
            Rectangle area = Owner?.Bounds ?? Screen.FromPoint(Cursor.Position).WorkingArea;
            var target = new Rectangle(
                area.Left + (area.Width - Width) / 2,
                area.Top + (area.Height - Height) / 2,
                Width, Height);
            Rectangle screen = Screen.FromRectangle(area).WorkingArea;
            target.X = Math.Max(screen.Left, Math.Min(target.X, screen.Right - target.Width));
            target.Y = Math.Max(screen.Top, Math.Min(target.Y, screen.Bottom - target.Height));

            Rectangle start = _from ?? Shrunk(target);

            // Set before the window first appears, so it never flashes at full size.
            Opacity = 0;
            Bounds = start;

            Animate(start, target, 0, 1, OpenMs, EaseOut, done: null);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.Cancel || _closeAnimated) return;

            // Only animate closes the user asked for (Close button, Esc, the X), not e.g. Windows shutting down.
            if (e.CloseReason is not (CloseReason.UserClosing or CloseReason.None)) return;

            // Stay open while shrinking, then close for real.
            e.Cancel = true;
            if (_closing) return;
            _closing = true;

            Animate(Bounds, Shrunk(Bounds), Opacity, 0, CloseMs, EaseIn, done: () =>
            {
                _closeAnimated = true;
                Close();
            });
        }

        /// <summary>A tiny rectangle at the center of <paramref name="r"/>; Windows keeps a captioned window
        /// at least its minimum size, which the fade hides.</summary>
        private static Rectangle Shrunk(Rectangle r) =>
            new(r.Left + r.Width / 2, r.Top + r.Height / 2, 1, 1);

        private static double EaseOut(double t) => 1 - Math.Pow(1 - t, 3); // fast start, gentle stop
        private static double EaseIn(double t) => t * t * t;                // gentle start, fast finish

        /// <summary>Moves and resizes the window from <paramref name="from"/> to <paramref name="to"/> while fading.</summary>
        private void Animate(Rectangle from, Rectangle to, double opacityFrom, double opacityTo, int ms,
            Func<double, double> ease, Action? done)
        {
            _animation.Stop();
            _animation.Tick -= _animationTick;

            var start = DateTime.UtcNow;
            _animationTick = (s, e) =>
            {
                double t = Math.Min(1, (DateTime.UtcNow - start).TotalMilliseconds / ms);
                double k = ease(t);

                Bounds = new Rectangle(
                    Lerp(from.X, to.X, k), Lerp(from.Y, to.Y, k),
                    Lerp(from.Width, to.Width, k), Lerp(from.Height, to.Height, k));
                Opacity = opacityFrom + (opacityTo - opacityFrom) * k;

                if (t < 1) return;
                _animation.Stop();
                done?.Invoke();
            };
            _animation.Tick += _animationTick;
            _animation.Start();
        }

        private EventHandler? _animationTick;

        private static int Lerp(int a, int b, double k) => (int)Math.Round(a + (b - a) * k);

        #endregion

        /// <summary>The app's version, e.g. "1.0.0" (without the build's "+commit" suffix).</summary>
        private static string AppVersion()
        {
            var assembly = Assembly.GetEntryAssembly() ?? typeof(FrmAbout).Assembly;
            string? version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assembly.GetName().Version?.ToString();
            if (string.IsNullOrEmpty(version)) return "unknown";

            int plus = version.IndexOf('+');
            return plus >= 0 ? version[..plus] : version;
        }

        private void OpenFolder(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not open the folder:{Environment.NewLine}{ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logo?.Dispose();
                _animation.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
