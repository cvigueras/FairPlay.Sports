namespace FairPlay.Sports.Application.Common.Querying;

public interface IQuerySort<T>
{
    IQueryable<T> Apply(IQueryable<T> source);
}
