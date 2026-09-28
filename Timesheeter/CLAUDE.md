# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A Windows Forms (.NET 10, `net10.0-windows`) desktop timesheet app. It persists data via `LibSqlLite`, a sibling class-library project (`../LibSqlLite`) that maps plain C# classes to SQLite tables through reflection — see `../LibSqlLite/CLAUDE.md` for that library's full API and conventions before writing any data-access code here.

There is no git repository at this level or above it. Both projects live under a common parent directory (`Timesheet_App`) but are otherwise independent checkouts; do not assume git commands will work here.

## Commands

```
dotnet build Timesheeter.slnx     # builds Timesheeter + its LibSqlLite project reference
dotnet run --project Timesheeter.csproj
```

`Timesheeter.slnx` references both `Timesheeter.csproj` and `../LibSqlLite/LibSqlLite.csproj`. There are no tests in this project — `LibSqlLite`'s tests live in the separate `../LibSqlLite.Tests` project and are out of scope for changes made here.

## Architecture

**Control composition, not forms.** The app is a single `Form1` hosting a stack of docked `UserControl`s rather than multiple `Form`s:

```
Form1
 └─ UCMain (Dock.Fill)
     └─ UCSelectionMenu (Dock.Fill)
         ├─ menu buttons (e.g. _btnCodes)
         └─ UCNewProjectCode (Dock.Fill, hidden until its menu button is clicked)
```

- **`Lib/UCSubPage`** is the base class for any "sub-page" reached from a menu: it lays out a top bar (`_pnlTopBar`, `Dock.Top`, holding a back button) over a content panel (`_pnlElements`, `Dock.Fill`). The back button click just hides the control (`Visible = false`) rather than navigating a stack — the parent menu control owns showing/hiding it.
- New sub-pages should inherit `Lib.UCSubPage` (see `UserControls/UCNewProjectCode.cs`) and put their fields/inputs in `_pnlElements`, not directly on the control.
- A parent control (e.g. `UCSelectionMenu`) owns instantiating its child sub-page, setting `Dock = DockStyle.Fill`, and starts it `Visible = false; Enabled = false` until its corresponding menu button is clicked.
- Designer-generated fields (`InitializeComponent`, private control fields) live in the paired `.Designer.cs` file per control — edit those through the WinForms designer, not by hand, unless making a small mechanical change.

**Data layer.** `Data/Customers.cs` defines a `Customers.Customer` model (with `[PrimaryKey]` from `LibSqlLite`) intended for persistence via `LibSqlLite.SqliteStore`, but no `SqliteStore` instance is wired up yet anywhere in this project — there's no existing pattern to follow for store lifetime/DI here yet, so check with the user on where it should live (e.g. a static/singleton on `Program`, or owned by `Form1`) before introducing one.
