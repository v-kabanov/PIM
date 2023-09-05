// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-14
// Comment  
// **********************************************************************************************/
// 

using LiteDB;
using Pim.CommonLib;

namespace FulltextStorageLib;

public class NoteLiteDb
{
    public static void Map(BsonMapper mapper)
    {
        Check.DoRequireArgumentNotNull(mapper, nameof(mapper));

        mapper.Entity<Note>()
            .Ignore(n => n.Name)
            .Ignore(n => n.IsTransient);
    }

    public static LiteDatabase GetNoteDatabase(string connectionString)
    {
        return GetNoteDatabase(new ConnectionString(connectionString));
    }

    public static LiteDatabase GetNoteDatabase(ConnectionString connectionProperties)
    {
        Check.DoRequireArgumentNotNull(connectionProperties, nameof(connectionProperties));
        var mapper = new BsonMapper();
        var result = new LiteDatabase(connectionProperties, mapper);
        Map(mapper);
        return result;
    }
}