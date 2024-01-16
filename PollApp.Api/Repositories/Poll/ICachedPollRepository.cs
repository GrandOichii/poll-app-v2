namespace PollApp.Api.Repositories;

public interface ICachedPollRepository {
    public Task<Poll?> Get(string id);
    public Task Remember(Poll poll);
}