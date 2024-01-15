using FakeItEasy;
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
    
    // [Fact]
    // public void ShouldFetchAll()
    // {
    //     // Arrange
    //     var polls = A.Fake<IEnumerable<GetPoll>>();
    //     A.CallTo(() => _pollService.All()).Returns(polls);
    
    //     // Act

    
    //     // Assert
    // }
}