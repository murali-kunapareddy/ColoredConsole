using bcd;
using static bcd.ColoredConsole;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cc = new ColoredConsole();
            cc.DrawBox("This is test");
            cc.DrawTopLine();
            cc.WriteLine("Header");
            cc.DrawSeparator();
            cc.WriteLine("This is body text");
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