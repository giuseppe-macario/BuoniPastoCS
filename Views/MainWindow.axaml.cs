using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using BuoniPastoGui.ViewModels;

namespace BuoniPastoGui.Views;

public partial class MainWindow : Window
{
    private readonly Thickness _normalThickness = new(2);
    private readonly Thickness _highlightThickness = new(3);

    private IBrush? _normalBorderBrush;
    private IBrush _highlightBrush = new SolidColorBrush(Colors.DodgerBlue);
    private readonly IBrush _invalidBrush = new SolidColorBrush(Colors.IndianRed);

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        _normalBorderBrush = DropZone.BorderBrush ?? new SolidColorBrush(Colors.Gray);
        TryLoadAccentBrush();

        DropZone.AddHandler(DragDrop.DragEnterEvent, DropZone_DragEnter);
        DropZone.AddHandler(DragDrop.DragLeaveEvent, DropZone_DragLeave);
        DropZone.AddHandler(DragDrop.DragOverEvent, DropZone_DragOver);
        DropZone.AddHandler(DragDrop.DropEvent, DropZone_Drop);

        SetDropZoneNormal();
    }

    private MainWindowViewModel VM => (MainWindowViewModel)DataContext!;

    private void TryLoadAccentBrush()
    {
        if (Application.Current?.Resources.TryGetValue("SystemAccentColor", out var accentObj) == true &&
            accentObj is Color accentColor)
        {
            _highlightBrush = new SolidColorBrush(accentColor);
        }
    }

    private async void SelectFile_Click(object? sender, RoutedEventArgs e)
        => await VM.SelectFileAsync(this);

    private async void Analyze_Click(object? sender, RoutedEventArgs e)
        => await VM.AnalizzaAsync();

    private void Clear_Click(object? sender, RoutedEventArgs e)
        => VM.Clear();

    private void SetDropZoneNormal()
    {
        DropZone.BorderThickness = _normalThickness;
        DropZone.BorderBrush = _normalBorderBrush;
        DropZone.Opacity = 1.0;
    }

    private void SetDropZoneHighlight()
    {
        DropZone.BorderThickness = _highlightThickness;
        DropZone.BorderBrush = _highlightBrush;
        DropZone.Opacity = 1.0;
    }

    private void SetDropZoneInvalid()
    {
        DropZone.BorderThickness = _highlightThickness;
        DropZone.BorderBrush = _invalidBrush;
        DropZone.Opacity = 1.0;
    }

    private static string? GetLocalPath(IStorageItem? item)
    {
        if (item is null) return null;

        // Su macOS spesso è affidabile usare item.Path.LocalPath
        var lp = item.Path.LocalPath;
        if (!string.IsNullOrWhiteSpace(lp))
            return lp;

        // Fallback: estensione TryGetLocalPath (se presente)
        return item.TryGetLocalPath();
    }

    private static bool IsPdfPath(string? path)
        => !string.IsNullOrWhiteSpace(path)
           && File.Exists(path)
           && string.Equals(Path.GetExtension(path), ".pdf", StringComparison.OrdinalIgnoreCase);

    private void DropZone_DragEnter(object? sender, DragEventArgs e)
    {
        // niente: lo decide DragOver
        e.Handled = true;
    }

    private void DropZone_DragLeave(object? sender, DragEventArgs e)
    {
        SetDropZoneNormal();
        e.Handled = true;
    }

    private void DropZone_DragOver(object? sender, DragEventArgs e)
    {
#pragma warning disable CS0618 // DragEventArgs.Data obsolete in newer Avalonia
        var items = e.Data.GetFiles();
#pragma warning restore CS0618

        var first = items?.FirstOrDefault();
        var path = GetLocalPath(first);

        if (IsPdfPath(path))
        {
            SetDropZoneHighlight();
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            SetDropZoneInvalid();
            VM.SetStatus("Formato non supportato");
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void DropZone_Drop(object? sender, DragEventArgs e)
    {
#pragma warning disable CS0618
        var items = e.Data.GetFiles();
#pragma warning restore CS0618

        var first = items?.FirstOrDefault();
        var path = GetLocalPath(first);

        if (IsPdfPath(path))
        {
            VM.SetSelectedFilePath(path!);
            SetDropZoneNormal();
        }
        else
        {
            SetDropZoneInvalid();
            VM.SetStatus("Formato non supportato");
        }

        e.Handled = true;
    }
}
