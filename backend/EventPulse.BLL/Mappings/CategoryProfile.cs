using AutoMapper;

namespace EventPulse.BLL.Mappings;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<DAL.Entities.Category, DTOs.Category.CategoryResponse>();
    }
}
