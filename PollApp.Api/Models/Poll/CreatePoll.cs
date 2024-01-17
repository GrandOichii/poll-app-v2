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

    public void Validate() {
        if (string.IsNullOrEmpty(Title))
            throw new InvalidPollCreationException("poll title is empty");
        if (string.IsNullOrEmpty(Description))
            throw new InvalidPollCreationException("poll description is empty");
        if (Options.Count == 0)
            throw new InvalidPollCreationException("no poll options specified");
        // TODO check options
        foreach (var option in Options)
            option.Validate();
    }
}