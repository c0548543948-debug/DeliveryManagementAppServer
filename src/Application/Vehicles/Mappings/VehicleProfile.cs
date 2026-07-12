using AutoMapper;
using DeliveryManagementApp.Application.Vehicles.DTOs;
using DeliveryManagementApp.Domain.Entities;

namespace DeliveryManagementApp.Application.Vehicles.Mappings;

public class VehicleProfile : Profile
{
    public VehicleProfile()
    {
        CreateMap<Vehicle, VehicleDto>()
            .ForMember(d => d.VehicleId, opt => opt.MapFrom(s => s.Id))
            .ReverseMap();
    }
}
