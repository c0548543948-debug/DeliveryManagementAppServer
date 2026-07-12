using AutoMapper;
using DeliveryManagementApp.Application.RouteItems.DTOs;
using DeliveryManagementApp.Application.Routes.DTOs;
using DeliveryManagementApp.Domain.Entities;

namespace DeliveryManagementApp.Application.Routes.Mappings
{
    public class RouteProfile : Profile
    {
        public RouteProfile()
        {
            // מיפוי ה-Route ל-RouteDto
            CreateMap<Route, RouteDto>()
            .ForMember(dest => dest.RouteId, opt => opt.MapFrom(src => src.Id));
        }
    }


}

