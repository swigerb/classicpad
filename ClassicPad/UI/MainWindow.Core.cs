using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ClassicPad.App;
using ClassicPad.Printing;
using ClassicPad.Services;

namespace ClassicPad.UI;

internal sealed partial class MainWindow : Form
{
    private readonly DocumentSession document = new();
    private readonly RichTextBox editor;
    private readonly StatusStrip statusStrip;
    private ToolStripStatusLabel positionLabel = null!;
    private ToolStripStatusLabel zoomLabel = null!;
    private ToolStripStatusLabel encodingLabel = null!;
    private ToolStripMenuItem goToMenuItem = null!;
    private ToolStripMenuItem wordWrapMenuItem = null!;
    private ToolStripMenuItem statusBarMenuItem = null!;
    private ToolStripMenuItem findNextMenuItem = null!;
    private ToolStripMenuItem findPreviousMenuItem = null!;
    private ToolStripMenuItem zoomResetMenuItem = null!;
    private ToolStripMenuItem saveMenuItem = null!;

    private readonly TextPrintDocument printDocument;
    private readonly PageSetupDialog pageSetupDialog;
    private readonly PrintDialog printDialog;
    private readonly FontDialog fontDialog;
    private readonly FindReplaceService findReplaceService;

    private string? pendingOpenPath;
    private bool isLoading;
    private float zoomFactor = 1.0f;

