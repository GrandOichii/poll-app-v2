namespace PollApp.Api.Services;



public interface IPollService {
    public Task<IEnumerable<GetPoll>> All(string userId);
    public Task<IEnumerable<GetPoll>> Add(CreatePoll poll, string ownerId);
    public Task<GetPoll> Vote(string ownerId, string pollId, int optionI);
    public Task<GetPoll> ById(string userId, string pollId);
}