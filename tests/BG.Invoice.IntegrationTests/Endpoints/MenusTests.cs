using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using BG.Invoice.IntegrationTests.Infrastructure;

namespace BG.Invoice.IntegrationTests.Endpoints;

public class MenusTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MenusTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task<string> LoginAndGetToken(HttpClient client, string userName)
    {
        return await AuthHelper.LoginAsAsync(client, userName, "Admin123!");
    }

    [Fact]
    public async Task Menus_Admin_Returns6Menus()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        var token = await LoginAndGetToken(client, "admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/me/menus");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var menus = JsonSerializer.Deserialize<JsonElement[]>(json);

        menus.Should().HaveCount(6);
    }

    [Fact]
    public async Task Menus_Vendor_Returns4Menus()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        var token = await LoginAndGetToken(client, "vendor1");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/me/menus");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var menus = JsonSerializer.Deserialize<JsonElement[]>(json);

        menus.Should().HaveCount(4);
    }

    [Fact]
    public async Task Menus_NoToken_Returns401()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.GetAsync("/api/me/menus");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Menus_VendorTokenOnAdminEndpoint_Returns403()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        var token = await LoginAndGetToken(client, "vendor1");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
