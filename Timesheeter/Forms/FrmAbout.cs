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

        private readonly Bitmap? _logo;

        /// <summary>Shows the About dialog over <paramref name="owner"/>.</summary>
        public static void ShowAbout(IWin32Window? owner)
        {
            using var about = new FrmAbout();
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
            StartPosition = FormStartPosition.CenterParent;
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
            if (disposing) _logo?.Dispose();
            base.Dispose(disposing);
        }
    }
}
