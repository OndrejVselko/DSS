using Avalonia.Controls;
using Avalonia.Input;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Manipulations;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.UI.Avalonia;
using NetTopologySuite.IO;
using Services;
using Shared;
using SimulationCore;
using System;
using System.Collections;
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
    private StatisticUpdate _lastUpdate;
    private Mapsui.UI.Avalonia.MapControl _mapControl = null!;
    private int _lastSick = 0;
    private int _lastDead = 0;
    private int _lastVaccinated = 0;
    private ObservableCollection<RegionAbility> _activeRegionAbilities = new();
    private ObservableCollection<RegionAbility> _availableRegionAbilities = new();
    private string? _lastLoadedRegionIso = null;




    public SimulationView(AppService appService, MainWindow mainWindow)
    {
        _appService = appService;
        _mainWindow = mainWindow;
        InitializeComponent();
        _mapControl = this.FindControl<Mapsui.UI.Avalonia.MapControl>("MapControl")!;

        InitializeMap();
        InitializeAbilities();
        UpdateDiseaseValues();
        InitializeVaccine();
        _appService.UpdateRegionsDiseaseValues();

        _mapControl.Map.Navigator.OverrideZoomBounds = new MMinMax(0.080, 0.4);
        //System.Diagnostics.Debug.WriteLine($"Resolution: {_mapControl.Map.Navigator.Viewport.Resolution}");


        var regions = _appService.GetAllRegions().Values
        .Where(r => !string.IsNullOrEmpty(r.IsoCode))
        .ToDictionary(r => r.IsoCode, r => r);

        _lastUpdate = new StatisticUpdate(DateOnly.MinValue, 0, 0, 0, 0, 0, 0, regions);


        _appService.OnDaySimulated += OnDaySimulated;
        _appService.OnLogAdded += OnLogAdded;


        this.FindControl<ListBox>("LogBox")!.ItemsSource = _logItems;
        this.FindControl<TextBlock>("DateText")!.Text = _appService.GetDate().ToString();

        this.FindControl<ListBox>("ActiveAbilitiesRegionBox")!.ItemsSource = _activeRegionAbilities;
        this.FindControl<ListBox>("AvailableAbilitiesRegionBox")!.ItemsSource = _availableRegionAbilities;

        _appService.UpdateAllRegions();
    }

    private void UpdateDiseaseValues()
    {
        this.FindControl<TextBlock>("DiseaseNameText")!.Text = _appService.GetDiseaseName();
        this.FindControl<TextBox>("SpreadingSpeedBox")!.Text = _appService.GetDiseaseDefaultSpeed().ToString();
        this.FindControl<TextBlock>("TotalSpreadingText")!.Text = "Celkem: " + _appService.GetDiseaseTotalSpeed().ToString("F2");
        this.FindControl<TextBox>("DeathBox")!.Text = (_appService.GetDiseaseDefaultDeath() * 100).ToString();
        this.FindControl<TextBlock>("TotalDeathText")!.Text = "Celkem: " + (_appService.GetDiseaseTotalDeath() * 100).ToString("F2");
    }

    private void UpdateDiseaseTotals()
    {
        this.FindControl<TextBlock>("TotalSpreadingText")!.Text = "Celkem: " + _appService.GetDiseaseTotalSpeed().ToString("F2");
        this.FindControl<TextBlock>("TotalDeathText")!.Text = "Celkem: " + (_appService.GetDiseaseTotalDeath() * 100).ToString("F2");
    }

    private void InitializeAbilities()
    {
        foreach (var ability in _appService.GetAvailableDiseaseAbilities().Values)
            _availableAbilities.Add(ability);

        this.FindControl<ListBox>("ActiveDiseaseAbilitiesBox")!.ItemsSource = _activeAbilities;
        this.FindControl<ListBox>("AvailableDiseaseAbilitiesBox")!.ItemsSource = _availableAbilities;
    }

    public void InitializeVaccine()
    {
        (double, double) values = _appService.GetVaccineParameters();
        this.FindControl<TextBox>("VaccineProtectionBox")!.Text += (values.Item1 * 100).ToString();
        this.FindControl<TextBox>("VaccineDeathBox")!.Text += (values.Item2 * 100).ToString();
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
            if (isoCode == "-99")
                isoCode = feature.Attributes["ISO_A2_EH"]?.ToString() ?? "";
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
    private void OnDaySimulated(StatisticUpdate update)
    {
        _lastUpdate = update;
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            this.FindControl<TextBlock>("DateText")!.Text = update.Date.ToString();
            if (_logItems.Count > 50)
                _logItems.RemoveAt(_logItems.Count - 1);
            UpdateMapColors(update.RegionsByIso);

            UpdateDiseaseTotals();

            var region = _selectedRegionCode == null ? null
                : update.RegionsByIso.GetValueOrDefault(_selectedRegionCode);
            UpdateBottomPanel(region);

            _lastSick = update.TotalSick;
            _lastDead = update.TotalDead;
            _lastVaccinated = update.TotalVaccinated;
        });
    }

    private void OnLogAdded(Log log)
    {
        _logItems.Insert(0, log.ToString());
    }

    // --- Play/Pause ---
    private void OnPlayPauseClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isRunning)
        {
            _appService.StopSimulation();
            this.FindControl<Button>("PlayPauseButton")!.Content = "▶️";
            _isRunning = false;
        }
        else
        {
            _appService.StartSimulation();
            this.FindControl<Button>("PlayPauseButton")!.Content = "⏸️";
            _isRunning = true;
        }
    }

    // --- Rychlost šíření ---
    private void OnSpreadingSpeedConfirmed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var text = this.FindControl<TextBox>("SpreadingSpeedBox")!.Text ?? "";
        this.FindControl<TextBlock>("TotalSpreadingText")!.Text = "Updating...";
        this.FindControl<TextBlock>("SpreadingLabel")!.Text = "Šíření: Updating...";
        try { _appService.ChangeDefaultSpreadingSpeed(text); }
        catch { }
    }

    // --- Úmrtnost ---
    private void OnDeathConfirmed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Function expects string, not double. Temporary solution :(
        var text = this.FindControl<TextBox>("DeathBox")!.Text ?? "";
        this.FindControl<TextBlock>("TotalDeathText")!.Text = "Updating...";
        this.FindControl<TextBlock>("DeathLabel")!.Text = "Úmrtnost: Updating...";
        if (double.TryParse(text, out var d))
        {
            try { _appService.ChangeDeathProbability((d / 100).ToString()); }
            catch { }
        }

    }

    // --- Ability ---
    private void OnDiseaseAbilitySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox?.SelectedItem is DiseaseAbility ability)
        {
            string text = $"{ability.Name}\n{ability.Description}\n\n" +
                          $"Modifikátor šíření: {ability.SpreadingModifier:F2}\n" +
                          $"Modifikátor úmrtí: {ability.DeathModifier:F2}";
            this.FindControl<TextBlock>("DiseaseAbilityDescText")!.Text = text;
        }
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
        this.FindControl<TextBlock>("TotalSpreadingText")!.Text = "Updating...";
        this.FindControl<TextBlock>("SpreadingLabel")!.Text = "Šíření: Updating...";
        this.FindControl<TextBlock>("DeathLabel")!.Text = "Úmrtnost: Updating...";
        this.FindControl<TextBlock>("TotalDeathText")!.Text = "Updating...";
        var box = this.FindControl<ListBox>("ActiveDiseaseAbilitiesBox")!;
        if (box.SelectedItem is DiseaseAbility ability)
        {
            _activeAbilities.Remove(ability);
            _availableAbilities.Add(ability);
            _appService.RemoveDiseaseAbilityFromDisease(ability.Id);
        }
    }

    private void MoveFromAvailableToActive()
    {
        this.FindControl<TextBlock>("TotalSpreadingText")!.Text = "Updating...";
        this.FindControl<TextBlock>("SpreadingLabel")!.Text = "Šíření: Updating...";
        this.FindControl<TextBlock>("TotalDeathText")!.Text = "Updating...";
        this.FindControl<TextBlock>("DeathLabel")!.Text = "Úmrtnost: Updating...";
        var box = this.FindControl<ListBox>("AvailableDiseaseAbilitiesBox")!;
        if (box.SelectedItem is DiseaseAbility ability)
        {
            _availableAbilities.Remove(ability);
            _activeAbilities.Add(ability);
            _appService.AddDiseaseAbilityToDisease(ability.Id);
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
        if (int.TryParse(this.FindControl<TextBox>("DaySpeedBox")!.Text, out int newSpeed))
            _appService.ChangeSimulationSpeed(newSpeed);
    }

    // --- Ukončit ---
    private async void OnStopSimulationClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _appService.StopSimulation();
        await _appService.SaveSimulationAsync(_appService.GetLogs());
        _mainWindow.SetMenuSize();
        _mainWindow.Content = new MainMenuView(_mainWindow);
    }


    private void UpdateMapColors(Dictionary<string, Region> regionsByIso)
    {
        var layer = _mapControl.Map.Layers.FirstOrDefault(l => l.Name == "Countries") as MemoryLayer;
        if (layer == null) return;

        foreach (var feature in layer.Features.OfType<GeometryFeature>())
        {
            var iso = feature["ISO_A2"]?.ToString() ?? "";
            if (regionsByIso.TryGetValue(iso, out Region region))
            {
                double sickRatio = region.Population > 0
                    ? Math.Clamp((double)region.Sick / region.Population, 0, 1)
                    : 0;
                double deadRatio = region.Population > 0
                    ? Math.Clamp((double)region.Dead / region.Population, 0, 1)
                    : 0;

                // Složka nemocných: zelená #00FF00 → červená #FF0000
                byte sickR = (byte)(sickRatio * 255);
                byte sickG = (byte)((1.0 - sickRatio) * 255);
                byte sickB = 0;

                // Složka mrtvých: černá #000000 → modrá #0000FF
                byte deadR = 0;
                byte deadG = 0;
                byte deadB = (byte)(deadRatio * 255);

                // Výsledná barva = mix obou složek (aditivní blend)
                // Zdraví (0 sick, 0 dead) = zelená; vše mrtvé = modrá; vše nemocné = červená
                // Základ je zelená pro zdravé regiony
                byte baseR = (byte)((1.0 - sickRatio) * 0 + sickRatio * 255);  // 0→255
                byte baseG = (byte)((1.0 - sickRatio) * 255);                   // 255→0
                byte baseB = 0;

                // Přimíchat modrou za mrtvé
                byte r = (byte)Math.Min(255, baseR * (1.0 - deadRatio) + deadR * deadRatio);
                byte g = (byte)Math.Min(255, baseG * (1.0 - deadRatio) + deadG * deadRatio);
                byte b = (byte)Math.Min(255, baseB * (1.0 - deadRatio) + deadB * deadRatio);

                var style = feature.Styles.OfType<VectorStyle>().FirstOrDefault();
                if (style != null)
                    style.Fill = new Brush(Color.FromArgb(255, r, g, b));
            }
        }
        _mapControl.Map.RefreshGraphics();
    }

    private void OnClearRegionClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _selectedRegionCode = null;
        UpdateBottomPanel(null);
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
            var region = _appService.GetAllRegions().Values
                .FirstOrDefault(r => r.IsoCode == _selectedRegionCode);
            UpdateBottomPanel(region);
        }
    }

    private void UpdateBottomPanel(Region? region)
    {
        var detailsPanel = this.FindControl<StackPanel>("RegionDetailsPanel")!;
        var vaccinatingCheck = this.FindControl<CheckBox>("VaccinatingCheckBox")!;
        var activePanel = this.FindControl<Grid>("ActiveAbilitiesRegionPanel")!;
        var moveButton = this.FindControl<Button>("MoveAbilityRegionButton")!;
        var availablePanel = this.FindControl<Grid>("AvailableAbilitiesRegionPanel")!;
        var descPanel = this.FindControl<Grid>("AbilityDescriptionPanel")!;

        if (region == null)
        {
            _lastLoadedRegionIso = null;
            detailsPanel.IsVisible = false;
            var allRegions = _appService.GetAllRegions().Values;
            this.FindControl<TextBlock>("RegionNameLabel")!.Text = "Celý svět";
            this.FindControl<TextBlock>("PopulationLabel")!.Text =
                $"Populace: {allRegions.Sum(r => r.Population):N0}";

            int sick = allRegions.Sum(r => r.Sick);
            int dead = allRegions.Sum(r => r.Dead);
            int vaccinated = allRegions.Sum(r => r.Vaccinated);

            SetStatLabel("SickLabel", sick);
            SetDeltaLabel("SickDeltaLabel", sick - _lastSick, false);
            SetStatLabel("DeadLabel", dead);
            SetDeltaLabel("DeadDeltaLabel", dead - _lastDead, true);
            SetStatLabel("VaccinatedLabel", vaccinated);
            SetDeltaLabel("VaccinatedDeltaLabel", vaccinated - _lastVaccinated, true);

            vaccinatingCheck.IsChecked = allRegions.All(r => r.Vaccinating);

            activePanel.IsVisible = false;
            moveButton.IsVisible = false;
            availablePanel.IsVisible = false;
            descPanel.IsVisible = false;
        }
        else
        {
            detailsPanel.IsVisible = true;
            this.FindControl<TextBlock>("RegionNameLabel")!.Text = region.Name;
            this.FindControl<TextBlock>("PopulationLabel")!.Text =
                $"Populace: {region.Population:N0}";

            SetStatLabel("SickLabel", region.Sick);
            SetDeltaLabel("SickDeltaLabel", _lastUpdate.RegionsByIso[region.IsoCode].LastUpdate.NewSick, false);
            SetStatLabel("DeadLabel", region.Dead);
            SetDeltaLabel("DeadDeltaLabel", _lastUpdate.RegionsByIso[region.IsoCode].LastUpdate.NewDead, true);
            SetStatLabel("VaccinatedLabel", region.Vaccinated);
            SetDeltaLabel("VaccinatedDeltaLabel", _lastUpdate.RegionsByIso[region.IsoCode].LastUpdate.NewVaccinated, true);

            this.FindControl<TextBlock>("SpreadingLabel")!.Text =
                $"Šíření: {region.RegionSpreadingSpeed:F2} / {region.TotalSpreadingSpeed:F2}";
            this.FindControl<TextBlock>("RandomOccurrenceLabel")!.Text =
                $"Náhodný výskyt: {(region.TotalRandomOccurrence * 100):F2} %";
            this.FindControl<TextBlock>("DeathLabel")!.Text =
                $"Úmrtnost: {(region.DiseaseDeathPropability * 100):F2} %/ {(region.TotalDeathProbability * 100):F2} %";

            var healthcareBox = this.FindControl<TextBox>("HealthcareIndexBox")!;
            if (!healthcareBox.IsFocused)
                healthcareBox.Text = region.HealthcareIndex.ToString();

            vaccinatingCheck.IsChecked = region.Vaccinating;

            // Načti abilities jen při změně regionu
            if (region.IsoCode != _lastLoadedRegionIso)
            {
                _lastLoadedRegionIso = region.IsoCode;
                _activeRegionAbilities.Clear();
                _availableRegionAbilities.Clear();

                foreach (var ability in region.Abilities)
                    _activeRegionAbilities.Add(ability);

                foreach (var ability in _appService.GetAvailableRegionAbilities().Values)
                    if (!region.Abilities.Contains(ability))
                        _availableRegionAbilities.Add(ability);
            }

            activePanel.IsVisible = true;
            moveButton.IsVisible = true;
            availablePanel.IsVisible = true;
            descPanel.IsVisible = true;
        }
    }

    private void SetStatLabel(string name, int value)
    {
        this.FindControl<TextBlock>(name)!.Text = value.ToString("N0");
    }

    private void SetDeltaLabel(string name, int delta, bool alwaysGray)
    {
        var label = this.FindControl<TextBlock>(name)!;
        label.Text = delta >= 0 ? $"(+{delta})" : $"({delta})";

        if (alwaysGray)
            label.Foreground = Avalonia.Media.Brushes.Gray;
        else
            label.Foreground = delta > 0
                ? Avalonia.Media.Brushes.Red
                : delta < 0
                    ? Avalonia.Media.Brushes.Green
                    : Avalonia.Media.Brushes.Gray;
    }

    private void OnVaccinatingChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var isChecked = this.FindControl<CheckBox>("VaccinatingCheckBox")!.IsChecked == true;

        if (_selectedRegionCode == null)
        {
            // celý svět
            if (isChecked) _appService.StartVaccinatingAllRegions();
            else _appService.StopVaccinatingAllRegions();
        }
        else
        {
            var region = _appService.GetAllRegions().Values
                .FirstOrDefault(r => r.IsoCode == _selectedRegionCode);
            if (region == null) return;
            if (isChecked) _appService.StartVaccinatingSingleRegion(region.Id);
            else _appService.StopVaccinatingSingleRegion(region.Id);
        }
    }

    private void OnHealthcareConfirmed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.FindControl<TextBlock>("SpreadingLabel")!.Text = "Šíření: Updating...";
        this.FindControl<TextBlock>("DeathLabel")!.Text = "Úmrtnost: Updating...";
        if (_selectedRegionCode == null) return;
        var region = _appService.GetAllRegions().Values
            .FirstOrDefault(r => r.IsoCode == _selectedRegionCode);
        if (region == null) return;

        var text = this.FindControl<TextBox>("HealthcareIndexBox")!.Text ?? "";
        if (double.TryParse(text, out double value))
            _appService.SetRegionHealthcareIndex(region.Id, text);
    }


    //----------------------------------------
    private void OnAbilitySelectionRegionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox?.SelectedItem is RegionAbility ability)
        {
            string text = ability.Name + "\n" + ability.Description; ;
            if (ability.SpreadingModifier != 1)
                text += "Modifikátor rychlosti šíření: " + ability.SpreadingModifier + "\n";
            if (ability.DeathModifier != 1)
                text += "Modifikátor úmrtnosti: " + ability.DeathModifier + "\n";
            if (ability.BorderModifier != 1)
                text += "Modifikátor náhodného výskytu: " + ability.BorderModifier + "\n";
            if (ability.VaccinationCapacityModifier != 1)
                text += "Modifikátor očkovací kapacity: " + ability.VaccinationCapacityModifier + "\n";

            text += "\n *Modifikátory násobí původní hodnoty";
            this.FindControl<TextBlock>("AbilityDescriptionText")!.Text = text;

        }
    }

    private void OnActiveAbilityRegionDoubleTapped(object? sender, TappedEventArgs e)
    {
        MoveFromActiveToAvailableRegion();
    }

    private void OnAvailableAbilityRegionDoubleTapped(object? sender, TappedEventArgs e)
    {
        MoveFromAvailableToActiveRegion();
    }

    private void OnMoveAbilityRegionClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var activeBox = this.FindControl<ListBox>("ActiveAbilitiesRegionBox")!;
        var availableBox = this.FindControl<ListBox>("AvailableAbilitiesRegionBox")!;

        if (activeBox.SelectedItem is RegionAbility)
            MoveFromActiveToAvailableRegion();
        else if (availableBox.SelectedItem is RegionAbility)
            MoveFromAvailableToActiveRegion();
    }


    private void MoveFromActiveToAvailableRegion()
    {
        this.FindControl<TextBlock>("SpreadingLabel")!.Text = "Šíření: Updating...";
        this.FindControl<TextBlock>("DeathLabel")!.Text = "Úmrtnost: Updating...";
        var region = _appService.GetAllRegions().Values
            .FirstOrDefault(r => r.IsoCode == _selectedRegionCode);
        if (region == null) return;
        var box = this.FindControl<ListBox>("ActiveAbilitiesRegionBox")!;
        if (box.SelectedItem is RegionAbility ability)
        {
            _activeRegionAbilities.Remove(ability);
            _availableRegionAbilities.Add(ability);
            _appService.RemoveRegionAbility(region.Id, ability);
        }
    }

    private void MoveFromAvailableToActiveRegion()
    {
        this.FindControl<TextBlock>("SpreadingLabel")!.Text = "Šíření: Updating...";
        this.FindControl<TextBlock>("DeathLabel")!.Text = "Úmrtnost: Updating...";
        var region = _appService.GetAllRegions().Values
            .FirstOrDefault(r => r.IsoCode == _selectedRegionCode);
        if (region == null) return;
        var box = this.FindControl<ListBox>("AvailableAbilitiesRegionBox")!;
        if (box.SelectedItem is RegionAbility ability)
        {
            _availableRegionAbilities.Remove(ability);
            _activeRegionAbilities.Add(ability);
            _appService.AddRegionAbility(region.Id, ability);
        }
    }

    private void OnDaySpeedTextChanged(object? sender, TextChangedEventArgs e)
    {
        var box = sender as TextBox;
        if (box == null) return;
        var text = box.Text ?? "";
        if (!string.IsNullOrEmpty(text) && !uint.TryParse(text, out _))
            box.Text = text[..^1];
    }

    private void OnPositiveDoubleTextChanged(object? sender, TextChangedEventArgs e)
    {
        var box = sender as TextBox;
        if (box == null) return;
        var text = box.Text ?? "";
        if (!string.IsNullOrEmpty(text) && (!double.TryParse(text, out double val) || val < 0))
            box.Text = text[..^1];
    }

    private void OnPercentDoubleTextChanged(object? sender, TextChangedEventArgs e)
    {
        var box = sender as TextBox;
        if (box == null) return;
        var text = box.Text ?? "";
        if (!string.IsNullOrEmpty(text) && (!double.TryParse(text, out double val) || val < 0 || val > 100))
            box.Text = text[..^1];
    }

    private void OnScreenshotClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var screenshotsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshots");
        Directory.CreateDirectory(screenshotsDir);

        var topLevel = TopLevel.GetTopLevel(this)!;
        var pixelSize = new Avalonia.PixelSize((int)topLevel.Width, (int)topLevel.Height);
        var size = new Avalonia.Size(topLevel.Width, topLevel.Height);
        var dpiVector = new Avalonia.Vector(96, 96);

        using var bitmap = new Avalonia.Media.Imaging.RenderTargetBitmap(pixelSize, dpiVector);
        topLevel.Measure(size);
        topLevel.Arrange(new Avalonia.Rect(size));
        bitmap.Render(topLevel);

        var path = Path.Combine(screenshotsDir, $"DSS_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
        bitmap.Save(path);
    }
}