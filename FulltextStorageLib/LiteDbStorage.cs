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
    public class LiteDbManager
    {
        public void Map(BsonMapper mapper)
        {
            Check.DoRequireArgumentNotNull(mapper, nameof(mapper));

            mapper.Entity<Note>()
                .Ignore(n => n.Name)
                .Ignore(n => n.IsTransient);

            var id = new ObjectId("");
        }
    }

    public class LiteDbStorage<TDoc> : IDocumentStorage<TDoc, string>
        where TDoc : class
    {
        public string DataFilePath { get; }
        
        private LiteDB.LiteDatabase _database;

        /// <inheritdoc />
        public void SaveOrUpdate(TDoc document)
        {
            _database.GetCollection<TDoc>().Upsert(document);
        }

        /// <inheritdoc />
        public void SaveOrUpdate(params TDoc[] docs)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public TDoc GetExisting(string id)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public TDoc Delete(string id)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<TDoc> GetAll()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public int CountAll()
        {
            throw new System.NotImplementedException();
        }

        private void SaveOrUpdateImpl()
        {
            
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