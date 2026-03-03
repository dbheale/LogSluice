using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogSluice.Models;
using LogSluice.Services;

namespace LogSluice.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private LogTabViewModel? _selectedTab;

    // UI State
    [ObservableProperty] private bool _hasTabs;

    public ObservableCollection<LogTabViewModel> Tabs { get; } = new();

    // Application Settings Exposed to UI
    [ObservableProperty] private ObservableCollection<string> _recentFiles;
    [ObservableProperty] private ObservableCollection<HighlightRule> _globalRules;
    [ObservableProperty] private bool _followTailDefault;
    [ObservableProperty] private bool _wrapTextDefault;

    // New rule binding properties for the global settings view
    [ObservableProperty] private string _newHighlightPattern = string.Empty;
    [ObservableProperty] private Color _newHighlightColor = Colors.Red;

    public AppSettings CurrentSettings { get; private set; }

    public MainViewModel()
    {
        CurrentSettings = SettingsService.Load();

        RecentFiles = new ObservableCollection<string>(CurrentSettings.RecentFiles);
        GlobalRules = new ObservableCollection<HighlightRule>(CurrentSettings.GlobalRules);
        FollowTailDefault = CurrentSettings.FollowTailDefault;
        WrapTextDefault = CurrentSettings.WrapTextDefault;

        // Require explicit click from Recent Files for cleaner startup
        UpdateTabState();
    }

    [RelayCommand]
    public void OpenFile(string path)
    {
        if (Tabs.Any(t => t.FilePath == path))
        {
            SelectedTab = Tabs.First(t => t.FilePath == path);
            return;
        }

        // Pass 'this' as the parent
        var newTab = new LogTabViewModel(path, this, FollowTailDefault, WrapTextDefault);
        Tabs.Add(newTab);
        SelectedTab = newTab;

        if (!CurrentSettings.RecentFiles.Contains(path))
        {
            CurrentSettings.RecentFiles.Add(path);
            RecentFiles.Add(path);
            SettingsService.Save(CurrentSettings);
        }
        UpdateTabState();
    }

    public void AddGlobalRuleFromTab(HighlightRule rule)
    {
        if (rule == null) return;
        var updatedRules = new ObservableCollection<HighlightRule>(GlobalRules) { rule };
        GlobalRules = updatedRules;

        CurrentSettings.GlobalRules = GlobalRules.ToList();
        SettingsService.Save(CurrentSettings);

        // Sync all open tabs
        foreach (var tab in Tabs) tab.SyncGlobalRules(GlobalRules);
    }

    [RelayCommand]
    public void CloseTab(LogTabViewModel tabToClose)
    {
        if (tabToClose == null) return;
        Tabs.Remove(tabToClose);
        tabToClose.Dispose();
        UpdateTabState();
    }

    [RelayCommand]
    public void ClearRecentFiles()
    {
        CurrentSettings.RecentFiles.Clear();
        RecentFiles.Clear();
        SettingsService.Save(CurrentSettings);
    }

    // --- Global Settings Commands ---

    [RelayCommand]
    public void AddGlobalHighlight()
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
            var updatedRules = new ObservableCollection<HighlightRule>(GlobalRules) { newRule };
            GlobalRules = updatedRules;

            // Sync to DB
            CurrentSettings.GlobalRules = GlobalRules.ToList();
            SettingsService.Save(CurrentSettings);

            // Push to active tabs
            foreach (var tab in Tabs) tab.SyncRules(GlobalRules);

            NewHighlightPattern = string.Empty;
        }
        catch { }
    }

    [RelayCommand]
    public void RemoveGlobalHighlight(HighlightRule ruleToRemove)
    {
        if (ruleToRemove == null) return;
        var updatedRules = new ObservableCollection<HighlightRule>(GlobalRules);
        updatedRules.Remove(ruleToRemove);
        GlobalRules = updatedRules;

        CurrentSettings.GlobalRules = GlobalRules.ToList();
        SettingsService.Save(CurrentSettings);

        foreach (var tab in Tabs) tab.SyncRules(GlobalRules);
    }

    // Save UI defaults
    partial void OnFollowTailDefaultChanged(bool value)
    {
        CurrentSettings.FollowTailDefault = value;
        SettingsService.Save(CurrentSettings);
    }

    partial void OnWrapTextDefaultChanged(bool value)
    {
        CurrentSettings.WrapTextDefault = value;
        SettingsService.Save(CurrentSettings);
    }

    private void UpdateTabState() => HasTabs = Tabs.Count > 0;
}