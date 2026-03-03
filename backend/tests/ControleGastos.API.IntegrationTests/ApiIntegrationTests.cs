using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ControleGastos.Application.Errors;
using Xunit;

namespace ControleGastos.API.IntegrationTests;

public class ApiIntegrationTests
{
    [Fact]
    public async Task PersonEndpoints_ShouldCreateAndGetById()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var createPersonResponse = await client.PostAsJsonAsync("/api/person", new
        {
            name = "Alice",
            birthDate = "1990-01-10"
        });

        Assert.Equal(HttpStatusCode.Created, createPersonResponse.StatusCode);

        var createdBody = await createPersonResponse.Content.ReadFromJsonAsync<JsonElement>();
        var personId = createdBody.GetProperty("id").GetGuid();

        var getPersonResponse = await client.GetAsync($"/api/person/{personId}");

        Assert.Equal(HttpStatusCode.OK, getPersonResponse.StatusCode);

        var getBody = await getPersonResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Alice", getBody.GetProperty("name").GetString());
        Assert.Equal(personId, getBody.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task PersonGetById_ShouldReturnNotFoundProblemDetails_WhenPersonDoesNotExist()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/person/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(BusinessErrorMessages.PersonNotFound, body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task TransactionCreate_ShouldReturnBadRequestProblemDetails_WhenMinorRegistersIncome()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var personId = await CreatePersonAsync(client, "Pedro", "2012-01-10");
        var categoryId = await CreateCategoryAsync(client, "Mesada", 2);

        var response = await client.PostAsJsonAsync("/api/transaction", new
        {
            personId,
            categoryId,
            description = "Receita",
            value = 120,
            transactionType = 2,
            date = DateTime.Today.ToString("yyyy-MM-dd")
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(BusinessErrorMessages.MinorCannotRegisterIncome, body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task TotalsByPerson_ShouldReturnSummaryBasedOnPersistedTransactions()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var personId = await CreatePersonAsync(client, "Ana", "1995-08-20");
        var categoryId = await CreateCategoryAsync(client, "Mercado", 1);

        var createTransactionResponse = await client.PostAsJsonAsync("/api/transaction", new
        {
            personId,
            categoryId,
            description = "Compra",
            value = 100,
            transactionType = 1,
            date = "2026-03-01"
        });

        Assert.Equal(HttpStatusCode.Created, createTransactionResponse.StatusCode);

        var totalsResponse = await client.GetAsync("/api/totals/persons");

        Assert.Equal(HttpStatusCode.OK, totalsResponse.StatusCode);

        var body = await totalsResponse.Content.ReadFromJsonAsync<JsonElement>();
        var summary = body.GetProperty("summary");

        Assert.Equal(0m, summary.GetProperty("totalIncome").GetDecimal());
        Assert.Equal(100m, summary.GetProperty("totalExpense").GetDecimal());
        Assert.Equal(-100m, summary.GetProperty("balance").GetDecimal());
    }

    private static async Task<Guid> CreatePersonAsync(HttpClient client, string name, string birthDate)
    {
        var response = await client.PostAsJsonAsync("/api/person", new
        {
            name,
            birthDate
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateCategoryAsync(HttpClient client, string description, int purpose)
    {
        var response = await client.PostAsJsonAsync("/api/category", new
        {
            description,
            purpose
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }
}
