# ColoredConsole.NET

A richly-formatted console library for C# — colored text, bordered boxes, section headers,
separators, prompts, progress bars, and optional file logging.

_Current Version: 2.0.0_ &nbsp;|&nbsp; &copy; 2024-2026 MIT Licensed.

---

## Getting Started

```csharp
using bcd;

// ColoredConsole is IDisposable — use a using block to flush logs on exit
using (var cc = new ColoredConsole())
{
    cc.DrawBox("My Application");

    cc.DrawTopLine();
    cc.WriteLine("Some header text");
    cc.DrawSeparator();
    cc.WriteLine("Body text");
    cc.DrawBottomLine();
}
```

---

## Theming

Pass a built-in preset or a custom `Theme` to the constructor:

```csharp
var cc = new ColoredConsole(Theme.Hacker);   // black + green + cyan
var cc = new ColoredConsole(Theme.Light);    // white background, dark text
var cc = new ColoredConsole(Theme.Default);  // black + white + yellow borders

// Custom theme
var myTheme = new Theme
{
    BackColor   = ConsoleColor.DarkBlue,
    ForeColor   = ConsoleColor.White,
    LineColor   = ConsoleColor.Cyan,
    AccentColor = ConsoleColor.Yellow
};
var cc = new ColoredConsole(myTheme);
```

---

## WriteOptions

Every write method accepts an optional `WriteOptions` object. Any property left `null`
falls back to the active theme default.

```csharp
cc.WriteLine("Important!", new WriteOptions
{
    ForeColor  = ConsoleColor.Red,
    TextStyle  = TextStyle.Caps,
    TabStop    = 1
});
```

| Property | Type | Description |
|---|---|---|
| `LineStyle` | `LineStyle?` | Border style: `Dotted`, `Dashed`, `Single`, `Double` |
| `TextPosition` | `TextPosition?` | `Left`, `Center`, `Right` |
| `TabStop` | `int` | 4-space indents from inner left border |
| `AutoNumber` | `bool` | Prefix with auto-incrementing counter |
| `TextStyle` | `TextStyle?` | `None`, `Spaced`, `Caps`, `SpacedCaps` |
| `BackColor` | `ConsoleColor?` | Background color |
| `ForeColor` | `ConsoleColor?` | Text color |
| `LineColor` | `ConsoleColor?` | Border color |

---

## Section Headers

```csharp
cc.DrawSectionHeader("Configuration");
cc.WriteLine("key = value", new WriteOptions { TabStop = 1 });
```

---

## Auto-Numbering

```csharp
cc.DrawSectionHeader("Steps");
cc.WriteLine("Install dependencies",  new WriteOptions { AutoNumber = true });
cc.WriteLine("Configure environment", new WriteOptions { AutoNumber = true });
cc.WriteLine("Run the application",   new WriteOptions { AutoNumber = true });
cc.ResetAutoNumber(); // reset counter back to 0
```

---

## Progress Bar

```csharp
cc.WriteLine("Processing...");
using (var pb = new ProgressBar(cc))          // pass cc for consistent width/theme
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
using (var cc = new ColoredConsole())
{
    cc.EnableLogging("Logs");         // creates Logs/yy-MM-dd.log, appends each session
    cc.WriteLine("Application started");
    cc.WriteLog("Custom log entry");  // raw log write (no console output)
}                                     // Dispose() flushes and closes the log file
```

---

## Change Log

| Date | Version | Description |
|---|---|---|
| 2020-09-10 | 1.0.0 | Initial version |
| 2023-02-20 | 1.0.1 | `DrawSeparator` updated |
| 2023-02-21 | 1.0.2 | `Prompt` feature added |
| 2023-03-15 | 1.0.3 | `ProgressBar` added |
| 2023-03-26 | 1.0.4 | File logging added |
| 2024-09-24 | 1.0.5 | `ProgressBar` updated; `AutoNumber` added |
| 2026-03-09 | **2.0.0** | **Breaking:** namespace renamed `bcd`; `Theme` and `WriteOptions` types; `DrawSectionHeader` replaces `DrawSeparator(string)`; `IDisposable` logging with `StreamWriter` (no more per-line file open); word-aware text wrapping; auto-detect console width; thread-safe counter; multiple bug fixes |
