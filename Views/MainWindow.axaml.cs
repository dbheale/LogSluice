using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using LogSluice.ViewModels;

namespace LogSluice.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        var files = e.DataTransfer.TryGetFiles();
        if (files == null) return;

        var vm = DataContext as MainViewModel;

        foreach (var file in files)
        {
            string? localPath = file.TryGetLocalPath();

            if (!string.IsNullOrEmpty(localPath))
            {
                vm?.OpenFile(localPath);
            }
        }
    }
    
    private async void OpenFileButton_Click(object? sender, RoutedEventArgs e)
    {
        // 1. Open the native OS file picker
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Log File",
            AllowMultiple = true // Let users select multiple logs at once!
        });

        // 2. If they picked files, pass them to our ViewModel
        if (files.Count >= 1)
        {
            var vm = DataContext as MainViewModel;
            
            foreach (var file in files)
            {
                string? localPath = file.TryGetLocalPath();
                if (!string.IsNullOrEmpty(localPath))
                {
                    vm?.OpenFile(localPath);
                }
            }
        }
    }
}