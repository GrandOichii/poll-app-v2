namespace PollApp.Api.Services;

public interface IPollService {
    public Task<IEnumerable<GetPoll>> All();
    public Task<IEnumerable<GetPoll>> Add(CreatePoll poll, string id);
}