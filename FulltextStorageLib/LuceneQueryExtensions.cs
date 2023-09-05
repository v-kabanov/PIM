// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-02
// Comment  
// **********************************************************************************************/
// 

using Lucene.Net.Search;
using Pim.CommonLib;

namespace FulltextStorageLib;

public static class LuceneQueryExtensions
{
    /// <summary>
    ///     Add a query to <paramref name="combinedQuery"/>.
    /// </summary>
    /// <param name="combinedQuery">
    ///     Query combining other ones
    /// </param>
    /// <param name="query">
    ///     Optional, nothing done if null
    /// </param>
    /// <param name="occur">
    ///     How to treat <paramref name="query"/>
    /// </param>
    /// <returns>
    ///     <paramref name="combinedQuery"/> for chaining
    /// </returns>
    public static BooleanQuery Add(this BooleanQuery combinedQuery, Query query, Occur occur)
    {
        Check.DoRequireArgumentNotNull(combinedQuery, nameof(combinedQuery));

        if (query == null)
            return combinedQuery;

        combinedQuery.Add(query, occur);
        return combinedQuery;
    }

    /// <summary>
    ///     Add a query to <paramref name="combinedQuery"/> using logical AND.
    ///     <paramref name="combinedQuery"/> will return an item only if it is returned by <paramref name="query"/>.
    /// </summary>
    /// <param name="combinedQuery">
    ///     Query combining other ones
    /// </param>
    /// <param name="query">
    ///     Optional, nothing done if null
    /// </param>
    /// <returns>
    ///     <paramref name="combinedQuery"/> for chaining
    /// </returns>
    public static BooleanQuery And(this BooleanQuery combinedQuery, Query query)
    {
        return Add(combinedQuery, query, Occur.MUST);
    }

    /// <summary>
    ///     Add a query to <paramref name="combinedQuery"/> using logical AND NOT.
    ///     <paramref name="combinedQuery"/> will return an item only if it is NOT returned by <paramref name="query"/>.
    /// </summary>
    /// <param name="combinedQuery">
    ///     Query combining other ones
    /// </param>
    /// <param name="query">
    ///     Optional, nothing done if null
    /// </param>
    /// <returns>
    ///     <paramref name="combinedQuery"/> for chaining
    /// </returns>
    public static BooleanQuery AndNot(this BooleanQuery combinedQuery, Query query)
    {
        return Add(combinedQuery, query, Occur.MUST_NOT);
    }

    /// <summary>
    ///     Add a query to <paramref name="combinedQuery"/> using logical OR.
    ///     <paramref name="combinedQuery"/> will return an if it is returned by <paramref name="query"/>, even if other
    ///     combined queries do not return it.
    /// </summary>
    /// <param name="combinedQuery">
    ///     Query combining other ones
    /// </param>
    /// <param name="query">
    ///     Optional, nothing done if null
    /// </param>
    /// <returns>
    ///     <paramref name="combinedQuery"/> for chaining
    /// </returns>
    public static BooleanQuery Or(this BooleanQuery combinedQuery, Query query)
    {
        return Add(combinedQuery, query, Occur.SHOULD);
    }
}