namespace PollApp.Api.Models;

[Serializable]
public class InvalidPollCreationException : Exception
{
    public InvalidPollCreationException(string message) : base(message) { }
}

public class CreatePoll {
    public required string Title { get; set; }

    public required string Description { get; set; }

    public required List<PostOption> Options { get; set; }
    public DateTime ExpireDate { get; set; }
    public bool VotesVisible { get; set; }
    public void Validate() {
        if (ExpireDate < DateTime.Now)
            throw new InvalidPollCreationException("can't create already expired poll");
        if (string.IsNullOrEmpty(Title))
            throw new InvalidPollCreationException("poll title is empty");
        if (string.IsNullOrEmpty(Description))
            throw new InvalidPollCreationException("poll description is empty");
        if (Options.Count < 2)
            throw new InvalidPollCreationException($"invalid poll options count: {Options.Count}");            
        foreach (var option in Options)
            option.Validate();
    }
}