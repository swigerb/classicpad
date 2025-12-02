using System;
using System.Drawing;
using System.Drawing.Printing;

namespace ClassicPad.Printing;

internal sealed class TextPrintDocument : PrintDocument
{
    private string[] lines = Array.Empty<string>();
    private int currentLine;
    private Font? printFont;

    public void Prepare(string content, Font sourceFont)
    {
        printFont = (Font)sourceFont.Clone();
        lines = content.ReplaceLineEndings("\n").Split('\n');
        currentLine = 0;
    }

    protected override void OnEndPrint(PrintEventArgs e)
    {
        base.OnEndPrint(e);
        currentLine = 0;
        if (printFont is not null)
        {
            printFont.Dispose();
            printFont = null;
        }
    }

    protected override void OnPrintPage(PrintPageEventArgs e)
    {
        base.OnPrintPage(e);
        if (printFont is null)
        {
            throw new InvalidOperationException("Call Prepare before printing.");
        }

        var graphics = e.Graphics ?? throw new InvalidOperationException("A printer graphics context is required.");
        var layoutRectangle = e.MarginBounds;
        float lineHeight = printFont.GetHeight(graphics);
        float yPosition = layoutRectangle.Top;

        while (currentLine < lines.Length)
        {
            if (yPosition + lineHeight > layoutRectangle.Bottom && yPosition > layoutRectangle.Top)
            {
                e.HasMorePages = true;
                return;
            }

            string line = lines[currentLine++];
            graphics.DrawString(line, printFont, Brushes.Black, layoutRectangle.Left, yPosition, StringFormat.GenericTypographic);
            yPosition += lineHeight;
        }

        e.HasMorePages = false;
    }
}
