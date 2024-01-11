
using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace PollApp.Api.Services;

[Serializable]
public class EmailTakenException : Exception
{
    public EmailTakenException(string email) : base("email " + email + " is taken") { }
}

[Serializable]
public class InvalidLoginCredentialsException : Exception
{
    public InvalidLoginCredentialsException() : base("invalid login credentials") { }
    public InvalidLoginCredentialsException(string message) : base(message) { }
}

public class UserService : IUserService
{
    public readonly IMongoCollection<User> _usersCollection;
    public readonly IMapper _mapper;
    public UserService(IMapper mapper, IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings) {
        _mapper = mapper;

        _usersCollection = new MongoClient(
            pollStoreDatabaseSettings.Value.ConnectionString
        ).GetDatabase(
            pollStoreDatabaseSettings.Value.DatabaseName
        ).GetCollection<User>(
            pollStoreDatabaseSettings.Value.UserCollectionName
        );
    }

    public async Task<GetUser> Register(PostUser user)
    {
        var count = await _usersCollection.CountDocumentsAsync(u => u.Email == user.Email);
        if (count > 0) {
            throw new EmailTakenException(user.Email);
        }

        var result = user.ToUser();
        await _usersCollection.InsertOneAsync(result);

        return _mapper.Map<GetUser>(result);
    }

    public async Task<string> Login(PostUser user)
    {
        var users = await _usersCollection.FindAsync(u => u.Email == user.Email);
        var existing = await users.FirstAsync() ?? throw new InvalidLoginCredentialsException();

        if (!BCrypt.Net.BCrypt.Verify(user.Password, existing.PasswordHash)) throw new InvalidLoginCredentialsException();

        return "jwt token";
    }
}