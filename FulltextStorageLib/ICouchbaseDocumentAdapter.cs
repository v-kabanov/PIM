// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-27
// Comment  
// **********************************************************************************************/
// 

using Couchbase.Lite;

namespace FulltextStorageLib
{
    public interface ICouchbaseDocumentAdapter<TDoc> : IDocumentAdapter<TDoc>
    {
        TDoc Read(Document document);
    }
}