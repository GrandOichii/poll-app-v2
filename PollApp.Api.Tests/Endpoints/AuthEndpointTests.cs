using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PollApp.Api.Services;
using PollApp.Api.Tests.Endpoints.Mocks;
using System.Net.Http.Json;
using PollApp.Api.Models;

namespace PollApp.Api.Tests.Endpoints;

public class AuthEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointTests(WebApplicationFactory<Program> factory) {
        _factory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureServices(services => {
                services.AddSingleton<IUserService, MockUserService>();
            });
        });
    }

    [Fact]
    public async Task ShouldRegister() {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.PostAsync("/api/v1/auth/register", JsonContent.Create(new PostUser{
            Email = "test@email.com",
            Password = "pass"
        }));

        // Assert
        result.Should().BeSuccessful();
    }

    // TODO add more cases
    [Theory]
    [InlineData("mail@email.com", ""), InlineData("", "password")]
    public async Task ShouldNotRegister(string email, string password) {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.PostAsync("/api/v1/auth/register", JsonContent.Create(new PostUser{
            Email = email,
            Password = password
        }));

        // Assert
        result.Should().HaveClientError();
    }

    [Fact]
    public async Task ShouldLogin() {
        // Arrange
        var client = _factory.CreateClient();
        var login = new PostUser {
            Email = "mail@email.com",
            Password = "password"
        };
        // Act
        await client.PostAsync("/api/v1/auth/register", JsonContent.Create(login));
        var result = await client.PostAsync("/api/v1/auth/login", JsonContent.Create(login));

        // Assert
        result.Should().BeSuccessful();
    }

    [Theory]
    [InlineData("", ""), InlineData("mymail@email.com", ""), InlineData("mymail@email.com", "wrong-password"), InlineData("wrong@email.com", "password")]
    public async Task ShouldNotLogin(string email, string password) {
        // Arrange
        var client = _factory.CreateClient();
        var user = new PostUser {
            Email = "mymail@email.com",
            Password = "password"
        };

        // Act
        await client.PostAsync("/api/v1/auth/register", JsonContent.Create(user));
        var result = await client.PostAsync("/api/v1/auth/login", JsonContent.Create(new PostUser {
            Email = email,
            Password = password
        }));

        // Assert
        result.Should().HaveClientError();
    }
}