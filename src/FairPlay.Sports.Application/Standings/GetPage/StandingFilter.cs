using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Standings.GetPage;

/// <summary>
/// Type/Division/Category describe a criterion, not a Standing field - they are not applied by
/// <see cref="Apply"/> itself. <see cref="GetStandingsPageHandler"/> resolves them to
/// <see cref="TeamIds"/> via <c>ITeamRepository</c> before the filter runs, since Standing carries
/// no navigation to Team's classification.
/// </summary>
public sealed record StandingFilter(
    Guid? TeamId = null,
    FootballType? Type = null,
    Division? Division = null,
    AgeCategory? Category = null,
    IReadOnlyCollection<Guid>? TeamIds = null) : IQueryFilter<Standing>
{
    public IQueryable<Standing> Apply(IQueryable<Standing> source)
    {
        var query = source;

        if (TeamId is not null)
        {
            query = query.Where(standing => standing.TeamId == TeamId);
        }

        if (TeamIds is not null)
        {
            query = query.Where(standing => TeamIds.Contains(standing.TeamId));
        }

        return query;
    }
}
