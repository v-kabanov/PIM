using Lucene.Net.Search;
using NFluent;

namespace FulltextStorageLib.QueryBuilding
{
    class Search
    {
        public ILuceneIndex Index { get; private set; }

        public Query Query { get; private set; }

        public Search(ILuceneIndex index, Query query)
        {
            Check.That(index).IsNotNull();
            Check.That(query).IsNotNull();

            Index = index;
            Query = query;
        }

        public SearchCombination And()
        {
            return new SearchCombination(this, BooleanOperation.And, Index);
        }
        public SearchCombination Or()
        {
            return new SearchCombination(this, BooleanOperation.Or, Index);
        }
    }
}