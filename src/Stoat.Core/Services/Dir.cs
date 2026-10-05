namespace Stoat.Core.Services;

public static class StoatDir
{
    private static string? _cache;
    private static string? _local;
    private static string? _options;
    
    public static string Local
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_local))
                return _local;

            _local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StoatSharp");

            return _local;
        }
    }

    public static string Cache
    {
        get
        {
            if(!string.IsNullOrWhiteSpace(_cache))
                return _cache;
            
            _cache = Path.Combine(Path.GetTempPath(), "StoatSharp");
            
            return _cache;
        }
    }

    public static string Options => Path.Combine(Local, "Configuration");

    public static string StoatDirectory => AppContext.BaseDirectory;
}