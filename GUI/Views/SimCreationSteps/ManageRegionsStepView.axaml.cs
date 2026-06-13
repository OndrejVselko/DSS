using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SimulationCore;
using System.Collections.ObjectModel;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using Shared;

namespace GUI;

public partial class ManageRegionsStepView : UserControl
{
    private readonly NewSimulationView _parent;
    private ObservableCollection<Region> _regions = new();
    private ObservableCollection<RegionAbility> _activeAbilities = new();
    private ObservableCollection<RegionAbility> _availableAbilities = new();
    private Region? _selectedRegion;
    private int _infectedRegions;

    public ManageRegionsStepView(NewSimulationView parent)
    {
        _parent = parent;
        InitializeComponent();

        foreach (var region in _parent.AppService.GetAllRegions().Values)
            _regions.Add(region);

        this.FindControl<ListBox>("RegionListBox")!.ItemsSource = _regions;
        this.FindControl<ListBox>("ActiveAbilitiesBox")!.ItemsSource = _activeAbilities;
        this.FindControl<ListBox>("AvailableAbilitiesBox")!.ItemsSource = _availableAbilities;
    }

    private void OnRegionSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SaveCurrentRegion();

        var listBox = sender as ListBox;
        if (listBox?.SelectedItem is not Region region) return;

        _selectedRegion = region;

        this.FindControl<TextBlock>("RegionNameText")!.Text = region.Name;
        this.FindControl<TextBox>("PopulationBox")!.Text = region.Population.ToString();
        this.FindControl<TextBox>("HealthcareBox")!.Text = region.HealthcareIndex.ToString();

        _activeAbilities.Clear();
        _availableAbilities.Clear();
        this.FindControl<TextBlock>("AbilityDescriptionText")!.Text = "";

        foreach (var ability in region.Abilities)
            _activeAbilities.Add(ability);

        foreach (var ability in _parent.AppService.GetAvailableRegionAbilities().Values)
            if (!region.Abilities.Contains(ability))
                _availableAbilities.Add(ability);

        this.FindControl<CheckBox>("InfectedCheckBox")!.IsChecked = region.Sick > 0;
    }

    private void SaveCurrentRegion()
    {
        if (_selectedRegion == null) return;

        if (int.TryParse(this.FindControl<TextBox>("PopulationBox")!.Text, out int population))
            _selectedRegion.Population = population;

        if (double.TryParse(this.FindControl<TextBox>("HealthcareBox")!.Text, out double healthcare))
            _selectedRegion.HealthcareIndex = healthcare;
    }

    private void OnInfectedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_selectedRegion == null) return;
        var infected = this.FindControl<CheckBox>("InfectedCheckBox")!.IsChecked == true;
        _selectedRegion.Sick = infected ? 1 : 0;
        ValidateNext();
    }

    public void ValidateNext()
    {
        bool anyInfected = _parent.AppService.GetAllRegions().Values.Any(r => r.Sick > 0);
        _parent.RegionsNextButton = anyInfected;
        _parent.SetNextEnabled(anyInfected);
    }

    private void OnAbilitySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox?.SelectedItem is RegionAbility ability)
        {
            string text = ability.Name + "\n" + ability.Description; ;
            if (ability.SpreadingModifier != 1)
                text += "Modifikátor rychlosti šíøení: " + ability.SpreadingModifier + "\n";
            if (ability.DeathModifier != 1)
                text += "Modifikátor úmrtnosti: " + ability.DeathModifier + "\n";
            if (ability.BorderModifier != 1)
                text += "Modifikátor náhodného výskytu: " + ability.BorderModifier + "\n";
            if (ability.VaccinationCapacityModifier != 1)
                text += "Modifikátor oèkovací kapacity: " + ability.VaccinationCapacityModifier + "\n";

            text += "\n *Modifikátory násobí pùvodní hodnoty";
            this.FindControl<TextBlock>("AbilityDescriptionText")!.Text = text;
                
        }
    }

    private void OnActiveAbilityDoubleTapped(object? sender, TappedEventArgs e)
    {
        MoveFromActiveToAvailable();
    }

    private void OnAvailableAbilityDoubleTapped(object? sender, TappedEventArgs e)
    {
        MoveFromAvailableToActive();
    }

    private void OnMoveAbilityClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var activeBox = this.FindControl<ListBox>("ActiveAbilitiesBox")!;
        var availableBox = this.FindControl<ListBox>("AvailableAbilitiesBox")!;

        if (activeBox.SelectedItem is RegionAbility)
            MoveFromActiveToAvailable();
        else if (availableBox.SelectedItem is RegionAbility)
            MoveFromAvailableToActive();
    }

    private void MoveFromActiveToAvailable()
    {
        if (_selectedRegion == null) return;
        var box = this.FindControl<ListBox>("ActiveAbilitiesBox")!;
        if (box.SelectedItem is RegionAbility ability)
        {
            _activeAbilities.Remove(ability);
            _availableAbilities.Add(ability);
            _selectedRegion.RemoveAbility(ability);
        }
    }

    private void MoveFromAvailableToActive()
    {
        if (_selectedRegion == null) return;
        var box = this.FindControl<ListBox>("AvailableAbilitiesBox")!;
        if (box.SelectedItem is RegionAbility ability)
        {
            _availableAbilities.Remove(ability);
            _activeAbilities.Add(ability);
            _selectedRegion.AddAbility(ability);
        }
    }

    private void OnIntTextChanged(object? sender, TextChangedEventArgs e)
    {
        var box = sender as TextBox;
        if (box == null) return;
        var text = box.Text ?? "";
        if (!string.IsNullOrEmpty(text) && (!int.TryParse(text, out int val) || val < 0))
            box.Text = text[..^1];
    }

    private void OnDoubleTextChanged(object? sender, TextChangedEventArgs e)
    {
        var box = sender as TextBox;
        if (box == null) return;
        var text = box.Text ?? "";
        if (!string.IsNullOrEmpty(text) && (!double.TryParse(text, out double val) || val < 0))
            box.Text = text[..^1];
    }

    public void SaveAll() => SaveCurrentRegion();
}