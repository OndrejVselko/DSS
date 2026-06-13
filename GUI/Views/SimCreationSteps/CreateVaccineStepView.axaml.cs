using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Shared;

namespace GUI;

public partial class CreateVaccineStepView : UserControl
{
    private readonly NewSimulationView _parent;

    public CreateVaccineStepView(NewSimulationView parent)
    {
        _parent = parent;
        InitializeComponent();
    }

    public bool TrySaveVaccine()
    {
        var protectionText = this.FindControl<TextBox>("ProtectionBox")!.Text ?? "";
        var deathProtectionText = this.FindControl<TextBox>("DeathProtectionBox")!.Text ?? "";

        if (!double.TryParse(protectionText, out double protection) || protection < 0 || protection > 100)
            return false;
        if (!double.TryParse(deathProtectionText, out double deathProtection) || deathProtection < 0 || deathProtection > 100)
            return false;

        try
        {
            _parent.AppService.SetVaccine(protection / 100.0, deathProtection / 100.0);
            return true;
        }
        catch { return false; }
    }

    public bool IsValid()
    {
        var protectionText = this.FindControl<TextBox>("ProtectionBox")!.Text ?? "";
        var deathProtectionText = this.FindControl<TextBox>("DeathProtectionBox")!.Text ?? "";

        return double.TryParse(protectionText, out double protection) && protection >= 0 && protection <= 100
            && double.TryParse(deathProtectionText, out double deathProtection) && deathProtection >= 0 && deathProtection <= 100;
    }

    private void OnPercentDoubleTextChanged(object? sender, TextChangedEventArgs e)
    {
        var box = sender as TextBox;
        if (box == null) return;
        var text = box.Text ?? "";
        if (!string.IsNullOrEmpty(text) && (!double.TryParse(text, out double val) || val < 0 || val > 100))
            box.Text = text[..^1];
        else
            _parent.SetNextEnabled(IsValid());
    }
}