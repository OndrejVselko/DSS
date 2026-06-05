using Avalonia.Controls;
using Services;

namespace GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void SetSimulationSize()
    {
        Width = 1600;
        Height = 900;
    }

    public void SetMenuSize()
    {
        Width = 1200;
        Height = 700;
    }
}
