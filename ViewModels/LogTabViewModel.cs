using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogSluice.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace LogSluice.ViewModels;

public partial class LogTabViewModel : ObservableObject, IDisposable
{
    private readonly System.Timers.Timer _timer;
    private MainViewModel _parent;
    public ObservableCollection<FontFamily> AvailableFonts { get; } = new()
    {
        FontFamily.Parse("Cascadia Code"),
        FontFamily.Parse("Consolas"),
        FontFamily.Parse("Courier New"),
        FontFamily.Parse("Arial"),
        FontFamily.Parse("Segoe UI")
    };

    [ObservableProperty] private string _title;
    [ObservableProperty] private bool _followTail = true;
    [ObservableProperty] private bool _wrapText = false;
    [ObservableProperty] private double _fontSize = 12;
    [ObservableProperty] private FontFamily _fontFamily = FontFamily.Parse("Consolas");
    [ObservableProperty] private string _newHighlightPattern = string.Empty;
    [ObservableProperty] private Color _newHighlightColor = Colors.Red;
    [ObservableProperty] private bool _isGlobalRule = false; // The CheckBox state
    [ObservableProperty] private ObservableCollection<HighlightRule> _localRules = new();
    [ObservableProperty] private ObservableCollection<HighlightRule> _globalRules = new();
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isSidebarVisible = true;
    [ObservableProperty] private ObservableCollection<HighlightRule> _activeRules = new();

    public VirtualLogList Lines { get; }
    public string FilePath { get; }

    public LogTabViewModel(string filePath, MainViewModel parent, bool defaultFollowTail, bool defaultWrap)
    {
        _parent = parent;
        FilePath = filePath;
        Title = Path.GetFileName(filePath);
        FollowTail = defaultFollowTail;
        WrapText = defaultWrap;

        SyncGlobalRules(parent.GlobalRules);

        Lines = new VirtualLogList(filePath);

        _timer = new System.Timers.Timer(500);
        _timer.Elapsed += (s, e) => Lines.ScanForNewLines();
        _timer.Start();
    }

    public void SyncRules(ObservableCollection<HighlightRule> globalRules)
    {
        LocalRules = new ObservableCollection<HighlightRule>(globalRules);
    }

    public void SyncGlobalRules(ObservableCollection<HighlightRule> globalRules)
    {
        GlobalRules = globalRules;
        RefreshActiveRules();
    }

    partial void OnSearchTextChanged(string value)
    {
        RefreshActiveRules();
    }

    private void RefreshActiveRules()
    {
        var combined = new List<HighlightRule>();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            combined.Add(new HighlightRule
            {
                Pattern = SearchText,
                BackgroundColorName = "DarkOrange",
                ForegroundColorName = "White",
            });
        }

        combined.AddRange(GlobalRules);
        combined.AddRange(LocalRules);

        ActiveRules = new ObservableCollection<HighlightRule>(combined);
    }
    
    [RelayCommand]
    public void ToggleSidebar()
    {
        IsSidebarVisible = !IsSidebarVisible;
    }

    [RelayCommand]
    public void AddHighlight()
    {
        if (string.IsNullOrWhiteSpace(NewHighlightPattern)) return;
        try
        {
            var newRule = new HighlightRule
            {
                Pattern = NewHighlightPattern,
                BackgroundColorName = NewHighlightColor.ToString(),
                ForegroundColorName = "White"
            };

            if (IsGlobalRule)
            {
                _parent.AddGlobalRuleFromTab(newRule);
            }
            else
            {
                var updated = new ObservableCollection<HighlightRule>(LocalRules) { newRule };
                LocalRules = updated;
                RefreshActiveRules();
            }

            NewHighlightPattern = string.Empty;
        }
        catch { }
    }

    [RelayCommand]
    public void RemoveLocalHighlight(HighlightRule rule)
    {
        if (rule == null) return;
        var updated = new ObservableCollection<HighlightRule>(LocalRules);
        updated.Remove(rule);
        LocalRules = updated;
        RefreshActiveRules();
    }

    [RelayCommand]
    public void RemoveGlobalHighlightCommand(HighlightRule rule)
    {
        if (rule == null) return;
        _parent.RemoveGlobalHighlightCommand.Execute(rule);
        SyncGlobalRules(_parent.GlobalRules);
    }

    public void Dispose()
    {
        _timer.Stop();
        Lines.Dispose();
    }
}