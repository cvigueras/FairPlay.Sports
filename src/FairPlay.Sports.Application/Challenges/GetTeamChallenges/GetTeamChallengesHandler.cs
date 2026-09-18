using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Challenges.GetTeamChallenges;

public sealed class GetTeamChallengesHandler(IChallengeRepository challenges, ITeamRepository teams)
    : IRequestHandler<GetTeamChallengesQuery, Result<IReadOnlyList<ChallengeDto>>>
{
    private readonly IChallengeRepository _challenges = challenges;
    private readonly ITeamRepository _teams = teams;

    public async Task<Result<IReadOnlyList<ChallengeDto>>> Handle(
        GetTeamChallengesQuery request, CancellationToken cancellationToken)
    {
        if (!await _teams.ExistsByIdAsync(request.TeamId, cancellationToken))
            return Result<IReadOnlyList<ChallengeDto>>.NotFound($"Team '{request.TeamId}' was not found.");

        var teamChallenges = await _challenges.GetByTeamIdAsync(request.TeamId, cancellationToken);

        var teamIds = teamChallenges
            .SelectMany(challenge => new[] { challenge.ChallengerTeamId, challenge.ChallengedTeamId })
            .Distinct();
        var teamsById = (await _teams.GetByIdsAsync(teamIds, cancellationToken)).ToDictionary(team => team.Id);

        IReadOnlyList<ChallengeDto> dtos = teamChallenges
            .OrderByDescending(challenge => challenge.CreatedAt)
            .Select(challenge => ChallengeDto.FromDomain(
                challenge, teamsById[challenge.ChallengerTeamId], teamsById[challenge.ChallengedTeamId]))
            .ToList();

        return Result<IReadOnlyList<ChallengeDto>>.Success(dtos);
    }
}
