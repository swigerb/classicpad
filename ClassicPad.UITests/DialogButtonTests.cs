using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ClassicPad.Dialogs;
using Xunit;

namespace ClassicPad.UITests;

public sealed class DialogButtonTests
{
    [StaFact]
    public void FindDialogButtons_AutoSizeCorrectly()
    {
        WinFormsTestHost.EnsureInitialized();
        using var dialog = new FindDialog();
        dialog.CreateControl();

        var findNext = Assert.IsType<Button>(dialog.AcceptButton);
        var cancel = Assert.IsType<Button>(dialog.CancelButton);

        Assert.True(findNext.AutoSize, "Find Next button should auto size.");
        Assert.True(cancel.AutoSize, "Cancel button should auto size.");
        Assert.True(findNext.Height >= 24, "Buttons should be tall enough for DPI scaling.");
    }

    [StaFact]
    public void ReplaceDialogButtons_AutoSizeCorrectly()
    {
        WinFormsTestHost.EnsureInitialized();
        using var dialog = new ReplaceDialog();
        dialog.CreateControl();

        var findNext = Assert.IsType<Button>(dialog.AcceptButton);
        var allButtons = EnumerateButtons(dialog).ToList();

        Assert.Contains(allButtons, b => b.Text == "Find Next" && b.AutoSize);
        Assert.Contains(allButtons, b => b.Text == "Replace" && b.AutoSize);
        Assert.Contains(allButtons, b => b.Text == "Replace All" && b.AutoSize);
        Assert.Contains(allButtons, b => b.Text == "Cancel" && b.AutoSize);
        Assert.All(allButtons, b => Assert.True(b.Height >= 24, $"Button {b.Text} should be full height."));
    }

    private static IEnumerable<Button> EnumerateButtons(Control control)
    {
        foreach (Control child in control.Controls)
        {
            if (child is Button button)
            {
                yield return button;
            }

            foreach (var nested in EnumerateButtons(child))
            {
                yield return nested;
            }
        }
    }
}
