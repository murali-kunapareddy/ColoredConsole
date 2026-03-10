using System;
using System.Threading;
using System.Threading.Tasks;
using bcd;

namespace Test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using (var cc = new ColoredConsole())
            {
                cc.EnableLogging("Log");

                // ── Title ─────────────────────────────────────────────────────
                cc.DrawBox("ColoredConsole.NET — Demo v2.1");

                // ── Basic box structure ────────────────────────────────────────
                cc.DrawTopLine();
                cc.WriteLine("Basic Box Structure");
                cc.DrawSeparator();
                cc.WriteLine("This is standard body text.");
                cc.DrawBottomLine();

                // ── Semantic write methods ─────────────────────────────────────
                cc.DrawSectionHeader("Semantic Output");
                cc.WriteSuccess("Build completed in 1.4s");
                cc.WriteError("Connection refused on port 5432");
                cc.WriteWarning("Config file missing — using defaults");
                cc.WriteInfo("Watching /src for changes...");

                // ── Auto-numbering ─────────────────────────────────────────────
                cc.DrawSectionHeader("Numbered Steps");
                cc.WriteLine("Install dependencies",  new WriteOptions { AutoNumber = true });
                cc.WriteLine("Configure environment (this line is intentionally long to show word-aware wrapping across multiple lines)", new WriteOptions { AutoNumber = true });
                cc.WriteLine("Run the application",   new WriteOptions { AutoNumber = true });
                cc.ResetAutoNumber();

                // ── Multi-column layout ────────────────────────────────────────
                cc.DrawSectionHeader("System Status");
                cc.WriteColumns(
                    ("CPU",  "72%"),
                    ("RAM",  "4.2 GB"),
                    ("Disk", "88 GB free"));
                cc.WriteColumns(
                    ("Host",    "localhost"),
                    ("Port",    "5432"),
                    ("DB",      "production"));

                // ── Multi-column with explicit alignment ───────────────────────
                cc.DrawSeparator();
                cc.WriteColumns(
                    values:     new[] { "Name", "Status", "Score" },
                    alignments: new[] { TextPosition.Left, TextPosition.Center, TextPosition.Right },
                    foreColors: new[] { ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Cyan });
                cc.WriteColumns(
                    values:     new[] { "Alice",  "Pass",   "98" },
                    alignments: new[] { TextPosition.Left, TextPosition.Center, TextPosition.Right });
                cc.WriteColumns(
                    values:     new[] { "Bob",    "Fail",   "42" },
                    alignments: new[] { TextPosition.Left, TextPosition.Center, TextPosition.Right });

                // ── Table: default style (double border + single inner) ────────
                cc.DrawSectionHeader("Table — Default Style");
                cc.DrawTable(
                    headers: new[] { "Name", "Role", "Department", "Status" },
                    rows: new[]
                    {
                        new[] { "Alice Johnson",  "Developer",   "Engineering", "Active"   },
                        new[] { "Bob Smith",      "Designer",    "Product",     "Active"   },
                        new[] { "Carol Williams", "Manager",     "Operations",  "On Leave" },
                        new[] { "David Brown",    "Analyst",     "Finance",     "Active"   }
                    },
                    options: new TableOptions
                    {
                        ColumnAlignments = new[] { TextPosition.Left, TextPosition.Left, TextPosition.Left, TextPosition.Center }
                    });

                // ── Table: all-single style with row separators ────────────────
                cc.DrawSectionHeader("Table — All Single + Row Separators");
                cc.DrawTable(
                    headers: new[] { "Metric", "Value", "Change" },
                    rows: new[]
                    {
                        new[] { "Revenue",  "$1.2M", "+12%" },
                        new[] { "Users",    "48,200", "+8%" },
                        new[] { "Uptime",   "99.97%", "─"  }
                    },
                    options: new TableOptions
                    {
                        Style             = TableStyle.AllSingle,
                        ShowRowSeparators = true,
                        ColumnAlignments  = new[] { TextPosition.Left, TextPosition.Right, TextPosition.Right }
                    });

                // ── Table: all-double style ────────────────────────────────────
                cc.DrawSectionHeader("Table — All Double");
                cc.DrawTable(
                    headers: new[] { "Server", "Region", "Load" },
                    rows: new[]
                    {
                        new[] { "web-01", "us-east-1", "23%" },
                        new[] { "web-02", "eu-west-1", "61%" },
                        new[] { "db-01",  "us-east-1", "45%" }
                    },
                    options: new TableOptions { Style = TableStyle.AllDouble });

                // ── Prompt ────────────────────────────────────────────────────
                cc.DrawSeparator();
                var name = cc.Prompt("What is your name?");
                cc.WriteSuccess($"Hello, {name}!");

                // ── Spinner (indeterminate progress) ──────────────────────────
                cc.DrawSectionHeader("Spinner Demo");
                using (var spinner = new Spinner(cc, "Connecting to database..."))
                {
                    Thread.Sleep(1200);
                    spinner.UpdateMessage("Running migrations...");
                    Thread.Sleep(1200);
                    spinner.UpdateMessage("Seeding initial data...");
                    Thread.Sleep(800);
                    spinner.Complete("Database ready");
                }

                // ── Progress bar ───────────────────────────────────────────────
                cc.DrawSectionHeader("Progress Bar Demo");
                cc.WriteLine("Uploading files...");
                using (var pb = new ProgressBar(cc))
                {
                    for (int i = 0; i <= 10; i++)
                    {
                        pb.Report((double)i / 10);
                        Thread.Sleep(300);
                    }
                }

                // ── Async writes ───────────────────────────────────────────────
                cc.DrawSectionHeader("Async Write Demo");
                await cc.WriteLineAsync("Line written asynchronously.");
                await cc.WriteLineAsync("Another async line.", new WriteOptions { ForeColor = ConsoleColor.Cyan });

                // ── Theme demo ─────────────────────────────────────────────────
                cc.DrawSectionHeader("Hacker Theme");
                using (var hacker = new ColoredConsole(Theme.Hacker))
                {
                    hacker.DrawTopLine();
                    hacker.WriteLine("H A C K E R   T H E M E", new WriteOptions { TextPosition = TextPosition.Center });
                    hacker.DrawSeparator();
                    hacker.WriteSuccess("System compromised.");
                    hacker.WriteInfo("Exfiltrating data... just kidding.");
                    hacker.DrawBottomLine();
                }

                cc.DrawBottomLine();
            }
        }
    }
}
