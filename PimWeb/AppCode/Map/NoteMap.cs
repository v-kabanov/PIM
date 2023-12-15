using FluentNHibernate.Mapping;

namespace PimWeb.AppCode.Map;

public class ClassMapBase<T> : ClassMap<T>
{
    protected ClassMapBase()
    {
        Schema("public");
    }
}

public class ClassMapVersionedBase<T> : ClassMapBase<T>
    where T: EntityBase
{
    protected ClassMapVersionedBase()
    {
        Version(x => x.IntegrityVersion).UnsavedValue("0");
        OptimisticLock.Version();
    }
}

public class NoteMap : ClassMapVersionedBase<Note>
{
    public const string ColumnNameLastUpdateTime = "last_update_time";
    public const string ColumnNameCreationTime = "create_time";
    
    /// <inheritdoc />
    public NoteMap()
    {
        //Table("note");
        Id(x => x.Id).GeneratedBy.Sequence("note_id_seq");
        Map(x => x.Text)
            .Not.Nullable();
        
        Map(x => x.CreateTime);
        Map(x => x.LastUpdateTime);
        
        HasManyToMany(x => x.Files)
            .Table("note_file")
            .ParentKeyColumn("note_id")
            .ChildKeyColumn("file_id")
            .NotFound.Exception()
            .Cascade.SaveUpdate();
    }
}