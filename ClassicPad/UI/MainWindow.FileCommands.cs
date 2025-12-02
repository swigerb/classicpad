using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ClassicPad.Services;

namespace ClassicPad.UI;

internal sealed partial class MainWindow
{
    private bool NewDocument()
    {
        if (!PromptToSaveIfNeeded())
        {
            return false;
        }

        isLoading = true;
        editor.Clear();
        document.Reset();
        UpdateFileMenuState();
        findReplaceService.Reset();
        UpdateFindMenuAvailability();
        isLoading = false;
        UpdateWindowCaption();
        UpdateStatusBar();
        return true;
    }

    private void LaunchNewWindow()
    {
        var executable = Environment.ProcessPath ?? Application.ExecutablePath;
        try
        {
            Process.Start(new ProcessStartInfo(executable) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to launch another ClassicPad window. {ex.Message}", "ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenDocumentInteractively()
    {
        if (!PromptToSaveIfNeeded())
        {
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*",
            Title = "Open",
            Multiselect = false,
            InitialDirectory = document.InitialDirectory
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            OpenDocument(dialog.FileName);
        }
    }

    private void OpenDocument(string path)
    {
        try
        {
            using var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var content = reader.ReadToEnd();
            isLoading = true;
            editor.Text = content;
            editor.SelectionStart = 0;
            editor.SelectionLength = 0;
            document.MarkSaved(path, reader.CurrentEncoding);
            UpdateFileMenuState();
            findReplaceService.Reset();
            UpdateFindMenuAvailability();
            UpdateWindowCaption();
            UpdateStatusBar();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to open file. {ex.Message}", "ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            isLoading = false;
        }
    }

    private bool SaveDocument()
    {
        if (string.IsNullOrWhiteSpace(document.FilePath))
        {
            return SaveDocumentAs();
        }

        return PersistDocument(document.FilePath, document.Encoding);
    }

    private bool SaveDocumentAs()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*",
            Title = "Save As",
            AddExtension = true,
            DefaultExt = "txt",
            InitialDirectory = document.InitialDirectory,
            FileName = document.DisplayName.Equals("Untitled", StringComparison.OrdinalIgnoreCase) ? "Untitled.txt" : document.DisplayName
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            return PersistDocument(dialog.FileName, new UTF8Encoding(false));
        }

        return false;
    }

    private bool PersistDocument(string path, Encoding encoding)
    {
        try
        {
            File.WriteAllText(path, editor.Text, encoding);
            document.MarkSaved(path, encoding);
            UpdateWindowCaption();
            UpdateStatusBar();
            UpdateFileMenuState();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to save file. {ex.Message}", "ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private bool PromptToSaveIfNeeded()
    {
        if (!document.IsDirty)
        {
            return true;
        }

        var result = MessageBox.Show(this, $"Do you want to save changes to {document.DisplayName}?", "ClassicPad", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
        return result switch
        {
            DialogResult.Yes => SaveDocument(),
            DialogResult.No => true,
            _ => false
        };
    }

    private void ShowPageSetup()
    {
        pageSetupDialog.ShowDialog(this);
    }

    private void PrintCurrentDocument()
    {
        printDocument.DocumentName = document.DisplayName;
        printDocument.Prepare(editor.Text, editor.Font);

        if (printDialog.ShowDialog(this) == DialogResult.OK)
        {
            printDocument.Print();
        }
    }

    private void UpdateFileMenuState()
    {
        if (saveMenuItem is null)
        {
            return;
        }

        saveMenuItem.Enabled = document.IsDirty;
    }
}
