using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PollApp.Api.Models;

public class User {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("email")]
    public required string Email { get; set; }
    [BsonElement("password_hash")]
    public required string PasswordHash { get; set; }
    [BsonElement("is_admin")]
    public bool IsAdmin { get; set; }
}