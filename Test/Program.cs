using bcd;
using static bcd.ColoredConsole;

namespace Test
{
    internal class Program
    {
        static ColoredConsole cc = new ColoredConsole();
        static void Main(string[] args)
        {
            
            cc.LogEnable = true;
            cc.LogFolder = "gol";
            cc.WriteLine("Logging is ON");
            cc.DrawBox("This is test");
            cc.DrawTopLine();
            cc.WriteLine("Header");
            cc.DrawSeparator();
            cc.WriteLine("This is body text");
            cc.DrawSeparator("Numabered Bullets");
            cc.WriteLine("This is sirst line with auto number on. ",autoNumber: true);
            cc.WriteLine("This is second line with auto number on + lengthier text to wrap into two lines.",autoNumber: true);
            cc.WriteLine("This is third lin again with auto number on.",autoNumber: true);
            cc.DrawSeparator();
            cc.WriteLine();
            var x = cc.Prompt("What's your name? ");
            cc.WriteLine(x);            
            cc.DrawSeparator(LineStyle.Double, LineStyle.Single);            
            cc.WriteLine("Performing some task... ");
            using (var progress = new ProgressBar())
            {
                int min = 0, max = 10;
                for (int i = min; i < max; i++)
                {
                    progress.Report((double)i/max);
                    Thread.Sleep(500);
                    // a = 1;
                }
            }
            
            cc.DrawSeparator(LineStyle.Double, LineStyle.Double);
            var y = cc.Prompt("Testing prompt to it's max. capability by asking log question. Did you get this message prompt properly? ");
            cc.WriteLine(y);
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
            cc.WriteLine("This is footer text");
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dashed);
            cc.WriteLine("This is sub footer text");
            cc.DrawBottomLine();

        }
    }
}