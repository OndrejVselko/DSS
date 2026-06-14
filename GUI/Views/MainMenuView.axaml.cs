using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GUI.Views;
using Services;

namespace GUI;

public partial class MainMenuView : UserControl
{
    private readonly AppService _appService = new AppService();
    private readonly MainWindow _mainWindow;

    public MainMenuView(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        InitializeComponent();
    }

    // BUTTON HANDLERS

    private void OnNewSimulationClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainWindow.Content = new NewSimulationView(_mainWindow, _appService);
    }

    private void OnOldSimulationsClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainWindow.Content = new SimulationHistoryView(_mainWindow, _appService);

    }

    private void OnAboutClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainWindow.Content = new AboutView(_mainWindow);
    }

    private void OnExitClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainWindow.Close();
    }
}