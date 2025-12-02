using System;
using System.Windows.Forms;
using ClassicPad.Dialogs;

namespace ClassicPad.Services;

internal sealed class FindReplaceService : IDisposable
{
    private readonly Func<string> selectionProvider;
    private readonly Func<FindRequestedEventArgs, bool> findHandler;
    private readonly Func<ReplaceRequestedEventArgs, bool> replaceHandler;
    private readonly Func<ReplaceRequestedEventArgs, int> replaceAllHandler;
    private readonly Action<string> notFoundCallback;
    private readonly Action<int> replaceAllReport;

    private FindDialog? findDialog;
    private ReplaceDialog? replaceDialog;
    private string searchText = string.Empty;
    private string replacementText = string.Empty;
    private bool matchCase;
    private SearchDirection direction = SearchDirection.Down;

    public FindReplaceService(
        Func<string> selectionProvider,
        Func<FindRequestedEventArgs, bool> findHandler,
        Func<ReplaceRequestedEventArgs, bool> replaceHandler,
        Func<ReplaceRequestedEventArgs, int> replaceAllHandler,
        Action<string> notFoundCallback,
        Action<int> replaceAllReport)
    {
        this.selectionProvider = selectionProvider;
        this.findHandler = findHandler;
        this.replaceHandler = replaceHandler;
        this.replaceAllHandler = replaceAllHandler;
        this.notFoundCallback = notFoundCallback;
        this.replaceAllReport = replaceAllReport;
    }

    public bool HasActiveSearch => !string.IsNullOrWhiteSpace(searchText);

    public void ShowFindDialog(IWin32Window owner)
    {
        EnsureFindDialog();
        PopulateFindDialogDefaults();
        findDialog!.Show(owner);
        findDialog.FocusSearchBox();
    }

    public void ShowReplaceDialog(IWin32Window owner)
    {
        EnsureReplaceDialog();
        PopulateReplaceDialogDefaults();
        replaceDialog!.Show(owner);
        replaceDialog.FocusFindBox();
    }

    public bool FindNext(IWin32Window owner, SearchDirection? overrideDirection = null)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            ShowFindDialog(owner);
            return false;
        }

        if (overrideDirection.HasValue)
        {
            direction = overrideDirection.Value;
        }

        var args = new FindRequestedEventArgs(searchText, matchCase, overrideDirection ?? direction);
        if (!findHandler(args))
        {
            notFoundCallback(searchText);
            return false;
        }

        return true;
    }

    public void Reset()
    {
        searchText = string.Empty;
        replacementText = string.Empty;
        matchCase = false;
        direction = SearchDirection.Down;
    }

    public void Dispose()
    {
        findDialog?.Dispose();
        replaceDialog?.Dispose();
    }

    private void EnsureFindDialog()
    {
        if (findDialog is not null && !findDialog.IsDisposed)
        {
            findDialog.BringToFront();
            return;
        }

        findDialog = new FindDialog();
        findDialog.FindNextRequested += (_, args) => HandleFind(args);
        findDialog.FormClosed += (_, _) => findDialog = null;
    }

    private void EnsureReplaceDialog()
    {
        if (replaceDialog is not null && !replaceDialog.IsDisposed)
        {
            replaceDialog.BringToFront();
            return;
        }

        replaceDialog = new ReplaceDialog();
        replaceDialog.ReplaceRequested += (_, args) => HandleReplace(args);
        replaceDialog.FormClosed += (_, _) => replaceDialog = null;
    }

    private void PopulateFindDialogDefaults()
    {
        if (findDialog is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(searchText))
        {
            searchText = selectionProvider();
        }

        findDialog.SearchText = searchText;
        findDialog.MatchCase = matchCase;
        findDialog.Direction = direction;
    }

    private void PopulateReplaceDialogDefaults()
    {
        if (replaceDialog is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(searchText))
        {
            searchText = selectionProvider();
        }

        replaceDialog.SearchText = searchText;
        replaceDialog.ReplacementText = replacementText;
        replaceDialog.MatchCase = matchCase;
        replaceDialog.Direction = direction;
    }

    private void HandleFind(FindRequestedEventArgs args)
    {
        UpdateState(args.SearchText, args.MatchCase, args.Direction);

        if (!findHandler(args))
        {
            notFoundCallback(args.SearchText);
        }
    }

    private void HandleReplace(ReplaceRequestedEventArgs args)
    {
        UpdateState(args.SearchText, args.MatchCase, args.Direction);
        replacementText = args.ReplacementText;

        switch (args.Kind)
        {
            case ReplaceRequestKind.FindNext:
                if (!findHandler(args))
                {
                    notFoundCallback(args.SearchText);
                }
                break;
            case ReplaceRequestKind.Replace:
                if (!replaceHandler(args))
                {
                    notFoundCallback(args.SearchText);
                }
                break;
            case ReplaceRequestKind.ReplaceAll:
                var replacements = replaceAllHandler(args);
                replaceAllReport(replacements);
                break;
        }
    }

    private void UpdateState(string search, bool matchCaseOption, SearchDirection searchDirection)
    {
        searchText = search;
        matchCase = matchCaseOption;
        direction = searchDirection;
    }
}
