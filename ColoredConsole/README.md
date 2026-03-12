# ColoredConsole.NET

A richly-formatted, theme-aware console library for C# — bordered boxes, tables, progress bars,
spinners, ANSI true-color gradients, and file logging.

**Version 2.1.0** &nbsp;|&nbsp; .NET Standard 2.0 &nbsp;|&nbsp; MIT License

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
using var cc = new ColoredConsole();

cc.DrawBox2(
    header: "My App  ·  v1.0",
    body: new[] { "Ready to process files.", "Press Enter to continue." },
    headerDecoration:   AnsiDecoration.Bold,
    headerGradientFrom: (80, 80, 255),
    headerGradientTo:   (255, 80, 80),
    bodyDecoration:     AnsiDecoration.Italic
);

cc.DrawTopLine();
cc.WriteSuccess("Build completed in 1.4 s");
cc.WriteWarning("Config file missing — using defaults");
cc.WriteError("Connection refused on port 5432");
cc.WriteInfo("Watching /src for changes...");
cc.DrawBottomLine();
```

---

## Boxes

```csharp
// Single-line box (SpacedCaps, centered by default)
cc.DrawBox("Section Title");
cc.DrawBox("Custom", lineStyle: LineStyle.Single, textPosition: TextPosition.Left,
           textStyle: TextStyle.None, foreColor: ConsoleColor.Cyan);

// Two-section box: header + separator + body
cc.DrawBox2(
    header: "Report  ·  2026-03-12",
    body: new[] { "Summary line one.", "Summary line two." },
    separatorStyle: LineStyle.Single    // ╟───╢ instead of ╠═══╣
);

// Manual composition
cc.DrawTopLine();
cc.WriteLine("First block");
cc.DrawSeparator();
cc.WriteLine("Second block");
cc.DrawBottomLine();
```

---

## Line Styles

Four styles for any border or separator. Mix vertical and horizontal independently:

```csharp
cc.DrawSeparator(LineStyle.Double, LineStyle.Double);  // ╠═══╣  (default)
cc.DrawSeparator(LineStyle.Double, LineStyle.Single);  // ╟───╢
cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);  // ║···║
cc.DrawSeparator(LineStyle.Double, LineStyle.Dashed);  // ║---║
```

---

## WriteOptions — fluent builder

```csharp
// Fluent chain
cc.WriteLine("Important",  new WriteOptions().Red().Tabbed(1));
cc.WriteLine("Info",       new WriteOptions().Cyan().Centered());
cc.WriteLine("Step 1",     new WriteOptions().Green().Numbered());
cc.WriteLine("Sub-item",   new WriteOptions().Dimmed().RightAligned());
cc.WriteLine("H E A D E R", new WriteOptions().Yellow().Styled(TextStyle.SpacedCaps));
```

---

## Semantic Writes

```csharp
cc.WriteSuccess("Tests passed (169 / 169)");     // ✓  green
cc.WriteError("Connection refused");              // ✗  red
cc.WriteWarning("Disk usage above 90 %");         // ⚠  yellow
cc.WriteInfo("Server listening on :8080");        // ℹ  cyan

cc.WriteSuccess("Nested context", tabStop: 1);
```

---

## Tables

```csharp
cc.DrawTable(
    headers: new[] { "Name", "Role", "Status" },
    rows: new[]
    {
        new[] { "Alice", "Developer", "Active" },
        new[] { "Bob",   "Designer",  "Active" }
    },
    options: new TableOptions
    {
        Style             = TableStyle.DoubleBorderSingleInner,  // default
        Alignment         = TableAlignment.Justified,
        ColumnAlignments  = new[] { TextPosition.Left, TextPosition.Left, TextPosition.Center },
        ShowRowSeparators = true
    });
```

| `TableStyle` | Outer | Inner |
|---|---|---|
| `DoubleBorderSingleInner` | `╔═╗` | `│` |
| `AllDouble` | `╔═╦═╗` | `║` |
| `AllSingle` | `┌─┬─┐` | `│` |

`TableAlignment`: `Left` · `Center` · `Right` · `Justified`

---

## Columns, Lists, Key-Value

```csharp
// Side-by-side columns
cc.WriteColumns(("CPU", "72 %"), ("RAM", "4.2 GB"), ("Disk", "88 GB free"));
cc.WriteColumns(
    values:     new[] { "Name", "Score" },
    alignments: new[] { TextPosition.Left, TextPosition.Right });

// Bullet list
cc.DrawList("Dependencies", new[] { "Newtonsoft.Json 13.0", "Dapper 2.1" });
cc.DrawList(null, items, bullet: '➤', foreColor: ConsoleColor.Cyan);

