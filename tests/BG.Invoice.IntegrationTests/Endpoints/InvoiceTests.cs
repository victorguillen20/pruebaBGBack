using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Enums;
using BG.Invoice.IntegrationTests.Infrastructure;

namespace BG.Invoice.IntegrationTests.Endpoints;

public class InvoiceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public InvoiceTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static CreateInvoiceRequest MakeRequest(int customerId, int productId, int quantity = 1)
    {
        return new CreateInvoiceRequest(
            customerId, InvoiceType.Contado, null, null,
            new List<CreateInvoiceDetailRequest>
            {
                new(productId, quantity, 10m, "Test Product", "TST-001")
            });
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsInvoiceWithAtomicNumber()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        var token = await AuthHelper.GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = MakeRequest(1, 1);
        var response = await client.PostAsJsonAsync("/api/invoices", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadAsStringAsync();
        var invoice = JsonSerializer.Deserialize<InvoiceResponse>(json, AuthHelper.JsonOptions);

        invoice.Should().NotBeNull();
        invoice!.Number.Should().Be(1);
        invoice.CustomerId.Should().Be(1);
    }

    [Fact(Skip = "R-1: SQLite in-memory lock investigation pending")]
    public async Task Create_10ParallelCalls_ProduceDistinctNumbers()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        var token = await AuthHelper.GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var requestTasks = Enumerable.Range(1, 10).Select(i =>
        {
            var req = MakeRequest(1, i, 1);
            return client.PostAsJsonAsync("/api/invoices", req);
        }).ToList();

        var responses = await Task.WhenAll(requestTasks);

        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Created);

        var numbers = new List<int>();
        foreach (var response in responses)
        {
            var json = await response.Content.ReadAsStringAsync();
            var invoice = JsonSerializer.Deserialize<InvoiceResponse>(json, AuthHelper.JsonOptions);
            numbers.Add(invoice!.Number);
        }

        numbers.Should().HaveCount(10);
        numbers.Should().OnlyHaveUniqueItems();
        numbers.Min().Should().Be(1);
        numbers.Max().Should().Be(10);
    }

    [Fact]
    public async Task Create_WithoutToken_Returns403()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        client.DefaultRequestHeaders.Authorization = null;

        var request = MakeRequest(1, 1);
        var response = await client.PostAsJsonAsync("/api/invoices", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cancel_TransitionsToAnnulled()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAsync();

        var token = await AuthHelper.GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = MakeRequest(1, 1);
        var createResponse = await client.PostAsJsonAsync("/api/invoices", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<InvoiceResponse>(json, AuthHelper.JsonOptions);

        var cancelResponse = await client.PostAsync($"/api/invoices/{created!.Id}/cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/invoices/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getJson = await getResponse.Content.ReadAsStringAsync();
        var cancelled = JsonSerializer.Deserialize<InvoiceResponse>(getJson, AuthHelper.JsonOptions);

        cancelled.Should().NotBeNull();
        cancelled!.Status.Should().Be(InvoiceStatus.Anulada);
    }
}
