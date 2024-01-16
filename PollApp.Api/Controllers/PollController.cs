namespace PollApp.Api.Controllers;

using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


public class PollQuery {
    [FromQuery(Name = "expired")]
    public bool Expired {get; set;} = false;

    public PollQuery() {}

    public bool Matches(GetPoll poll) {
        // FIXME
        return 
            Expired || (DateTime.Now < poll.ExpireDate)
        ;
    }
}

[ApiController]
[Route("/api/v1/poll")]
public class PollController : ControllerBase {
    private readonly IPollService _pollService;

    public PollController(IPollService pollService) {
        _pollService = pollService;
    }


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Queried([FromQuery] PollQuery query) {
        var id = this.ExtractClaim(ClaimTypes.NameIdentifier);

        // TODO? should this be in the service
        var all = await _pollService.All(id);
        return Ok(all.Where(query.Matches));
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

    [Authorize]
    [HttpGet("{pollId}")]
    public async Task<IActionResult> ById([FromRoute] string pollId) {
        var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);
        var result = await _pollService.ById(userId, pollId);
        return Ok(result);
    }
}