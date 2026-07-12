using DeliveryManagementApp.Application.Common.Behaviours;
using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Orders.Commands.CreateOrder;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DeliveryManagementApp.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<CreateOrderCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<CreateOrderCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<CreateOrderCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateOrderCommand(1, "Origin", "Destination", 1, 1, DateOnly.FromDateTime(DateTime.Now)), new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<CreateOrderCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateOrderCommand(1, "Origin", "Destination", 1, 1, DateOnly.FromDateTime(DateTime.Now)), new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}
