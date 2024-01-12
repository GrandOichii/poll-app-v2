
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

    public async Task<IEnumerable<GetPoll>> All()
    {
        return (await _pollsCollection.Find(_ => true).ToListAsync()).Select(_mapper.Map<GetPoll>);
    }

    public async Task<IEnumerable<GetPoll>> Add(CreatePoll poll, string id) {
        var result = _mapper.Map<Poll>(poll);
        result.OwnerId = id;
        await _pollsCollection.InsertOneAsync(result);
        return await All();
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

        return _mapper.Map<GetPoll>(poll);
    }

}

// public class Global {
//     private Global() {}
//     public static readonly Global Instance = new();

//     public List<Poll> Polls { get; } = new();
// }


// public class PollService_Old : IPollService
// {
//     public readonly IMapper _mapper;
//     public PollService_Old(IMapper mapper) {
//         _mapper = mapper;
//     }

//     public async Task<IEnumerable<GetPoll>> All()
//     {
//         return Global.Instance.Polls.Select(_mapper.Map<GetPoll>);
//     }

//     public async Task<IEnumerable<GetPoll>> Add(Poll poll) {
//         Global.Instance.Polls.Add(poll);
//         return await All();
//     }
// }