using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Enums;
using BG.Invoice.IntegrationTests.Infrastructure;

namespace BG.Invoice.IntegrationTests.Endpoints;

public class CustomerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CustomerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_Returns201WithLocationHeader()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        var token = await AuthHelper.GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateCustomerRequest("NEW-001", "New Customer", CustomerType.Persona, "555-9999", "new@test.com", "123 Main St", 1000m);
        var response = await client.PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.AbsolutePath.Should().StartWith("/api/customers/");
    }

    [Fact]
    public async Task GetById_ReturnsCustomerWithRelations()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        var token = await AuthHelper.GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/customers/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var customer = JsonSerializer.Deserialize<CustomerResponse>(json, AuthHelper.JsonOptions);

        customer.Should().NotBeNull();
        customer!.Id.Should().Be(1);
        customer.Name.Should().NotBeNullOrEmpty();
        customer.Identification.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Update_AppliesFields()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        var token = await AuthHelper.GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateCustomerRequest("Updated Name", CustomerType.Empresa, "555-0000", "updated@test.com", "456 New St", 5000m);
        var updateResponse = await client.PutAsJsonAsync("/api/customers/1", updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await client.GetAsync("/api/customers/1");
        var json = await getResponse.Content.ReadAsStringAsync();
        var customer = JsonSerializer.Deserialize<CustomerResponse>(json, AuthHelper.JsonOptions);

        customer.Should().NotBeNull();
        customer!.Name.Should().Be("Updated Name");
        customer.Phone.Should().Be("555-0000");
    }
}
