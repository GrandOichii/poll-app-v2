namespace PollApp.Api.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Poll {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("onwer")]
    public required string OwnerId { get; set; }

    [BsonElement("title")]
    public required string Title { get; set; }

    [BsonElement("description")]
    public required string Description { get; set; }
    
    [BsonElement("options")]
    public List<Option> Options { get; set; } = new();
}