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
        /// <summary>A form/side panel, slightly set apart from the main content.</summary>
        Sidebar,
        /// <summary>Left exactly as designed, including its children (e.g. color swatches).</summary>
        Ignore,
    }

    /// <summary>The colors for one mode and accent. Built by <see cref="Theme"/>; read-only.</summary>
    public sealed class ThemePalette
    {
        public required Color Window { get; init; }
        public required Color Sidebar { get; init; }
        public required Color Text { get; init; }
        public required Color MutedText { get; init; }
        public required Color Border { get; init; }

        public required Color Navigation { get; init; }
        public required Color NavigationText { get; init; }
        public required Color NavigationHover { get; init; }

        public required Color InputBack { get; init; }
        public required Color InputText { get; init; }

        public required Color GridBack { get; init; }
        public required Color GridAlternateRow { get; init; }
        public required Color GridLines { get; init; }
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
    /// Layout is never changed, only colors and flat styles, so designer files stay as they are.
    /// </summary>
    public static class Theme
    {
        /// <summary>Accent presets offered in Settings. Any other color works too.</summary>
        public static readonly IReadOnlyList<(string Name, Color Color)> AccentPresets =
        [
            ("Blue", Color.FromArgb(37, 99, 235)),
            ("Teal", Color.FromArgb(13, 148, 136)),
            ("Green", Color.FromArgb(22, 163, 74)),
            ("Purple", Color.FromArgb(124, 58, 237)),
            ("Orange", Color.FromArgb(234, 88, 12)),
            ("Rose", Color.FromArgb(225, 29, 72)),
            ("Slate", Color.FromArgb(71, 85, 105)),
        ];

        public static ThemeMode Mode { get; private set; } = ThemeMode.LightWithDarkNavigation;
        public static Color Accent { get; private set; } = AccentPresets[0].Color;
        public static ThemePalette Current { get; private set; } = BuildPalette(Mode, Accent);

        /// <summary>Raised after the mode or accent changes and open forms have been restyled.</summary>
        public static event EventHandler? Changed;

        private static readonly ConditionalWeakTable<Control, StrongBox<ThemeRole>> _roles = new();
        private static readonly ConditionalWeakTable<Control, object> _watched = new();
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
            if (RoleOf(control) == ThemeRole.Ignore) return;

            Style(control);

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
            bool inNavigation = IsInside(control, ThemeRole.Navigation);

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
                    if (inNavigation) StyleNavigationButton(button, p);
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
                    break;

                case ThemeRole.Sidebar:
                    control.BackColor = p.Sidebar;
                    control.ForeColor = p.Text;
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
            Button? active = null;
            _activeNavigationButton?.TryGetTarget(out active);
            bool isActive = button == active;

            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.BackColor = isActive ? p.Accent : p.Navigation;
            button.ForeColor = isActive ? p.AccentText : p.NavigationText;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = isActive ? p.AccentHover : p.NavigationHover;
            button.FlatAppearance.MouseDownBackColor = isActive ? p.AccentPressed : p.NavigationHover;
            button.Cursor = Cursors.Hand;
        }

        private static void StyleGrid(DataGridView grid, ThemePalette p)
        {
            grid.EnableHeadersVisualStyles = false; // otherwise Windows ignores the header colors
            grid.BackgroundColor = p.Window;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = p.GridLines;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            grid.DefaultCellStyle.BackColor = p.GridBack;
            grid.DefaultCellStyle.ForeColor = p.Text;
            grid.DefaultCellStyle.SelectionBackColor = p.GridSelection;
            grid.DefaultCellStyle.SelectionForeColor = p.Text;
            grid.AlternatingRowsDefaultCellStyle.BackColor = p.GridAlternateRow;

            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = p.Accent;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = p.AccentText;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = p.Accent;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = p.AccentText;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 0, 4, 0);

            grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.RowHeadersDefaultCellStyle.BackColor = p.GridBack;
            grid.RowHeadersDefaultCellStyle.SelectionBackColor = p.GridSelection;
        }

        #endregion

        #region Palettes

        private static ThemePalette BuildPalette(ThemeMode mode, Color accent)
        {
            Color accentText = Luminance(accent) > 0.55 ? Color.FromArgb(17, 24, 39) : Color.White;

            if (mode == ThemeMode.Dark)
            {
                Color gridBack = Color.FromArgb(43, 45, 49);
                return new ThemePalette
                {
                    IsDark = true,
                    Window = Color.FromArgb(30, 31, 34),
                    Sidebar = Color.FromArgb(37, 39, 43),
                    Text = Color.FromArgb(227, 229, 232),
                    MutedText = Color.FromArgb(160, 164, 171),
                    Border = Color.FromArgb(58, 61, 68),
                    Navigation = Color.FromArgb(24, 25, 28),
                    NavigationText = Color.FromArgb(220, 221, 222),
                    NavigationHover = Color.FromArgb(44, 46, 51),
                    InputBack = Color.FromArgb(49, 51, 56),
                    InputText = Color.FromArgb(227, 229, 232),
                    GridBack = gridBack,
                    GridAlternateRow = Color.FromArgb(49, 51, 56),
                    GridLines = Color.FromArgb(58, 61, 68),
                    GridHighlight = Blend(gridBack, accent, 0.18),
                    GridSelection = Blend(gridBack, accent, 0.40),
                    Accent = accent,
                    AccentText = accentText,
                    AccentHover = Blend(accent, Color.White, 0.15),
                    AccentPressed = Blend(accent, Color.Black, 0.15),
                };
            }

            bool darkNavigation = mode == ThemeMode.LightWithDarkNavigation;
            Color lightGrid = Color.White;
            return new ThemePalette
            {
                IsDark = false,
                Window = Color.FromArgb(245, 246, 248),
                Sidebar = Color.White,
                Text = Color.FromArgb(31, 41, 55),
                MutedText = Color.FromArgb(107, 114, 128),
                Border = Color.FromArgb(217, 221, 227),
                Navigation = darkNavigation ? Color.FromArgb(31, 36, 48) : Color.FromArgb(234, 236, 240),
                NavigationText = darkNavigation ? Color.FromArgb(230, 232, 236) : Color.FromArgb(31, 41, 55),
                NavigationHover = darkNavigation ? Color.FromArgb(44, 50, 64) : Color.FromArgb(220, 223, 229),
                InputBack = Color.White,
                InputText = Color.FromArgb(31, 41, 55),
                GridBack = lightGrid,
                GridAlternateRow = Color.FromArgb(248, 249, 251),
                GridLines = Color.FromArgb(229, 231, 235),
                GridHighlight = Blend(lightGrid, accent, 0.10),
                GridSelection = Blend(lightGrid, accent, 0.22),
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
            else form.HandleCreated += (s, e) => Set();
        }

        #endregion
    }
}
