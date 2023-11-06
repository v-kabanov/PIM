﻿using FluentNHibernate.Mapping;

namespace PimWeb.AppCode.Map;

public class ClassMapBase<T> : ClassMap<T>
{
    protected ClassMapBase()
    {
        Schema("public");
    }
}

public class NoteMap : ClassMapBase<Note>
{
    /// <inheritdoc />
    public NoteMap()
    {
        //Table("note");
        Id(x => x.Id).GeneratedBy.Sequence("note_id_seq");
        Map(x => x.Text);
        
        Map(x => x.CreateTime);
        Map(x => x.LastUpdateTime);

        Version(x => x.IntegrityVersion).UnsavedValue("0");
        OptimisticLock.Version();
    }
}