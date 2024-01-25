namespace PollApp.Api.Models;

public class GetPoll {
    public required string Id { get; set; }
 
    public required string Title { get; set; }

    public required string Description { get; set; }
    
    public required List<GetOption> Options { get; set; }
    public required bool CanVote { get; set; }
    public DateTime PostDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public bool VotesVisible { get; set; }
}