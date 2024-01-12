namespace PollApp.Api.Services;

public interface IUserService {
    public Task<GetUser> Register(PostUser user);
    public Task<string> Login(PostUser user);
    public Task<User> ById(string id);
}