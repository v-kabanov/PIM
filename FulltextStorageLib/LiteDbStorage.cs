// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-09
// Comment  
// **********************************************************************************************/

using System.Collections.Generic;
using FulltextStorageLib.Util;
using LiteDB;

namespace FulltextStorageLib
{
    public class LiteDbStorage<TDoc> : IDocumentStorage<TDoc, string>
        where TDoc: class
    {
        private LiteDatabase _database;

        private readonly LiteCollection<TDoc> _documents;

        public LiteDbStorage(LiteDatabase database)
        {
            Check.DoRequireArgumentNotNull(database, nameof(database));
            
            _database = database;
            _documents = database.GetCollection<TDoc>();
        }

        /// <inheritdoc />
        public void SaveOrUpdate(TDoc document)
        {
            _documents.Upsert(document);
        }

        /// <inheritdoc />
        public void SaveOrUpdate(params TDoc[] docs)
        {
            using (var tran = _database.BeginTrans())
            {
                foreach (var doc in docs)
                    SaveOrUpdate(doc);

                tran.Commit();
            }
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
}