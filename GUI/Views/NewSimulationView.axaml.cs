using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GUI.Views;
using Services;

namespace GUI;

public partial class NewSimulationView : UserControl
{
    private readonly MainWindow _mainWindow;
    private int _currentStep = 0;
    public AppService AppService { get; } = new AppService();
    public bool ScenarioLoaded { get; set; } = false;


    // Zadané hodnoty
    private string? _scenarioPath;
    // další hodnoty přibydou

    public NewSimulationView(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        InitializeComponent();
        NavigateToStep(0);
    }

    private void NavigateToStep(int step)
    {
        _currentStep = step;
        var container = this.FindControl<ContentControl>("StepContainer");
        container.Content = step switch
        {
            0 => new LoadJsonStepView(this),
            1 => new CreateDiseaseStepView(this),
            2 => new ManageRegionsStepView(this),
            3 => new CreateVaccineStepView(this),
            _ => null
        };
    }

    private void OnBackClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_currentStep > 0)
            NavigateToStep(_currentStep - 1);
        else
            _mainWindow.Content = new MainMenuView(_mainWindow);
    }

    private void OnNextClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_currentStep < 3)
            NavigateToStep(_currentStep + 1);
        else
        {
            // spustit simulaci
        }
    }
}