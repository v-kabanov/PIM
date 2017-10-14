// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-14
// Comment  
// **********************************************************************************************/
// 
using FulltextStorageLib.Util;
using LiteDB;

namespace FulltextStorageLib
{
    public class NoteLiteDb
    {
        public void Map(BsonMapper mapper)
        {
            Check.DoRequireArgumentNotNull(mapper, nameof(mapper));

            mapper.Entity<Note>()
                .Ignore(n => n.Name)
                .Ignore(n => n.IsTransient);
        }

        public LiteDatabase GetNoteDatabase(string connectionString)
        {
            var mapper = new BsonMapper();
            var result = new LiteDatabase(connectionString, mapper);
            Map(mapper);
            return result;
        }
    }
}