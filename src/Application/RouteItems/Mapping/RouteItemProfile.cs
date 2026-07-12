using System;
using System.Collections.Generic;
using System.Text;
using DeliveryManagementApp.Application.RouteItems.DTOs;
using DeliveryManagementApp.Application.Routes.DTOs;

namespace DeliveryManagementApp.Application.RouteItems.Mapping;

public class RouteItemProfile : Profile
{
    public RouteItemProfile()
    {
       
        CreateMap<RouteItem, RouteItemDto>()
            .ForMember(dest => dest.OriginAddress, opt => opt.MapFrom(src => src.Order != null ? src.Order.OriginAddress : string.Empty))
            .ForMember(dest => dest.DestinationAddress, opt => opt.MapFrom(src => src.Order != null ? src.Order.DestinationAddress : string.Empty));
    }
}
