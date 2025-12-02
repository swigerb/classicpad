using System;
using System.Drawing;
using System.Windows.Forms;
using ClassicPad.Services;

namespace ClassicPad.Dialogs;

internal sealed class FindDialog : Form
{
    private readonly TextBox searchBox;
    private readonly CheckBox matchCaseBox;
    private readonly RadioButton directionUpRadio;
    private readonly RadioButton directionDownRadio;
    private readonly Button findNextButton;

    public event EventHandler<FindRequestedEventArgs>? FindNextRequested;

    public FindDialog()
    {
        Text = "Find";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        searchBox = new TextBox { Width = 260 };
        searchBox.TextChanged += (_, _) => UpdateFindButtonState();

        matchCaseBox = new CheckBox { Text = "Match case", AutoSize = true };

        directionUpRadio = new RadioButton { Text = "Up", AutoSize = true };
        directionDownRadio = new RadioButton { Text = "Down", Checked = true, AutoSize = true };

        findNextButton = new Button
        {
            Text = "Find Next",
            DialogResult = DialogResult.None,
            Enabled = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(12, 6, 12, 6)
        };
        findNextButton.Click += (_, _) => OnFindNext();

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(12, 6, 12, 6)
        };

        AcceptButton = findNextButton;
        CancelButton = cancelButton;

        var directionGroup = new GroupBox { Text = "Direction", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        var directionLayout = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        directionLayout.Controls.Add(directionUpRadio);
        directionLayout.Controls.Add(directionDownRadio);
        directionGroup.Controls.Add(directionLayout);

        var buttonColumn = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Margin = new Padding(12, 0, 0, 0)
        };
        buttonColumn.Controls.Add(findNextButton);
        buttonColumn.Controls.Add(cancelButton);

        var searchLabel = new Label { Text = "Find what:", TextAlign = ContentAlignment.MiddleLeft, AutoSize = true };

        var searchLayout = new TableLayoutPanel { ColumnCount = 2, RowCount = 2, AutoSize = true };
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        searchLayout.Controls.Add(searchLabel, 0, 0);
        searchLayout.Controls.Add(searchBox, 1, 0);
        searchLayout.Controls.Add(matchCaseBox, 1, 1);

        var contentLayout = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        contentLayout.Controls.Add(searchLayout);
        contentLayout.Controls.Add(directionGroup);
        contentLayout.Controls.Add(buttonColumn);

        Controls.Add(contentLayout);
    }

    public string SearchText
    {
        get => searchBox.Text;
        set => searchBox.Text = value;
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

    public void FocusSearchBox()
    {
        searchBox.SelectionStart = 0;
        searchBox.SelectionLength = searchBox.TextLength;
        searchBox.Focus();
    }

    private void UpdateFindButtonState() => findNextButton.Enabled = !string.IsNullOrWhiteSpace(searchBox.Text);

    private void OnFindNext()
    {
        if (string.IsNullOrWhiteSpace(searchBox.Text))
        {
            return;
        }

        FindNextRequested?.Invoke(this, new FindRequestedEventArgs(searchBox.Text, MatchCase, Direction));
    }
}
