using System.IO;
using System.Windows.Forms;
using ClassicPad.UI;

namespace ClassicPad.App;

internal sealed class ClassicPadContext : ApplicationContext
{
    private readonly MainWindow mainWindow;

    public ClassicPadContext(string? initialPath)
    {
        mainWindow = new MainWindow();
        mainWindow.FormClosed += (_, __) => ExitThread();
        if (!string.IsNullOrWhiteSpace(initialPath) && File.Exists(initialPath))
        {
            mainWindow.EnqueueOpenOnStartup(initialPath);
        }
        mainWindow.Show();
    }
}
