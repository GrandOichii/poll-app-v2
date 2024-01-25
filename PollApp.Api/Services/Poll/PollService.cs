
using System.Data.Common;
using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PollApp.Api.Repositories;

namespace PollApp.Api.Services;

[Serializable]
public class PollNotFoundException : Exception
{
    public PollNotFoundException(string id) : base("poll with id " + id + " not found") { }
}

[Serializable]
public class AlreadyVotedException : Exception
{
    // TODO? discard userId and pollId
    public AlreadyVotedException(string userId, string pollId) : base("already voted for poll") { }
}

[Serializable]
public class OptionNotFoundException : Exception
{
    // TODO? discard userId and pollId
    public OptionNotFoundException(string pollId, int optionI) : base("poll has no option with index " + optionI) { }
}

public class PollService : IPollService
{
    private readonly IUserService _userService;
    private readonly IMongoCollection<Poll> _pollsCollection;
    private readonly IMapper _mapper;
    private readonly ICachedPollRepository _cache;
    public PollService(IMapper mapper, IUserService userService, ICachedPollRepository cache, IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings) {
        _mapper = mapper;
        _userService = userService;
        _cache = cache;

        _pollsCollection = new MongoClient(
        pollStoreDatabaseSettings.Value.ConnectionString
        ).GetDatabase(
            pollStoreDatabaseSettings.Value.DatabaseName
        ).GetCollection<Poll>(
            pollStoreDatabaseSettings.Value.PollCollectionName
        );
    }

    public async Task<IEnumerable<GetPoll>> All(string userId)
    {
        return (await _pollsCollection.Find(_ => true).ToListAsync())
            .Select(p => _mapper.Map<GetPoll>(p, opt => opt.Items["UserId"] = userId));
    }

    public async Task<IEnumerable<GetPoll>> Add(CreatePoll poll, string ownerId) {
        // TODO check ownerId
        poll.Validate();
        var result = _mapper.Map<Poll>(poll);
        result.OwnerId = ownerId;
        await _pollsCollection.InsertOneAsync(result);
        return await All(ownerId);
    }

    public async Task<GetPoll> Vote(string ownerId, string pollId, int optionI) {
        var poll = await ByIdInternal(pollId)
            ?? throw new PollNotFoundException(pollId);

        // checks
        if (optionI < 0 || optionI >= poll.Options.Count) throw new OptionNotFoundException(pollId, optionI);
        var user = _userService.ById(ownerId) ?? throw new UserNotFoundException(ownerId);
        
        var alreadyVoted = poll.Options.Any(option => option.Voters.Contains(ownerId));
        if (alreadyVoted) throw new AlreadyVotedException(ownerId, pollId);
        poll.Options[optionI].Voters.Add(ownerId);
        await _cache.Remember(poll);
        var result = await _pollsCollection.ReplaceOneAsync(p => p.Id == pollId, poll);
        if (result.ModifiedCount == 0) throw new Exception("failed to update");

        return _mapper.Map<GetPoll>(poll, opt => opt.Items["UserId"] = ownerId);
    }

    public async Task<GetPoll> ById(string userId, string pollId) {
        var poll = await ByIdInternal(pollId)
            ?? throw new PollNotFoundException(pollId);
        
        return _mapper.Map<GetPoll>(
            poll,
            opt => opt.Items["UserId"] = userId
        );
    }

    private async Task<Poll?> ByIdInternal(string pollId) {
        var result = await _cache.Get(pollId);
        if (result is not null) {
            return result;
        }
        result = await _pollsCollection.Find(p => p.Id == pollId).FirstOrDefaultAsync();
        if (result is null) {
            return result;
        }
        await _cache.Remember(result);
        // await Task.Delay(5000);
        return result;
    }
}