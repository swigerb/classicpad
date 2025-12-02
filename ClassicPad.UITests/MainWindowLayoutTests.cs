using System.Linq;
using System.Windows.Forms;
using ClassicPad.UI;
using Xunit;
using Xunit.Sdk;

namespace ClassicPad.UITests;

public sealed class MainWindowLayoutTests
{
    [StaFact]
    public void EditorIsPositionedBetweenMenuAndStatusBar()
    {
        WinFormsTestHost.EnsureInitialized();
        using var window = new MainWindow();
        window.CreateControl();

        var menu = window.MainMenuStrip ?? throw new XunitException("Main menu not initialized");
        var status = window.Controls.OfType<StatusStrip>().Single();
        var editor = window.Controls.OfType<RichTextBox>().Single();

        Assert.True(editor.Top >= menu.Bottom, "Editor should start below the menu strip.");
        Assert.True(editor.Bottom <= status.Top, "Editor should stop above the status strip.");
    }
}
