using Avalonia.Controls;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.Manipulations;
using NetTopologySuite.IO;
using Services;
using SimulationCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;


namespace GUI.Views;



public partial class SimulationView : UserControl
{
    private readonly AppService _appService;
    private readonly MainWindow _mainWindow;
    private readonly Random _random = new();
    private Dictionary<string, GeometryFeature> _countryFeatures = new();
    private string? _selectedRegionCode;
    private bool _isRunning = false;
    private ObservableCollection<DiseaseAbility> _activeAbilities = new();
    private ObservableCollection<DiseaseAbility> _availableAbilities = new();
    private ObservableCollection<string> _logItems = new();

    public SimulationView(AppService appService, MainWindow mainWindow)
    {
        _appService = appService;
        _mainWindow = mainWindow;
        InitializeComponent();

        InitializeMap();
        InitializeAbilities();
        UpdateDiseaseValues();

        _appService.OnDaySimulated += OnDaySimulated;

        this.FindControl<ListBox>("LogBox")!.ItemsSource = _logItems;
        this.FindControl<TextBlock>("DateText")!.Text = _appService.GetDate().ToString();

    }

    private void UpdateDiseaseValues()
    {
        this.FindControl<TextBlock>("DiseaseNameText")!.Text = _appService.GetDiseaseName();
        this.FindControl<TextBox>("SpreadingSpeedBox")!.Text = _appService.GetDiseaseDefaultSpeed().ToString();
        this.FindControl<TextBlock>("TotalSpreadingText")!.Text = _appService.GetDiseaseTotalSpeed().ToString();
        this.FindControl<TextBox>("DeathBox")!.Text = _appService.GetDiseaseDefaultDeath().ToString();
        this.FindControl<TextBlock>("TotalDeathText")!.Text = _appService.GetDiseaseTotalDeath().ToString();
    }

    private void InitializeAbilities()
    {
        foreach (var ability in _appService.GetAvailableDiseaseAbilities().Values)
            _availableAbilities.Add(ability);

        this.FindControl<ListBox>("ActiveDiseaseAbilitiesBox")!.ItemsSource = _activeAbilities;
        this.FindControl<ListBox>("AvailableDiseaseAbilitiesBox")!.ItemsSource = _availableAbilities;
    }

    private void InitializeMap()
    {
        var mapControl = this.FindControl<Mapsui.UI.Avalonia.MapControl>("MapControl")!;
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "worldmap.geojson");
        var geoJson = File.ReadAllText(path);
        var reader = new GeoJsonReader();
        var featureCollection = reader.Read<NetTopologySuite.Features.FeatureCollection>(geoJson);
        mapControl.Map.Widgets.Clear();
        var features = new List<IFeature>();
        foreach (var feature in featureCollection)
        {
            var isoCode = feature.Attributes["ISO_A2"]?.ToString() ?? "";
            var mapFeature = new GeometryFeature { Geometry = feature.Geometry };
            mapFeature["ISO_A2"] = isoCode;

            bool isInScenario = _appService.GetAllRegions().Values.Any(r => r.IsoCode == isoCode);
            mapFeature.Styles.Add(new VectorStyle
            {
                Fill = new Brush(isInScenario
                    ? Color.FromArgb(255, 180, 200, 180)
                    : Color.FromArgb(255, 220, 220, 220)),
                Outline = new Pen(Color.Black, 1)
            });

            features.Add(mapFeature);
            if (!string.IsNullOrEmpty(isoCode))
                _countryFeatures[isoCode] = mapFeature;
        }

