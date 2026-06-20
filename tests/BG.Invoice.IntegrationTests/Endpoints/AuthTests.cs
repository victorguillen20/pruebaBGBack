using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BG.Invoice.Application.Dtos;
using BG.Invoice.IntegrationTests.Infrastructure;

namespace BG.Invoice.IntegrationTests.Endpoints;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static string DecodeJwtPayload(string token)
    {
        var parts = token.Split('.');
        var payload = parts[1];
        payload.Should().NotBeNullOrEmpty();
        var padding = payload.Length % 4;
        if (padding == 2)
            payload += "==";
        else if (padding == 3)
            payload += "=";
        var bytes = Convert.FromBase64String(payload);
        return Encoding.UTF8.GetString(bytes);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwtWithAdminRole()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        var request = new LoginRequest("admin", "Admin123!");
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(json, AuthHelper.JsonOptions);

        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.User.Role.Should().Be("Admin");
        loginResponse.User.UserName.Should().Be("admin");

        var payload = DecodeJwtPayload(loginResponse.Token);
        payload.Should().Contain("claims/role\":\"Admin\"");
        payload.Should().Contain("claims/name\":\"admin\"");
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        var request = new LoginRequest("admin", "wrongpassword");
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
