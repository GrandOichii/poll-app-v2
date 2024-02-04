using System.Security.Claims;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PollApp.Api.Controllers;
using PollApp.Api.Models;
using PollApp.Api.Services;

namespace PollApp.Api.Tests.Controllers;

public class PollControllerTests {
    
    private readonly IPollService _pollService;
    private readonly PollController _pollController;

    public PollControllerTests() {
        _pollService = A.Fake<IPollService>();

        _pollController = new(_pollService);
    }

    private void AddUser(string id, string email) {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
            new(ClaimTypes.NameIdentifier, id),
            new(ClaimTypes.Email, email),
        }));

        _pollController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    
    [Fact]
    public async Task ShouldFetchQueried()
    {
        // Arrange
        var userId = "userid";
        var polls = A.Fake<IEnumerable<GetPoll>>();
        A.CallTo(() => _pollService.All(userId)).Returns(polls);
        AddUser("userid", "mail");

        // Act
        var result = await _pollController.Queried(new());
    
        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ShouldCreate() {
        // Arrange
        var userId = "userid";
        var polls = A.Fake<IEnumerable<GetPoll>>();
        var poll = A.Fake<CreatePoll>();
        A.CallTo(() => _pollService.Add(poll, userId)).Returns(polls);
        AddUser(userId, "mail");

        // Act
        var result = await _pollController.Create(poll);

        // Arrange
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ShouldNotCreate() {
        // Arrange
        var userId = "userid";
        var polls = A.Fake<IEnumerable<GetPoll>>();
        var poll = A.Fake<CreatePoll>();
        A.CallTo(() => _pollService.Add(poll, userId)).Throws<Exception>();
        AddUser(userId, "mail");

        // Act
        var act = () => _pollController.Create(poll);

        // Arrange
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ShouldVoteOnce() {
        // Arrange
        var vote = A.Fake<Vote>();
        var userId = "userid";
        A.CallTo(() => _pollService.Vote(userId, vote.PollId, vote.OptionI))
            .Returns(A.Fake<GetPoll>())
            .Once()
            .Then
            .Throws(new AlreadyVotedException(userId, vote.PollId));
        AddUser(userId, "mail");

        // Act
        var result1 = await _pollController.Vote(vote);
        var result2 = await _pollController.Vote(vote);

        // Arrange
        result1.Should().BeOfType<OkObjectResult>();
        result2.Should().BeOfType<BadRequestObjectResult>();
    }




    public static IEnumerable<object[]> CreatePollExceptionsList
    {
        get
        {
            yield return new object[] { new InvalidPollCreationException("") };
            yield return new object[] { new InvalidPollOptionCreationException("") };
        }
    }

    [Theory]
    [MemberData(nameof(CreatePollExceptionsList))]
    public async Task ShouldHaveClientError(Exception e) {
        // Arrange
        var userId = "userid";
        var polls = A.Fake<IEnumerable<GetPoll>>();
        var poll = A.Fake<CreatePoll>();
        AddUser(userId, "mail");
        A.CallTo(() => _pollService.Add(poll, userId)).Throws(e);


        // Act
        var result = await _pollController.Create(poll);

        // Arrange
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // TODO add more exception checks to vote endpoint
}