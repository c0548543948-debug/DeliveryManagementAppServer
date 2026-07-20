namespace DeliveryManagementApp.Application.Common.Interfaces;

public interface IRouteProgressService
{
    void StartRoute(int routeId, int totalStops);
    void SetCurrentStop(int routeId, int stopNumber);
    (int CurrentStop, int TotalStops)? GetProgress(int routeId);
}
