using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PollApp.Api.Controllers;
using PollApp.Api.Models;
using PollApp.Api.Services;

namespace PollApp.Api.Tests.Controllers;

public class AuthControllerTests {

    private readonly IUserService _userService;
    private readonly AuthController _authController;
    public AuthControllerTests() {
        _userService = A.Fake<IUserService>();

        _authController = new(_userService);
    }


    [Fact]
    public async Task ShouldRegister() {
        // Arrange
        var postUser = A.Fake<PostUser>();
        var user = A.Fake<GetUser>();
        A.CallTo(() => _userService.Register(postUser)).Returns(user);

        // Act
        var result = await _authController.Register(postUser);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    } 

    
    // [InlineData()]
    [Fact]
    public async Task ShouldNotRegister() {
        // Arrange
        var postUser = A.Fake<PostUser>();
        var user = A.Fake<GetUser>();
        A.CallTo(() => _userService.Register(postUser)).Throws(new EmailTakenException(""));

        // Act
        var result = await _authController.Register(postUser);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }


}