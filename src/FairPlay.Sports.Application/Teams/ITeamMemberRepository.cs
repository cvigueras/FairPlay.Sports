using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams;

public interface ITeamMemberRepository
{
    Task<IReadOnlyList<TeamMember>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeamMember>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Tracked lookup for the leave-team use case, which removes the entity.</summary>
    Task<TeamMember?> GetByTeamAndUserForUpdateAsync(
        Guid teamId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Untracked lookup for use cases (e.g. Challenges) that only need to read the role.</summary>
    Task<TeamMember?> GetByTeamAndUserAsync(
        Guid teamId, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAndTeamAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithRoleAsync(Guid teamId, TeamMemberRole role, CancellationToken cancellationToken = default);

    Task AddAsync(TeamMember member, CancellationToken cancellationToken = default);

    void Remove(TeamMember member);
}
