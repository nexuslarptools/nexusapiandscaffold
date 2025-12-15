using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using NEXUSDataLayerScaffold.Tests.Infrastructure;
using Xunit;

namespace NEXUSDataLayerScaffold.Tests;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(string? sub = null, string? roles = null, string? rolesNs = null, string? email = null)
    {
        var client = _factory.CreateClient();
        if (!string.IsNullOrWhiteSpace(sub)) client.DefaultRequestHeaders.Add("X-Test-Sub", sub);
        if (!string.IsNullOrWhiteSpace(roles)) client.DefaultRequestHeaders.Add("X-Test-Roles", roles);
        if (!string.IsNullOrWhiteSpace(rolesNs)) client.DefaultRequestHeaders.Add("X-Test-Roles-Namespace", rolesNs);
        if (!string.IsNullOrWhiteSpace(email)) client.DefaultRequestHeaders.Add("X-Test-Email", email);
        return client;
    }

    [Fact]
    public async Task ReaderEndpoint_Unauthenticated_Returns401()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/api/v1/TagTypes");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WizardEndpoint_WithReaderRole_Returns403()
    {
        var client = CreateClient(sub: "auth0|reader", roles: "Reader");
        var payload = new { guid = Guid.NewGuid(), pronouns = "they/them" };
        var resp = await client.PostAsJsonAsync("/api/v1/Pronouns", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task WizardEndpoint_WithWizardRole_AndDbRole_Returns200()
    {
        // Use the seeded wizard user with DB role assignment
        var client = CreateClient(sub: "auth0|wizard", roles: "Wizard", email: "wizard@example.com");
        var payload = new { guid = Guid.NewGuid(), pronouns = "ze/zir" };
        var resp = await client.PostAsJsonAsync("/api/v1/Pronouns", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task WizardOrHeadGMPolicy_Allows_HeadGM()
    {
        var client = CreateClient(sub: "auth0|headgm", roles: "HeadGM", email: "headgm@example.com");
        var resp = await client.GetAsync("/api/v1/Users");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
