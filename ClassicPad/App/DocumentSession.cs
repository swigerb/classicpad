using System;
using System.IO;
using System.Text;

namespace ClassicPad.App;

internal sealed class DocumentSession
{
    private const string UntitledLabel = "Untitled";

    public string? FilePath { get; private set; }
    public Encoding Encoding { get; private set; } = new UTF8Encoding(false);
    public bool IsDirty { get; private set; }

    public string DisplayName => string.IsNullOrWhiteSpace(FilePath) ? UntitledLabel : Path.GetFileName(FilePath);

    public string WindowCaption => $"{DisplayName} - ClassicPad";

    public string InitialDirectory => !string.IsNullOrWhiteSpace(FilePath)
        ? Path.GetDirectoryName(FilePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    public void MarkDirty() => IsDirty = true;

    public void Reset()
    {
        FilePath = null;
        Encoding = new UTF8Encoding(false);
        IsDirty = false;
    }

    public void MarkSaved(string? path, Encoding encoding)
    {
        FilePath = path;
        Encoding = encoding;
        IsDirty = false;
    }
}
