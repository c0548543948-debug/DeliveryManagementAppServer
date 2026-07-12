using AutoMapper;
using DeliveryManagementApp.Application.Orders.DTOs;
using DeliveryManagementApp.Domain.Entities;

namespace DeliveryManagementApp.Application.Orders.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.OrderId, opt => opt.MapFrom(s => s.Id))
            .ReverseMap();
    }
}
