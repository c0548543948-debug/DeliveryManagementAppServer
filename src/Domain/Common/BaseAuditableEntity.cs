namespace DeliveryManagementApp.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateOnly Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateOnly LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}
