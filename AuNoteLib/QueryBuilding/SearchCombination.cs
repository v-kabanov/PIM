using Lucene.Net.Search;

namespace AuNoteLib.QueryBuilding
{
    class SearchCombination
    {
        public Search Left { get; private set; }
        public BooleanOperation Operation { get; private set; }
        public ILuceneIndex Index { get; private set; }

        public SearchCombination(Search left, BooleanOperation operation, ILuceneIndex index)
        {
            Left = left;
            Operation = operation;
            Index = index;
        }

        public Search Query(string fieldName, string queryText, bool fuzzy)
        {
            var additionalQuery = new BooleanQuery();
            additionalQuery.Add(Left.Query, this.Occur);
            additionalQuery.Add(Index.CreateQuery(fieldName, queryText, fuzzy), this.Occur);
            return new Search(Index, additionalQuery);
        }

        private Occur Occur => Operation == BooleanOperation.And ? Occur.MUST : Occur.SHOULD;
    }
}