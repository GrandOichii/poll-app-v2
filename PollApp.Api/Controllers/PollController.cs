namespace PollApp.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/poll")]
public class PollController : ControllerBase {
    private readonly IPollService _pollService;

    public PollController(IPollService pollService) {
        _pollService = pollService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> All() {
        return Ok(await _pollService.All());
    }

    // TODO restrict to admin users
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreatePoll poll) {
        return Ok(await _pollService.Add(poll));
    }
}