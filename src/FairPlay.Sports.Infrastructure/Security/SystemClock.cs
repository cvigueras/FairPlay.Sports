using FairPlay.Sports.Application.Common;

namespace FairPlay.Sports.Infrastructure.Security;

/// <summary>Driven adapter: the real wall clock.</summary>
internal sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
