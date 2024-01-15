namespace PollApp.Api.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/poll")]
public class PollController : ControllerBase {
    private readonly IPollService _pollService;

    public PollController(IPollService pollService) {
        _pollService = pollService;
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> All() {
        var id = this.ExtractClaim(ClaimTypes.NameIdentifier);
        return Ok(await _pollService.All(id));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreatePoll poll) {
        var id = this.ExtractClaim(ClaimTypes.NameIdentifier);
        return Ok(await _pollService.Add(poll, id));
    }

    [Authorize(Roles = "User")]
    [HttpPost("vote")]
    public async Task<IActionResult> Vote([FromBody] Vote vote) {
        var id = this.ExtractClaim(ClaimTypes.NameIdentifier);
        try {
            var result = await _pollService.Vote(id, vote.PollId, vote.OptionI);
            return Ok(result);
        } catch (AlreadyVotedException e) {
            return BadRequest(e);
        }
    }
}