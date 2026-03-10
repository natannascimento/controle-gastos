using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ControleGastos.Application.Errors;
using Xunit;

namespace ControleGastos.API.IntegrationTests;

public class ApiIntegrationTests
{
    [Fact]
    public async Task AuthRegister_ShouldCreateUserAndReturnToken()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"alice-{Guid.NewGuid():N}@mail.com",
            name = "Alice",
            password = "12345678"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("accessToken").GetString()));
        Assert.True(body.GetProperty("expiresIn").GetInt32() > 0);
        Assert.Equal("Alice", body.GetProperty("user").GetProperty("name").GetString());
        Assert.Contains(response.Headers, h => h.Key == "Set-Cookie");
    }

    [Fact]
    public async Task AuthRefresh_ShouldRotateRefreshToken()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        await RegisterAsync(client, $"refresh-{Guid.NewGuid():N}@mail.com");

        var refreshResponse = await client.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var body = await refreshResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("accessToken").GetString()));
        Assert.Contains(refreshResponse.Headers, h => h.Key == "Set-Cookie");
    }

    [Fact]
    public async Task AuthLogout_ShouldInvalidateRefreshToken()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var token = await RegisterAsync(client, $"logout-{Guid.NewGuid():N}@mail.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var logoutResponse = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshResponse = await client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.BadRequest, refreshResponse.StatusCode);

        var body = await refreshResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("business_rule", body.GetProperty("type").GetString());
        Assert.Equal(BusinessErrorMessages.InvalidRefreshToken, body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task AuthGoogle_ShouldLinkExistingUserByEmail()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var email = $"google-link-{Guid.NewGuid():N}@mail.com";
        var registerToken = await RegisterAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerToken);

        var meResponse = await client.GetAsync("/api/users/me");
        var meBody = await meResponse.Content.ReadFromJsonAsync<JsonElement>();
        var registeredUserId = meBody.GetProperty("id").GetGuid();

        var googleResponse = await client.PostAsJsonAsync("/api/auth/google", new
        {
            idToken = $"test:google-sub-123:{email}:Alice Google"
        });

        Assert.Equal(HttpStatusCode.OK, googleResponse.StatusCode);

        var googleBody = await googleResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(registeredUserId, googleBody.GetProperty("user").GetProperty("id").GetGuid());
        Assert.Equal(2, googleBody.GetProperty("user").GetProperty("authProvider").GetInt32());
    }

    [Fact]
    public async Task UsersMe_ShouldRequireAuthentication()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("unauthorized", body.GetProperty("type").GetString());
    }

    [Fact]
    public async Task PersonEndpoints_ShouldCreateAndGetById()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = await CreateAuthenticatedClientAsync(factory);

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
        using var client = await CreateAuthenticatedClientAsync(factory);

        var response = await client.GetAsync($"/api/person/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("not_found", body.GetProperty("type").GetString());
        Assert.Equal(BusinessErrorMessages.PersonNotFound, body.GetProperty("message").GetString());
        Assert.Empty(body.GetProperty("errors").EnumerateObject());
    }

    [Fact]
    public async Task TransactionCreate_ShouldReturnBadRequestProblemDetails_WhenMinorRegistersIncome()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = await CreateAuthenticatedClientAsync(factory);

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
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("business_rule", body.GetProperty("type").GetString());
        Assert.Equal(BusinessErrorMessages.MinorCannotRegisterIncome, body.GetProperty("message").GetString());
        Assert.Empty(body.GetProperty("errors").EnumerateObject());
    }

    [Fact]
    public async Task CategoryCreate_ShouldReturnValidationEnvelope_WhenDescriptionIsInvalid()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = await CreateAuthenticatedClientAsync(factory);

        var response = await client.PostAsJsonAsync("/api/category", new
        {
            description = "",
            purpose = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("validation_error", body.GetProperty("type").GetString());
        Assert.Equal("Dados invalidos.", body.GetProperty("message").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("Description", out var fieldErrors));
        Assert.True(fieldErrors.GetArrayLength() > 0);
    }

    [Fact]
    public async Task TotalsByPerson_ShouldReturnSummaryBasedOnPersistedTransactions()
    {
        using var factory = new IntegrationTestWebApplicationFactory();
        using var client = await CreateAuthenticatedClientAsync(factory);

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

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(IntegrationTestWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        var token = await RegisterAsync(client, $"auth-{Guid.NewGuid():N}@mail.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<string> RegisterAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            name = "Integration User",
            password = "12345678"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
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
