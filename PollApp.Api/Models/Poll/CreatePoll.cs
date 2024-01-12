namespace PollApp.Api.Models;

public class CreatePoll {
    public required string Title { get; set; }

    public required string Description { get; set; }

    public required List<PostOption> Options { get; set; }
}