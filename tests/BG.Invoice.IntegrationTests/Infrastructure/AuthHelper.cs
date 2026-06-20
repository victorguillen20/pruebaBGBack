using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BG.Invoice.Application.Dtos;

namespace BG.Invoice.IntegrationTests.Infrastructure;

public static class AuthHelper
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<string> LoginAsAsync(HttpClient client, string userName, string password)
    {
        var loginRequest = new LoginRequest(userName, password);
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(json, JsonOptions);
        return loginResponse!.Token;
    }

    public static async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        return await LoginAsAsync(client, "admin", "Admin123!");
    }

    public static async Task<string> GetVendorTokenAsync(HttpClient client)
    {
        return await LoginAsAsync(client, "vendor1", "Admin123!");
    }

    public static HttpClient WithToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static HttpClient WithoutToken(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }
}