        var layer = new MemoryLayer { Name = "Countries", Features = features, Style = null };
        mapControl.Map.Layers.Add(layer);
        mapControl.Tapped += OnMapTapped;
    }

    private void OnMapTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var mapControl = this.FindControl<Mapsui.UI.Avalonia.MapControl>("MapControl")!;
        var pos = e.GetPosition(mapControl);
        var screenPosition = new ScreenPosition(pos.X, pos.Y);
        var mapInfo = mapControl.GetMapInfo(screenPosition, mapControl.Map.Layers);

        if (mapInfo?.Feature is GeometryFeature feature)
        {
            _selectedRegionCode = feature["ISO_A2"]?.ToString();
            // TODO: zobrazit info ve spodním panelu
        }
    }

    private void OnDaySimulated(string message)
    {
        UpdateDiseaseValues();
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            this.FindControl<TextBlock>("DateText")!.Text = "2.5.2015";
            _logItems.Insert(0, message);
            if (_logItems.Count > 50)
                _logItems.RemoveAt(_logItems.Count - 1);
            // TODO: přebarvit mapu
        });
    }

    // --- Play/Pause ---
    private void OnPlayPauseClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isRunning)
        {
            _appService.StopSimulation();
            this.FindControl<Button>("PlayPauseButton")!.Content = "|>";
            _isRunning = false;
        }
        else
        {
            _appService.StartSimulation();
            this.FindControl<Button>("PlayPauseButton")!.Content = "||";
            _isRunning = true;
        }
    }

    // --- Rychlost šíření ---
    private void OnSpreadingSpeedConfirmed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var text = this.FindControl<TextBox>("SpreadingSpeedBox")!.Text ?? "";
        try { _appService.ChangeDefaultSpreadingSpeed(text); }
        catch { }
    }

    // --- Úmrtnost ---
    private void OnDeathConfirmed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var text = this.FindControl<TextBox>("DeathBox")!.Text ?? "";
        try { _appService.ChangeDeathProbability(text); }
        catch { }
    }

    // --- Ability ---
    private void OnDiseaseAbilitySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox?.SelectedItem is DiseaseAbility ability)
            this.FindControl<TextBlock>("DiseaseAbilityDescText")!.Text = ability.Description;
    }

    private void OnActiveDiseaseAbilityDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        => MoveFromActiveToAvailable();

    private void OnAvailableDiseaseAbilityDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        => MoveFromAvailableToActive();

    private void OnMoveDiseaseAbilityClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var activeBox = this.FindControl<ListBox>("ActiveDiseaseAbilitiesBox")!;
        var availableBox = this.FindControl<ListBox>("AvailableDiseaseAbilitiesBox")!;
        if (activeBox.SelectedItem is DiseaseAbility)
            MoveFromActiveToAvailable();
        else if (availableBox.SelectedItem is DiseaseAbility)
            MoveFromAvailableToActive();

    }

    private void MoveFromActiveToAvailable()
    {
        var box = this.FindControl<ListBox>("ActiveDiseaseAbilitiesBox")!;
        if (box.SelectedItem is DiseaseAbility ability)
        {
            _activeAbilities.Remove(ability);
            _availableAbilities.Add(ability);
            _appService.RemoveDiseaseAbilityFromDisease(ability.Id);
            UpdateDiseaseValues();
        }
    }

    private void MoveFromAvailableToActive()
    {
        var box = this.FindControl<ListBox>("AvailableDiseaseAbilitiesBox")!;
        if (box.SelectedItem is DiseaseAbility ability)
        {
            _availableAbilities.Remove(ability);
            _activeAbilities.Add(ability);
            _appService.AddDiseaseAbilityToDisease(ability.Id);
            UpdateDiseaseValues();
        }
    }

    // --- Vakcína ---
    private void OnVaccineConfirmed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var protText = this.FindControl<TextBox>("VaccineProtectionBox")!.Text ?? "";
        var deathText = this.FindControl<TextBox>("VaccineDeathBox")!.Text ?? "";
        if (double.TryParse(protText, out double prot) && double.TryParse(deathText, out double death))
            _appService.ChangeVaccineEfficiency(prot / 100.0, death / 100.0);
    }

    // --- Rychlost dne ---
    private void OnDaySpeedConfirmed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // TODO: přidat do AppService
    }

    // --- Ukončit ---
    private void OnStopSimulationClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _appService.StopSimulation();
        _mainWindow.SetMenuSize();
        _mainWindow.Content = new MainMenuView(_mainWindow);
    }
}