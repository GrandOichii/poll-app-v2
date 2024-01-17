// using DevOne.Security.Cryptography.BCrypt;

namespace PollApp.Api.Models;

[Serializable]
public class InvalidRegisterCredentialsException : Exception
{
    public InvalidRegisterCredentialsException(string message) : base(message) { }
}

public class PostUser {
    public required string Email { get; set; }
    public required string Password { get; set; }

    public User ToUser() {
        return new User {
            Email = Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password)
        };
    }

    public void Validate() {
        // TODO better email validation
        if (string.IsNullOrEmpty(Email))
            throw new InvalidRegisterCredentialsException($"invalid email");
        // TODO better password validation
        if (string.IsNullOrEmpty(Password))
            throw new InvalidRegisterCredentialsException($"invalid email");
    }
}