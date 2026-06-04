using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using System.Linq;

namespace GUI;

public partial class LoadJsonStepView : UserControl
{
    private readonly NewSimulationView _parent;

    public LoadJsonStepView(NewSimulationView parent)
    {
        _parent = parent;
        InitializeComponent();
    }

    private async void OnBrowseClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Vyberte scénáø",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } }
            }
        });

        if (!files.Any()) return;

        var path = files[0].Path.LocalPath;
        this.FindControl<TextBox>("PathTextBox")!.Text = path;

        try
        {
            _parent.AppService.SetSimulation();
            await _parent.AppService.LoadData(path);
            this.FindControl<TextBlock>("StatusText")!.Text = "Scénáø naèten úspìšnì.";
            this.FindControl<TextBlock>("StatusText")!.Foreground = Avalonia.Media.Brushes.Green;
            _parent.ScenarioLoaded = true;
        }
        catch (System.Exception ex)
        {
            this.FindControl<TextBlock>("StatusText")!.Text = $"Chyba: {ex.Message}";
            _parent.ScenarioLoaded = false;
        }
    }
}