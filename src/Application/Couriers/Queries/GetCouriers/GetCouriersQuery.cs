using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Couriers.DTOs;

namespace DeliveryManagementApp.Application.Couriers.Queries.GetCouriers;

public record GetCouriersQuery : IRequest<List<CourierDto>>;

public class GetCouriersQueryHandler : IRequestHandler<GetCouriersQuery, List<CourierDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUserNameService _userNames;

    public GetCouriersQueryHandler(IApplicationDbContext context, IMapper mapper, IUserNameService userNames)
    {
        _context = context;
        _mapper = mapper;
        _userNames = userNames;
    }

    public async Task<List<CourierDto>> Handle(GetCouriersQuery request, CancellationToken cancellationToken)
    {
        var couriers = await _context.ExecuteQueryAsync(_context.Couriers, cancellationToken);
        var dtos = _mapper.Map<List<CourierDto>>(couriers);

        if (dtos.Count > 0)
        {
            var userIds = dtos.Select(d => d.ApplicationUserId).Distinct();
            var names = await _userNames.GetNamesAsync(userIds, cancellationToken);
            foreach (var dto in dtos)
            {
                if (names.TryGetValue(dto.ApplicationUserId, out var name))
                {
                    dto.FirstName = name.FirstName;
                    dto.LastName = name.LastName;
                }
            }
        }

        return dtos;
    }
}
