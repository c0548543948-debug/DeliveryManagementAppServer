using System;
using System.Collections.Generic;
using System.Text;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.RouteItems.DTOs;

public class RouteItemDto
{
    // המזהים נשארים כדי שנוכל לדעת לאיזה מסלול/הזמנה זה שייך
    public int Id { get; set; }
    public int OrderId { get; set; }

    // נתונים נוספים
    public int StopOrder { get; set; }
    public StopType StopType { get; set; }
    public TimeOnly? EstimatedArrival { get; set; }

    // פרטי ההזמנה לתצוגה
    public string OriginAddress { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public OrderStatus OrderStatus { get; set; }
}
