using Microsoft.AspNetCore.Mvc;

namespace PollApp.Api.Controllers;


[ApiController]
[Route("/api/v1/auth")]
public class AuthController : ControllerBase {
    private readonly IUserService _userService;

    public AuthController(IUserService userService) {
        _userService = userService;
    }

    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromBody] PostUser user) {
        try {

            var result = await _userService.Register(user);
            return Ok(result);
        
        } catch (EmailTakenException e) {
            return Conflict(e.Message);
        }
        // TODO catch bad request
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] PostUser user) {
        try {

            var result = await _userService.Login(user);
            return Ok(result);
        
        } catch (InvalidLoginCredentialsException e) {
            return BadRequest(e.Message);
        }
    }
}