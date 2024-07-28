using AutoMapper;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Recipe, RecipeDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            // Map other fields as necessary
            .ForMember(dest => dest.AverageRating, opt => opt.Ignore()); // Assuming you'll calculate this later
    }

}
