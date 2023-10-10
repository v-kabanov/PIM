using System;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using FulltextStorageLib;
using FulltextStorageLib.Util;
using LiteDB;
using NHibernate;
using NHibernate.Cfg;

namespace UpgradeDatabaseFromNetFramework
{
public class Note
{
    public virtual int Id { get; set; }

    public virtual DateTime CreateTime { get; set; }

    public virtual DateTime LastUpdateTime { get; set; }

    /// <summary>
    ///     0 for unsaved, incremented every time it's updated in the storage
    /// </summary>
    public virtual int IntegrityVersion { get; set; } = 1;

    public virtual string Text { get; set; }
    
    public virtual string Name => ExtractName(Text);

    /// <returns>
    ///     null if text contains any valid text (non-blank-space)
    /// </returns>
    private static string ExtractName(string text)
    {
        if (text == null)
            return null;

        var firstLine = StringHelper.ExtractFirstLine(text);

        if (firstLine?.Length > 150)
            return firstLine.GetTextWithLimit(0, 100, false);

        return firstLine;
    }

    public override string ToString()
    {
        return $"{Name}#{Id}";
    }
}

public class PostgresNoteMap : ClassMap<Note>
{
    /// <inheritdoc />
    public PostgresNoteMap()
    {
        Schema("public");
        Table("Note");
        Id(x => x.Id).GeneratedBy.Sequence("`Note_Id_seq`");
        Map(x => x.Text);
        Map(x => x.CreateTime);
        Map(x => x.LastUpdateTime);
        Map(x => x.IntegrityVersion);
    }
}

public class PostgreSqlNamingStrategy : INamingStrategy
{
    public string ClassToTableName(string className)
    {
        return DoubleQuote(className);
    }
    public string PropertyToColumnName(string propertyName)
    {
        return DoubleQuote(propertyName);
    }
    public string TableName(string tableName)
    {
        return DoubleQuote(tableName);
    }
    public string ColumnName(string columnName)
    {
        return DoubleQuote(columnName);
    }
    public string PropertyToTableName(string className,
        string propertyName)
    {
        return DoubleQuote(propertyName);
    }
    public string LogicalColumnName(string columnName,
        string propertyName)
    {
        return string.IsNullOrWhiteSpace(columnName) ?
            DoubleQuote(propertyName) :
            DoubleQuote(columnName);
    }
    private static string DoubleQuote(string raw)
    {
        // In some cases the identifier is single-quoted.
        // We simply remove the single quotes:
        raw = raw.Replace("`", "");
        return $"\"{raw}\"";
    }
}
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var rawConfig = new NHibernate.Cfg.Configuration();
                rawConfig.SetNamingStrategy(new PostgreSqlNamingStrategy());
                
                var configuration = Fluently.Configure(rawConfig)
                    .Database(PostgreSQLConfiguration.Standard.ConnectionString(c => c
                            .Host("localhost")
                            .Port(5432)
                            .Database("pim")
                            .Username("pimweb")
                            .Password("*"))
                            .ShowSql)
                    .Mappings(m => m.FluentMappings
                            .AddFromAssembly(Assembly.GetExecutingAssembly()));
                
                //configuration = configuration
                //    .ExposeConfiguration(x => x.SetNamingStrategy(new PostgreSqlNamingStrategy())
                        //.SetProperty("hbm2ddl.keywords", "auto-quote")
                //        );

                var sessionFactory = configuration.BuildSessionFactory();

                var db = new LiteDatabase("Filename=f:\\temp\\Pim.db; Upgrade=false; Password=;");
                //Console.WriteLine("Version: {0}", db.GetCollection("Note")?.Count());
                
                using var storage = new LiteDbStorage<FulltextStorageLib.Note>(db, new NoteAdapter(false));
                
                Console.WriteLine("{0} notes in database", storage.CountAll());

                using var session = sessionFactory.OpenSession();
                session.FlushMode = FlushMode.Commit;
                using var transaction = session.BeginTransaction();
                
                var i = 0;
                foreach (var doc in storage.GetAll())
                {
                    var note = new Note
                    {
                        Text = doc.Text,
                        CreateTime = doc.CreateTime,
                        LastUpdateTime = doc.LastUpdateTime,
                        IntegrityVersion = doc.Version
                    };
                    session.Save(note);
                    session.Flush();
                }
                
                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            Console.ReadLine();
        }
    }
}