using Avalonia.Controls;
using Avalonia.Input;
using SimulationCore;
using System.Collections.ObjectModel;

namespace GUI;

public partial class CreateDiseaseStepView : UserControl
{
    private readonly NewSimulationView _parent;
    private ObservableCollection<DiseaseAbility> _activeAbilities = new();
    private ObservableCollection<DiseaseAbility> _availableAbilities = new();

    public CreateDiseaseStepView(NewSimulationView parent)
    {
        _parent = parent;
        InitializeComponent();

        foreach (var ability in _parent.AppService.GetAvailableDiseaseAbilities().Values)
            _availableAbilities.Add(ability);

        this.FindControl<ListBox>("ActiveAbilitiesBox")!.ItemsSource = _activeAbilities;
        this.FindControl<ListBox>("AvailableAbilitiesBox")!.ItemsSource = _availableAbilities;
    }

    private void ValidateAndSetNext()
    {
        var name = this.FindControl<TextBox>("NameBox")!.Text ?? "";
        var lengthText = this.FindControl<TextBox>("LengthBox")!.Text ?? "";
        var speedText = this.FindControl<TextBox>("SpeedBox")!.Text ?? "";
        var deathText = this.FindControl<TextBox>("DeathBox")!.Text ?? "";

        bool valid = !string.IsNullOrWhiteSpace(name)
            && int.TryParse(lengthText, out int length) && length >= 1
            && double.TryParse(speedText, out double speed) && speed > 0
            && double.TryParse(deathText, out double death) && death >= 0 && death <= 100;

        _parent.SetNextEnabled(valid);
        _parent.DiseaseNextButton = valid;
    }

    private void OnAbilitySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox?.SelectedItem is DiseaseAbility ability)
        {
            string text = ability.Name + "\n" + ability.Description + "\n";
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
        => MoveFromActiveToAvailable();

    private void OnAvailableAbilityDoubleTapped(object? sender, TappedEventArgs e)
        => MoveFromAvailableToActive();

    private void OnMoveAbilityClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var activeBox = this.FindControl<ListBox>("ActiveAbilitiesBox")!;
        var availableBox = this.FindControl<ListBox>("AvailableAbilitiesBox")!;
        if (activeBox.SelectedItem is DiseaseAbility)
            MoveFromActiveToAvailable();
        else if (availableBox.SelectedItem is DiseaseAbility)
            MoveFromAvailableToActive();
    }

    private void MoveFromActiveToAvailable()
    {
        var box = this.FindControl<ListBox>("ActiveAbilitiesBox")!;
        if (box.SelectedItem is DiseaseAbility ability)
        {
            _activeAbilities.Remove(ability);
            _availableAbilities.Add(ability);
            _parent.AppService.RemoveDiseaseAbilityFromDisease(ability.Id);
        }
    }

    private void MoveFromAvailableToActive()
    {
        var box = this.FindControl<ListBox>("AvailableAbilitiesBox")!;
        if (box.SelectedItem is DiseaseAbility ability)
        {
            _availableAbilities.Remove(ability);
            _activeAbilities.Add(ability);
            _parent.AppService.AddDiseaseAbilityToDisease(ability.Id);
        }
    }

    private void OnIntTextChanged(object? sender, TextChangedEventArgs e)
    {
        var box = sender as TextBox;
        if (box == null) return;
        var text = box.Text ?? "";
        if (!string.IsNullOrEmpty(text) && !int.TryParse(text, out _))
            box.Text = text[..^1];
        else
            ValidateAndSetNext();
    }

    private void OnDoubleTextChanged(object? sender, TextChangedEventArgs e)
    {
        var box = sender as TextBox;
        if (box == null) return;
        var text = box.Text ?? "";
        if (!string.IsNullOrEmpty(text) && !double.TryParse(text, out _))
            box.Text = text[..^1];
        else
            ValidateAndSetNext();
    }

    private void OnNameTextChanged(object? sender, TextChangedEventArgs e)
    {
        ValidateAndSetNext();
    }

    public bool TrySaveDisease()
    {
        var name = this.FindControl<TextBox>("NameBox")!.Text ?? "";
        var lengthText = this.FindControl<TextBox>("LengthBox")!.Text ?? "";
        var speedText = this.FindControl<TextBox>("SpeedBox")!.Text ?? "";
        var deathText = this.FindControl<TextBox>("DeathBox")!.Text ?? "";

        if (!int.TryParse(lengthText, out int length) || length < 1)
        {
            this.FindControl<TextBlock>("ErrorText")!.Text = "Délka onemocnìní musí být kladné celé èíslo.";
            return false;
        }
        if (!double.TryParse(speedText, out double speed) || speed <= 0)
        {
            this.FindControl<TextBlock>("ErrorText")!.Text = "Rychlost šíøení musí být vìtší než 0.";
            return false;
        }
        if (!double.TryParse(deathText, out double death) || death < 0 || death > 100)
        {
            this.FindControl<TextBlock>("ErrorText")!.Text = "Úmrtnost musí být v rozsahu 0-100.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            this.FindControl<TextBlock>("ErrorText")!.Text = "Název nemoci nesmí být prázdný.";
            return false;
        }

        this.FindControl<TextBlock>("ErrorText")!.Text = "";
        _parent.AppService.SetDisease(name, speed, death / 100.0, length);
        return true;
    }
}