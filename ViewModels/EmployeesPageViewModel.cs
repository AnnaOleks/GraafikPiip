using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraafikPiip.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraafikPiip.ViewModels;

public partial class EmployeesPageViewModel : ObservableObject
{
    private readonly AppDatabase _db;
    public ObservableCollection<TootajaItem> Tootajad { get; } = new();

    [ObservableProperty] private bool isBusy;

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            isBusy = true;
            Tootajad.Clear();

            var list = await _db.GetTootajadAsync();
            foreach (var t in list)
            {
                var file = $"worker_{t.Id}.png";
                var exists = FileSystem.AppPackageFileExistsAsync($"Resources/Images/{file}");
                var photo = (await exists) ? file : "placeholder.png";

                Tootajad.Add(new TootajaItem
                {
                    Id = t.Id,
                    Name = t.Nimi,
                    Phone = t.Telefon,
                    Color = Color.FromArgb(string.IsNullOrWhiteSpace(t.Varv) ? "#FFFFFF" : t.Varv),
                    Photo = photo
                });
            }  
        }
        finally
        {
            isBusy = false;
        }
    }

    private Task OpenWorker(TootajaItem? item)
    {
        if (item is null) return Task.CompletedTask;
        // TODO: Shell.Current.GoToAsync(nameof(TootajaDetailsPage), new Dictionary<string, object>{{"id", item.Id}});
        return Task.CompletedTask;
    }

    public class TootajaItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public Color Color { get; set; } = Colors.White;
        public string Photo { get; set; } = "placeholder.png"; // файл из Resources/Images
    }
}
