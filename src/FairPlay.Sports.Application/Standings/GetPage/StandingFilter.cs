using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Standings;

namespace FairPlay.Sports.Application.Standings.GetPage;

public sealed record StandingFilter(Guid? TeamId = null) : IQueryFilter<Standing>
{
    public IQueryable<Standing> Apply(IQueryable<Standing> source)
    {
        var query = source;

        if (TeamId is not null)
        {
            query = query.Where(standing => standing.TeamId == TeamId);
        }

        return query;
    }
}
