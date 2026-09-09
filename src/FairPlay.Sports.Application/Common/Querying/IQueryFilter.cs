namespace FairPlay.Sports.Application.Common.Querying;

public interface IQueryFilter<T>
{
    IQueryable<T> Apply(IQueryable<T> source);
}
