using FluentNHibernate.Mapping;

namespace PimWeb.AppCode;

public class NoteMap : ClassMap<Note>
{
    /// <inheritdoc />
    public NoteMap()
    {
        Schema("public");
        //Table("note");
        Id(x => x.Id).GeneratedBy.Sequence("note_id_seq");
        Map(x => x.Text);
        
        Map(x => x.CreateTime);
        Map(x => x.LastUpdateTime);

        Version(x => x.IntegrityVersion).UnsavedValue("0");
        OptimisticLock.Version();
    }
}