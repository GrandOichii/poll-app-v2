namespace PollApp.Api.Models;

public class Vote {
    public required string PollId { get; set; }
    public required int OptionI { get; set; }
}