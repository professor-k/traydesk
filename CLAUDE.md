# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

TrayDesk is a Windows system-tray app that tracks how much of your day you spend standing vs. sitting at a height-adjustable desk. An Arduino + HC-SR04 ultrasonic sensor mounted under the desktop reports the distance to the floor once per second over a serial (COM) port; the app classifies each reading as "up" or "down" against a height threshold and warns (via a blinking tray icon) when your standing share falls below a configured target.

The repo contains two independent parts:
- **The .NET app** — Windows Forms tray application at the repo root (`*.cs`, `TrayDesk.csproj`).
- **The Arduino sketch** — `TrayDesk/TrayDesk.ino`, flashed to the microcontroller. It just prints `measureDistanceCm()` to serial every second. Pins: VCC=D4, TRIG=D5, ECHO=D6, GND=D7. The `ReportingSpan` app setting must match the sketch's `delay()` (both 1 second).

## Build & run

```powershell
dotnet build                              # Debug build
dotnet build -c Release
dotnet run                                # build + launch the tray app
```

Targets `net8.0-windows` with WinForms. There are **no tests** and no linter configured. The `TrayDesk.sln` and stale `obj/` folders reference older frameworks (net5.0/net6.0) — the csproj is the source of truth (net8.0).

## Architecture

The app is a classic WinForms `ApplicationContext` tray app with no main window. Data flows one direction: serial port → timer → state → icon.

- **`Program.cs`** — entry point. Configures log4net (`XmlConfigurator`, see `App.config`), wires global exception handlers, runs `TrayDeskApplicationContext`.
- **`TrayDeskApplicationContext.cs`** — the composition root and UI loop. Owns the `NotifyIcon`, the context menu (Pause/Resume, Exit), and a 1-second WinForms `Timer`. On each tick it: reopens the serial port if dropped, picks an `IconType` from current state, redraws the tray icon, and updates the tooltip. **Important:** `GetHicon()` leaks a native handle — the tick calls `DestroyIcon` via P/Invoke after assigning the icon. Preserve this when touching icon code.
- **`SerialPortParser.cs`** — finds the Arduino and reads from it. There is no fixed COM port: it discovers the port by reading the Windows registry under `usbser` and `CH341SER_A64` (CH340 clones) service Enum keys (see `_arduinoKeys`). Serial reads can split mid-number, so it buffers partial lines and only parses complete `\r`/`\n`-terminated tokens, raising `DataReceived(int)` per reading. `TryReopenIfNeeded()` is called every tick to survive sleep/wake and reconnects.
- **`UpDownTimer.cs`** — the domain model / state. Accumulates `Up`/`Down` `TimeSpan`s, computes `UpShare`, `AvailableDownTime`, and the `ShowWarning`/`ShowError` flags. Subscribes to `SystemEvents.SessionSwitch` so time is **only counted while the PC is unlocked**. `ShowError` fires if no reading arrives within 5 seconds (sensor malfunction). `ResetIfDaybreak()` zeroes the counters at the configured `Daybreak` time.
- **`IconBuilder.cs`** — renders the 16×16 tray icon by hand-plotting pixels. Holds a 3×5 bitmap font for digits 0–9 and draws two `HH:MM` rows (green = up, red = down). `IconType` is a `[Flags]` enum (`Disconnected`/`Warn`/`Error` + `Blink`); the tick toggles `Blink` each second to animate warnings.

### State persistence

There is no database. All configuration **and** the running Up/Down/LastReport state live in .NET user settings (`Properties/Settings.settings` → `Settings.Designer.cs`, backed by `App.config`). `UpDownTimer` reads settings on construction and calls `Settings.Default.Save()` once a minute (when `LastReport.Second == 0`). This means counters survive restarts within the same day.

### Configuration (no settings UI — edit `App.config`)

| Setting | Meaning |
|---|---|
| `HeightThreshold` | cm from sensor to floor; readings above = "up", below = "down". Set between your sit/stand heights. |
| `MinUpShare` | target standing fraction, 0–1 (e.g. 0.4). Warns when actual share drops below this. |
| `ReportingSpan` | time each reading represents. Must equal the Arduino `delay()` (1s). |
| `Daybreak` | time of day the counters reset to zero. |
| `DontWarnBefore` | suppress warnings until this much total time has accrued (lets you sit first). |

## Conventions

- C# `LanguageVersion=latest` — collection expressions (`[ ... ]`), target-typed `new`, switch expressions, range/index operators are all used. Match the existing terse, comment-the-why style.
- Logging is log4net to a `traydesk.log` file appender (INFO root level). Use the existing `LogManager.GetLogger` pattern.
- Windows-only by design (registry port discovery, `SystemEvents`, WinForms, `user32.dll`).
