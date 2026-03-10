# ColoredConsole.NET — Enhancement Summary

> Version 2.1.0 | Implemented March 2026

---

## Overview

All 14 planned enhancements were implemented across three priority tiers.
The library grew from a single-file utility into a structured, production-ready
NuGet package with themed rendering, rich table support, ANSI color, unit tests,
and CI/CD automation.

---

## Part 1 — High Impact

### 1. Table Rendering (`DrawTable`)

**File:** `ColoredConsole.cs`, `TableOptions.cs`

Renders fully-bordered, aligned data tables. Column widths are auto-computed
from cell content and proportionally scaled if the total exceeds the console width.

Three built-in styles controlled by `TableOptions.Style`:

| Style | Outer border | Inner dividers |
|---|---|---|
| `DoubleBorderSingleInner` (default) | `╔═╤═╗` / `╠═╪═╣` | `│` |
| `AllDouble` | `╔═╦═╗` / `╠═╬═╣` | `║` |
| `AllSingle` | `┌─┬─┐` / `├─┼─┤` | `│` |

Key options: `ColumnAlignments`, `ColumnWidths`, `ShowHeader`, `ShowRowSeparators`,
`HeaderForeColor`, `DataForeColor`.

**Usage:**
```csharp
cc.DrawTable(
    headers: new[] { "Name", "Role", "Status" },
    rows: new[] {
        new[] { "Alice", "Developer", "Active" },
        new[] { "Bob",   "Designer",  "Active" }
    },
    options: new TableOptions {
        ColumnAlignments  = new[] { TextPosition.Left, TextPosition.Left, TextPosition.Center },
        ShowRowSeparators = true
    });
```

---

### 2. Multi-Column Layout (`WriteColumns`)

**File:** `ColoredConsole.cs`

Writes N values side-by-side in equal-width columns inside the box border.
Available width is divided equally; columns are separated by `│`.

Two overloads:

```csharp
// String array with optional per-column alignment and colors
cc.WriteColumns(
    values:     new[] { "Name", "Status", "Score" },
    alignments: new[] { TextPosition.Left, TextPosition.Center, TextPosition.Right },
    foreColors: new[] { ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Cyan });

// Tuple shorthand — formats as "Label: Value"
cc.WriteColumns(("CPU", "72%"), ("RAM", "4.2 GB"), ("Disk", "88 GB free"));
```

---

### 3. Spinner — Indeterminate Progress (`Spinner`)

**File:** `Spinner.cs`

Animated Braille-dot spinner (`⠋⠙⠹⠸⠼⠴⠦⠧⠇⠏`) for tasks without a known
duration. Renders at 80ms intervals. Thread-safe; shares the same lock pattern
as `ProgressBar`.

```csharp
using (var spinner = new Spinner(cc, "Connecting to database..."))
{
    await DoWorkAsync();
    spinner.UpdateMessage("Running migrations...");
    await RunMigrationsAsync();
    spinner.Complete("Database ready");   // renders ✓ Done
}
// Dispose() calls Complete(success: true) automatically
```

`Complete(message, success: false)` renders `✗ Failed` in red.

---

### 4. Semantic Write Methods

**File:** `ColoredConsole.cs`

Four convenience methods that apply the theme's semantic colors and a Unicode prefix symbol:

| Method | Symbol | Default color |
|---|---|---|
| `WriteSuccess(msg)` | ✓ | `Theme.SuccessColor` (Green) |
| `WriteError(msg)` | ✗ | `Theme.ErrorColor` (Red) |
| `WriteWarning(msg)` | ⚠ | `Theme.WarningColor` (DarkYellow) |
| `WriteInfo(msg)` | ℹ | `Theme.InfoColor` (Cyan) |

All four accept an optional `tabStop` parameter and respect custom theme colors.
Semantic colors can be overridden per-theme:

```csharp
var theme = new Theme { SuccessColor = ConsoleColor.DarkGreen };
```

---

### 5. Async Writes

**File:** `ColoredConsole.cs`

`Task`-returning wrappers that offload to the thread pool, safe to `await` in
async contexts:

```csharp
await cc.WriteLineAsync("Connecting...");
await cc.WriteAsync("Progress: ", new WriteOptions().Cyan());
```

Console output remains inherently synchronous on Windows; these wrappers ensure
callers can participate in async call chains without blocking.

---

## Part 2 — Medium Impact

### 6. Fluent `WriteOptions` Builder

**File:** `WriteOptions.cs`

