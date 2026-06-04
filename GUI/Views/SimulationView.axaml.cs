using Avalonia.Controls;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mapsui.Manipulations;
using Services;


namespace GUI.Views;

public partial class SimulationView : UserControl
{
    private Dictionary<string, GeometryFeature> _countryFeatures = new();
    private string? _selectedRegionCode;
    private readonly AppService _appService;


    public SimulationView(AppService appService)
    {
        InitializeComponent();
        _appService = appService;
        var mapControl = this.FindControl<Mapsui.UI.Avalonia.MapControl>("MapControl");
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "worldmap.geojson");
        var geoJson = File.ReadAllText(path);
        var reader = new GeoJsonReader();
        var featureCollection = reader.Read<NetTopologySuite.Features.FeatureCollection>(geoJson);
        var first = featureCollection.First();
        foreach (var attr in first.Attributes.GetNames())
            System.Diagnostics.Debug.WriteLine(attr);


        var features = new List<IFeature>();
        foreach (var feature in featureCollection)
        {
            var mapFeature = new GeometryFeature { Geometry = feature.Geometry };
            mapFeature.Styles.Add(new VectorStyle
            {
                Fill = new Brush(Color.FromArgb(255, 180, 180, 180)),
                Outline = new Pen(Color.Black, 1)
            });
            features.Add(mapFeature);

            var isoCode = feature.Attributes["ISO_A2"]?.ToString() ?? "";
            mapFeature["ISO_A2"] = isoCode;
            if (!string.IsNullOrEmpty(isoCode))
                _countryFeatures[isoCode] = mapFeature;
        }

        var layer = new MemoryLayer
        {
            Name = "Countries",
            Features = features,
            Style = null
        };

        mapControl.Map.Layers.Add(layer);
        mapControl.Tapped += OnMapTapped;

    }

    private void OnMapTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var mapControl = this.FindControl<Mapsui.UI.Avalonia.MapControl>("MapControl");
        var pos = e.GetPosition(mapControl);
        var screenPosition = new ScreenPosition(pos.X, pos.Y);
        var mapInfo = mapControl.GetMapInfo(screenPosition, mapControl.Map.Layers);
        if (mapInfo?.Feature is GeometryFeature feature)
        {
            var isoCode = feature["ISO_A2"]?.ToString();
            _selectedRegionCode = isoCode;
            System.Diagnostics.Debug.WriteLine($"Kliknuto: {isoCode}");
        }
    }
}
