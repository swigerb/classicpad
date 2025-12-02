using System;

namespace ClassicPad.Services;

internal enum SearchDirection
{
    Up,
    Down
}

internal class FindRequestedEventArgs : EventArgs
{
    public FindRequestedEventArgs(string searchText, bool matchCase, SearchDirection direction)
    {
        SearchText = searchText;
        MatchCase = matchCase;
        Direction = direction;
    }

    public string SearchText { get; }

    public bool MatchCase { get; }

    public SearchDirection Direction { get; }
}

internal enum ReplaceRequestKind
{
    FindNext,
    Replace,
    ReplaceAll
}

internal sealed class ReplaceRequestedEventArgs : FindRequestedEventArgs
{
    public ReplaceRequestedEventArgs(string searchText, bool matchCase, SearchDirection direction, string replacement, ReplaceRequestKind kind)
        : base(searchText, matchCase, direction)
    {
        ReplacementText = replacement;
        Kind = kind;
    }

    public string ReplacementText { get; }

    public ReplaceRequestKind Kind { get; }
}
