using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinTrack.Application.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace FinTrack.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string Senha = "senha-forte-123";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
    }

    public static string NovoEmail() => $"usuario-{Guid.NewGuid():N}@exemplo.com";

    public async Task<HttpClient> CriarClienteAutenticadoAsync()
    {
        var client = CreateClient();
        var credenciais = new CredenciaisDto { Email = NovoEmail(), Senha = Senha };

        (await client.PostAsJsonAsync("/api/auth/registrar", credenciais, Json)).EnsureSuccessStatusCode();
        var login = await client.PostAsJsonAsync("/api/auth/login", credenciais, Json);
        login.EnsureSuccessStatusCode();
        var token = await login.Content.ReadFromJsonAsync<TokenDto>(Json);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    public Task InitializeAsync() => _postgres.StartAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
