using Models.DTOs.Auth;
using Models.DTOs.TenantResponse;

namespace BlazorApp.Services;

public sealed class Session
{
    public event Action? Changed;


    // Set Tenant Credentials in Session
    public AuthResponseDto? Current { get; private set; }
    public bool IsAuthenticated => Current is not null && !string.IsNullOrWhiteSpace(Current.AccessToken) && Current.ExpiresAt > DateTime.UtcNow;
    public string? ApiKey { get; private set; }
    public string ClientId { get; private set; } = "";

    public void SetCredentials(AuthResponseDto response, string apiKey, string clientId)
    {
        Current = response;
        ApiKey = apiKey;
        ClientId = clientId;
        Tenant = null;
        Changed?.Invoke();
    }


    // Set Tenant Data in Session 
    public TenantResponseDto? Tenant { get; set; }

    public void SetTenant(TenantResponseDto tenant)
    {
        Tenant = tenant;
        Changed?.Invoke();
    }


    // Clear Session
    public void Clear()
    {
        Current = null;
        ApiKey = null;
        ClientId = "";
        Tenant = null;

        Changed?.Invoke();
    }
}