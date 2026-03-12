# ColoredConsole.NET

A richly-formatted, theme-aware console library for C# — bordered boxes, tables, progress bars, spinners, ANSI true-color gradients, and file logging, all in a single `IDisposable` instance.

[![NuGet](https://img.shields.io/nuget/v/ColoredConsole.NET)](https://www.nuget.org/packages/ColoredConsole.NET)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-purple)](https://docs.microsoft.com/dotnet/standard/net-standard)

---

## Features

| Category | What you get |
|---|---|
| **Borders** | `DrawBox`, `DrawBox2`, `DrawTopLine`, `DrawBottomLine`, `DrawSeparator` |
| **Line styles** | `Double` `═`, `Single` `─`, `Dotted` `·`, `Dashed` `-` — mix freely |
| **Text** | Alignment (Left / Center / Right), tab stops, word-wrap, `TextStyle` transforms |
| **Semantic writes** | `WriteSuccess` ✓ · `WriteError` ✗ · `WriteWarning` ⚠ · `WriteInfo` ℹ |
| **Tables** | Auto-sized columns, 3 border styles, 4 alignments, optional row separators |
| **Columns** | `WriteColumns` — equal-width side-by-side layout |
| **Lists & KV** | `DrawList` (bullet list), `DrawKeyValue` (key: value pairs) |
| **Animation** | `ProgressBar` (determinate), `Spinner` (indeterminate Braille dots) |
| **ANSI color** | 256-color palette, true-color RGB, gradients, Bold/Italic/Underline/… |
| **Themes** | `Default` · `Light` · `Hacker` — or build your own `Theme` |
| **Logging** | Daily `.log` files, configurable formatter, async-safe |
| **Async** | `WriteLineAsync` / `WriteAsync` — awaitable wrappers |
| **Misc** | ASCII fallback mode, runtime `Resize()`, auto-numbering, `Prompt()` |

---

## Installation

```
dotnet add package ColoredConsole.NET
```

```csharp
using bcd;
```

---

## Quick Start

```csharp
using bcd;

using var cc = new ColoredConsole();

cc.DrawBox2(
    header: "My App  ·  v1.0",
    body: new[] { "Ready to process 42 files.", "Press Enter to continue." },
    headerDecoration:   AnsiDecoration.Bold,
    headerGradientFrom: (80, 80, 255),
    headerGradientTo:   (255, 80, 80)
);

cc.DrawTopLine();
cc.WriteSuccess("Build completed in 1.4 s");
cc.WriteWarning("Config file missing — using defaults");
cc.DrawBottomLine();
```

Output:
```
╔═════════════════════════════════════════════════╗
║          M Y   A P P   ·   V 1 . 0              ║  ← gradient + bold
╠═════════════════════════════════════════════════╣
║ Ready to process 42 files.                      ║
║ Press Enter to continue.                        ║
╚═════════════════════════════════════════════════╝
╔═════════════════════════════════════════════════╗
║ ✓ Build completed in 1.4 s                      ║
║ ⚠ Config file missing — using defaults          ║
╚═════════════════════════════════════════════════╝
```

---

## API Reference

### DrawBox — single-line box

```csharp
cc.DrawBox("Section Title");
cc.DrawBox("Right-aligned", textPosition: TextPosition.Right, textStyle: TextStyle.None);
cc.DrawBox("Single style",  lineStyle: LineStyle.Single, foreColor: ConsoleColor.Cyan);
```

```
╔════════════════════════════════════════╗
║    S E C T I O N   T I T L E          ║
╚════════════════════════════════════════╝
```

---

### DrawBox2 — header + description

Renders a two-section box: a styled title, an inner separator, and one or more body lines.
Both sections support independent ANSI decorations and gradients.

```csharp
cc.DrawBox2(
    header: "ColoredConsole.NET · User Guide · V2.1",
    body: new[]
    {
        "Welcome! This guide walks through every feature of the library.",
        "Each section demonstrates a specific API with working examples."
    },
    headerDecoration:   AnsiDecoration.Bold,
    headerGradientFrom: (80, 80, 255),          // blue
    headerGradientTo:   (255, 80, 80),           // red
    bodyDecoration:     AnsiDecoration.Italic,
    separatorStyle:     LineStyle.Double         // ╠═══╣ (default matches lineStyle)
);
```

```
╔═════════════════════════════════════════════════════════════╗
║   C O L O R E D C O N S O L E . N E T  ·  U S E R  G U I D E  ║  ← gradient + bold
╠═════════════════════════════════════════════════════════════╣
║ Welcome! This guide walks through every feature.            ║  ← italic
║ Each section demonstrates a specific API with examples.     ║
╚═════════════════════════════════════════════════════════════╝
```

**`DrawBox2` parameters:**

| Parameter | Default | Description |
|---|---|---|
| `header` | — | Title text |
| `body` | — | Body lines (string array) |
| `lineStyle` | theme | Outer border style |
| `separatorStyle` | `lineStyle` | Inner separator fill (`Double`=`═`, `Single`=`─`, `Dotted`=`·`, `Dashed`=`-`) |
| `headerPosition` | `Center` | Header alignment |
| `headerStyle` | `SpacedCaps` | Header text style |
| `headerDecoration` | `None` | ANSI flags: `Bold`, `Italic`, `Underline`, … |
| `headerGradientFrom` | `null` | RGB start of header gradient |
| `headerGradientTo` | `null` | RGB end of header gradient |
| `bodyPosition` | `Left` | Body alignment |
| `bodyStyle` | `None` | Body text style |
| `bodyDecoration` | `None` | ANSI flags for body |
| `bodyTabStop` | `0` | Body indent (4-space stops) |
| `headerForeColor` | theme accent | Header color (overridden by gradient) |
| `bodyForeColor` | theme foreground | Body color |
| `lineColor` | theme | Border color |

---

### Box building blocks

Compose borders freely around any content:

```csharp
cc.DrawTopLine();
cc.WriteLine("First block");
cc.DrawSeparator();                               // full-width separator
cc.WriteLine("Second block");
cc.DrawSeparator(LineStyle.Double, LineStyle.Single);  // mixed styles
cc.WriteLine("Third block");
cc.DrawBottomLine();
```

```
╔════════════════════════════════════════╗
║ First block                            ║
╠════════════════════════════════════════╣
║ Second block                           ║
╟────────────────────────────────────────╢
║ Third block                            ║
╚════════════════════════════════════════╝
```

---

### Line styles

Every border and separator supports four styles. Mix vertical and horizontal styles independently:

| Style | Characters |
|---|---|
| `Double` | `╔ ═ ╗ ║ ╠ ╣ ╚ ╝` |
| `Single` | `┌ ─ ┐ │ ├ ┤ └ ┘` |
| `Dotted` | `║ · ║` (fill only) |
| `Dashed` | `║ - ║` (fill only) |

```csharp
cc.DrawSeparator(LineStyle.Double, LineStyle.Double);  // ╠═══╣
cc.DrawSeparator(LineStyle.Double, LineStyle.Single);  // ╟───╢
cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);  // ║···║
cc.DrawSeparator(LineStyle.Double, LineStyle.Dashed);  // ║---║
```

---

### WriteLine — alignment, tab stops, word-wrap

```csharp
cc.WriteLine("Left-aligned (default)");
cc.WriteLine("Centered",      new WriteOptions { TextPosition = TextPosition.Center });
cc.WriteLine("Right-aligned", new WriteOptions { TextPosition = TextPosition.Right  });
cc.WriteLine("Indented",      new WriteOptions { TabStop = 2 });

// Long lines wrap at word boundaries — never mid-word
cc.WriteLine("A very long line that will be split at word boundaries and " +
             "automatically wrapped to fit inside the box borders.");
```

---

### TextStyle — text transformations

Applied before rendering. Works in headers, body, and any `WriteLine` call.

| `TextStyle` | Input → Output |
|---|---|
| `None` | `Hello World` → `Hello World` |
| `Caps` | `Hello World` → `HELLO WORLD` |
| `Spaced` | `Hello World` → `H e l l o  W o r l d` |
| `SpacedCaps` | `Hello World` → `H E L L O   W O R L D` |

---

### WriteOptions — fluent builder

All write methods accept `WriteOptions`. Use the object initializer or chain fluent methods:

```csharp
// Object initializer
new WriteOptions { ForeColor = ConsoleColor.Cyan, TabStop = 1, AutoNumber = true }

// Fluent chain
new WriteOptions().Cyan().Tabbed(1).Numbered()
new WriteOptions().Green().Centered()
new WriteOptions().Red().Tabbed(2).RightAligned()
new WriteOptions().Yellow().Styled(TextStyle.SpacedCaps)
new WriteOptions().Dimmed().WithBorder(ConsoleColor.DarkGray)
```

**Available fluent methods:** `WithColor`, `WithBack`, `WithBorder`, `Tabbed`, `Aligned`,
`Styled`, `Numbered`, `Green`, `Red`, `Yellow`, `Cyan`, `White`, `Dimmed`, `Centered`, `RightAligned`.

---

### Semantic writes

One-liner helpers for common status messages, using the theme's semantic colors:

```csharp
cc.WriteSuccess("Build completed in 1.4 s");            // ✓  green
cc.WriteError("Connection refused on port 5432");        // ✗  red
cc.WriteWarning("Config file missing — using defaults"); // ⚠  dark-yellow
cc.WriteInfo("Watching /src for changes...");            // ℹ  cyan

// Tab stops for nested contexts
cc.WriteSuccess("Step 1 passed", tabStop: 1);
cc.WriteError("Step 3 failed",   tabStop: 1);
cc.WriteWarning("Continuing",    tabStop: 2);
```

---

### Auto-numbering

```csharp
var opt = new WriteOptions { AutoNumber = true };

cc.DrawTopLine();
cc.WriteLine("Download the SDK",        opt);   // 1. Download …
cc.WriteLine("Verify checksum",         opt);   // 2. Verify …
cc.WriteLine("Install to /usr/local",   opt);   // 3. Install …
cc.ResetAutoNumber();                           // counter → 0
cc.DrawSeparator();
cc.WriteLine("Edit appsettings.json",   opt);   // 1. Edit …
cc.DrawBottomLine();
```

---

### DrawKeyValue and DrawList

```csharp
// Key: value pairs — key in accent color, value in foreground
cc.DrawTopLine();
cc.DrawKeyValue("Host",     "localhost");
cc.DrawKeyValue("Port",     "5432",      tabStop: 1);
cc.DrawKeyValue("Status",   "Online",    valueColor: ConsoleColor.Green);
cc.DrawBottomLine();

// Bulleted list with optional section header
cc.DrawList("Dependencies", new[]
{
    "Newtonsoft.Json 13.0",
    "Dapper 2.1",
    "xunit 2.7"
});

// Custom bullet and color
cc.DrawList(null, items, bullet: '➤', foreColor: ConsoleColor.Cyan);
```

---

### WriteColumns

Renders values side-by-side in equal-width columns, separated by `│`:

```csharp
// Tuple shorthand — formats as "Label: Value"
cc.WriteColumns(("CPU", "72 %"), ("RAM", "4.2 GB"), ("Disk", "88 GB free"));

// String array with per-column alignment and color
cc.WriteColumns(
    values:     new[] { "Name",  "Status", "Score" },
    alignments: new[] { TextPosition.Left, TextPosition.Center, TextPosition.Right },
    foreColors: new[] { ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Cyan });
```

---

### DrawTable

Renders fully-bordered, auto-sized data tables. Column widths are computed from content
and proportionally scaled if total width exceeds the console.

```csharp
cc.DrawTable(
    headers: new[] { "Name", "Role", "Department", "Status" },
    rows: new[]
    {
        new[] { "Alice Johnson",  "Developer", "Engineering", "Active"   },
        new[] { "Bob Smith",      "Designer",  "Product",     "Active"   },
        new[] { "Carol Williams", "Manager",   "Operations",  "On Leave" }
    },
    options: new TableOptions
    {
        Style             = TableStyle.DoubleBorderSingleInner,  // default
        ColumnAlignments  = new[] { TextPosition.Left, TextPosition.Left,
                                    TextPosition.Left, TextPosition.Center },
        ShowRowSeparators = false
    });
```

**Table styles:**

| `TableStyle` | Outer border | Inner dividers |
|---|---|---|
| `DoubleBorderSingleInner` (default) | `╔ ═ ╗` | `│` |
| `AllDouble` | `╔ ═ ╦ ═ ╗` | `║` |
| `AllSingle` | `┌ ─ ┬ ─ ┐` | `│` |

**Table alignment** (how the table sits inside the outer box):

```csharp
new TableOptions { Alignment = TableAlignment.Left      }  // default
new TableOptions { Alignment = TableAlignment.Center    }
new TableOptions { Alignment = TableAlignment.Right     }
new TableOptions { Alignment = TableAlignment.Justified }  // columns expand to full width
```

**TableOptions reference:**

| Option | Type | Default | Description |
|---|---|---|---|
| `Style` | `TableStyle` | `DoubleBorderSingleInner` | Border style |
| `Alignment` | `TableAlignment` | `Left` | Table position inside the box |
| `ColumnAlignments` | `TextPosition[]` | `Left` for all | Per-column text alignment |
| `ColumnWidths` | `int[]` | auto | Override auto-sizing |
| `ShowHeader` | `bool` | `true` | Render the header row |
| `ShowRowSeparators` | `bool` | `false` | Draw a divider between every data row |
| `HeaderForeColor` | `ConsoleColor?` | theme accent | Header text color |
| `DataForeColor` | `ConsoleColor?` | theme foreground | Data text color |

---

### DrawSectionHeader

Embeds text in a separator-style line with a blank bordered line above and below:

```csharp
cc.DrawSectionHeader("Configuration");                           // left, Caps, tabStop 1
cc.DrawSectionHeader("Results", textPosition: TextPosition.Center);
cc.DrawSectionHeader("Notes",   lineStyle: LineStyle.Dotted, foreColor: ConsoleColor.Cyan);
```

```
║                                                  ║
╠══ CONFIGURATION ══════════════════════════════════╣
║                                                  ║
```

---

### Themes

```csharp
var cc = new ColoredConsole(Theme.Default);   // dark background, white/yellow
var cc = new ColoredConsole(Theme.Light);     // white background, dark text
var cc = new ColoredConsole(Theme.Hacker);    // black, green borders, cyan accents

// Custom theme
var cc = new ColoredConsole(new Theme
{
    BackColor    = ConsoleColor.Black,
    ForeColor    = ConsoleColor.White,
    LineColor    = ConsoleColor.Magenta,
    AccentColor  = ConsoleColor.DarkMagenta,
    SuccessColor = ConsoleColor.Green,
    ErrorColor   = ConsoleColor.Red,
    WarningColor = ConsoleColor.DarkYellow,
    InfoColor    = ConsoleColor.Cyan,
    LineStyle    = LineStyle.Double
});
```

Multiple instances can coexist with different themes — useful for nested panels:

```csharp
using var outer = new ColoredConsole(Theme.Default);
using var inner = new ColoredConsole(Theme.Hacker, width: outer.Width);
```

---

### AnsiDecoration — ANSI text effects

Use `AnsiDecoration` flags in `DrawBox2` (and anywhere you call `BuildTextRenderer` internally).
Requires a terminal with ANSI support (`AnsiConsole.IsSupported`).

```csharp
// Combine flags with |
headerDecoration: AnsiDecoration.Bold | AnsiDecoration.Underline
bodyDecoration:   AnsiDecoration.Italic
```

| Flag | Effect |
|---|---|
| `Bold` | **bold text** |
| `Italic` | *italic text* |
| `Underline` | underlined text |
| `Dim` | faint/dimmed text |
| `Blink` | blinking text |
| `Strikethrough` | ~~strikethrough~~ |

---

### AnsiConsole — 256-color and true-color

A static helper class that emits ANSI escape sequences directly. Guard calls with `IsSupported`:

```csharp
if (AnsiConsole.IsSupported)
{
    // 256-color palette (indices 0–255)
    AnsiConsole.SetForeground(214);       // orange
    Console.Write("Status: ");
    AnsiConsole.Reset();

    // True-color RGB
    AnsiConsole.Write("Ready", r: 0, g: 200, b: 100);

    // Horizontal gradient (per-character true-color)
    AnsiConsole.WriteGradient("ColoredConsole.NET",
        from: (255, 80, 0),
        to:   (0, 120, 255));

    // Decorations
    AnsiConsole.Bold();
    AnsiConsole.Italic();
    AnsiConsole.SetForeground(220);       // gold
    Console.Write("Important");
    AnsiConsole.Reset();
}
```

`AnsiConsole.IsSupported` auto-detects support: checks `WT_SESSION` / `TERM_PROGRAM`
environment variables; on Windows, attempts to enable Virtual Terminal Processing
via `kernel32.dll SetConsoleMode`.

---

### ASCII fallback mode

Replaces all Unicode box-drawing characters (`╔ ═ ║ …`) with plain ASCII (`+ - |`).
Useful for CI/CD logs, Windows `cmd.exe`, and legacy terminals.

```csharp
// At construction
var cc = new ColoredConsole(asciiMode: true);

// Toggle at runtime
cc.SetAsciiMode(true);
cc.SetAsciiMode(false);
bool current = cc.AsciiMode;
```

---

### Resize

```csharp
cc.Resize();          // re-reads Console.WindowWidth (clamped 40–120)
cc.Resize(100);       // explicit width
int w = cc.Width;     // current render width
```

---

### Async writes

```csharp
await cc.WriteLineAsync("Connecting...");
await cc.WriteLineAsync("Status: OK", new WriteOptions().Green().Centered());
await cc.WriteAsync("Progress: ");         // animation-safe (cursor repositions)
```

---

### Spinner — indeterminate progress

Animated Braille-dot spinner (`⠋⠙⠹⠸⠼⠴⠦⠧⠇⠏`) at 80 ms intervals:

```csharp
using (var spinner = new Spinner(cc, "Connecting to database..."))
{
    await ConnectAsync();
    spinner.UpdateMessage("Running migrations...");
    await MigrateAsync();
    spinner.Complete("Database ready");            // ✓ Database ready
}

// On failure
spinner.Complete("Connection timed out", success: false);  // ✗ Connection timed out

// Dispose() auto-calls Complete(success: true) if not already called
```

---

### ProgressBar — determinate progress

```csharp
cc.DrawTopLine();
cc.WriteLine("Uploading 150 files...", new WriteOptions().Dimmed());
cc.DrawSeparator(LineStyle.Double, LineStyle.Single);

using (var pb = new ProgressBar(cc))
{
    for (int i = 0; i <= 100; i++)
    {
        pb.Report(i / 100.0);   // 0.0 → 1.0
        Thread.Sleep(50);
    }
}

cc.DrawBottomLine();

// Custom color and indent
using (var pb = new ProgressBar(cc, new WriteOptions { ForeColor = ConsoleColor.Cyan, TabStop = 1 }))
{
    pb.Report(0.5);
}
```

Thread-safe: `Report(double)` is safe to call from any thread.

---

### File logging

```csharp
using var cc = new ColoredConsole();

// Opens ./Logs/yyyy-MM-dd.log (append mode, one file per day)
cc.EnableLogging("Logs");

// Every WriteLine, WriteSuccess, etc. is automatically appended
cc.WriteLine("Application started");    // → logged as "[14:23:05.412] Application started"

// Raw log write (no console output)
cc.WriteLog("=== Session started ===");

// Custom formatter
cc.EnableLogging("Logs", msg => $"{DateTime.UtcNow:O} | {msg}");

// Dispose() flushes and closes the log file
```

---

### Prompt — user input

```csharp
cc.DrawTopLine();
var name = cc.Prompt("What is your name?");   // renders "? " inside the border
if (!string.IsNullOrWhiteSpace(name))
    cc.WriteSuccess($"Hello, {name}!");
cc.DrawBottomLine();
```

Returns `null` when `Console.IsInputRedirected`.

---

## Constructor

```csharp
new ColoredConsole(
    theme:     Theme.Default,   // optional — theme preset or custom Theme
    width:     0,               // 0 = auto-detect Console.WindowWidth (clamped 40–120)
    asciiMode: false            // true = replace Unicode box chars with + - |
)
```

`ColoredConsole` is `IDisposable`. Always wrap in `using` to flush the log `StreamWriter`.

---

## Change Log

| Version | Date | Notes |
|---|---|---|
| **2.1.0** | 2026-03 | `DrawTable` (3 styles, 4 alignments), `WriteColumns`, `Spinner`, `WriteSuccess/Error/Warning/Info`, `WriteLineAsync/WriteAsync`, fluent `WriteOptions`, configurable log formatter, `Resize()`, ASCII mode, `DrawKeyValue`, `DrawList`, `AnsiConsole` (256-color, true-color, gradients, decorations), `DrawBox2` (header + body with ANSI), `AnsiDecoration` flags, unit test suite (169 tests), CI/CD |
| **2.0.0** | 2026-03 | **Breaking.** Namespace `bcd`; `Theme` and `WriteOptions` types; `DrawSectionHeader` replaces `DrawSeparator(string)`; `IDisposable` logging; word-aware wrapping; auto-detect width; thread-safe counter; multiple bug fixes |
| 1.0.5 | 2024-09 | `ProgressBar` updated; `AutoNumber` added |
| 1.0.4 | 2023-03 | File logging added |
| 1.0.3 | 2023-03 | `ProgressBar` added |
| 1.0.2 | 2023-02 | `Prompt` feature added |
| 1.0.1 | 2023-02 | `DrawSeparator` updated |
| 1.0.0 | 2020-09 | Initial release |

---

## License

MIT © 2024-2026
