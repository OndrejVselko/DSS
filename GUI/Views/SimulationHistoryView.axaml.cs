using Avalonia.Controls;
using Data;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GUI.Views;

public partial class SimulationHistoryView : UserControl
{
    private readonly AppService _appService;
    private readonly MainWindow _mainWindow;
    private List<SimulationRecord> _simulations = new();

    public SimulationHistoryView(MainWindow mainWindow, AppService appService)
    {
        _appService = appService;
        _mainWindow = mainWindow;
        InitializeComponent();
        LoadSimulations();
    }

    private async void LoadSimulations()
    {
        _simulations = await _appService.GetAllSimulationsAsync();
        this.FindControl<ListBox>("SimulationListBox")!.ItemsSource =
            _simulations.Select(s => $"[{s.CreatedAt:dd.MM.yyyy HH:mm}] {s.DiseaseName}").ToList();
    }

    private async void OnSimulationSelected(object? sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox?.SelectedIndex < 0 || listBox?.SelectedIndex >= _simulations.Count) return;

        var sim = _simulations[listBox!.SelectedIndex];
        var detailPanel = this.FindControl<StackPanel>("DetailPanel")!;
        detailPanel.IsVisible = true;

        this.FindControl<TextBlock>("DetailTitle")!.Text = sim.DiseaseName;
        this.FindControl<TextBlock>("DetailDate")!.Text = $"Datum: {sim.CreatedAt:dd.MM.yyyy HH:mm}";
        this.FindControl<TextBlock>("DetailSpeed")!.Text = $"Rychlost šíøení: {sim.DefaultSpreadingSpeed:F2}";
        this.FindControl<TextBlock>("DetailDeath")!.Text = $"Úmrtnost: {sim.DefaultDeathProbability * 100:F2} %";
        this.FindControl<TextBlock>("DetailSickness")!.Text = $"Délka nemoci: {sim.SicknessLength} dní";
        this.FindControl<TextBlock>("DetailImmunity")!.Text = $"Délka imunity: {sim.ImmunityLength} dní";

        var logs = await _appService.GetLogsAsync(sim.Id);
        var sb = new StringBuilder();
        foreach (var log in logs)
            sb.AppendLine($"{log.Content}");

        this.FindControl<TextBox>("LogTextBox")!.Text = sb.ToString();
    }

    private void OnBackClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainWindow.Content = new MainMenuView(_mainWindow);
    }
}