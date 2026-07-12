using DeliveryManagementApp.Shared;

namespace DeliveryManagementApp.TestAppHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);
        builder.Build().Run();
    }
}
