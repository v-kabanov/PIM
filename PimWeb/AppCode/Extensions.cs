using JetBrains.Annotations;

namespace PimWeb.AppCode;

public static class Extensions
{
    public static IQueryable<Note> ApplyPage(this IQueryable<Note> query, int pageSize, int pageNumber, bool takeOneExtra = false)
    {
        var result = query;
        if (pageNumber > 0)
            result = result.Skip(pageSize * pageNumber);
        
        var take = takeOneExtra ? pageSize + 1 : pageSize;
        
        return result.Take(take);
    }

    public static NHibernate.IQuery ApplyPage(this NHibernate.IQuery query, int pageSize, int pageNumber, bool takeOneExtra = false)
    {
        var result = query;
        if (pageNumber > 0)
            result = result.SetFirstResult(pageSize * pageNumber);
        
        var take = takeOneExtra ? pageSize + 1 : pageSize;
        
        return result.SetFetchSize(take);
    }
    
    public static IQueryable<Note> Sort(this IQueryable<Note> query, SearchableDocumentTime documentTime)
    {
        if (documentTime == SearchableDocumentTime.Creation)
            return query.OrderByDescending(x => x.CreateTime);
        
        return query.OrderByDescending(x => x.LastUpdateTime);
    }
    
    public static IQueryable<Note> ApplyTimeFilter(this IQueryable<Note> query, DateTimeOffset? lastUpdatePeriodStart, DateTimeOffset? lastUpdatePeriodEnd
        , DateTimeOffset? creationPeriodStart, DateTimeOffset? creationPeriodEnd)
    {
        if (lastUpdatePeriodStart.HasValue)
            query = query.Where(x => x.LastUpdateTime >= lastUpdatePeriodStart.Value.UtcDateTime);
        if (lastUpdatePeriodEnd.HasValue)
            query = query.Where(x => x.LastUpdateTime < lastUpdatePeriodEnd.Value.UtcDateTime);

        if (creationPeriodStart.HasValue)
            query = query.Where(x => x.CreateTime >= creationPeriodStart.Value.UtcDateTime);
        if (creationPeriodEnd.HasValue)
            query = query.Where(x => x.CreateTime < creationPeriodEnd.Value.UtcDateTime);
        
        return query;
    }

    public static NHibernate.IQuery AddOptionalFilterParameter([NotNull] this NHibernate.IQuery query, string parameterName, DateTime? value)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (value.HasValue)
            query.SetDateTime(parameterName, value.Value.ToUniversalTime());
        
        return query;
    }
}
