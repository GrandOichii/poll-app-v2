using MongoDB.Bson.Serialization.Attributes;

namespace PollApp.Api.Models;

public class Option {
    [BsonElement("text")]
    public required string Text { get; set; }
    [BsonElement("voters")]
    public List<string> Voters { get; set; } = new();
}