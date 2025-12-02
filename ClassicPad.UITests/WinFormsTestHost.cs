using System.Windows.Forms;

namespace ClassicPad.UITests;

internal static class WinFormsTestHost
{
    private static bool initialized;

    public static void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        initialized = true;
    }
}
