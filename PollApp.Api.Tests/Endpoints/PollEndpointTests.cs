using System.Net.Http.Json;
using FluentAssertions;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PollApp.Api.Models;
using PollApp.Api.Services;
using PollApp.Api.Tests.Endpoints.Mocks;

namespace PollApp.Api.Tests.Endpoints;


public class PollEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PollEndpointTests(WebApplicationFactory<Program> factory) {
        _factory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureServices(services => {
                services.AddSingleton<IPollService, MockPollService>();
                services.AddSingleton<IUserService, MockUserService>();
            });
        });
    }

    private static async Task<string> GetJwtToken(HttpClient client, string email, string password) {
        var user = new PostUser {
            Email = email,
            Password = password
        };
        var reg = await client.PostAsync("/api/v1/auth/register", JsonContent.Create(user));
        reg.Should().BeSuccessful();
        var result = await client.PostAsync("/api/v1/auth/login", JsonContent.Create(user));
        result.Should().BeSuccessful();
        return await result.Content.ReadAsStringAsync();
    }

    private static async Task Login(HttpClient client, string email = "mymail@email.com", string password = "password") {
        var token = await GetJwtToken(client, email, password);
        client.SetBearerToken(token);
        
    }

    private static async Task<GetPoll> CreatePost(HttpClient client, PostUser user, CreatePoll poll) {
        await Login(client, user.Email, user.Password);
        var result = await client.PostAsync("/api/v1/poll/create", JsonContent.Create(poll));

        return (await result.Content.ReadFromJsonAsync<List<GetPoll>>())![0];
    }

    private static DateTime Tomorrow() => DateTime.Now.AddDays(1);

    [Fact]
    public async Task ShouldFetchAll() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client);

        // Act
        var result = await client.GetAsync("/api/v1/poll");

        // Assert
        result.Should().BeSuccessful();
    }

    // TODO test queired fetch

    [Fact]
    public async Task ShouldCreate() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin@email.com", "password");

        // Act
        var result = await client.PostAsync("/api/v1/poll/create", JsonContent.Create(new CreatePoll{
            Description = "description",
            Title = "title",
            Options = new() {
                new PostOption{ Text = "Option1" },
                new PostOption{ Text = "Option2" }
            },
            ExpireDate = Tomorrow()
        }));

        // Assert
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task ShouldNotCreateUnauthorized() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "ordinary@email.com", "password");

        // Act
        var result = await client.PostAsync("/api/v1/poll/create", JsonContent.Create(new CreatePoll{
            Description = "description",
            Title = "title",
            Options = new() {
                new PostOption{ Text = "Option1" },
                new PostOption{ Text = "Option2" }
            }
        }));

        // Assert
        result.Should().HaveClientError();
    }

    // TODO add more cases
    public static IEnumerable<object[]> CreatePollList
    {
        get
        {
            yield return new object[] { new CreatePoll {
                Title="",
                Description="Description",
                Options=new() {
                    new PostOption { Text = "Option1" },
                    new PostOption { Text = "Option2" },
                },
                ExpireDate = DateTime.Now.AddDays(1)
            } };
            yield return new object[] { new CreatePoll {
                Title="Title",
                Description="",
                Options=new() {
                    new PostOption { Text = "Option1" },
                    new PostOption { Text = "Option2" },
                },
                ExpireDate = DateTime.Now.AddDays(1)
            } };
            yield return new object[] { new CreatePoll {
                Title="Title",
                Description="Description",
                Options=new(),
                ExpireDate = DateTime.Now.AddDays(1)
            } };
            yield return new object[] { new CreatePoll {
                Title="Title",
                Description="Description",
                Options=new() {
                    new PostOption { Text = "Option1" },
                },
            } };
            yield return new object[] { new CreatePoll {
                Title="Title",
                Description="Description",
                Options=new() {
                    new PostOption { Text = "Option1" },
                    new PostOption { Text = "" },
                },
                ExpireDate = DateTime.Now.AddDays(1)
            } };
            yield return new object[] { new CreatePoll {
                Title="Title",
                Description="Description",
                Options=new() {
                    new PostOption { Text = "Option1" },
                    new PostOption { Text = "Option2" },
                },
                ExpireDate = DateTime.Now.AddDays(-1)
            } };
        }
    }

    [Theory]
    [MemberData(nameof(CreatePollList))]
    public async Task ShouldNotCreateInvalid(CreatePoll poll) {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin@email.com", "password");

        // Act
        var result = await client.PostAsync("/api/v1/poll/create", JsonContent.Create(poll));

        // Assert
        result.Should().HaveClientError();
    }

    [Fact]
    public async Task ShouldFetchById() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin@email.com", "password");
        var post = await client.PostAsync("/api/v1/poll/create", JsonContent.Create(new CreatePoll{
            Description = "description",
            Title = "title",
            Options = new() {
                new PostOption{ Text = "Option1" },
                new PostOption{ Text = "Option2" }
            },
            ExpireDate = Tomorrow()
        }));
        var poll = (await post.Content.ReadFromJsonAsync<List<GetPoll>>())![0];
        
        // Act
        var result = await client.GetAsync($"/api/v1/poll/{poll!.Id}");

        // Assert
        result.Should().BeSuccessful();
       
    }

    [Fact]
    public async Task ShouldNotFetchById() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin@email.com", "password");
        
        // Act
        var result = await client.GetAsync("/api/v1/poll/invalid_id");

        // Assert
        result.Should().HaveClientError();
    }

    [Theory]
    [InlineData(0), InlineData(1)]
    public async Task ShouldVote(int optionI) {

        // Arrange
        var client = _factory.CreateClient();
        var poll = await CreatePost(client, new PostUser{
            Email = "admin@email.com",
            Password = "password"
        }, new CreatePoll{
            Description = "description",
            Title = "title",
            Options = new() {
                new PostOption{ Text = "Option1" },
                new PostOption{ Text = "Option2" }
            },
            ExpireDate = Tomorrow()
        });
        await Login(client, "user@email.com", "password");

        // Act
        var result = await client.PostAsync("/api/v1/poll/vote", JsonContent.Create(new Vote {
            OptionI = optionI,
            PollId = poll.Id
        }));

        // Assert
        poll.CanVote.Should().BeTrue();
        poll.Options[optionI].VoteCount.Should().Be(0);
        result.Should().BeSuccessful();
        var newPoll = await result.Content.ReadFromJsonAsync<GetPoll>();
        newPoll.Should().NotBeNull();
        newPoll!.CanVote.Should().BeFalse();
        newPoll!.Options[optionI].VoteCount.Should().Be(1);
    }

    [Fact]
    public async Task ShouldVoteInvisible() {
        var optionI = 0;
        // Arrange
        var client = _factory.CreateClient();
        var poll = await CreatePost(client, new PostUser{
            Email = "admin@email.com",
            Password = "password"
        }, new CreatePoll{
            Description = "description",
            Title = "title",
            Options = new() {
                new PostOption{ Text = "Option1" },
                new PostOption{ Text = "Option2" }
            },
            ExpireDate = Tomorrow(),
            VotesVisible = false
        });
        await Login(client, "user@email.com", "password");

        // Act
        var result = await client.PostAsync("/api/v1/poll/vote", JsonContent.Create(new Vote {
            OptionI = optionI,
            PollId = poll.Id
        }));

        // Assert
        var resPoll = await result.Content.ReadFromJsonAsync<GetPoll>();
        resPoll.VotesVisible.Should().BeFalse();
        resPoll.Should().NotBeNull();
        resPoll!.Options[optionI].VoteCount.Should().Be(0);
    }

    
    [Fact]
    public async Task ShouldFailVoteAdmin() {
        // Arrange
        var client = _factory.CreateClient();
        var poll = await CreatePost(client, new PostUser{
            Email = "admin@email.com",
            Password = "password"
        }, new CreatePoll{
            Description = "description",
            Title = "title",
            Options = new() {
                new PostOption{ Text = "Option1" },
                new PostOption{ Text = "Option2" }
            },
            ExpireDate = Tomorrow()
        });

        // Act
        var result = await client.PostAsync("/api/v1/poll/vote", JsonContent.Create(new Vote {
            OptionI = 0,
            PollId = poll.Id
        }));

        // Assert
        result.Should().HaveClientError();
    }

    [Fact]
    public async Task ShouldFailVoteInvalidId() {
        // Arrange
        var client = _factory.CreateClient();
        var poll = await CreatePost(client, new PostUser{
            Email = "admin@email.com",
            Password = "password"
        }, new CreatePoll{
            Description = "description",
            Title = "title",
            Options = new() {
                new PostOption{ Text = "Option1" },
                new PostOption{ Text = "Option2" }
            },
            ExpireDate = Tomorrow()
        });
        await Login(client, "user@email.com", "password");

        // Act
        var result = await client.PostAsync("/api/v1/poll/vote", JsonContent.Create(new Vote {
            OptionI = 0,
            PollId = "invalid_id"
        }));

        // Assert
        result.Should().HaveClientError();
    }


    [Theory]
    [InlineData(-1), InlineData(10)]
    public async Task ShouldFailVoteInvalidOption(int optionI) {
        // Arrange
        var client = _factory.CreateClient();
        var poll = await CreatePost(client, new PostUser{
            Email = "admin@email.com",
            Password = "password"
        }, new CreatePoll{
            Description = "description",
            Title = "title",
            Options = new() {
                new PostOption{ Text = "Option1" },
                new PostOption{ Text = "Option2" }
            },
            ExpireDate = Tomorrow()
        });
        await Login(client, "user@email.com", "password");

        // Act
        var result = await client.PostAsync("/api/v1/poll/vote", JsonContent.Create(new Vote {
            OptionI = optionI,
            PollId = poll.Id
        }));

        // Assert
        result.Should().HaveClientError();
    }

}