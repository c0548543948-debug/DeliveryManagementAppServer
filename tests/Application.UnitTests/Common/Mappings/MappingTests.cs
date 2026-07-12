using System.Runtime.CompilerServices;
using AutoMapper;
using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Orders.DTOs;
using DeliveryManagementApp.Application.Customers.DTOs;
using DeliveryManagementApp.Application.Couriers.DTOs;
using DeliveryManagementApp.Application.Vehicles.DTOs;
using DeliveryManagementApp.Application.Routes.DTOs;
using DeliveryManagementApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DeliveryManagementApp.Application.UnitTests.Common.Mappings;

public class MappingTests
{
    private ILoggerFactory? _loggerFactory;
    private MapperConfiguration? _configuration;
    private IMapper? _mapper;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _loggerFactory = LoggerFactory.Create(b => b.AddDebug().SetMinimumLevel(LogLevel.Debug));

        _configuration = new MapperConfiguration(cfg =>
            cfg.AddMaps(typeof(IApplicationDbContext).Assembly),
            loggerFactory: _loggerFactory);

        _mapper = _configuration.CreateMapper();
    }

    [Test]
    public void ShouldHaveValidConfiguration()
    {
        _configuration!.AssertConfigurationIsValid();
    }

    [Test]
    [TestCase(typeof(Order), typeof(OrderDto))]
    [TestCase(typeof(Customer), typeof(CustomerDto))]
    [TestCase(typeof(Courier), typeof(CourierDto))]
    [TestCase(typeof(Vehicle), typeof(VehicleDto))]
    [TestCase(typeof(Domain.Entities.Route), typeof(RouteDto))]
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        var instance = GetInstanceOf(source);
        _mapper!.Map(instance, source, destination);
    }

    private static object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;
        return RuntimeHelpers.GetUninitializedObject(type);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _loggerFactory?.Dispose();
    }
}
