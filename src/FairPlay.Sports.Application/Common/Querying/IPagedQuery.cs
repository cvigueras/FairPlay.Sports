namespace FairPlay.Sports.Application.Common.Querying;

public interface IPagedQuery
{
    int Page { get; }
    int PageSize { get; }
    string? Sort { get; }
}
