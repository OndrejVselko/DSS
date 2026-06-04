using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GUI.Views;
using Services;

namespace GUI;

public partial class MainMenuView : UserControl
{
    private readonly AppService _appService;
    private readonly MainWindow _mainWindow;

    public MainMenuView(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        InitializeComponent();
    }

    private void OnNewSimulationClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainWindow.Content = new NewSimulationView(_mainWindow);
    }

    private void OnOldSimulationsClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // TODO
    }

    private void OnExitClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainWindow.Close();
    }
}