    public MainWindow()
    {
        Text = document.WindowCaption;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
        MinimumSize = new Size(480, 320);
        StartPosition = FormStartPosition.WindowsDefaultBounds;

        editor = CreateEditor();
        var menuStrip = BuildMenu();
        statusStrip = BuildStatusStrip();

        Controls.Add(editor);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;

        printDocument = new TextPrintDocument();
        pageSetupDialog = new PageSetupDialog { Document = printDocument, EnableMetric = true };
        printDialog = new PrintDialog { Document = printDocument, UseEXDialog = true, AllowSomePages = true };
        fontDialog = new FontDialog { FontMustExist = true, ShowEffects = false };

        findReplaceService = new FindReplaceService(
            GetDefaultSearchText,
            args => TryFind(args, wrapSearch: true),
            args => TryReplace(args),
            args => ReplaceAll(args),
            search => MessageBox.Show(this, $"Cannot find \"{search}\".", "ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Information),
            count => MessageBox.Show(this, $"Replaced {count} occurrence(s).", "ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Information));

        UpdateFindMenuAvailability();
        AllowDrop = true;
        DragEnter += HandleDragEnter;
        DragDrop += HandleDragDrop;

        editor.TextChanged += (_, _) => OnEditorTextChanged();
        editor.SelectionChanged += (_, _) => UpdateStatusBar();
        editor.LinkClicked += OnEditorLinkClicked;

        FormClosing += OnFormClosing;

        UpdateStatusBar();
        UpdateWindowCaption();
        UpdateFileMenuState();
    }

    public void EnqueueOpenOnStartup(string path) => pendingOpenPath = path;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (!string.IsNullOrWhiteSpace(pendingOpenPath) && File.Exists(pendingOpenPath))
        {
            OpenDocument(pendingOpenPath);
            pendingOpenPath = null;
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Control | Keys.Add:
            case Keys.Control | Keys.Oemplus:
            case Keys.Control | Keys.Shift | Keys.Oemplus:
                ZoomIn();
                return true;
            case Keys.Control | Keys.Subtract:
            case Keys.Control | Keys.OemMinus:
                ZoomOut();
                return true;
            case Keys.Control | Keys.D0:
            case Keys.Control | Keys.NumPad0:
                ResetZoom();
                return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private RichTextBox CreateEditor()
    {
        var box = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            AcceptsTab = true,
            DetectUrls = false,
            HideSelection = false,
            Multiline = true,
            WordWrap = false,
            ScrollBars = RichTextBoxScrollBars.Both,
            Font = new Font("Consolas", 11f, GraphicsUnit.Point)
        };
        box.ZoomFactor = zoomFactor;
        box.ShortcutsEnabled = true;
        return box;
    }

    private MenuStrip BuildMenu()
    {
        var menu = new MenuStrip { Dock = DockStyle.Top };

        var fileMenu = new ToolStripMenuItem("File");
        saveMenuItem = CreateMenuItem("Save", Keys.Control | Keys.S, (_, _) => SaveDocument());
        var saveAsMenuItem = CreateMenuItem("Save As...", Keys.Control | Keys.Shift | Keys.S, (_, _) => SaveDocumentAs());
        fileMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            CreateMenuItem("New", Keys.Control | Keys.N, (_, _) => NewDocument()),
            CreateMenuItem("New Window", Keys.Control | Keys.Shift | Keys.N, (_, _) => LaunchNewWindow()),
            CreateMenuItem("Open...", Keys.Control | Keys.O, (_, _) => OpenDocumentInteractively()),
            saveMenuItem,
            saveAsMenuItem,
            new ToolStripSeparator(),
            CreateMenuItem("Page Setup...", Keys.None, (_, _) => ShowPageSetup()),
            CreateMenuItem("Print...", Keys.Control | Keys.P, (_, _) => PrintCurrentDocument()),
            new ToolStripSeparator(),
            CreateMenuItem("Exit", Keys.Alt | Keys.F4, (_, _) => Close())
        });
        fileMenu.DropDownOpening += (_, _) => UpdateFileMenuState();

        var editMenu = new ToolStripMenuItem("Edit");
        var undo = CreateMenuItem("Undo", Keys.Control | Keys.Z, (_, _) => editor.Undo());
        var cut = CreateMenuItem("Cut", Keys.Control | Keys.X, (_, _) => editor.Cut());
        var copy = CreateMenuItem("Copy", Keys.Control | Keys.C, (_, _) => editor.Copy());
        var paste = CreateMenuItem("Paste", Keys.Control | Keys.V, (_, _) => editor.Paste());
        var delete = CreateMenuItem("Delete", Keys.Delete, (_, _) => editor.SelectedText = string.Empty);
        var searchWithBing = CreateMenuItem("Search with Bing", Keys.Control | Keys.E, (_, _) => SearchWithBing());
        var find = CreateMenuItem("Find...", Keys.Control | Keys.F, (_, _) => findReplaceService.ShowFindDialog(this));
        findNextMenuItem = CreateMenuItem("Find Next", Keys.F3, (_, _) => findReplaceService.FindNext(this));
        findPreviousMenuItem = CreateMenuItem("Find Previous", Keys.Shift | Keys.F3, (_, _) => findReplaceService.FindNext(this, SearchDirection.Up));
        var replace = CreateMenuItem("Replace...", Keys.Control | Keys.H, (_, _) => findReplaceService.ShowReplaceDialog(this));
        goToMenuItem = CreateMenuItem("Go To...", Keys.Control | Keys.G, (_, _) => ShowGoToDialog());
        goToMenuItem.Enabled = !editor.WordWrap;
        var selectAll = CreateMenuItem("Select All", Keys.Control | Keys.A, (_, _) => editor.SelectAll());
        var timeDate = CreateMenuItem("Time/Date", Keys.F5, (_, _) => InsertTimeStamp());

        editMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            undo,
            new ToolStripSeparator(),
            cut,
            copy,
            paste,
            delete,
            new ToolStripSeparator(),
            searchWithBing,
            find,
            findNextMenuItem,
            findPreviousMenuItem,
            replace,
            goToMenuItem,
            new ToolStripSeparator(),
            selectAll,
            timeDate
        });
        editMenu.DropDownOpening += (_, _) => UpdateEditMenuState(undo, cut, copy, paste, delete, searchWithBing, selectAll);

        var formatMenu = new ToolStripMenuItem("Format");
        wordWrapMenuItem = CreateMenuItem("Word Wrap", Keys.None, (_, _) => ToggleWordWrap());
        wordWrapMenuItem.Checked = editor.WordWrap;
        wordWrapMenuItem.CheckOnClick = false;
        var fontMenuItem = CreateMenuItem("Font...", Keys.Control | Keys.Shift | Keys.F, (_, _) => ChooseFont());
        formatMenu.DropDownItems.AddRange(new ToolStripItem[] { wordWrapMenuItem, fontMenuItem });

