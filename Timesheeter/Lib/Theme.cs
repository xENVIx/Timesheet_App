using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Timesheeter.Lib
{
    public enum ThemeMode
    {
        Light,
        LightWithDarkNavigation,
        Dark,
    }

    /// <summary>What a container is for, which decides its colors. Set with <see cref="Theme.SetRole"/>.</summary>
    public enum ThemeRole
    {
        /// <summary>Inherits its parent's colors (the default).</summary>
        None,
        /// <summary>The navigation column; buttons inside it are styled as navigation items.</summary>
        Navigation,
        /// <summary>A form/side panel, set apart from the main content by a separator line.</summary>
        Sidebar,
        /// <summary>Left exactly as designed, including everything inside it (e.g. color swatches).</summary>
        Ignore,
    }

    /// <summary>The colors for one mode and accent. Built by <see cref="Theme"/>; read-only.</summary>
    public sealed class ThemePalette
    {
        /// <summary>Main content background.</summary>
        public required Color Window { get; init; }
        /// <summary>Form/side panel background.</summary>
        public required Color Sidebar { get; init; }
        public required Color Text { get; init; }
        public required Color MutedText { get; init; }
        /// <summary>Separator lines between areas.</summary>
        public required Color Border { get; init; }

        public required Color Navigation { get; init; }
        public required Color NavigationText { get; init; }
        public required Color NavigationHover { get; init; }
        public required Color NavigationActive { get; init; }
        public required Color NavigationActiveText { get; init; }

        public required Color InputBack { get; init; }
        public required Color InputText { get; init; }

        public required Color GridBack { get; init; }
        public required Color GridAlternateRow { get; init; }
        public required Color GridLines { get; init; }
        public required Color GridHeaderBack { get; init; }
        public required Color GridHeaderText { get; init; }
        /// <summary>Background for special grid rows such as dates and totals.</summary>
        public required Color GridHighlight { get; init; }
        public required Color GridSelection { get; init; }

        public required Color Accent { get; init; }
        public required Color AccentText { get; init; }
        public required Color AccentHover { get; init; }
        public required Color AccentPressed { get; init; }

        public required bool IsDark { get; init; }
    }

    /// <summary>
    /// App-wide look. Call <see cref="Set"/> to choose the mode and accent color, and
    /// <see cref="Apply"/> on a form to style it and everything added to it later.
    /// Layouts are never moved or resized, so designer files stay as they are.
    /// </summary>
    /// <remarks>
    /// All three modes use one neutral grey scale, so any accent color sits on it cleanly.
    /// The accent is used sparingly: main buttons, the current navigation item, selection.
    /// </remarks>
    public static class Theme
    {
        // One neutral grey scale (zinc) shared by every mode, lightest to darkest.
        // Declared before Current: static fields initialise in file order, and Current's palette
        // reads these. Below it they were still Color.Empty when the palette was built, which the
        // designer (where Theme.Set never runs) passed on to DateTimePicker and it threw.
        private static readonly Color Grey50 = Color.FromArgb(250, 250, 250);
        private static readonly Color Grey100 = Color.FromArgb(244, 244, 245);
        private static readonly Color Grey200 = Color.FromArgb(228, 228, 231);
        private static readonly Color Grey300 = Color.FromArgb(212, 212, 216);
        private static readonly Color Grey400 = Color.FromArgb(161, 161, 170);
        private static readonly Color Grey500 = Color.FromArgb(113, 113, 122);
        private static readonly Color Grey600 = Color.FromArgb(82, 82, 91);
        private static readonly Color Grey800 = Color.FromArgb(39, 39, 42);
        private static readonly Color Grey850 = Color.FromArgb(31, 31, 35);
        private static readonly Color Grey900 = Color.FromArgb(24, 24, 27);
        private static readonly Color Grey950 = Color.FromArgb(17, 17, 19);

        /// <summary>Accent presets offered in Settings: deep enough for white text. Any other color works too.</summary>
        public static readonly IReadOnlyList<(string Name, Color Color)> AccentPresets =
        [
            ("Blue", Color.FromArgb(37, 99, 235)),
            ("Indigo", Color.FromArgb(79, 70, 229)),
            ("Violet", Color.FromArgb(124, 58, 237)),
            ("Teal", Color.FromArgb(15, 118, 110)),
            ("Green", Color.FromArgb(21, 128, 61)),
            ("Amber", Color.FromArgb(180, 83, 9)),
            ("Rose", Color.FromArgb(190, 18, 60)),
            ("Slate", Color.FromArgb(71, 85, 105)),
        ];

        public static ThemeMode Mode { get; private set; } = ThemeMode.LightWithDarkNavigation;
        public static Color Accent { get; private set; } = AccentPresets[0].Color;
        public static ThemePalette Current { get; private set; } = BuildPalette(Mode, Accent);

        /// <summary>Raised after the mode or accent changes and open forms have been restyled.</summary>
        public static event EventHandler? Changed;

        // Spacing: roomier grids are most of what makes the app feel less cramped.
        private const int GridRowHeight = 30;
        private const int GridHeaderHeight = 34;
        private static readonly Padding GridCellPadding = new Padding(8, 0, 8, 0);
        private const int ActiveNavigationBarWidth = 3;

        private static readonly ConditionalWeakTable<Control, StrongBox<ThemeRole>> _roles = new();
        private static readonly ConditionalWeakTable<Control, object> _watched = new();
        private static readonly ConditionalWeakTable<Control, object> _painted = new();
        private static WeakReference<Button>? _activeNavigationButton;

        public static void Set(ThemeMode mode, Color accent)
        {
            Mode = mode;
            Accent = Color.FromArgb(255, accent);
            Current = BuildPalette(Mode, Accent);

            foreach (Form form in Application.OpenForms) Apply(form);

            Changed?.Invoke(null, EventArgs.Empty);
        }

        public static void SetRole(Control control, ThemeRole role)
        {
            _roles.AddOrUpdate(control, new StrongBox<ThemeRole>(role));
            if (control.Parent != null) Apply(control);
        }

        /// <summary>Highlights the navigation button for the page being shown.</summary>
        public static void SetActiveNavigationButton(Button button)
        {
            Button? previous = null;
            _activeNavigationButton?.TryGetTarget(out previous);
            _activeNavigationButton = new WeakReference<Button>(button);

            if (previous != null && previous != button) Apply(previous);
            Apply(button);
        }

        /// <summary>Styles a control and its children, now and whenever children are added later.</summary>
        public static void Apply(Control control)
        {
            // Ignore covers the children too, including ones added after the role was set.
            if (IsInside(control, ThemeRole.Ignore)) return;

            Style(control);
            control.Invalidate();

            if (!_watched.TryGetValue(control, out _))
            {
                _watched.Add(control, new object());
                control.ControlAdded += (s, e) => { if (e.Control != null) Apply(e.Control); };
            }

            foreach (Control child in control.Controls) Apply(child);
        }

        #region Styling per control type

        private static void Style(Control control)
        {
            var p = Current;

            switch (control)
            {
                case Form form:
                    form.BackColor = p.Window;
                    form.ForeColor = p.Text;
                    SetDarkTitleBar(form, p.IsDark);
                    break;

                case DataGridView grid:
                    StyleGrid(grid, p);
                    break;

                case Button button:
                    if (IsInside(button, ThemeRole.Navigation)) StyleNavigationButton(button, p);
                    else StyleButton(button, p);
                    break;

                case TextBox textBox:
                    textBox.BackColor = p.InputBack;
                    textBox.ForeColor = p.InputText;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ComboBox comboBox:
                    comboBox.BackColor = p.InputBack;
                    comboBox.ForeColor = p.InputText;
                    comboBox.FlatStyle = FlatStyle.Flat;
                    break;

                case DateTimePicker picker:
                    // Windows draws the picker's text box itself, so only the drop-down calendar
                    // follows the theme. In dark mode the field stays light.
                    picker.CalendarMonthBackground = p.InputBack;
                    picker.CalendarForeColor = p.InputText;
                    picker.CalendarTitleBackColor = p.Accent;
                    picker.CalendarTitleForeColor = p.AccentText;
                    picker.CalendarTrailingForeColor = p.MutedText;
                    break;

                case CheckBox or RadioButton or Label:
                    // Inherit the parent's colors (navigation, sidebar or window).
                    control.ResetBackColor();
                    control.ResetForeColor();
                    break;

                default:
                    StyleContainer(control, p);
                    break;
            }
        }

        private static void StyleContainer(Control control, ThemePalette p)
        {
            switch (RoleOf(control))
            {
                case ThemeRole.Navigation:
                    control.BackColor = p.Navigation;
                    control.ForeColor = p.NavigationText;
                    PaintOnce(control, DrawRightSeparator);
                    break;

                case ThemeRole.Sidebar:
                    control.BackColor = p.Sidebar;
                    control.ForeColor = p.Text;
                    PaintOnce(control, DrawRightSeparator);
                    break;

                default:
                    // Panels, user controls etc. take their parent's colors.
                    control.ResetBackColor();
                    control.ResetForeColor();
                    break;
            }
        }

        private static void StyleButton(Button button, ThemePalette p)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.BackColor = p.Accent;
            button.ForeColor = p.AccentText;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = p.AccentHover;
            button.FlatAppearance.MouseDownBackColor = p.AccentPressed;
            button.Cursor = Cursors.Hand;
        }

        private static void StyleNavigationButton(Button button, ThemePalette p)
        {
            bool isActive = IsActiveNavigationButton(button);

            // The current page gets a tinted background, accent-colored text and a bar on the left,
            // rather than a solid accent block.
            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.BackColor = isActive ? p.NavigationActive : p.Navigation;
            button.ForeColor = isActive ? p.NavigationActiveText : p.NavigationText;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = isActive ? p.NavigationActive : p.NavigationHover;
            button.FlatAppearance.MouseDownBackColor = isActive ? p.NavigationActive : p.NavigationHover;
            button.Cursor = Cursors.Hand;
            PaintOnce(button, DrawActiveNavigationBar);
        }

        private static void StyleGrid(DataGridView grid, ThemePalette p)
        {
            grid.EnableHeadersVisualStyles = false; // otherwise Windows ignores the header colors
            grid.BackgroundColor = p.GridBack;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = p.GridLines;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.RowHeadersVisible = false; // the grey row-selector column

            grid.DefaultCellStyle.BackColor = p.GridBack;
            grid.DefaultCellStyle.ForeColor = p.Text;
            grid.DefaultCellStyle.SelectionBackColor = p.GridSelection;
            grid.DefaultCellStyle.SelectionForeColor = p.Text;
            grid.DefaultCellStyle.Padding = GridCellPadding;
            grid.AlternatingRowsDefaultCellStyle.BackColor = p.GridAlternateRow;

            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersDefaultCellStyle.BackColor = p.GridHeaderBack;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = p.GridHeaderText;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = p.GridHeaderBack;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = p.GridHeaderText;
            grid.ColumnHeadersDefaultCellStyle.Padding = GridCellPadding;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = GridHeaderHeight;

            // New rows use the template; rows that already exist are resized here.
            grid.RowTemplate.Height = GridRowHeight;
            foreach (DataGridViewRow row in grid.Rows) row.Height = GridRowHeight;

            // Every column as wide as its widest value or header, so nothing is cut off with "...".
            // Re-measured whenever the data changes; a grid wider than its space gets a scrollbar.
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        #endregion

        #region Custom painting

        /// <summary>Adds a paint step to a control once; it reads <see cref="Current"/> each time.</summary>
        private static void PaintOnce(Control control, Action<Control, PaintEventArgs> paint)
        {
            if (_painted.TryGetValue(control, out _)) return;
            _painted.Add(control, new object());
            control.Paint += (s, e) => paint(control, e);
        }

        private static void DrawRightSeparator(Control control, PaintEventArgs e)
        {
            using var pen = new Pen(Current.Border);
            e.Graphics.DrawLine(pen, control.Width - 1, 0, control.Width - 1, control.Height);
        }

        private static void DrawActiveNavigationBar(Control control, PaintEventArgs e)
        {
            if (control is not Button button || !IsActiveNavigationButton(button)) return;

            using var brush = new SolidBrush(Current.Accent);
            e.Graphics.FillRectangle(brush, 0, 0, ActiveNavigationBarWidth, button.Height);
        }

        #endregion

        #region Palettes


        private static ThemePalette BuildPalette(ThemeMode mode, Color accent)
        {
            Color accentText = Luminance(accent) > 0.45 ? Grey900 : Color.White;

            if (mode == ThemeMode.Dark)
            {
                return new ThemePalette
                {
                    IsDark = true,
                    Window = Grey900,
                    Sidebar = Grey850,
                    Text = Grey200,
                    MutedText = Grey400,
                    Border = Grey800,

                    Navigation = Grey950,
                    NavigationText = Grey400,
                    NavigationHover = Grey850,
                    NavigationActive = Blend(Grey950, accent, 0.22),
                    NavigationActiveText = Color.White,

                    InputBack = Grey800,
                    InputText = Grey100,

                    GridBack = Grey900,
                    GridAlternateRow = Blend(Grey900, Grey850, 0.6),
                    GridLines = Grey800,
                    GridHeaderBack = Grey850,
                    GridHeaderText = Grey400,
                    GridHighlight = Grey850,
                    GridSelection = Blend(Grey900, accent, 0.35),

                    Accent = accent,
                    AccentText = accentText,
                    // Darker on hover, not lighter: lightening drops white text below readable contrast.
                    AccentHover = Blend(accent, Color.Black, 0.10),
                    AccentPressed = Blend(accent, Color.Black, 0.20),
                };
            }

            bool darkNavigation = mode == ThemeMode.LightWithDarkNavigation;
            return new ThemePalette
            {
                IsDark = false,
                Window = Grey100,
                Sidebar = Color.White,
                Text = Grey900,
                MutedText = Grey500,
                Border = Grey200,

                Navigation = darkNavigation ? Grey900 : Grey50,
                NavigationText = darkNavigation ? Grey300 : Grey600,
                NavigationHover = darkNavigation ? Grey800 : Grey100,
                NavigationActive = darkNavigation ? Blend(Grey900, accent, 0.25) : Blend(Color.White, accent, 0.12),
                NavigationActiveText = darkNavigation ? Color.White : Blend(accent, Color.Black, 0.2),

                InputBack = Color.White,
                InputText = Grey900,

                GridBack = Color.White,
                GridAlternateRow = Grey50,
                GridLines = Grey100,
                GridHeaderBack = Grey100,
                GridHeaderText = Grey600,
                GridHighlight = Grey100,
                GridSelection = Blend(Color.White, accent, 0.14),

                Accent = accent,
                AccentText = accentText,
                AccentHover = Blend(accent, Color.Black, 0.10),
                AccentPressed = Blend(accent, Color.Black, 0.20),
            };
        }

        /// <summary>Mixes <paramref name="amount"/> (0-1) of <paramref name="with"/> into <paramref name="color"/>.</summary>
        public static Color Blend(Color color, Color with, double amount)
        {
            int Mix(int a, int b) => (int)Math.Round(a + (b - a) * amount);
            return Color.FromArgb(Mix(color.R, with.R), Mix(color.G, with.G), Mix(color.B, with.B));
        }

        /// <summary>Relative luminance, 0 (black) to 1 (white), used to pick readable text on the accent.</summary>
        private static double Luminance(Color c)
        {
            static double Channel(int v)
            {
                double s = v / 255.0;
                return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
            }
            return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
        }

        #endregion

        #region Helpers

        private static ThemeRole RoleOf(Control control) =>
            _roles.TryGetValue(control, out var box) ? box.Value : ThemeRole.None;

        private static bool IsInside(Control control, ThemeRole role)
        {
            for (Control? c = control; c != null; c = c.Parent)
            {
                if (RoleOf(c) == role) return true;
            }
            return false;
        }

        private static bool IsActiveNavigationButton(Button button)
        {
            Button? active = null;
            _activeNavigationButton?.TryGetTarget(out active);
            return button == active;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        /// <summary>Dark window title bar on Windows 10 (1809+) and 11; ignored elsewhere.</summary>
        private static void SetDarkTitleBar(Form form, bool dark)
        {
            void Set()
            {
                int value = dark ? 1 : 0;
                try
                {
                    // 20 = DWMWA_USE_IMMERSIVE_DARK_MODE; 19 on Windows 10 builds before 20H1.
                    if (DwmSetWindowAttribute(form.Handle, 20, ref value, sizeof(int)) != 0)
                        DwmSetWindowAttribute(form.Handle, 19, ref value, sizeof(int));
                }
                catch (DllNotFoundException) { }
                catch (EntryPointNotFoundException) { }
            }

            if (form.IsHandleCreated) Set();
            else if (!_watched.TryGetValue(form, out _)) form.HandleCreated += (s, e) => SetDarkTitleBar(form, Current.IsDark);
        }

        #endregion
    }
}
