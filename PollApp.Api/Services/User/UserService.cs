
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

[Serializable]
public class UserNotFoundException : Exception
{
    public UserNotFoundException(string id) : base("user with id " + id + " not found") { }
}

public class UserService : IUserService
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    public UserService(IMapper mapper, IConfiguration configuration, IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings) {
        _mapper = mapper;
        _configuration = configuration;

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

        user.Validate();

        var result = user.ToUser();
        await _usersCollection.InsertOneAsync(result);

        return _mapper.Map<GetUser>(result);
    }

    public async Task<string> Login(PostUser user)
    {
        var users = await _usersCollection.FindAsync(u => u.Email == user.Email);
        var existing = await users.FirstAsync() ?? throw new InvalidLoginCredentialsException();

        if (!BCrypt.Net.BCrypt.Verify(user.Password, existing.PasswordHash)) throw new InvalidLoginCredentialsException();

        return CreateToken(existing);
    }

    public async Task<User> ById(string id) {
        var result = await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync()
            ?? throw new UserNotFoundException(id);
        return result;
    }


    private string CreateToken(User user) {
        var claims = new List<Claim>(){
            new(ClaimTypes.NameIdentifier, user.Id!),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: cred
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

}