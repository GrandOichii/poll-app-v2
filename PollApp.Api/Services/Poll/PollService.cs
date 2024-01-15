
using System.Data.Common;
using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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
    public PollService(IMapper mapper, IUserService userService, IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings) {
        _mapper = mapper;
        _userService = userService;

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
        var result = _mapper.Map<Poll>(poll);
        result.OwnerId = ownerId;
        await _pollsCollection.InsertOneAsync(result);
        return await All(ownerId);
    }

    public async Task<GetPoll> Vote(string ownerId, string pollId, int optionI) {
        var poll = await _pollsCollection.Find(p => p.Id == pollId).FirstOrDefaultAsync()
            ?? throw new PollNotFoundException(pollId);
        if (poll.Options.Count <= optionI) throw new OptionNotFoundException(pollId, optionI);
        var user = _userService.ById(ownerId) ?? throw new UserNotFoundException(ownerId);
        var alreadyVoted = poll.Options.Any(option => option.Voters.Contains(ownerId));
        if (alreadyVoted) throw new AlreadyVotedException(ownerId, pollId);
        poll.Options[optionI].Voters.Add(ownerId);
        var result = await _pollsCollection.ReplaceOneAsync(p => p.Id == pollId, poll);
        if (result.ModifiedCount == 0) throw new Exception("failed to update");

        return _mapper.Map<GetPoll>(poll, opt => opt.Items["UserId"] = ownerId);
    }

}