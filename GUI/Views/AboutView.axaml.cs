using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GUI.Views;

namespace GUI;

public partial class AboutView : UserControl
{
    private readonly MainWindow _mainWindow;


    public AboutView(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
    }

    private void OnBackClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainWindow.Content = new MainMenuView(_mainWindow);
    }
}