Every `WriteOptions` property is now settable via a fluent chain, eliminating
verbose object initializers for common patterns:

```csharp
// Object initializer (still works)
new WriteOptions { ForeColor = ConsoleColor.Cyan, TabStop = 1 }

// Fluent chain (new)
new WriteOptions().Cyan().Tabbed(1).Numbered()
new WriteOptions().Green().Centered()
new WriteOptions().Red().Tabbed(2)
new WriteOptions().Dimmed().RightAligned()
```

Available shorthand methods: `WithColor`, `WithBack`, `WithBorder`, `Tabbed`,
`Aligned`, `Styled`, `Numbered`, `Green`, `Red`, `Yellow`, `Cyan`, `White`,
`Dimmed`, `Centered`, `RightAligned`.

---

### 7. Configurable Log Formatter

**File:** `ColoredConsole.cs`

`EnableLogging` now accepts an optional `Func<string, string>` formatter that
replaces the default `[HH:mm:ss.fff] message` format:

```csharp
// UTC ISO-8601 timestamps
cc.EnableLogging("Logs", msg => $"{DateTime.UtcNow:O} | {msg}");

// Structured JSON-style
cc.EnableLogging("Logs", msg => $"{{\"ts\":\"{DateTime.UtcNow:O}\",\"msg\":\"{msg}\"}}");

// Default (no formatter argument)
cc.EnableLogging("Logs");   // → [14:23:05.412] message
```

`EnableLogging` also now handles absolute paths (not just subfolder names),
which is required for isolated test logging.

---

### 8. Console Resize Handling (`Resize`)

**File:** `ColoredConsole.cs`

`_width` and `_availableWidth` are now mutable so the instance can adapt when
the terminal is resized:

```csharp
// Re-read Console.WindowWidth (clamped 40–120)
cc.Resize();

// Or supply an explicit width
cc.Resize(100);
```

Fields changed from `readonly` to regular fields to support this.

---

### 9. ASCII Fallback Mode

**File:** `ColoredConsole.cs`

When enabled, replaces all Unicode box-drawing characters with plain ASCII
(`+`, `-`, `|`). Essential for CI/CD logs, Windows cmd.exe, or any terminal
that renders box chars as `?`.

```csharp
// At construction
var cc = new ColoredConsole(asciiMode: true);

// At runtime (toggle)
cc.SetAsciiMode(true);
cc.SetAsciiMode(false);
```

`AsciiMode` property exposes the current state. Affected methods: `DrawTopLine`,
`DrawBottomLine`, `DrawSeparator`, `DrawTable`, and `BorderChar()`.

---

### 10. `DrawKeyValue` and `DrawList`

**File:** `ColoredConsole.cs`

**`DrawKeyValue`** renders a single key-value pair in the box border with the
key in accent color and the value in foreground color:

```csharp
cc.DrawKeyValue("Host",     "localhost");
cc.DrawKeyValue("Port",     "5432",    tabStop: 1);
cc.DrawKeyValue("Database", "prod_db", keyColor: ConsoleColor.DarkYellow);
```

**`DrawList`** renders a bullet list with an optional section header:

```csharp
cc.DrawList("Dependencies", new[] {
    "Newtonsoft.Json 13.0.3",
    "Dapper 2.1.28",
    "Serilog 3.1.1"
});

// ASCII-safe bullet (auto when asciiMode = true, or override manually)
cc.DrawList("Steps", steps, bullet: '-');
```

---

## Part 3 — Lower Priority / Polish

### 11. NuGet Package Tags & Repository Metadata

**File:** `ColoredConsole.csproj`

Added `<PackageTags>` and `<RepositoryUrl>` for NuGet discoverability:

```xml
<PackageTags>console;cli;colored;terminal;progress-bar;spinner;table;logging;box-drawing;ansi;256-color</PackageTags>
<RepositoryUrl>https://github.com/muralibcd/ColoredConsole</RepositoryUrl>
<RepositoryType>git</RepositoryType>
```

---

### 12. Unit Tests (`Tests` project)

**Files:** `Tests/Tests.csproj`, `Tests/ColoredConsoleTests.cs`

A dedicated xUnit test project with **32 tests** covering:

