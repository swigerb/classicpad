# ClassicPad

ClassicPad is a lightweight, classic Notepad-inspired text editor written in C# WinForms. It recreates the familiar Windows Notepad workflow—menus, dialogs, shortcuts, and printing—while targeting modern .NET runtimes. The project is intentionally split into focused source files so each component stays easy to navigate and maintain.

## Highlights

- 📝 Full Notepad-style menu surface (File, Edit, Format, View, Help) with working verbs and keyboard accelerators.
- 🔍 Modeless Find/Replace, Find Next/Previous, and Go To line navigation with wrap-around search.
- 📄 Drag-and-drop loading, multi-instance launch, rich clipboard integration, and automatic dirty tracking.
- 🔡 Word Wrap, custom fonts, and zoom controls (including Ctrl+Plus/Minus/0 shortcuts) plus optional status bar.
- 🖨️ Page setup, printing, and a bundled classic notepad-style application icon (`Assets/classicpad.ico`).
- 🛠️ Clean architecture using WinForms partial classes (`MainWindow.*`) and focused services (printing, dialogs, search).

## Requirements

- Windows 10 version 19041 or later.
- [.NET 8 SDK](https://dotnet.microsoft.com/download) (Desktop workload).
- Visual Studio 2022, VS Code, or any editor that can build .NET desktop projects.

## Building & Running

```powershell
# Clone your repo, then:
cd classicpad
dotnet build         # Produces bin/Debug/.../classicpad.exe
dotnet run           # Launches ClassicPad directly
```

The compiled binary lives at `ClassicPad/bin/<Configuration>/net8.0-windows10.0.19041.0/classicpad.exe`. Copy the entire folder (the EXE plus dependencies) to redistribute.

## Project Layout

```text
ClassicPad/
├── App/                # Application bootstrap and session state
├── Assets/             # classicpad.ico
├── Dialogs/            # Find, Replace, Go To modal forms
├── Printing/           # TextPrintDocument for paginated output
├── Services/           # Search contracts + Find/Replace orchestration
├── UI/
│   ├── MainWindow.Core.cs            # Shell, menus, status bar, drag/drop, shortcuts
│   ├── MainWindow.FileCommands.cs    # File/open/save/print logic
│   └── MainWindow.EditViewHelpCommands.cs # Edit/View/Help verbs and search helpers
└── classicpad.cs       # Entry point (STA)
```

Each file stays under ~500 LOC and includes sparse, high-signal comments when additional context helps future contributors.

## Keyboard & Menu Tips

| Command                 | Shortcut(s)                         |
|-------------------------|-------------------------------------|
| New / Open / Save       | `Ctrl+N`, `Ctrl+O`, `Ctrl+S`        |
| Find / Replace          | `Ctrl+F`, `Ctrl+H`                  |
| Find Next / Previous    | `F3`, `Shift+F3`                    |
| Go To Line              | `Ctrl+G` (disabled when Word Wrap)  |
| Time/Date stamp         | `F5`                                |
| Zoom In / Out / Reset   | `Ctrl++`, `Ctrl+-`, `Ctrl+0`        |
| Toggle Word Wrap        | Format → Word Wrap                  |
| Toggle Status Bar       | View → Status Bar (wrap off only)   |

## Tests

ClassicPad is a WinForms desktop application. The primary validation is `dotnet build` plus manual smoke testing of menu verbs. For automated UI testing, consider adding WinAppDriver or Playwright driver scripts in future contributions.

## Contributing

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for branching, coding-style, and testing guidance. Please review the [Code of Conduct](CODE_OF_CONDUCT.md) before participating.

## License

ClassicPad is licensed under the [MIT License](LICENSE).
