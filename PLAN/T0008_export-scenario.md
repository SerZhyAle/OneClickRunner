# Spec: T0008 - Export scenario to XML

**Ticket:** T0008 · **Proposal:** A4 · **Status:** Draft · **Priority:** 55 · **Tier:** Easy · **Date:** 2026-07-11
**Complexity:** Primitive (1-2 files, reuses existing serializer) - implement directly on approval, no tactical plan.

## Problem
The settings window can **Import** a scenario from an `.xml` file but cannot **Export** one.
Given the one-file-per-scenario storage model this asymmetry is arbitrary - export is nearly
free and makes scenarios shareable/backupable.

## Approach
- `OneClickRunner/MainWindow.xaml` - add an **Export** button next to Import.
- `OneClickRunner/MainWindow.xaml.cs` - on click, take the selected scenario, show a
  `SaveFileDialog` (default name from the scenario name, `.xml`), and serialize the `AppItem`
  with the same `XmlSerializer` used by `ConfigurationService`/Import.
- Reuse existing serialization; do not fork a second format.

## Done criteria
- Exporting a selected scenario writes an `.xml` that Import can read back into an equivalent scenario.
- Export with no selection shows the same "select an item" guidance as Run/Edit/Remove.

## Links
- Symmetric with the existing Import; interacts with ordering (T0005: imported items append).
