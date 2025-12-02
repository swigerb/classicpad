# Contributing to ClassicPad

Thank you for helping improve ClassicPad! This document summarizes the workflow, expectations, and tooling used in this repository.

## Getting Started

1. **Set up tooling**
   - Install the .NET 8 SDK with Windows desktop workload.
   - Use Visual Studio 2022, VS Code, or Rider with WinForms tooling.
2. **Fork and clone** your GitHub repository.
3. **Create a branch** off `main` for each contribution (e.g., `feature/find-dialog-refactor`).

## Development Workflow

1. Run `dotnet build` before committing to make sure the WinForms app compiles.
2. Keep `ClassicPad/UI/MainWindow.*.cs` under 1,000 LOC per file; add new partials when expanding UI logic.
3. Prefer descriptive comments above complex blocks instead of inline narrations.
4. Use the existing namespace layout (`App`, `Dialogs`, `Printing`, `Services`, `UI`).
5. When adding assets (icons, manifests, etc.), include them under `ClassicPad/Assets` and wire them up in the `.csproj` file.

## Coding Standards

- Target C# latest features available in .NET 8 while keeping code readable for WinForms developers.
- Use expression-bodied members sparingly; clarity takes priority over brevity.
- Favor `Span`, `ReadOnlySpan`, or pooling only when profiling proves a benefit—this project prioritizes maintainability.
- Keep UI work on the UI thread; use `Invoke`/`BeginInvoke` only when introducing background tasks.

## Testing

- Desktop UI changes are primarily verified via manual smoke testing.
- Confirm major user flows: open/save, find/replace, word wrap toggling, status bar visibility, zoom, and printing.
- If you introduce reusable logic (e.g., services), consider adding unit tests under a new `ClassicPad.Tests` project.

## Commit & PR Guidelines

- Write conventional, present-tense commit messages (e.g., `Add print preview dialog`).
- Reference related GitHub issues in your PR description.
- Keep PRs focused; multiple unrelated changes should be split into separate branches.
- Update `README.md` when you add or modify user-visible behavior.

## Reporting Issues

When opening an issue, include:

- ClassicPad version or Git commit hash.
- Windows build number.
- Steps to reproduce and expected vs. actual results.
- Screenshots or crash logs when applicable.

Thanks again for contributing! 🎉
