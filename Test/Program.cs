using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using bcd;

// ────────────────────────────────────────────────────────────────────────────
//  ColoredConsole.NET — Interactive User Guide
//
//  Run this project to see every feature of the library in action.
//  Each section is self-contained and labelled so it doubles as a
//  copy-paste reference for your own projects.
// ────────────────────────────────────────────────────────────────────────────

namespace Test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // ── Enable session logging ─────────────────────────────────────
            // All cc.WriteLine / WriteSuccess / etc. calls are also written to
            // a daily .log file under ./Log/.
            using var cc = new ColoredConsole();
            cc.EnableLogging("Log");

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  1. WELCOME
            // ═══════════════════════════════════════════════════════════════

            cc.DrawBox("ColoredConsole.NET  ·  User Guide  ·  v2.1");
            cc.WriteLine();
            cc.WriteLine("  Welcome! This guide walks through every feature of the library.");
            cc.WriteLine("  Each section demonstrates a specific API with working examples.");
            cc.WriteLine();
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  2. BOX STRUCTURE — TOP / BOTTOM / SEPARATOR
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("2. Box Structure");

            cc.WriteLine("  DrawTopLine() / DrawBottomLine() / DrawSeparator() are the");
            cc.WriteLine("  building blocks. Compose them freely around any content.");
            cc.WriteLine();

            cc.DrawTopLine();
            cc.WriteLine("  First content block");
            cc.DrawSeparator();
            cc.WriteLine("  Second content block (after a separator)");
            cc.DrawSeparator();
            cc.WriteLine("  Third content block");
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  3. LINE STYLES
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("3. Line Styles");

            cc.WriteLine("  Every border and separator supports Double, Single, Dotted and Dashed.");
            cc.WriteLine();

            cc.DrawTopLine(LineStyle.Double);
            cc.WriteLine("  Double  — default style, clear visual weight");
            cc.DrawSeparator(LineStyle.Double, LineStyle.Double);

            cc.DrawSeparator(LineStyle.Double, LineStyle.Single);
            cc.WriteLine("  Single separator fill  (Double vertical + Single horizontal)");

            cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
            cc.WriteLine("  Dotted separator fill  ·····················");

            cc.DrawSeparator(LineStyle.Double, LineStyle.Dashed);
            cc.WriteLine("  Dashed separator fill  ---------------------");

            cc.DrawBottomLine(LineStyle.Double);
            cc.WriteLine();

            cc.DrawTopLine(LineStyle.Single);
            cc.WriteLine("  Single-line box — lightweight style for inner panels");
            cc.DrawSeparator(LineStyle.Single, LineStyle.Single);
            cc.WriteLine("  Single/Single separator  — uses ├ and ┤ joiners");
            cc.DrawBottomLine(LineStyle.Single);

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  4. WRITELINE — TEXT ALIGNMENT & TAB STOPS
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("4. WriteLine — Alignment & Tab Stops");

            cc.DrawTopLine();
            cc.WriteLine("  Left-aligned (default)");
            cc.WriteLine("Centered text",                  new WriteOptions { TextPosition = TextPosition.Center });
            cc.WriteLine("Right-aligned text",             new WriteOptions { TextPosition = TextPosition.Right  });
            cc.DrawSeparator();
            cc.WriteLine("No indent (TabStop 0)",          new WriteOptions { TabStop = 0 });
            cc.WriteLine("One level in (TabStop 1)",       new WriteOptions { TabStop = 1 });
            cc.WriteLine("Two levels in (TabStop 2)",      new WriteOptions { TabStop = 2 });
            cc.WriteLine("Three levels in (TabStop 3)",    new WriteOptions { TabStop = 3 });
            cc.DrawSeparator();
            // Word-aware wrapping — long lines are split at word boundaries, never mid-word
            cc.WriteLine("Word-wrap demo — this line is deliberately long to show how the renderer " +
                         "splits at word boundaries and never cuts a word in half, even across " +
                         "multiple wrapped continuation lines.");
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  5. TEXT STYLES
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("5. Text Styles");

            cc.WriteLine("  TextStyle transforms text before it is rendered.");
            cc.WriteLine();
            cc.DrawTopLine();
            cc.WriteLine("none  →  Hello World",      new WriteOptions { TextStyle = TextStyle.None      });
            cc.WriteLine("caps  →  Hello World",      new WriteOptions { TextStyle = TextStyle.Caps      });
            cc.WriteLine("spaced  →  Hello World",    new WriteOptions { TextStyle = TextStyle.Spaced    });
            cc.WriteLine("spacedcaps  →  Hello World",new WriteOptions { TextStyle = TextStyle.SpacedCaps});
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  6. WRITE OPTIONS — FLUENT BUILDER & COLOR SHORTHANDS
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("6. WriteOptions — Fluent Builder");

            cc.WriteLine("  Chain methods to build options inline. Every property has a shorthand.");
            cc.WriteLine();
            cc.DrawTopLine();
            cc.WriteLine("Default theme foreground");
            cc.WriteLine("Green text",                        new WriteOptions().Green());
            cc.WriteLine("Red text",                          new WriteOptions().Red());
            cc.WriteLine("Yellow text",                       new WriteOptions().Yellow());
            cc.WriteLine("Cyan text",                         new WriteOptions().Cyan());
            cc.WriteLine("White text",                        new WriteOptions().White());
            cc.WriteLine("Dimmed (DarkGray) text",            new WriteOptions().Dimmed());
            cc.DrawSeparator();
            cc.WriteLine("Cyan + centered",                   new WriteOptions().Cyan().Centered());
            cc.WriteLine("Green + right-aligned",             new WriteOptions().Green().RightAligned());
            cc.WriteLine("Yellow + tabbed(1)",                new WriteOptions().Yellow().Tabbed(1));
            cc.WriteLine("Red + tabbed(2) + right-aligned",   new WriteOptions().Red().Tabbed(2).RightAligned());
            cc.DrawSeparator();
            cc.WriteLine("Caps style",                        new WriteOptions().White().Styled(TextStyle.Caps));
            cc.WriteLine("Spaced style",                      new WriteOptions().Cyan().Styled(TextStyle.Spaced));
            cc.WriteLine("SpacedCaps style",                  new WriteOptions().Yellow().Styled(TextStyle.SpacedCaps));
            cc.DrawSeparator();
            cc.WriteLine("Custom border color",               new WriteOptions().Green().WithBorder(ConsoleColor.Green));
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  7. SEMANTIC WRITE METHODS
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("7. Semantic Writes");

            cc.WriteLine("  One-liner helpers for common status messages.");
            cc.WriteLine();
            cc.DrawTopLine();
            cc.WriteSuccess("Build completed in 1.4 s");
            cc.WriteError("Connection refused on port 5432");
            cc.WriteWarning("Config file missing — using defaults");
            cc.WriteInfo("Watching /src for changes...");
            cc.DrawSeparator();
            // With tab stops for nested contexts
            cc.WriteSuccess("Step 1 passed",   tabStop: 1);
            cc.WriteSuccess("Step 2 passed",   tabStop: 1);
            cc.WriteError("Step 3 failed",     tabStop: 1);
            cc.WriteWarning("Continuing anyway", tabStop: 2);
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  8. AUTO-NUMBERING
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("8. Auto-Numbering");

            cc.WriteLine("  Pass AutoNumber = true (or .Numbered()) to prefix each WriteLine");
            cc.WriteLine("  with a sequential counter.  ResetAutoNumber() resets to zero.");
            cc.WriteLine();

            var numbered = new WriteOptions { AutoNumber = true };

            cc.DrawTopLine();
            cc.WriteLine("  Phase 1 — Installation", new WriteOptions().Cyan().Styled(TextStyle.Caps));
            cc.DrawSeparator(LineStyle.Double, LineStyle.Single);
            cc.WriteLine("Download the SDK",    numbered);
            cc.WriteLine("Verify checksum (intentionally long step to demonstrate word-wrap continuing the same numbered item across lines)", numbered);
            cc.WriteLine("Install to /usr/local", numbered);

            cc.ResetAutoNumber();

            cc.DrawSeparator(LineStyle.Double, LineStyle.Single);
            cc.WriteLine("  Phase 2 — Configuration", new WriteOptions().Cyan().Styled(TextStyle.Caps));
            cc.DrawSeparator(LineStyle.Double, LineStyle.Single);
            cc.WriteLine("Edit appsettings.json", numbered);
            cc.WriteLine("Set environment variables", numbered);
            cc.WriteLine("Run database migrations", numbered);
            cc.DrawBottomLine();

            cc.ResetAutoNumber();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  9. DRAW KEY-VALUE
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("9. DrawKeyValue");

            cc.WriteLine("  Renders key: value pairs. Key is in accent color, value in foreground.");
            cc.WriteLine();
            cc.DrawTopLine();
            cc.DrawKeyValue("Host",        "localhost");
            cc.DrawKeyValue("Port",        "5432");
            cc.DrawKeyValue("Database",    "production");
            cc.DrawKeyValue("User",        "admin");
            cc.DrawKeyValue("SSL",         "enabled");
            cc.DrawSeparator();
            // With tab stop for nesting
            cc.DrawKeyValue("  Server",   "web-01",         tabStop: 1);
            cc.DrawKeyValue("  Region",   "us-east-1",      tabStop: 1);
            cc.DrawKeyValue("  Load",     "23 %",           tabStop: 1);
            cc.DrawSeparator();
            // With custom colors
            cc.DrawKeyValue("Status",     "Online",
                keyColor:   ConsoleColor.White,
                valueColor: ConsoleColor.Green);
            cc.DrawKeyValue("Errors",     "3",
                keyColor:   ConsoleColor.White,
                valueColor: ConsoleColor.Red);
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  10. DRAW LIST
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("10. DrawList");

            cc.WriteLine("  DrawList renders a bulleted list with an optional section header.");
            cc.WriteLine();

            // No header
            cc.DrawTopLine();
            cc.WriteLine("  Default bullet  •", new WriteOptions().Dimmed());
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
            cc.DrawList(null, new[]
            {
                "First item",
                "Second item",
                "Third item with enough text to trigger word-wrapping on a narrow render width"
            });
            cc.DrawSeparator();

            // With header (DrawSectionHeader inside)
            cc.DrawList("Dependencies", new[]
            {
                "Microsoft.Extensions.Logging 8.0",
                "Newtonsoft.Json 13.0",
                "xunit 2.7"
            });
            cc.DrawSeparator();

            // Custom bullet & foreColor
            cc.WriteLine("  Custom bullet  ➤  and custom foreColor", new WriteOptions().Dimmed());
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
            cc.DrawList(null, new[]
            {
                "Feature A",
                "Feature B",
                "Feature C"
            }, bullet: '➤', foreColor: ConsoleColor.Cyan);
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  11. WRITE COLUMNS
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("11. WriteColumns");

            cc.WriteLine("  Renders values side-by-side in equal-width columns.");
            cc.WriteLine("  The tuple overload formats pairs as  Label: Value.");
            cc.WriteLine();

            cc.DrawTopLine();
            cc.WriteLine("  Tuple overload — label: value pairs", new WriteOptions().Dimmed());
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
            cc.WriteColumns(
                ("CPU",     "72 %"),
                ("RAM",     "4.2 GB"),
                ("Disk",    "88 GB free"));
            cc.WriteColumns(
                ("Host",    "localhost"),
                ("Port",    "5432"),
                ("DB",      "production"));
            cc.DrawSeparator();

            // String array with per-column alignment and foreColor
            cc.WriteLine("  String array — custom alignment + foreground color per column", new WriteOptions().Dimmed());
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
            cc.WriteColumns(
                values:     new[] { "Name",    "Status",   "Score"  },
                alignments: new[] { TextPosition.Left, TextPosition.Center, TextPosition.Right },
                foreColors: new[] { ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Cyan });
            cc.WriteColumns(
                values:     new[] { "Alice",   "Pass",     "98"     },
                alignments: new[] { TextPosition.Left, TextPosition.Center, TextPosition.Right });
            cc.WriteColumns(
                values:     new[] { "Bob",     "Fail",     "42"     },
                alignments: new[] { TextPosition.Left, TextPosition.Center, TextPosition.Right });
            cc.WriteColumns(
                values:     new[] { "Carol",   "Pass",     "87"     },
                alignments: new[] { TextPosition.Left, TextPosition.Center, TextPosition.Right });
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  12. TABLES — STYLES
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("12. Tables — Border Styles");

            cc.WriteLine("  DrawTable supports three styles: DoubleBorderSingleInner (default),");
            cc.WriteLine("  AllSingle, and AllDouble. Each table is wrapped inside the outer box.");
            cc.WriteLine();

            // ── Default: double outer border, single inner dividers ────────
            cc.WriteLine("  DoubleBorderSingleInner (default)", new WriteOptions().Dimmed());
            cc.DrawTable(
                headers: new[] { "Name", "Role", "Department", "Status" },
                rows: new[]
                {
                    new[] { "Alice Johnson",  "Developer", "Engineering", "Active"   },
                    new[] { "Bob Smith",      "Designer",  "Product",     "Active"   },
                    new[] { "Carol Williams", "Manager",   "Operations",  "On Leave" },
                    new[] { "David Brown",    "Analyst",   "Finance",     "Active"   }
                },
                options: new TableOptions
                {
                    ColumnAlignments = new[] { TextPosition.Left, TextPosition.Left,
                                               TextPosition.Left, TextPosition.Center }
                });

            cc.WriteLine();

            // ── AllSingle with row separators ─────────────────────────────
            cc.WriteLine("  AllSingle + ShowRowSeparators + right-aligned numeric columns", new WriteOptions().Dimmed());
            cc.DrawTable(
                headers: new[] { "Metric", "Value", "Change" },
                rows: new[]
                {
                    new[] { "Revenue", "$1.2 M",  "+12 %" },
                    new[] { "Users",   "48,200",  "+8 %"  },
                    new[] { "Uptime",  "99.97 %", "—"     }
                },
                options: new TableOptions
                {
                    Style             = TableStyle.AllSingle,
                    ShowRowSeparators = true,
                    ColumnAlignments  = new[] { TextPosition.Left,
                                                TextPosition.Right,
                                                TextPosition.Right }
                });

            cc.WriteLine();

            // ── AllDouble ─────────────────────────────────────────────────
            cc.WriteLine("  AllDouble", new WriteOptions().Dimmed());
            cc.DrawTable(
                headers: new[] { "Server", "Region", "Load" },
                rows: new[]
                {
                    new[] { "web-01", "us-east-1", "23 %" },
                    new[] { "web-02", "eu-west-1", "61 %" },
                    new[] { "db-01",  "us-east-1", "45 %" }
                },
                options: new TableOptions { Style = TableStyle.AllDouble });

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  13. TABLES — ALIGNMENT
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("13. Tables — Horizontal Alignment");

            cc.WriteLine("  Tables can be Left (default), Center, Right, or Justified inside the box.");
            cc.WriteLine();

            string[] tblHeaders = { "Product", "Price", "Qty" };
            string[][] tblRows =
            {
                new[] { "Widget A", "$12.99", "150" },
                new[] { "Widget B", "$7.49",  "320" },
                new[] { "Widget C", "$24.00",  "42" }
            };
            var tblOpt = new TableOptions
            {
                ColumnAlignments = new[] { TextPosition.Left, TextPosition.Right, TextPosition.Right }
            };

            cc.WriteLine("  Left (default — 2-space gap from left border)", new WriteOptions().Dimmed());
            cc.DrawTable(tblHeaders, tblRows, tblOpt);
            cc.WriteLine();

            cc.WriteLine("  Center", new WriteOptions().Dimmed());
            cc.DrawTable(tblHeaders, tblRows, new TableOptions
            {
                Alignment        = TableAlignment.Center,
                ColumnAlignments = tblOpt.ColumnAlignments
            });
            cc.WriteLine();

            cc.WriteLine("  Right (2-space gap from right border)", new WriteOptions().Dimmed());
            cc.DrawTable(tblHeaders, tblRows, new TableOptions
            {
                Alignment        = TableAlignment.Right,
                ColumnAlignments = tblOpt.ColumnAlignments
            });
            cc.WriteLine();

            cc.WriteLine("  Justified (columns expand to full width; 2-space gap each side)", new WriteOptions().Dimmed());
            cc.DrawTable(tblHeaders, tblRows, new TableOptions
            {
                Alignment        = TableAlignment.Justified,
                ColumnAlignments = tblOpt.ColumnAlignments
            });

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  14. TABLES — ADVANCED OPTIONS
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("14. Tables — Advanced Options");

            cc.WriteLine();

            // No header row
            cc.WriteLine("  ShowHeader = false — data-only table", new WriteOptions().Dimmed());
            cc.DrawTable(
                headers: new[] { "Key", "Value" },
                rows: new[]
                {
                    new[] { "OS",      "Windows 11"  },
                    new[] { "Runtime", ".NET 8.0"    },
                    new[] { "Version", "v2.1.0"      }
                },
                options: new TableOptions { ShowHeader = false });
            cc.WriteLine();

            // Explicit column widths
            cc.WriteLine("  Explicit ColumnWidths — override auto-sizing", new WriteOptions().Dimmed());
            cc.DrawTable(
                headers: new[] { "Event", "Timestamp", "Level" },
                rows: new[]
                {
                    new[] { "App started",    "2026-03-11 09:00:01", "INFO"  },
                    new[] { "Config loaded",  "2026-03-11 09:00:02", "INFO"  },
                    new[] { "Port conflict",  "2026-03-11 09:00:05", "ERROR" }
                },
                options: new TableOptions
                {
                    ColumnWidths     = new[] { 18, 22, 6 },
                    ColumnAlignments = new[] { TextPosition.Left,
                                               TextPosition.Center,
                                               TextPosition.Center }
                });

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  15. DRAW BOX
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("15. DrawBox");

            cc.WriteLine("  DrawBox is a shortcut for top border + single content line + bottom border.");
            cc.WriteLine("  Default TextStyle is SpacedCaps.  All parameters are optional.");
            cc.WriteLine();

            cc.DrawBox("Default — SpacedCaps centered");
            cc.DrawBox("Single line style",
                lineStyle:    LineStyle.Single,
                textPosition: TextPosition.Left);
            cc.DrawBox("Right-aligned title",
                textPosition: TextPosition.Right,
                textStyle:    TextStyle.None,
                foreColor:    ConsoleColor.Cyan);
            cc.DrawBox("Plain text — TextStyle.None",
                textStyle: TextStyle.None,
                foreColor:  ConsoleColor.White);

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  16. SECTION HEADERS
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("16. DrawSectionHeader Variants");

            cc.WriteLine("  DrawSectionHeader embeds text in a separator-style line and adds");
            cc.WriteLine("  a blank bordered line above and below for visual breathing room.");
            cc.WriteLine();
            cc.DrawTopLine();
            cc.WriteLine("  Content before the first header");

            cc.DrawSectionHeader("Left-aligned header (default)",
                textPosition: TextPosition.Left);
            cc.WriteLine("  Content");

            cc.DrawSectionHeader("Centered header",
                textPosition: TextPosition.Center);
            cc.WriteLine("  Content");

            cc.DrawSectionHeader("Single line style header",
                lineStyle: LineStyle.Single);
            cc.WriteLine("  Content");

            cc.DrawSectionHeader("Dotted line header",
                lineStyle: LineStyle.Dotted,
                foreColor:  ConsoleColor.Cyan);
            cc.WriteLine("  Content");
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  17. THEMES
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("17. Themes");

            cc.WriteLine("  Three built-in presets: Default (dark), Light, and Hacker.");
            cc.WriteLine("  Each instance carries its own theme; mix freely.");
            cc.WriteLine();

            // Light theme
            using (var light = new ColoredConsole(Theme.Light, width: cc.Width))
            {
                light.DrawTopLine();
                light.WriteLine("  Theme.Light — white background, dark text", new WriteOptions().Styled(TextStyle.None));
                light.DrawSeparator();
                light.WriteSuccess("All checks passed");
                light.WriteWarning("Disk space below 10 %");
                light.DrawBottomLine();
            }

            cc.WriteLine();

            // Hacker theme
            using (var hacker = new ColoredConsole(Theme.Hacker, width: cc.Width))
            {
                hacker.DrawTopLine();
                hacker.WriteLine("H A C K E R   T H E M E", new WriteOptions { TextPosition = TextPosition.Center });
                hacker.DrawSeparator();
                hacker.WriteSuccess("Access granted.");
                hacker.WriteInfo("Initialising payload... just kidding.");
                hacker.WriteError("Firewall detected.");
                hacker.DrawBottomLine();
            }

            cc.WriteLine();

            // Custom theme
            var customTheme = new Theme
            {
                ForeColor    = ConsoleColor.White,
                LineColor    = ConsoleColor.Magenta,
                AccentColor  = ConsoleColor.DarkMagenta,
                SuccessColor = ConsoleColor.Green,
                ErrorColor   = ConsoleColor.Red
            };
            using (var custom = new ColoredConsole(customTheme, width: cc.Width))
            {
                custom.DrawTopLine();
                custom.WriteLine("  Custom theme — Magenta borders", new WriteOptions().Styled(TextStyle.None));
                custom.DrawSeparator();
                custom.WriteSuccess("Custom success");
                custom.WriteError("Custom error");
                custom.DrawBottomLine();
            }

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  18. ASCII MODE
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("18. ASCII Mode");

            cc.WriteLine("  ASCII mode replaces all Unicode box-drawing characters with +, -, |.");
            cc.WriteLine("  Useful for CI/CD logs, SSH sessions, and legacy terminals.");
            cc.WriteLine();

            // Create a dedicated ASCII-mode instance
            using (var ascii = new ColoredConsole(width: cc.Width, asciiMode: true))
            {
                ascii.DrawTopLine();
                ascii.WriteLine("  ASCII mode — borders use + - |");
                ascii.DrawSeparator();
                ascii.WriteSuccess("Looks great in plain-text logs");
                ascii.WriteInfo("Logging, piping, grep-friendly");
                ascii.DrawList(null, new[] { "Item one", "Item two", "Item three" });
                ascii.DrawBottomLine();
            }

            cc.WriteLine();

            // Runtime toggle on an existing instance
            cc.DrawTopLine();
            cc.WriteLine("  Toggle ascii mode at runtime via SetAsciiMode()");
            cc.DrawSeparator();
            cc.SetAsciiMode(true);
            cc.WriteLine("  Now in ASCII mode");
            cc.DrawSeparator();
            cc.SetAsciiMode(false);
            cc.WriteLine("  Back to Unicode mode");
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  19. RESIZE
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("19. Resize");

            int originalWidth = cc.Width;

            cc.WriteLine($"  Current width: {cc.Width} columns");
            cc.WriteLine();

            cc.Resize(50);
            cc.DrawTopLine();
            cc.WriteLine($"  After Resize(50) — width is now {cc.Width}");
            cc.DrawBottomLine();

            cc.WriteLine();
            cc.Resize(originalWidth);   // restore
            cc.DrawTopLine();
            cc.WriteLine($"  After Resize({originalWidth}) — restored to {cc.Width}");
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  20. ASYNC WRITES
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("20. Async Writes");

            cc.WriteLine("  WriteLineAsync / WriteAsync offload rendering to the thread pool.");
            cc.WriteLine("  Safe to await from any async context.");
            cc.WriteLine();
            cc.DrawTopLine();

            await cc.WriteLineAsync("Line written with WriteLineAsync");
            await cc.WriteLineAsync("Another async line in Cyan", new WriteOptions().Cyan());
            await cc.WriteLineAsync("Right-aligned async line",   new WriteOptions().Yellow().RightAligned());
            await cc.WriteAsync("Write (no newline) — cursor repositions for animation");
            // WriteAsync is animation-safe (cursor goes back to start of line)
            // Follow up with a proper WriteLine to advance past it
            cc.WriteLine();
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  21. SPINNER — INDETERMINATE PROGRESS
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("21. Spinner — Indeterminate Progress");

            cc.WriteLine("  Use Spinner when task duration is unknown.");
            cc.WriteLine("  Call UpdateMessage() any time; call Complete() to finish.");
            cc.WriteLine();
            cc.DrawTopLine();
            cc.WriteLine("  Connecting to services...");
            cc.DrawSeparator(LineStyle.Double, LineStyle.Single);

            using (var spinner = new Spinner(cc, "Connecting to database..."))
            {
                Thread.Sleep(1200);
                spinner.UpdateMessage("Running migrations...");
                Thread.Sleep(1200);
                spinner.UpdateMessage("Seeding initial data...");
                Thread.Sleep(800);
                spinner.Complete("Database ready");
            }

            using (var spinner = new Spinner(cc, "Fetching remote config..."))
            {
                Thread.Sleep(900);
                spinner.Complete("Failed — using local defaults", success: false);
            }

            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  22. PROGRESS BAR — DETERMINATE PROGRESS
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("22. ProgressBar — Determinate Progress");

            cc.WriteLine("  Use ProgressBar when you know the total work units.");
            cc.WriteLine("  Call Report(double) with values in [0.0, 1.0] from any thread.");
            cc.WriteLine();
            cc.DrawTopLine();

            cc.WriteLine("  Uploading 150 files...", new WriteOptions().Dimmed());
            cc.DrawSeparator(LineStyle.Double, LineStyle.Single);
            using (var pb = new ProgressBar(cc))
            {
                for (int i = 0; i <= 20; i++)
                {
                    pb.Report((double)i / 20);
                    Thread.Sleep(120);
                }
            }

            cc.DrawSeparator(LineStyle.Double, LineStyle.Single);
            cc.WriteLine("  Processing 8 chunks with Blue bar...", new WriteOptions().Dimmed());
            cc.DrawSeparator(LineStyle.Double, LineStyle.Single);
            using (var pb = new ProgressBar(cc, new WriteOptions { ForeColor = ConsoleColor.Cyan, TabStop = 1 }))
            {
                for (int i = 0; i <= 8; i++)
                {
                    pb.Report((double)i / 8);
                    Thread.Sleep(200);
                }
            }

            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  23. LOGGING
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("23. File Logging");

            cc.WriteLine("  EnableLogging(folder) opens a daily .log file in append mode.");
            cc.WriteLine("  Every WriteLine / WriteSuccess / etc. is automatically logged.");
            cc.WriteLine("  WriteLog() sends a raw message directly to the log file.");
            cc.WriteLine();

            // Write some explicit log entries
            cc.WriteLog("=== User Guide run started ===");
            cc.WriteLog("Demonstrating logging feature");

            // Show the log file location
            var logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            var logFiles  = Directory.Exists(logFolder)
                ? Directory.GetFiles(logFolder, "*.log")
                : Array.Empty<string>();

            cc.DrawTopLine();
            cc.DrawKeyValue("Log folder", logFolder);
            cc.DrawKeyValue("Log files",  logFiles.Length.ToString());

            if (logFiles.Length > 0)
            {
                cc.DrawSeparator();
                cc.WriteLine("  Last 3 lines of today's log:", new WriteOptions().Dimmed());
                cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);

                var latestLog = logFiles[^1];
                // Open with FileShare.ReadWrite — the StreamWriter inside ColoredConsole
                // still holds the file open, so we must share both read and write access.
                string[] allLines;
                using (var fs = new FileStream(latestLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                    allLines = sr.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                int start = Math.Max(0, allLines.Length - 3);
                for (int i = start; i < allLines.Length; i++)
                    cc.WriteLine(allLines[i], new WriteOptions().Dimmed().Tabbed(1));
            }
            cc.DrawBottomLine();

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  24. ANSI CONSOLE (256-color & true-color)
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("24. AnsiConsole — 256-color & True-color");

            cc.WriteLine("  AnsiConsole provides ANSI escape sequences for terminals that");
            cc.WriteLine("  support 256-color, true-color RGB, and text decorations.");
            cc.WriteLine($"  AnsiConsole.IsSupported = {AnsiConsole.IsSupported}");
            cc.WriteLine();

            if (AnsiConsole.IsSupported)
            {
                cc.DrawTopLine();
                cc.WriteLine("  256-color palette samples (indices 196–201):");
                cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);

                Console.Write("  ");
                for (int i = 196; i <= 201; i++)
                {
                    AnsiConsole.Write($"  Color {i}  ", i);
                }
                Console.WriteLine();

                cc.DrawSeparator();
                cc.WriteLine("  True-color RGB gradient: Red → Blue");
                cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
                Console.Write("  ");
                AnsiConsole.WriteGradient(
                    "  ● ColoredConsole.NET supports full 24-bit color gradients ●  ",
                    (255, 80, 80),
                    (80, 80, 255));
                Console.WriteLine();

                cc.DrawSeparator();
                cc.WriteLine("  Text decorations:");
                cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
                Console.Write("  ");
                AnsiConsole.Bold();      AnsiConsole.SetForeground(220); Console.Write("Bold  ");       AnsiConsole.Reset();
                AnsiConsole.Italic();    AnsiConsole.SetForeground(114); Console.Write("Italic  ");     AnsiConsole.Reset();
                AnsiConsole.Underline(); AnsiConsole.SetForeground(39);  Console.Write("Underline  ");  AnsiConsole.Reset();
                AnsiConsole.Dim();       AnsiConsole.SetForeground(250); Console.Write("Dim");          AnsiConsole.Reset();
                Console.WriteLine();

                cc.DrawBottomLine();
            }
            else
            {
                cc.DrawTopLine();
                cc.WriteWarning("ANSI is not supported in this terminal — skipping color demo.");
                cc.WriteInfo("Run in Windows Terminal, VS Code, macOS Terminal, or Linux for full color.");
                cc.DrawBottomLine();
            }

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  25. PROMPT — USER INPUT
            // ═══════════════════════════════════════════════════════════════

            cc.DrawSectionHeader("25. Prompt — User Input");

            cc.WriteLine("  Prompt() renders a question inside the box border and reads a line");
            cc.WriteLine("  of input inline.  Returns null when input is redirected.");
            cc.WriteLine();

            if (!Console.IsInputRedirected)
            {
                cc.DrawTopLine();
                var name = cc.Prompt("What is your name?");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    cc.WriteSuccess($"Hello, {name}!  Welcome to ColoredConsole.NET.");
                }
                cc.DrawBottomLine();
            }
            else
            {
                cc.DrawTopLine();
                cc.WriteInfo("Input is redirected — skipping interactive prompt.");
                cc.DrawBottomLine();
            }

            Pause();

            // ═══════════════════════════════════════════════════════════════
            //  CLOSING
            // ═══════════════════════════════════════════════════════════════

            cc.WriteLine();
            cc.DrawBox("End of User Guide  ·  ColoredConsole.NET v2.1");
            cc.WriteLine();
            cc.DrawTopLine();
            cc.WriteSuccess("All features demonstrated successfully.");
            cc.WriteInfo($"Session log saved to: {logFolder}");
            cc.DrawSeparator();
            cc.WriteLine("  Quick reference:", new WriteOptions().Dimmed());
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
            cc.DrawKeyValue("NuGet package", "ColoredConsole.NET");
            cc.DrawKeyValue("Namespace",     "bcd");
            cc.DrawKeyValue("Main class",    "ColoredConsole  (IDisposable)");
            cc.DrawKeyValue("Themes",        "Theme.Default  |  Theme.Light  |  Theme.Hacker");
            cc.DrawKeyValue("Table styles",  "DoubleBorderSingleInner  |  AllSingle  |  AllDouble");
            cc.DrawKeyValue("Alignment",     "Left  |  Center  |  Right  |  Justified");
            cc.DrawBottomLine();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>Waits for Enter between sections when running interactively.</summary>
        private static void Pause()
        {
            if (Console.IsInputRedirected) return;
            Console.WriteLine();
            Console.Write("  [Press Enter for next section]");
            Console.ReadLine();
            Console.WriteLine();
        }
    }
}
