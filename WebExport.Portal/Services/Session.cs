using Models.DTOs.Auth;
using Models.DTOs.TenantResponse;

public class Session
{
    public event Action? Changed;


    public TenantResponseDto? Tenant { get; set; }
    public AuthResponseDto? Current { get; private set; }
    public bool IsAuthenticated => Current is not null && !string.IsNullOrWhiteSpace(Current.AccessToken) && Current.ExpiresAt > DateTime.UtcNow;
    public string? ApiKey { get; private set; }
    public string ClientId { get; private set; } = "";
    public string Theme { get; set; } = "";

    public int Seconds { get; set; } = 0;


    // Set Tenant Credentials in Session
    public void SetCredentials(AuthResponseDto response, string apiKey, string clientId)
    {
        Current = response;
        ApiKey = apiKey;
        ClientId = clientId;
        Tenant = null;

        Changed?.Invoke();
    }


    // Set Tenant Data in Session 
    public void SetTenant(TenantResponseDto tenant)
    {
        Tenant = tenant;
        Changed?.Invoke();
    }


    public void SetTheme(string theme)
    {
        Theme = theme;
        Changed?.Invoke();
    }


    // Clear Session
    public void Clear()
    {
        Current = null;
        ApiKey = null;
        ClientId = "";
        Tenant = null;
        Seconds = 0;

        Changed?.Invoke();
    }
}