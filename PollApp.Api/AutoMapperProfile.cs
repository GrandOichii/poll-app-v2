using AutoMapper;

namespace PollApp.Api;

public class AutoMapperProfile : Profile {
    public AutoMapperProfile()
    {
        CreateMap<Poll, GetPoll>();
        CreateMap<CreatePoll, Poll>();
        
        CreateMap<PostOption, Option>();
        CreateMap<Option, GetOption>()
            .ForMember(go => go.VoteCount, b => b.MapFrom(o => o.Voters.Count));

        CreateMap<User, GetUser>();
    }
}