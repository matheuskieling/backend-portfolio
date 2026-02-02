using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.TimeSlots;

public sealed record UnblockSlotsCommand(Guid ProfileId, IReadOnlyList<Guid> SlotIds);

public sealed record UnblockSlotsResult(int UnblockedCount, IReadOnlyList<Guid> UnblockedSlotIds);

public sealed class UnblockSlotsHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public UnblockSlotsHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        ITimeSlotRepository timeSlotRepository,
        ISchedulingUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _timeSlotRepository = timeSlotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UnblockSlotsResult> HandleAsync(
        UnblockSlotsCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(command.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(command.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var slots = await _timeSlotRepository.GetByIdsAsync(command.SlotIds, cancellationToken);

        var unblockedSlotIds = new List<Guid>();

        foreach (var slot in slots)
        {
            if (slot.Availability?.HostProfileId != command.ProfileId)
                continue;

            if (slot.IsBlocked)
            {
                slot.Unblock();
                _timeSlotRepository.Update(slot);
                unblockedSlotIds.Add(slot.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UnblockSlotsResult(unblockedSlotIds.Count, unblockedSlotIds);
    }
}
