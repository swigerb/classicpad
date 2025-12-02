using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ClassicPad.Dialogs;
using ClassicPad.Services;

namespace ClassicPad.UI;

internal sealed partial class MainWindow
{
    private void UpdateEditMenuState(ToolStripMenuItem undo, ToolStripMenuItem cut, ToolStripMenuItem copy, ToolStripMenuItem paste, ToolStripMenuItem delete, ToolStripMenuItem searchWithBing, ToolStripMenuItem selectAll)
    {
        undo.Enabled = editor.CanUndo;
        bool hasSelection = editor.SelectionLength > 0;
        cut.Enabled = hasSelection;
        copy.Enabled = hasSelection;
        delete.Enabled = hasSelection;
        bool canPaste = false;
        try
        {
            canPaste = Clipboard.ContainsText();
        }
        catch (ExternalException)
        {
            canPaste = false;
        }
        paste.Enabled = canPaste;
        searchWithBing.Enabled = hasSelection;
        selectAll.Enabled = editor.TextLength > 0;
        goToMenuItem.Enabled = !editor.WordWrap;
        findNextMenuItem.Enabled = findReplaceService.HasActiveSearch;
        findPreviousMenuItem.Enabled = findReplaceService.HasActiveSearch;
    }

    private void UpdateViewMenuState()
    {
        statusBarMenuItem.Enabled = !editor.WordWrap;
        if (!statusBarMenuItem.Enabled)
        {
            statusBarMenuItem.Checked = false;
        }

        zoomResetMenuItem.Enabled = Math.Abs(zoomFactor - 1.0f) > 0.01f;
    }

