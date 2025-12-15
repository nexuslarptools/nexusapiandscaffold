using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NEXUSDataLayerScaffold.Tests.Infrastructure;
using Xunit;

namespace NEXUSDataLayerScaffold.Tests;

public class EmailIdentityIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EmailIdentityIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(string? sub = null, string? roles = null, string? email = null)
    {
        var client = _factory.CreateClient();
        if (!string.IsNullOrWhiteSpace(sub)) client.DefaultRequestHeaders.Add("X-Test-Sub", sub);
        if (!string.IsNullOrWhiteSpace(roles)) client.DefaultRequestHeaders.Add("X-Test-Roles", roles);
        if (!string.IsNullOrWhiteSpace(email)) client.DefaultRequestHeaders.Add("X-Test-Email", email);
        return client;
    }

    [Fact]
    public async Task Users_Permission_WizardUser_ReturnsWizard()
    {
        var client = CreateClient(sub: "auth0|wizard", roles: "Wizard", email: "wizard@example.com");
        var resp = await client.GetAsync("/api/v1/Users/Permission");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadAsStringAsync();
        content.Should().Contain("\"AuthLevel\":\"Wizard\"");
    }

    [Fact]
    public async Task Larps_Get_WithWizardRole_Returns200()
    {
        var client = CreateClient(sub: "auth0|wizard", roles: "Wizard", email: "wizard@example.com");
        var resp = await client.GetAsync("/api/v1/Larps");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task Larps_Get_Unauthenticated_Returns401()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/api/v1/Larps");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Users_GetAllUsers_Allows_HeadGM_Policy()
    {
        // Controller decorated with [Authorize(Policy = "WizardOrHeadGM")], claims-only check
        var client = CreateClient(sub: "auth0|headgm", roles: "HeadGM", email: "headgm@example.com");
        var resp = await client.GetAsync("/api/v1/Users");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CharacterSheets_List_WithReaderRoleButNoEmail_Returns401()
    {
        // Reader policy passes via claims, but controller calls UsersLogic.IsUserAuthed which now requires email
        var client = CreateClient(sub: "auth0|reader", roles: "Reader", email: null);
        var resp = await client.GetAsync("/api/v1/CharacterSheets");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Users_CurrentGuid_WithEmail_ReturnsNonEmptyGuid()
    {
        var client = CreateClient(sub: "auth0|wizard", roles: "Wizard", email: "wizard@example.com");
        var resp = await client.GetAsync("/api/v1/Users/CurrentGuid");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var guidStr = await resp.Content.ReadAsStringAsync();
        guidStr.Should().NotBeNullOrWhiteSpace();
        guidStr.Should().NotBe("00000000-0000-0000-0000-000000000000");
    }
}
