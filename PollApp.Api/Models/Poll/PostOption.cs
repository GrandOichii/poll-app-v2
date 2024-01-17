namespace PollApp.Api.Models;

[Serializable]
public class InvalidPollOptionCreationException : Exception
{
    public InvalidPollOptionCreationException(string message) : base(message) { }
}

public class PostOption {
    public required string Text { get; set; }

    public void Validate() {
        if (string.IsNullOrEmpty(Text))
            throw new InvalidPollOptionCreationException("option text is empty");
    }
}