using AutoMapper;

namespace PollApp.Api;

public class AutoMapperProfile : Profile {
    public AutoMapperProfile()
    {
        CreateMap<Poll, GetPoll>()
            .ForMember(p => p.CanVote, o => o.MapFrom((src, dest, destMember, context) => src.Options.All(o => !o.Voters.Contains(context.Items["UserId"]))))
            // TODO there seems to be a more elegant way to do this, but I don't know how
            .AfterMap((src, dest) => {
                var now = DateTime.Now;
                dest.Options.ForEach(o => o.VoteCount = src.VotesVisible || (src.ExpireDate < now) ? o.VoteCount : 0);
            });

        CreateMap<CreatePoll, Poll>()
            .ForMember(p => p.PostDate, b => b.MapFrom(_ => DateTime.Now));
        
        CreateMap<PostOption, Option>();
        CreateMap<Option, GetOption>()
            .ForMember(o => o.VoteCount, o => o.MapFrom(o => o.Voters.Count))
        ;

        CreateMap<User, GetUser>();
    }
}