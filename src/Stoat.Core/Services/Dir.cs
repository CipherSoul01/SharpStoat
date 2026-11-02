namespace Stoat.Core.Services;

public static class StoatDir
{
    private static string? _cache;
    private static string? _local;
    private static string? _options;
    private const string _name = "Stoat.Client";
    
    public static string Local
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_local))
                return _local;

            _local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _name);

            return _local;
        }
    }

    public static string Cache
    {
        get
        {
            if(!string.IsNullOrWhiteSpace(_cache))
                return _cache;

            if (OperatingSystem.IsLinux())
            {
                _cache = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".cache",
                    _name);

                return _cache;
            }
            
            _cache = Path.Combine(Path.GetTempPath(), _name);
            
            return _cache;
        }
    }

    public static string Options => Path.Combine(Local, "configuration");

    public static string StoatDirectory => AppContext.BaseDirectory;
}