namespace PollApp.Api.Models;

public class GetPoll {
    public required string Title { get; set; }

    public required string Description { get; set; }
    
    public required List<GetOption> Options { get; set; }
}