using MongoDB.Bson.Serialization.Attributes;

namespace PollApp.Api.Models;

public class Option {
    [BsonElement("text")]
    public required string Text { get; set; }
}