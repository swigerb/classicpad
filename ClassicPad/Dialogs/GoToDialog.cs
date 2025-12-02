using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassicPad.Dialogs;

internal sealed class GoToDialog : Form
{
    private readonly NumericUpDown lineSelector;

    public GoToDialog(int maxLine)
    {
        Text = "Go To";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        var infoLabel = new Label { Text = "Line number:", AutoSize = true };
        lineSelector = new NumericUpDown
        {
            Minimum = 1,
            Maximum = Math.Max(1, maxLine),
            Width = 200,
            Value = 1
        };

        var goButton = new Button { Text = "Go To", DialogResult = DialogResult.OK, Width = 100 };
        var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 100 };

        AcceptButton = goButton;
        CancelButton = cancelButton;

        var layout = new TableLayoutPanel { ColumnCount = 2, AutoSize = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.Controls.Add(infoLabel, 0, 0);
        layout.Controls.Add(lineSelector, 1, 0);

        var buttonPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Margin = new Padding(0, 12, 0, 0) };
        buttonPanel.Controls.Add(cancelButton);
        buttonPanel.Controls.Add(goButton);

        var root = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true };
        root.Controls.Add(layout);
        root.Controls.Add(buttonPanel);
        Controls.Add(root);
    }

    public int SelectedLine => (int)lineSelector.Value;
}
