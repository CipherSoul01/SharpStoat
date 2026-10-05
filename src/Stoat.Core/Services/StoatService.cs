using System.Text.Json;
using Stoat.Api.Interfaces;
using Stoat.Api.Interfaces.Stoat;
using Stoat.Api.Model;
using Stoat.Core.Interfaces;

namespace Stoat.Core.Services;

public class StoatService : IStoatService
{
    private readonly IStoatServerConfiguration _configuration;
    
    private readonly IStoatAuth _auth;
    
    public IStoatServerConfiguration Configuration => _configuration;
    public IAuthService Auth { get; init; }
    
    public StoatService(IAuthService auth, IStoatAuth internalAuth, IStoatServerConfiguration configuration)
    {
        Auth = auth;
        _auth = internalAuth;
        _configuration = configuration;
    }

    public async Task<StoatResponse<bool>> SetBaseAddressAsync(Uri address)
    {
        var outAddress = _configuration.BaseAddress;
        
        _configuration.SetBaseAddress(address);
        
        var result = await _auth.GetConfig();

        if (!result.IsSuccessful || result.Content == null)
        {
            _configuration.SetBaseAddress(outAddress ?? new Uri("https://stoat.chat/"));

            return new StoatResponse<bool>([result.Error?.Message ?? "Unknown error"]);
        }
        
        _configuration.SetConfig(result.Content);
        
        if(!Directory.Exists(Path.Combine(StoatDir.Options)))
            Directory.CreateDirectory(Path.Combine(StoatDir.Options));
        
        File.WriteAllText(
            Path.Combine(StoatDir.Options, "server.json"), 
            JsonSerializer.Serialize(result.Content));
        
        return new StoatResponse<bool>(true);
    }

    public async Task<StoatResponse<bool>> SetupConfigAsync()
    {
        var serverConfig = Path.Combine(StoatDir.Options, "server.json");

        try
        {
            if(!File.Exists(serverConfig))
                throw new FileNotFoundException($"The configuration file {serverConfig} does not exist.");
            
            var config = JsonSerializer.Deserialize<StoatConfig>(File.ReadAllText(serverConfig));
            
            if(config == null)
                throw new NullReferenceException($"The configuration file {serverConfig} does not exist.");
            
            _configuration.SetBaseAddress(new Uri(config.App ?? "https://stoat.chat/"));
            _configuration.SetConfig(config);
            
            return new StoatResponse<bool>(true);
        }
        catch(Exception e)
        {
            await SetBaseAddressAsync(new Uri("https://stoat.chat/"));
            
            return new StoatResponse<bool>([ e.Message ]);
        }
    }
}