# LogSluice 🌊

A modern, cross-platform, high-performance tail and log viewer built for the 2020s.

## The "Why"

For nearly two decades, developers on Windows have relied on tools like BareTail to monitor live log files. BareTail is a legend—it's incredibly fast, lightweight, and gets the job done. However, it hasn't seen a meaningful update in 20 years. 

Modern development environments demand modern tools. We needed a log viewer that was completely cross-platform, supported modern high-DPI dark-mode interfaces, and could handle today's massive, gigabyte-scale application logs without locking up or eating all available RAM. 

**LogSluice** was built to carry that torch forward. It combines the blazing-fast, real-time tailing of legacy tools with a modern UI and true data virtualization.

## Key Features

* **Gigabyte-Scale Performance:** LogSluice doesn't load your files into RAM. It uses a custom virtualized byte-offset engine paired with UI virtualization. Whether your log is 10 KB or 10 GB, it opens instantly and uses a flat, minimal memory footprint.
* **Smart "Follow Tail":** Automatically scrolls as your application writes to the log. If you manually scroll up to inspect a stack trace, LogSluice is smart enough to temporarily pause auto-scrolling so you don't lose your place.
* **Advanced Highlighting:**
  * Define custom text to instantly colorize errors, warnings, or specific IDs.
  * Native color picker for building your highlight palette.
  * **Global Highlights:** Apply your favorite highlights to every file you open.
  * **Tab Highlights:** Apply temporary rules only to the current file you are debugging.
* **Real-Time Search:** A pinned search bar that instantly highlights matching terms across the entire document without slowing down the rendering engine.
* **Modern, Tabbed UI:** Open multiple logs side-by-side using drag-and-drop or the native file explorer. Adjust word wrap, font sizes, and styles on a per-tab basis.
* **Cross-Platform:** Built on Avalonia UI 11 and .NET, LogSluice runs flawlessly on Windows, macOS, and Linux. (Has only been tested on Windows...)

## Tech Stack

* **Framework:** .NET 10+
* **UI:** Avalonia UI 11 (Fluent Dark Theme)
* **Architecture:** MVVM (CommunityToolkit.Mvvm)

## Getting Started

To build from source:
1. Ensure you have the [.NET 10 SDK](https://dotnet.microsoft.com/download) installed.
2. Clone the repository.
3. Run `dotnet restore` and `dotnet run` from the project root.