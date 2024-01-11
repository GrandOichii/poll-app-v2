
using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace PollApp.Api.Services;

public class PollService : IPollService
{
    public readonly IMongoCollection<Poll> _pollsCollection;
    public readonly IMapper _mapper;
    public PollService(IMapper mapper, IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings) {
        _mapper = mapper;

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

    public async Task<IEnumerable<GetPoll>> Add(CreatePoll poll) {
        await _pollsCollection.InsertOneAsync(_mapper.Map<Poll>(poll));
        return await All();
    }
}

public class Global {
    private Global() {}
    public static readonly Global Instance = new();

    public List<Poll> Polls { get; } = new();
}


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