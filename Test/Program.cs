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
            cc.Write("Header");
            cc.DrawSeparator();
            cc.Write("This is body text");
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
            cc.Write("This is footer text");
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dashed);
            cc.Write("This is sub footer text");
            cc.DrawBottomLine();
        }
    }
}