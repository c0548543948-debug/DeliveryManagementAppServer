using AutoMapper;
using DeliveryManagementApp.Application.Couriers.DTOs;
using DeliveryManagementApp.Domain.Entities;

namespace DeliveryManagementApp.Application.Couriers.Mappings;

public class CourierProfile : Profile
{
    public CourierProfile()
    {
        CreateMap<Courier, CourierDto>()
            .ForMember(d => d.CourierId, opt => opt.MapFrom(s => s.Id))
            .ReverseMap();
    }
}
