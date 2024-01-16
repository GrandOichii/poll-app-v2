using AutoMapper;

namespace PollApp.Api;

public class AutoMapperProfile : Profile {
    public AutoMapperProfile()
    {
        CreateMap<Poll, GetPoll>()
            .ForMember(gp => gp.CanVote, b => b.MapFrom((src, dest, destMember, context) => !src.Options.Any(o => o.Voters.Contains(context.Items["UserId"]))));
        CreateMap<CreatePoll, Poll>()
            .ForMember(p => p.PostDate, b => b.MapFrom(_ => DateTime.Now));
        
        CreateMap<PostOption, Option>();
        CreateMap<Option, GetOption>()
            .ForMember(go => go.VoteCount, b => b.MapFrom(o => o.Voters.Count));

        CreateMap<User, GetUser>();
    }
}