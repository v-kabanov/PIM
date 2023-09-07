// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-09
// Comment  
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using LiteDB;
using Pim.CommonLib;

namespace FulltextStorageLib;

public class LiteDbStorage<TDoc> : IDocumentStorage<TDoc, string>
    where TDoc: class
{
    private LiteDatabase _database;

    private readonly ILiteCollection<TDoc> _documents;

    public IDocumentAdapter<TDoc> DocumentAdapter { get; }

    public LiteDbStorage(LiteDatabase database, IDocumentAdapter<TDoc> documentAdapter)
    {
        Check.DoRequireArgumentNotNull(database, nameof(database));
        Check.DoRequireArgumentNotNull(documentAdapter, nameof(documentAdapter));
            
        _database = database;
        _documents = database.GetCollection<TDoc>();
        DocumentAdapter = documentAdapter;
    }

    /// <inheritdoc />
    public void SaveOrUpdate(TDoc document)
    {
        var save = DocumentAdapter.IsTransient(document);
        var incrementVersion = save;

        if (!save)
        {
            var id = DocumentAdapter.GetId(document);
            var existingDoc = _documents.FindById(id);
            save = existingDoc == null;
            if (!save)
            {
                incrementVersion = save = DocumentAdapter.IsChanged(existingDoc, document);
            }
        }

        if (save)
        {
            if (incrementVersion)
                DocumentAdapter.IncrementVersion(document);
            _documents.Upsert(document);
        }
    }

    /// <inheritdoc />
    public void SaveOrUpdate(params TDoc[] docs)
    {
        if(!_database.BeginTrans())
            throw new InvalidOperationException("Transaction is outstanding.");

        foreach (var doc in docs)
            SaveOrUpdate(doc);

       _database.Commit();
    }

    /// <inheritdoc />
    public TDoc GetExisting(string id)
    {
        return _documents.FindById(id);
    }

    /// <inheritdoc />
    public TDoc Delete(string id)
    {
        var result = GetExisting(id);
        _documents.Delete(id);
        return result;
    }

    /// <inheritdoc />
    public IEnumerable<TDoc> GetAll()
    {
        return _documents.FindAll();
    }

    /// <inheritdoc />
    public int CountAll()
    {
        return _documents.Count();
    }


    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects).
                _database?.Dispose();
                _database = null;
            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below.
            // set large fields to null.

            _disposedValue = true;
        }
    }

    // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~LiteDbStorage() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        Dispose(true);
        // uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }
}