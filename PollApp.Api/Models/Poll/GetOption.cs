namespace PollApp.Api.Models;

public class GetOption {
    public required string Text { get; set; }
    public required int VoteCount { get; set; }
}