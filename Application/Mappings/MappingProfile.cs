using Application.Features.Product.Commands;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateProductCommand, Domain.Entites.Product>()
                .ForMember(destination => destination.Description, source => source.MapFrom(src => src.Remarks));
        }
    }
}
