using System.Text;
using ClassicPad.App;
using Xunit;

namespace ClassicPad.Tests;

public sealed class DocumentSessionTests
{
    [Fact]
    public void Defaults_ToUntitledUtf8()
    {
        var session = new DocumentSession();

        Assert.Null(session.FilePath);
        Assert.False(session.IsDirty);
        Assert.Equal("Untitled", session.DisplayName);
        Assert.Equal("Untitled - ClassicPad", session.WindowCaption);
        Assert.Equal(Encoding.UTF8.WebName, session.Encoding.WebName);
    }

    [Fact]
    public void MarkDirty_FlagsSession()
    {
        var session = new DocumentSession();
        session.MarkDirty();
        Assert.True(session.IsDirty);
    }

    [Fact]
    public void MarkSaved_UpdatesState()
    {
        var session = new DocumentSession();
        var encoding = Encoding.Unicode;

        session.MarkSaved("C:/temp/test.txt", encoding);

        Assert.Equal("test.txt", session.DisplayName);
        Assert.False(session.IsDirty);
        Assert.Same(encoding, session.Encoding);
        Assert.Contains("test.txt", session.WindowCaption);
    }

    [Fact]
    public void Reset_ClearsState()
    {
        var session = new DocumentSession();
        session.MarkSaved("C:/temp/test.txt", Encoding.Unicode);
        session.MarkDirty();

        session.Reset();

        Assert.Null(session.FilePath);
        Assert.False(session.IsDirty);
        Assert.Equal("Untitled", session.DisplayName);
        Assert.Equal(Encoding.UTF8.WebName, session.Encoding.WebName);
    }
}