        var viewMenu = new ToolStripMenuItem("View");
        var zoomMenu = new ToolStripMenuItem("Zoom");
        zoomMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            CreateMenuItem("Zoom In", Keys.Control | Keys.Add, (_, _) => ZoomIn()),
            CreateMenuItem("Zoom Out", Keys.Control | Keys.Subtract, (_, _) => ZoomOut()),
            zoomResetMenuItem = CreateMenuItem("Restore Default Zoom", Keys.Control | Keys.D0, (_, _) => ResetZoom())
        });
        statusBarMenuItem = CreateMenuItem("Status Bar", Keys.None, (_, _) => ToggleStatusBar());
        statusBarMenuItem.Checked = true;
        viewMenu.DropDownItems.AddRange(new ToolStripItem[] { zoomMenu, new ToolStripSeparator(), statusBarMenuItem });
        viewMenu.DropDownOpening += (_, _) => UpdateViewMenuState();

        var helpMenu = new ToolStripMenuItem("Help");
        helpMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            CreateMenuItem("View Help", Keys.F1, (_, _) => ShowHelpPage()),
            CreateMenuItem("Send Feedback", Keys.None, (_, _) => LaunchFeedbackHub()),
            new ToolStripSeparator(),
            CreateMenuItem("About ClassicPad", Keys.None, (_, _) => ShowAboutDialog())
        });

        menu.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, formatMenu, viewMenu, helpMenu });
        return menu;
    }

    private StatusStrip BuildStatusStrip()
    {
        var strip = new StatusStrip { Dock = DockStyle.Bottom };
        positionLabel = new ToolStripStatusLabel("Ln 1, Col 1");
        zoomLabel = new ToolStripStatusLabel("100%");
        encodingLabel = new ToolStripStatusLabel("UTF-8");
        var filler = new ToolStripStatusLabel { Spring = true };
        strip.Items.Add(positionLabel);
        strip.Items.Add(filler);
        strip.Items.Add(zoomLabel);
        strip.Items.Add(encodingLabel);
        return strip;
    }

    private ToolStripMenuItem CreateMenuItem(string text, Keys shortcut, EventHandler handler)
    {
        var item = new ToolStripMenuItem(text) { ShortcutKeys = shortcut == Keys.None ? Keys.None : shortcut, ShowShortcutKeys = shortcut != Keys.None };
        item.Click += handler;
        return item;
    }

    private void UpdateWindowCaption() => Text = document.WindowCaption + (document.IsDirty ? "*" : string.Empty);

    private void OnEditorTextChanged()
    {
        if (isLoading)
        {
            return;
        }

        document.MarkDirty();
        UpdateWindowCaption();
        UpdateStatusBar();
        UpdateFileMenuState();
    }

    private void UpdateStatusBar()
    {
        int caretIndex = editor.SelectionStart;
        int line = editor.GetLineFromCharIndex(caretIndex) + 1;
        int lineStart = editor.GetFirstCharIndexOfCurrentLine();
        if (lineStart < 0)
        {
            lineStart = 0;
        }
        int column = caretIndex - lineStart + 1;
        positionLabel.Text = $"Ln {line}, Col {column}";
        zoomLabel.Text = $"{(int)(zoomFactor * 100)}%";
        encodingLabel.Text = document.Encoding.WebName.ToUpperInvariant();
        if (statusBarMenuItem is not null)
        {
            statusStrip.Visible = statusBarMenuItem.Checked;
        }
    }

    private void UpdateFindMenuAvailability()
    {
        if (findNextMenuItem is null || findPreviousMenuItem is null)
        {
            return;
        }

        bool hasSearch = findReplaceService.HasActiveSearch;
        findNextMenuItem.Enabled = hasSearch;
        findPreviousMenuItem.Enabled = hasSearch;
    }

    private void HandleDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void HandleDragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
        {
            if (PromptToSaveIfNeeded())
            {
                OpenDocument(files[0]);
            }
        }
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!PromptToSaveIfNeeded())
        {
            e.Cancel = true;
        }
        else
        {
            findReplaceService.Dispose();
        }
    }

    private string GetDefaultSearchText()
    {
        var selection = editor.SelectedText;
        if (!string.IsNullOrWhiteSpace(selection))
        {
            return selection;
        }

        var word = GetWordAtCaret();
        return string.IsNullOrWhiteSpace(word) ? string.Empty : word;
    }

    private string GetWordAtCaret()
    {
        int caret = editor.SelectionStart;
        var text = editor.Text;
        if (string.IsNullOrEmpty(text) || caret >= text.Length)
        {
            return string.Empty;
        }

        int start = caret;
        while (start > 0 && !char.IsWhiteSpace(text[start - 1]))
        {
            start--;
        }

        int end = caret;
        while (end < text.Length && !char.IsWhiteSpace(text[end]))
        {
            end++;
        }

        return text[start..end];
    }

    private void OnEditorLinkClicked(object? sender, LinkClickedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.LinkText))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to launch link. {ex.Message}", "ClassicPad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
