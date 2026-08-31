using FairPlay.Sports.Application.Common;

namespace FairPlay.Sports.Infrastructure.Security;

internal sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
