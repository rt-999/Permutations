using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Permutations.Api.Tests;

public class PermutationsApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> StartAsync(int n)
    {
        var response = await _client.PostAsJsonAsync("/api/permutations/start", new { n });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("sessionId").GetString()!;
    }

    private Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string? sessionId)
    {
        var request = new HttpRequestMessage(method, url);
        if (sessionId is not null) request.Headers.Add(SessionIdHeader.Name, sessionId);
        return _client.SendAsync(request);
    }

    [Fact]
    public async Task Start_ReturnsTotalAsString_ExactForN20()
    {
        var response = await _client.PostAsJsonAsync("/api/permutations/start", new { n = 20 });
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.String, json.GetProperty("total").ValueKind);
        Assert.Equal("2432902008176640000", json.GetProperty("total").GetString());
    }

    [Fact]
    public async Task Start_InvalidN_Returns400WithCode()
    {
        var response = await _client.PostAsJsonAsync("/api/permutations/start", new { n = 21 });
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalid_n", json.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Next_WithoutHeader_Returns400()
    {
        var response = await SendAsync(HttpMethod.Post, "/api/permutations/next", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Next_UnknownSession_Returns404()
    {
        var response = await SendAsync(HttpMethod.Post, "/api/permutations/next", "does-not-exist");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("session_not_found", json.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Next_UntilExhausted_Returns409()
    {
        var id = await StartAsync(3);

        for (var i = 0; i < 6; i++)
            Assert.Equal(HttpStatusCode.OK, (await SendAsync(HttpMethod.Post, "/api/permutations/next", id)).StatusCode);

        var response = await SendAsync(HttpMethod.Post, "/api/permutations/next", id);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("permutations_exhausted", json.GetProperty("code").GetString());
    }

    [Fact]
    public async Task All_ReturnsPage_WithStringNumbers()
    {
        var id = await StartAsync(4);

        var response = await SendAsync(HttpMethod.Get, "/api/permutations/all?page=2&pageSize=10", id);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("3", json.GetProperty("totalPages").GetString());
        Assert.Equal("11", json.GetProperty("firstIndex").GetString());
        Assert.Equal(10, json.GetProperty("items").GetArrayLength());
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("99999999999999999999")]
    public async Task All_NonNumericPage_Returns400(string page)
    {
        var id = await StartAsync(4);

        var response = await SendAsync(HttpMethod.Get, $"/api/permutations/all?page={page}&pageSize=10", id);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalid_page", json.GetProperty("code").GetString());
    }

    [Fact]
    public async Task All_Exit_Next_ContinuesAfterLastShown()
    {
        var id = await StartAsync(4);

        await SendAsync(HttpMethod.Get, "/api/permutations/all?page=1&pageSize=10", id);   // shows 1..10
        var exit = await SendAsync(HttpMethod.Post, "/api/permutations/all/exit", id);
        var next = await SendAsync(HttpMethod.Post, "/api/permutations/next", id);
        var json = await next.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.NoContent, exit.StatusCode);
        Assert.Equal("11", json.GetProperty("index").GetString());
    }
}
