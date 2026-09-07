using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.VisualTree;
using Stoat.Core.Image;
using Stoat.Core.Image.Pipeline;
using Stoat.Core.Interfaces;
using Stoat.Core.Interfaces.Leases;
using IImage = Stoat.Core.Interfaces.Decode.IImage;

namespace Stoat.Client.Control.Image;

public class AsyncImage : TemplatedControl
{
    public static readonly StyledProperty<string?> SourceProperty = AvaloniaProperty.Register<AsyncImage, string?>(
        nameof(Source));

    public static readonly StyledProperty<IAsyncImageLoader> LoaderProperty = AvaloniaProperty.Register<AsyncImage, IAsyncImageLoader>(
        nameof(Loader));
    
    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<AsyncImage, Stretch>(
        nameof(Stretch));

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty = AvaloniaProperty.Register<AsyncImage, StretchDirection>(
        nameof(StretchDirection));
    
    public static readonly DirectProperty<AsyncImage, bool> IsLoadingProperty = AvaloniaProperty.RegisterDirect<AsyncImage, bool>(
        nameof(IsLoading), o => o.IsLoading);
    
    public static readonly DirectProperty<AsyncImage, IImage> CurrentImageProperty = AvaloniaProperty.RegisterDirect<AsyncImage, IImage>(
        nameof(CurrentImage), o => o.CurrentImage);

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty = AvaloniaProperty.Register<AsyncImage, CornerRadius>(
        nameof(CornerRadius));

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    
    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    
    public IAsyncImageLoader Loader
    {
        get => GetValue(LoaderProperty);
        set => SetValue(LoaderProperty, value);
    }
    
    public bool IsLoading
    {
        get => _isLoading;
        set => SetAndRaise(IsLoadingProperty, ref _isLoading, value);
    }
    
    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }
    
    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }
    
    public IImage CurrentImage
    {
        get => _currentImage;
        set => SetAndRaise(CurrentImageProperty, ref _currentImage, value);
    }
    
    private readonly ParametrizedLogger? _logger;
    private readonly ImageRequestCoordinator _requestCoordinator = new ImageRequestCoordinator();
    private bool _isLoading;
    private IImage _currentImage;
    private bool _suppressSourceUpdate;
    private bool _settingCurrentImage;
    
    

    static AsyncImage()
    {
        AffectsRender<AsyncImage>(CurrentImageProperty, StretchProperty, StretchDirectionProperty,
            CornerRadiusProperty);
        
        AffectsMeasure<AsyncImage>(CurrentImageProperty, StretchProperty, StretchDirectionProperty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        if(Source is not null)
            InitializeImage();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        var avProp = change.Property;
        
        if(avProp == SourceProperty || avProp == LoaderProperty)
            InitializeImage();
        else if(avProp == CurrentImageProperty)
            ClearSourceIfUserProvideImage();
    }

    private void ClearSourceIfUserProvideImage() {
        if (!_settingCurrentImage && CurrentImage is not null) {
            _requestCoordinator.Cancel();
            if (Source is not null) {
                _suppressSourceUpdate = true;
                try {
                    Source = null;
                }
                finally {
                    _suppressSourceUpdate = false;
                }
            }
        }
    }
    
    private async void InitializeImage()
    {
        if (!this.IsAttachedToVisualTree())
        {
            Stop();
            return;
        }
        
        var request = _requestCoordinator.Begin();

        if (string.IsNullOrWhiteSpace(Source))
        {
            _requestCoordinator.Cancel();
            IsLoading = false;
            return;
        }

        var loader = Loader ?? StoatLoaders.AsyncImageLoader;
        
        IsLoading = true;
        SetCurrentImage(null);

        IImageLease? source = null;

        try
        {
            if (string.IsNullOrWhiteSpace(Source))
                return;

            source = await loader.LoadAsync(
                new ImageLoadRequest(Source, null,
                    TopLevel.GetTopLevel(this)?.StorageProvider),
                request.CancellationToken);
        }
        catch (OperationCanceledException) when (request.CancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger?.Log(this, "AsyncImage image resolution failed: {0}", ex);
        }
        finally
        {
            if (_requestCoordinator.TrySetLease(request, source))
            {
                SetCurrentImage(source?.Image);
                
                if(_requestCoordinator.TryComplete(request))
                    IsLoading = false;
            }
        }
    }

    private void Stop()
    {
        if(CurrentImage is not null)
            SetCurrentImage(null);
        
        _requestCoordinator.Cancel();
        IsLoading = false;
    }

    private void SetCurrentImage(IImage? image)
    {
        _settingCurrentImage = true;

        try
        {
            CurrentImage = image;
        }
        finally
        {
            _settingCurrentImage = false;
        }
    }
}