// Key: value
cc.DrawKeyValue("Host",   "localhost");
cc.DrawKeyValue("Status", "Online", valueColor: ConsoleColor.Green);
```

---

## DrawSectionHeader

Embeds text in a separator-style line with surrounding blank lines:

```csharp
cc.DrawSectionHeader("Configuration");
cc.DrawSectionHeader("Results", textPosition: TextPosition.Center);
cc.DrawSectionHeader("Notes",   lineStyle: LineStyle.Dotted);
```

---

## ANSI — 256-color, True-color & Decorations

### AnsiDecoration flags (for DrawBox2)

```csharp
cc.DrawBox2(
    header: "Dashboard",
    body: new[] { "All systems nominal." },
    headerDecoration:   AnsiDecoration.Bold | AnsiDecoration.Underline,
    headerGradientFrom: (0, 200, 100),
    headerGradientTo:   (0, 80, 255),
    bodyDecoration:     AnsiDecoration.Italic
);
```

Flags: `Bold` · `Italic` · `Underline` · `Dim` · `Blink` · `Strikethrough`

### AnsiConsole (standalone)

```csharp
if (AnsiConsole.IsSupported)
{
    AnsiConsole.SetForeground(214);                           // 256-color orange
    AnsiConsole.Write("Status", r: 0, g: 200, b: 100);       // true-color RGB

    AnsiConsole.WriteGradient("ColoredConsole.NET",
        from: (255, 80, 0), to: (0, 120, 255));              // horizontal gradient

    AnsiConsole.Bold(); AnsiConsole.SetForeground(220);
    Console.Write("Important");
    AnsiConsole.Reset();
}
```

---

## Themes

```csharp
new ColoredConsole(Theme.Default)    // dark background, white text, yellow borders
new ColoredConsole(Theme.Light)      // white background, dark text
new ColoredConsole(Theme.Hacker)     // black background, green borders, cyan accents

new ColoredConsole(new Theme
{
    ForeColor    = ConsoleColor.White,
    LineColor    = ConsoleColor.Magenta,
    AccentColor  = ConsoleColor.DarkMagenta,
    SuccessColor = ConsoleColor.Green,
    ErrorColor   = ConsoleColor.Red
})
```

---

## Spinner and ProgressBar

```csharp
// Indeterminate — Braille dot animation
using (var spinner = new Spinner(cc, "Connecting..."))
{
    await DoWorkAsync();
    spinner.UpdateMessage("Running migrations...");
    await MigrateAsync();
    spinner.Complete("Database ready");                   // ✓
    // spinner.Complete("Failed", success: false);        // ✗
}

// Determinate — call Report(0.0–1.0) from any thread
using (var pb = new ProgressBar(cc))
{
    for (int i = 0; i <= 100; i++)
    {
        pb.Report(i / 100.0);
        Thread.Sleep(50);
    }
}
```

---

## File Logging

```csharp
using var cc = new ColoredConsole();
cc.EnableLogging("Logs");                              // Logs/yyyy-MM-dd.log, append mode
cc.WriteLine("Started");                               // auto-logged
cc.WriteLog("Raw entry — no console output");

// Custom timestamp format
cc.EnableLogging("Logs", msg => $"{DateTime.UtcNow:O} | {msg}");
// Dispose() flushes and closes the file
```

---

## Other Features

```csharp
// Async writes
await cc.WriteLineAsync("Connecting...", new WriteOptions().Cyan());
await cc.WriteAsync("Tick ");

// Resize
cc.Resize();          // re-reads Console.WindowWidth
cc.Resize(80);        // explicit width (clamped 40–120)

// ASCII mode — replaces ╔═║… with + - |
cc.SetAsciiMode(true);
var cc2 = new ColoredConsole(asciiMode: true);

// Prompt
var input = cc.Prompt("Enter your name:");

// Auto-numbering
cc.WriteLine("First step",  new WriteOptions { AutoNumber = true });
cc.WriteLine("Second step", new WriteOptions { AutoNumber = true });
cc.ResetAutoNumber();
```

---

## Constructor

```csharp
new ColoredConsole(
    theme:     Theme.Default,  // theme preset or custom Theme
    width:     0,              // 0 = auto-detect (clamped 40–120)
    asciiMode: false           // true = ASCII box chars
)
```

`ColoredConsole` is `IDisposable` — always use `using` to flush the log file.

---

## Change Log

| Version | Notes |
|---|---|
| **2.1.0** | `DrawTable`, `WriteColumns`, `Spinner`, semantic writes, async, fluent `WriteOptions`, `DrawKeyValue`, `DrawList`, `AnsiConsole`, `DrawBox2` with `AnsiDecoration`, configurable log formatter, `Resize`, ASCII mode, 169 unit tests |
| **2.0.0** | Breaking: namespace `bcd`; `Theme`; `WriteOptions`; `DrawSectionHeader`; `IDisposable` logging; word-wrap; auto-width |
| 1.0.x | Initial releases (2020–2024) |

---

MIT © 2024-2026