| Category | Tests |
|---|---|
| `WriteLine` + word-wrap | Blank line, short message, long wrap, no mid-word split |
| Auto-numbering | Counter increment, reset |
| Semantic writes | `WriteSuccess/Error/Warning/Info` prefix symbols |
| `DrawBox` | Line count, corner characters |
| `DrawTopLine` | Width correctness, single/double corners |
| `DrawSeparator` | Mixed joiner correctness (including the `SGL_RJ` bug fix) |
| `DrawTable` | Line count with/without header/row separators, cell content |
| `WriteColumns` | Single output line, tuple formatting |
| `DrawKeyValue` / `DrawList` | Content presence |
| ASCII mode | `+`/`-` characters, runtime toggle |
| Fluent `WriteOptions` | Chain correctness |
| `Resize` | Width update |
| Logging | Custom formatter, default timestamp format |

Run with: `dotnet test Tests/Tests.csproj`

---

### 13. ANSI 256-Color and True-Color Support (`AnsiConsole`)

**File:** `AnsiConsole.cs`

A static helper class that emits ANSI escape sequences for terminals that
support them. Bypasses the 16-color limit of `ConsoleColor`.

```csharp
if (AnsiConsole.IsSupported)
{
    // 256-color palette (0–255)
    AnsiConsole.SetForeground(214);       // orange
    Console.Write("Status: ");
    AnsiConsole.Reset();

    // True-color RGB
    AnsiConsole.Write("Ready", r: 0, g: 200, b: 100);

    // Decorations
    AnsiConsole.Bold();
    AnsiConsole.Underline();
    Console.Write("Important");
    AnsiConsole.Reset();

    // Horizontal gradient (true-color)
    AnsiConsole.WriteGradient("ColoredConsole.NET",
        from: (255, 80, 0),
        to:   (0, 120, 255));
}
```

`AnsiConsole.IsSupported` auto-detects: checks for `WT_SESSION` / `TERM_PROGRAM`
env vars, and on Windows attempts to enable Virtual Terminal Processing via
`kernel32.dll` `SetConsoleMode`.

---

### 14. GitHub Actions CI/CD (`.github/workflows/ci.yml`)

**File:** `.github/workflows/ci.yml`

Two jobs:

**`build-and-test`** — runs on every push and pull request:
1. `dotnet restore`
2. `dotnet build --configuration Release`
3. `dotnet test Tests/Tests.csproj`

**`publish-nuget`** — runs only on GitHub Release events (requires `NUGET_API_KEY` secret):
1. `dotnet pack --configuration Release`
2. `dotnet nuget push` to `api.nuget.org`

---

## File Inventory

### New files
| File | Purpose |
|---|---|
| `ColoredConsole/Enums.cs` | `LineStyle`, `TextPosition`, `TextStyle` enums |
| `ColoredConsole/Theme.cs` | `Theme` class with `Default`, `Light`, `Hacker` presets |
| `ColoredConsole/WriteOptions.cs` | `WriteOptions` class with fluent builder methods |
| `ColoredConsole/TableOptions.cs` | `TableStyle` enum + `TableOptions` config class |
| `ColoredConsole/Spinner.cs` | Indeterminate-progress animated spinner |
| `ColoredConsole/AnsiConsole.cs` | ANSI escape-code helpers (256-color, true-color, decorations) |
| `Tests/Tests.csproj` | xUnit test project |
| `Tests/ColoredConsoleTests.cs` | 32 unit tests |
| `.github/workflows/ci.yml` | GitHub Actions CI + NuGet publish pipeline |

### Modified files
| File | Changes |
|---|---|
| `ColoredConsole/ColoredConsole.cs` | All bug fixes, new regions, ASCII mode, resize, table/column/semantic/async methods |
| `ColoredConsole/ProgressBar.cs` | Constructor injection, `double` thread-safety via `Interlocked` + `BitConverter` |
| `ColoredConsole/ColoredConsole.csproj` | Version 2.1.0, namespace `Bcd`, `LangVersion latest`, NuGet tags |
| `ColoredConsole/README.md` | Full rewrite with all new API docs and examples |
| `Test/Program.cs` | Demonstrates all Part 1 features |
| `ColoredConsole.sln` | Added `Tests` project |

---

## Breaking Changes from v1.x

| Change | Migration |
|---|---|
| Namespace `bcd` → `Bcd` | `using bcd;` → `using Bcd;` |
| `DrawSeparator(string, ...)` removed | Use `DrawSectionHeader(string, ...)` |
| `LogEnable` / `LogFolder` properties removed | Use `cc.EnableLogging("folder")` |
| `AutoNumberCounter` is now read-only | Use `cc.ResetAutoNumber()` to reset |
| `DefaultWidth`, `DefaultLineStyle`, etc. removed | Configure via `Theme` + constructor |

---

*ColoredConsole.NET v2.1.0 — MIT Licensed*
