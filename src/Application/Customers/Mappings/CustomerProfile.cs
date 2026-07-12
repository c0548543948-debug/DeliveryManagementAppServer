using AutoMapper;
using DeliveryManagementApp.Application.Customers.DTOs;
using DeliveryManagementApp.Domain.Entities;

namespace DeliveryManagementApp.Application.Customers.Mappings;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>().ReverseMap();
    }
}
