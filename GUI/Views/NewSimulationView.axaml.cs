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
    public AppService AppService { get; set; }
    public bool ScenarioLoaded { get; set; } = false;

    private CreateDiseaseStepView? _createDiseaseStep;
    private ManageRegionsStepView? _manageRegionsStep;
    private CreateVaccineStepView? _createVaccineStep;

    public bool DiseaseNextButton = false;
    public bool RegionsNextButton = false;

    // Zadané hodnoty
    private string? _scenarioPath;
    // další hodnoty přibydou

    public NewSimulationView(MainWindow mainWindow, AppService service)
    {
        this.AppService = service;
        _mainWindow = mainWindow;
        InitializeComponent();
        NavigateToStep(0);
    }

    private void NavigateToStep(int step)
    {
        _currentStep = step;
        this.FindControl<Button>("NextButton")!.IsEnabled = step switch
        {
            0 => ScenarioLoaded,
            1 => DiseaseNextButton,
            2 => RegionsNextButton,
            3 => _createVaccineStep?.IsValid() ?? false,
            _ => true
        };
        var container = this.FindControl<ContentControl>("StepContainer");
        container.Content = step switch
        {
            0 => new LoadJsonStepView(this),
            1 => _createDiseaseStep ??= new CreateDiseaseStepView(this),
            2 => _manageRegionsStep ??= new ManageRegionsStepView(this),
            3 => _createVaccineStep ??= new CreateVaccineStepView(this),
            _ => null
        };
    }

    public void SetNextEnabled(bool enabled)
    {
        this.FindControl<Button>("NextButton")!.IsEnabled = enabled;
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
        if (_currentStep == 1)
        {
            var step = _createDiseaseStep;
            if (step == null || !step.TrySaveDisease())
                return;
        }
        if (_currentStep == 3)
        {
            var step = _createVaccineStep;
            if (step == null || !step.TrySaveVaccine())
                return;
        }
        if (_currentStep < 3)
            NavigateToStep(_currentStep + 1);
        else
        {
            _manageRegionsStep?.SaveAll();
            AppService.SetStartDate();
            _mainWindow.SetSimulationSize();
            _mainWindow.Content = new SimulationView(AppService, _mainWindow);
        }
    }

    public void ResetSteps()
    {
        _createDiseaseStep = null;
        _manageRegionsStep = null;
        _createVaccineStep = null;
    }
}