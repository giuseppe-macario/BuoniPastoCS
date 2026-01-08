using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using BuoniPastoGui.Models;
using BuoniPastoGui.Services;

namespace BuoniPastoGui.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly PdfBuoniService _service = new();

    private string? _selectedFilePath;
    public string? SelectedFilePath
    {
        get => _selectedFilePath;
        private set { _selectedFilePath = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> Files { get; } = new();
    public ObservableCollection<BuonoResult> Risultati { get; } = new();

    private int _totPranzo;
    public int TotPranzo
    {
        get => _totPranzo;
        private set { _totPranzo = value; OnPropertyChanged(); }
    }

    private int _totCena;
    public int TotCena
    {
        get => _totCena;
        private set { _totCena = value; OnPropertyChanged(); }
    }

    private string _status = "Seleziona un PDF.";
    public string Status
    {
        get => _status;
        private set { _status = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void SetStatus(string message)
    {
        Status = message ?? "";
    }

    // Usato sia dal file picker che dal drag&drop
    public void SetSelectedFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        SelectedFilePath = path;

        Files.Clear();
        Files.Add(path);

        Risultati.Clear();
        TotPranzo = 0;
        TotCena = 0;

        Status = "PDF selezionato. Premi “Analizza”.";
    }

    public async Task SelectFileAsync(Window window)
    {
        var sp = window.StorageProvider;

        var picked = await sp.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Seleziona PDF",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } }
            }
        });

        var file = picked.FirstOrDefault();
        var path = file?.TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(path))
        {
            Status = "Nessun file selezionato.";
            return;
        }

        SetSelectedFilePath(path);
    }

    public void Clear()
    {
        SelectedFilePath = null;
        Files.Clear();
        Risultati.Clear();
        TotPranzo = 0;
        TotCena = 0;
        Status = "Seleziona un PDF.";
    }

    public async Task AnalizzaAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedFilePath))
        {
            Status = "Seleziona un PDF.";
            return;
        }

        Status = "Analisi in corso...";
        Risultati.Clear();

        List<BuonoResult> all = await Task.Run(() =>
            _service.AnalizzaFile(SelectedFilePath)
        );

        foreach (var r in all
                     .OrderBy(r => r.Data)
                     .ThenBy(r => r.Tipo))
        {
            Risultati.Add(r);
        }

        TotPranzo = Risultati.Count(r => r.Tipo == TipoBuono.Pranzo);
        TotCena   = Risultati.Count(r => r.Tipo == TipoBuono.Cena);

        Status = "Fatto.";
    }
}