    private void SearchWithBing()
    {
        if (editor.SelectionLength == 0)
        {
            return;
        }

        var query = editor.SelectedText.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        var url = "https://www.bing.com/search?q=" + Uri.EscapeDataString(query);
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to open Bing. {ex.Message}", "ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowGoToDialog()
    {
        if (editor.WordWrap)
        {
            MessageBox.Show(this, "Go To is unavailable when Word Wrap is enabled.", "ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        int maxLine = editor.GetLineFromCharIndex(editor.TextLength) + 1;
        using var dialog = new GoToDialog(maxLine);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            int lineIndex = dialog.SelectedLine - 1;
            int charIndex = editor.GetFirstCharIndexFromLine(lineIndex);
            if (charIndex >= 0)
            {
                editor.SelectionStart = charIndex;
                editor.SelectionLength = 0;
                editor.ScrollToCaret();
            }
            else
            {
                MessageBox.Show(this, "Line number out of range.", "ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void InsertTimeStamp()
    {
        var timestamp = DateTime.Now.ToString("t M/d/yyyy", CultureInfo.CurrentCulture);
        editor.SelectedText = timestamp;
    }

    private void ToggleWordWrap()
    {
        editor.WordWrap = !editor.WordWrap;
        editor.ScrollBars = editor.WordWrap ? RichTextBoxScrollBars.Vertical : RichTextBoxScrollBars.Both;
        wordWrapMenuItem.Checked = editor.WordWrap;
        if (editor.WordWrap)
        {
            statusBarMenuItem.Checked = false;
        }
        goToMenuItem.Enabled = !editor.WordWrap;
        UpdateStatusBar();
        UpdateViewMenuState();
    }

    private void ChooseFont()
    {
        fontDialog.Font = editor.Font;
        if (fontDialog.ShowDialog(this) == DialogResult.OK)
        {
            editor.Font = fontDialog.Font;
            UpdateStatusBar();
        }
    }

    private void ToggleStatusBar()
    {
        if (editor.WordWrap)
        {
            statusBarMenuItem.Checked = false;
            return;
        }

        statusBarMenuItem.Checked = !statusBarMenuItem.Checked;
        statusStrip.Visible = statusBarMenuItem.Checked;
    }

    private void ZoomIn() => AdjustZoom(0.1f);

    private void ZoomOut() => AdjustZoom(-0.1f);

    private void ResetZoom()
    {
        zoomFactor = 1.0f;
        editor.ZoomFactor = zoomFactor;
        UpdateStatusBar();
        UpdateViewMenuState();
    }

    private void AdjustZoom(float delta)
    {
        zoomFactor = Math.Clamp(zoomFactor + delta, 0.5f, 4.0f);
        editor.ZoomFactor = zoomFactor;
        UpdateStatusBar();
        UpdateViewMenuState();
    }

    private void ShowHelpPage()
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://learn.microsoft.com/windows/apps") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to open help page. {ex.Message}", "ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LaunchFeedbackHub()
    {
        try
        {
            Process.Start(new ProcessStartInfo("feedback-hub:") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to launch Feedback Hub. {ex.Message}", "ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowAboutDialog()
    {
        var message = "ClassicPad\r\nA lightweight Notepad-style editor built with .NET WinForms.";
        MessageBox.Show(this, message, "About ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private bool TryFind(FindRequestedEventArgs args, bool wrapSearch)
    {
        if (string.IsNullOrEmpty(args.SearchText))
        {
            return false;
        }

        var text = editor.Text;
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var comparison = args.MatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
        int startIndex = args.Direction == SearchDirection.Down
            ? Math.Min(editor.SelectionStart + editor.SelectionLength, text.Length)
            : editor.SelectionStart - 1;

        int foundIndex = -1;
        if (args.Direction == SearchDirection.Down)
        {
            foundIndex = text.IndexOf(args.SearchText, startIndex, comparison);
            if (foundIndex < 0 && wrapSearch)
            {
                foundIndex = text.IndexOf(args.SearchText, 0, comparison);
            }
        }
        else
        {
            int searchStart = startIndex < 0 ? text.Length - 1 : startIndex;
            foundIndex = text.LastIndexOf(args.SearchText, searchStart, comparison);
            if (foundIndex < 0 && wrapSearch)
            {
                foundIndex = text.LastIndexOf(args.SearchText, text.Length - 1, comparison);
            }
        }

        bool succeeded = false;
        if (foundIndex >= 0)
        {
            editor.SelectionStart = foundIndex;
            editor.SelectionLength = args.SearchText.Length;
            editor.ScrollToCaret();
            succeeded = true;
        }

        UpdateFindMenuAvailability();
        return succeeded;
    }

    private bool TryReplace(ReplaceRequestedEventArgs args)
    {
        bool replaced = false;
        if (SelectionMatches(args.SearchText, args.MatchCase))
        {
            editor.SelectedText = args.ReplacementText;
            replaced = true;
        }

        var findArgs = new FindRequestedEventArgs(args.SearchText, args.MatchCase, args.Direction);
        bool foundNext = TryFind(findArgs, wrapSearch: true);
        bool result = replaced || foundNext;
        UpdateFindMenuAvailability();
        return result;
    }

    private int ReplaceAll(ReplaceRequestedEventArgs args)
    {
        if (string.IsNullOrEmpty(args.SearchText))
        {
            return 0;
        }

        var text = editor.Text;
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var comparison = args.MatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
        var builder = new StringBuilder(text.Length);
        int count = 0;
        int index = 0;
        int previousIndex = 0;

        while ((index = text.IndexOf(args.SearchText, previousIndex, comparison)) >= 0)
        {
            builder.Append(text, previousIndex, index - previousIndex);
            builder.Append(args.ReplacementText);
            previousIndex = index + args.SearchText.Length;
            count++;
        }

        builder.Append(text, previousIndex, text.Length - previousIndex);

        if (count > 0)
        {
            isLoading = true;
            editor.Text = builder.ToString();
            editor.SelectionStart = Math.Min(editor.TextLength, previousIndex);
            editor.SelectionLength = 0;
            document.MarkDirty();
            isLoading = false;
            UpdateWindowCaption();
            UpdateStatusBar();
            UpdateFileMenuState();
        }

        UpdateFindMenuAvailability();
        return count;
    }

    private bool SelectionMatches(string textToMatch, bool matchCase)
    {
        if (editor.SelectionLength != textToMatch.Length)
        {
            return false;
        }

        return string.Compare(editor.SelectedText, textToMatch, !matchCase, CultureInfo.CurrentCulture) == 0;
    }
}
