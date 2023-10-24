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
    
    public static IQueryable<Note> Sort(this IQueryable<Note> query, SearchableDocumentTime documentTime)
    {
        if (documentTime == SearchableDocumentTime.Creation)
            return query.OrderByDescending(x => x.CreateTime);
        
        return query.OrderByDescending(x => x.LastUpdateTime);
    }
    
    public static IQueryable<Note> ApplyTimeFilter(this IQueryable<Note> query, DateTime? lastUpdatePeriodStart, DateTime? lastUpdatePeriodEnd
        , DateTime? creationPeriodStart, DateTime? creationPeriodEnd)
    {
        if (lastUpdatePeriodStart.HasValue)
            query = query.Where(x => x.LastUpdateTime >= lastUpdatePeriodStart);
        if (lastUpdatePeriodEnd.HasValue)
            query = query.Where(x => x.LastUpdateTime < lastUpdatePeriodEnd);

        if (creationPeriodStart.HasValue)
            query = query.Where(x => x.CreateTime >= creationPeriodStart);
        if (creationPeriodEnd.HasValue)
            query = query.Where(x => x.CreateTime < creationPeriodEnd);
        
        return query;
    }
}