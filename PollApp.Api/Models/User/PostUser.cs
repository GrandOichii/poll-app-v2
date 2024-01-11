// using DevOne.Security.Cryptography.BCrypt;

namespace PollApp.Api.Models;

public class PostUser {
    public required string Email { get; set; }
    public required string Password { get; set; }

    public User ToUser() {
        return new User {
            Email = Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password)
        };
    }
}