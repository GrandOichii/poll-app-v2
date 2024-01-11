namespace PollApp.Api.Models;

public class PostUser {
    public required string Email { get; set; }
    public required string Password { get; set; }

    public User ToUser() {
        // TODO hash password
        return new User {
            Email = Email,
            PasswordHash = Password
        };
    }
}