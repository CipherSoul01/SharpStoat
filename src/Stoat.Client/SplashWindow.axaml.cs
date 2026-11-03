using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Splat;
using Stoat.Core.Interfaces;

namespace Stoat.Client;

public partial class SplashWindow : Window
{
    private readonly IStoatService _stoat;

    private readonly ScaleTransform _scale = new();
    private readonly TranslateTransform _translation = new();
    private readonly RotateTransform _bodyRotation = new();

    private readonly DispatcherTimer _animationTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(16)
    };

    private readonly Random _random = new();

    private double _phase;
    private double _bodyMotion;
    private double _targetBodyMotion;
    private double _nextBodyMotion;

    public SplashWindow()
    {
        _stoat = AppLocator.Current.GetService<IStoatService>()!;

        InitializeComponent();

        var bodyTransform = new TransformGroup
        {
            Children =
            {
                _scale,
                _bodyRotation,
                _translation
            }
        };

        Logo.RenderTransformOrigin = RelativePoint.Center;
        Logo.RenderTransform = bodyTransform;

        HalloweenDecoration.RenderTransformOrigin = RelativePoint.Center;
        HalloweenDecoration.RenderTransform = bodyTransform;

        _nextBodyMotion = 3;

        _animationTimer.Tick += Animate;

        Opened += (_, _) => _animationTimer.Start();
        Closed += (_, _) => _animationTimer.Stop();
    }

    public async Task InitializeServicesAsync()
    {
        await _stoat.SetupConfigAsync();
        await Task.Delay(5000);
    }

    private void Animate(object? sender, EventArgs e)
    {
        const double deltaTime = 0.016;

        _phase += deltaTime;

        var breathing = Math.Sin(_phase * 1.5);

        var scale = 1.0 + breathing * 0.035;

        _scale.ScaleX = scale;
        _scale.ScaleY = scale;

        _translation.Y = breathing * -3;

        if (_phase >= _nextBodyMotion)
        {
            _targetBodyMotion = _random.Next(2) == 0
                ? _random.NextDouble() * -1
                : _random.NextDouble();

            _nextBodyMotion = _phase + _random.NextDouble() * 2 + 1;
        }

        _bodyMotion += (_targetBodyMotion - _bodyMotion) * 0.035;

        _bodyRotation.Angle = _bodyMotion * 6;
        _translation.X = _bodyMotion * 4;

        if (_phase + 0.5 < _nextBodyMotion)
            _targetBodyMotion *= 0.97;
    }
}