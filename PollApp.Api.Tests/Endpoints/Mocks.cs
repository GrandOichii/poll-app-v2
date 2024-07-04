using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amazon.Runtime.Internal.Util;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PollApp.Api.Models;
using PollApp.Api.Services;
using Xunit.Abstractions;

namespace PollApp.Api.Tests.Endpoints.Mocks;

public class MockUserService : IUserService
{
    private readonly IMapper _mapper;

    public MockUserService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public List<User> Users { get; set; }=new();
    public async Task<User> ById(string id)
    {
        return Users.FirstOrDefault(u => u.Id == id)
            ?? throw new UserNotFoundException(id);
    }

    public async Task<string> Login(PostUser user)
    {
        var existing = Users.FirstOrDefault(u => user.Email == u.Email)
            ?? throw new InvalidLoginCredentialsException();

        if (!BCrypt.Net.BCrypt.Verify(user.Password, existing.PasswordHash))
            throw new InvalidLoginCredentialsException();

        return CreateToken(existing);
    }

    public async Task<GetUser> Register(PostUser user)
    {
        var existing = Users.FirstOrDefault(u => u.Email == user.Email);
        if (existing is not null) throw new EmailTakenException(user.Email);


        user.Validate();

        var result = user.ToUser();
        result.Id = Users.Count.ToString();
        result.IsAdmin = user.Email.StartsWith("admin");
        Users.Add(result);

        return _mapper.Map<GetUser>(result);
    }

    private static string CreateToken(User user) {
        var claims = new List<Claim>(){
            new(ClaimTypes.NameIdentifier, user.Id!),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
        };

        // ! key has to be the same as the jwt token in the appsettings in the api project
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super secret token"));
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

public class MockPollService : IPollService
{
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public MockPollService(IMapper mapper, IUserService userService)
    {
        _mapper = mapper;
        _userService = userService;
    }

    public List<Poll> Polls { get; set; }=new();

    public async Task<IEnumerable<GetPoll>> Add(CreatePoll poll, string ownerId)
    {
        await _userService.ById(ownerId);
        
        poll.Validate();
        var result = _mapper.Map<Poll>(poll);
        result.OwnerId = ownerId;
        result.Id = Polls.Count.ToString();
        Polls.Add(result);
        
        return await All(ownerId);
    }

    public async Task<IEnumerable<GetPoll>> All(string userId)
    {
        await _userService.ById(userId);
        return Polls.Select(p => _mapper.Map<GetPoll>(p, opt => opt.Items["UserId"] = userId));
    }

    public async Task<GetPoll> ById(string userId, string pollId)
    {
        await _userService.ById(userId);
        return _mapper.Map<GetPoll>(Polls.FirstOrDefault(p => p.Id == pollId), opt => opt.Items["UserId"] = userId)
            ?? throw new PollNotFoundException(pollId);
        ;
    }

    public async Task<GetPoll> Vote(string ownerId, string pollId, int optionI)
    {
        await _userService.ById(ownerId);
        var poll = Polls.FirstOrDefault(p => p.Id == pollId)
            ?? throw new PollNotFoundException(pollId)
        ;

        if (optionI < 0 || optionI >= poll.Options.Count) throw new OptionNotFoundException(pollId, optionI);
        if (poll.Options.Any(o => o.Voters.Contains(ownerId))) throw new AlreadyVotedException(ownerId, pollId);

        poll.Options[optionI].Voters.Add(ownerId);
        return _mapper.Map<GetPoll>(poll, opt => opt.Items["UserId"] = ownerId);
    }
}