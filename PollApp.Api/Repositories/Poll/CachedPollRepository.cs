
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace PollApp.Api.Repositories;

public class CachedPollRespository : ICachedPollRepository
{
    private readonly IDistributedCache _cache;
    public CachedPollRespository(IDistributedCache cache) {
        _cache = cache;
    }

    private static string ToKey(string id) => $"poll-{id}";

    public async Task<Poll?> Get(string id)
    {
        var key = ToKey(id);
        string? data = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(data)) {
            return null;
        }

        return BsonSerializer.Deserialize<Poll>(BsonDocument.Parse(data));
    }

    public async Task Remember(Poll poll)
    {
        var key = ToKey(poll.Id!);
        await _cache.SetStringAsync(
            key,
            poll.ToJson()
        );
    }
}