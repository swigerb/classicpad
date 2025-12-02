using System;
using System.Linq;
using System.Windows.Forms;
using ClassicPad.App;

namespace ClassicPad;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        using var context = new ClassicPadContext(args.FirstOrDefault());
        Application.Run(context);
    }
}
