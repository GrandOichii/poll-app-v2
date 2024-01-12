using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace PollApp.Api.Tests.Endpoints;

public class AuthEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointTests(WebApplicationFactory<Program> factory) {
        _factory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureServices(services => {
                // TODO
            });
        });
    }

    [Fact]
    public async Task Test() {
        // Arrange
        var client = _factory.CreateClient();

        // Act


        // Assert
        client.Should().NotBeNull();
        
    }
}