using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.TimeSlots;

public sealed record BlockSlotsCommand(Guid ProfileId, IReadOnlyList<Guid> SlotIds);

public sealed record BlockSlotsResult(int BlockedCount, IReadOnlyList<Guid> BlockedSlotIds);

public sealed class BlockSlotsHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public BlockSlotsHandler(
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

    public async Task<BlockSlotsResult> HandleAsync(
        BlockSlotsCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(command.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(command.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var slots = await _timeSlotRepository.GetByIdsAsync(command.SlotIds, cancellationToken);

        var blockedSlotIds = new List<Guid>();

        foreach (var slot in slots)
        {
            if (slot.Availability?.HostProfileId != command.ProfileId)
                continue;

            slot.Block();
            _timeSlotRepository.Update(slot);
            blockedSlotIds.Add(slot.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BlockSlotsResult(blockedSlotIds.Count, blockedSlotIds);
    }
}
