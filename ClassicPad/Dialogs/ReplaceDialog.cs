using System;
using System.Drawing;
using System.Windows.Forms;
using ClassicPad.Services;

namespace ClassicPad.Dialogs;

internal sealed class ReplaceDialog : Form
{
    private readonly TextBox findBox;
    private readonly TextBox replaceBox;
    private readonly CheckBox matchCaseBox;
    private readonly RadioButton directionUpRadio;
    private readonly RadioButton directionDownRadio;
    private readonly Button findNextButton;
    private readonly Button replaceButton;
    private readonly Button replaceAllButton;

    public event EventHandler<ReplaceRequestedEventArgs>? ReplaceRequested;

    public ReplaceDialog()
    {
        Text = "Replace";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        findBox = new TextBox { Width = 260 };
        replaceBox = new TextBox { Width = 260 };
        findBox.TextChanged += (_, _) => UpdateButtonState();

        matchCaseBox = new CheckBox { Text = "Match case", AutoSize = true };

        directionUpRadio = new RadioButton { Text = "Up", AutoSize = true };
        directionDownRadio = new RadioButton { Text = "Down", Checked = true, AutoSize = true };

        findNextButton = CreateActionButton("Find Next");
        replaceButton = CreateActionButton("Replace");
        replaceAllButton = CreateActionButton("Replace All");
        var cancelButton = CreateActionButton("Cancel");
        cancelButton.DialogResult = DialogResult.Cancel;
        findNextButton.Enabled = false;
        replaceButton.Enabled = false;
        replaceAllButton.Enabled = false;

        findNextButton.Click += (_, _) => RaiseAction(ReplaceRequestKind.FindNext);
        replaceButton.Click += (_, _) => RaiseAction(ReplaceRequestKind.Replace);
        replaceAllButton.Click += (_, _) => RaiseAction(ReplaceRequestKind.ReplaceAll);

        AcceptButton = findNextButton;
        CancelButton = cancelButton;

        var searchLabel = new Label { Text = "Find what:", TextAlign = ContentAlignment.MiddleLeft, AutoSize = true };
        var replaceLabel = new Label { Text = "Replace with:", TextAlign = ContentAlignment.MiddleLeft, AutoSize = true };

        var grid = new TableLayoutPanel { ColumnCount = 2, AutoSize = true };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.Controls.Add(searchLabel, 0, 0);
        grid.Controls.Add(findBox, 1, 0);
        grid.Controls.Add(replaceLabel, 0, 1);
        grid.Controls.Add(replaceBox, 1, 1);
        grid.Controls.Add(matchCaseBox, 1, 2);

        var directionGroup = new GroupBox { Text = "Direction", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        var directionLayout = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        directionLayout.Controls.Add(directionUpRadio);
        directionLayout.Controls.Add(directionDownRadio);
        directionGroup.Controls.Add(directionLayout);

        var buttonColumn = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, Margin = new Padding(12, 0, 0, 0) };
        buttonColumn.Controls.Add(findNextButton);
        buttonColumn.Controls.Add(replaceButton);
        buttonColumn.Controls.Add(replaceAllButton);
        buttonColumn.Controls.Add(cancelButton);

        var contentLayout = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        contentLayout.Controls.Add(grid);
        contentLayout.Controls.Add(directionGroup);
        contentLayout.Controls.Add(buttonColumn);

        Controls.Add(contentLayout);
    }

    public string SearchText
    {
        get => findBox.Text;
        set => findBox.Text = value;
    }

    public string ReplacementText
    {
        get => replaceBox.Text;
        set => replaceBox.Text = value;
    }

    public bool MatchCase
    {
        get => matchCaseBox.Checked;
        set => matchCaseBox.Checked = value;
    }

    public SearchDirection Direction
    {
        get => directionUpRadio.Checked ? SearchDirection.Up : SearchDirection.Down;
        set
        {
            directionUpRadio.Checked = value == SearchDirection.Up;
            directionDownRadio.Checked = value != SearchDirection.Up;
        }
    }

    public void FocusFindBox()
    {
        findBox.SelectionStart = 0;
        findBox.SelectionLength = findBox.TextLength;
        findBox.Focus();
    }

    private void UpdateButtonState()
    {
        var enabled = !string.IsNullOrWhiteSpace(findBox.Text);
        findNextButton.Enabled = enabled;
        replaceButton.Enabled = enabled;
        replaceAllButton.Enabled = enabled;
    }

    private void RaiseAction(ReplaceRequestKind kind)
    {
        if (string.IsNullOrWhiteSpace(findBox.Text))
        {
            return;
        }

        ReplaceRequested?.Invoke(this, new ReplaceRequestedEventArgs(findBox.Text, MatchCase, Direction, ReplacementText, kind));
    }

    private static Button CreateActionButton(string text)
    {
        return new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(12, 6, 12, 6)
        };
    }
}
